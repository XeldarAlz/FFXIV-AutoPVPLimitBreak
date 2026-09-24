using System;
using System.Globalization;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using PvpAutoLb.Core.Changelog;

namespace PvpAutoLb.Windows;

public sealed class ChangelogWindow : Window
{
    private const float TitleFontScale = 1.35f;
    private const float VersionFontScale = 1.12f;
    private const float PageHeaderGap = 14f;
    private const float RailWidth = 26f;
    private const float RailDotRadius = 5f;
    private const float RailHaloScale = 2f;
    private const float CardGap = 14f;
    private const float CardPadX = 18f;
    private const float CardPadY = 16f;
    private const float SeparatorGap = 12f;
    private const float BulletColumn = 18f;
    private const float BulletRadius = 2.5f;
    private const float BulletGap = 8f;
    private const float PillPadX = 8f;
    private const float PillPadY = 3f;
    private const float PillGap = 10f;
    private const int GlowLayers = 3;
    private const float GlowSpread = 6f;
    private const float RevealMs = 360f;
    private const float RevealStaggerMs = 80f;
    private const float RevealSlide = 10f;
    private const string DateFormat = "d MMM yyyy";
    private const string MetaSeparator = " · ";

    private const string TitleText = "What's new";
    private const string SubtitleText = "Every update, newest first.";
    private const string VersionFormat = "Version {0}";
    private const string LatestText = "Latest";
    private const string NewText = "New";
    private const string ChangeSingularFormat = "{0} change";
    private const string ChangePluralFormat = "{0} changes";

    private static readonly string LatestPillText = LatestText.ToUpperInvariant();
    private static readonly string NewPillText = NewText.ToUpperInvariant();

    private readonly Configuration configuration;
    private readonly string[] versionLabels = new string[ChangelogData.Entries.Length];
    private readonly string[] metaLabels = new string[ChangelogData.Entries.Length];

    private long openTick = long.MinValue / 2;
    private int unseenAtOpen;

    public ChangelogWindow(Configuration configuration) : base("Auto PVP LB Changelog###PvpAutoLbChangelog")
    {
        this.configuration = configuration;
        Size = new Vector2(560, 520);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(420, 300),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };

        var entries = ChangelogData.Entries;
        for (var index = 0; index < entries.Length; index++)
        {
            var entry = entries[index];
            var changeFormat = entry.Highlights.Length == 1 ? ChangeSingularFormat : ChangePluralFormat;
            versionLabels[index] = string.Format(CultureInfo.InvariantCulture, VersionFormat, entry.Version);
            metaLabels[index] = string.Concat(FormatDate(entry.Date), MetaSeparator,
                string.Format(CultureInfo.InvariantCulture, changeFormat, entry.Highlights.Length));
        }
    }

    // The badge clears the moment the window opens, so which entries count as new is decided once per open
    // from the version seen before this one.
    public override void OnOpen()
    {
        openTick = Environment.TickCount64;
        unseenAtOpen = ChangelogData.UnseenCount(configuration.LastSeenChangelogVersion);
        configuration.MarkChangelogSeen();
    }

    public override void Draw()
    {
        using var style = Styling.PushWindowStyle();
        DrawPageHeader();

        var entries = ChangelogData.Entries;
        for (var index = 0; index < entries.Length; index++)
        {
            var progress = Reveal(index);
            if (progress < 1f)
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (1f - progress) * RevealSlide * ImGuiHelpers.GlobalScale);
            }

            using (ImRaii.PushStyle(ImGuiStyleVar.Alpha, MathF.Max(0.0001f, progress)))
            {
                DrawEntry(index, index == entries.Length - 1);
            }
        }
    }

    private float Reveal(int index)
    {
        if (Plugin.PluginInterface.UiBuilder.ShouldUseReducedMotion)
        {
            return 1f;
        }

        var elapsed = Environment.TickCount64 - openTick - index * RevealStaggerMs;
        var linear = Math.Clamp(elapsed / RevealMs, 0f, 1f);
        return linear * linear * (3f - 2f * linear);
    }

    private static void DrawPageHeader()
    {
        ImGui.SetWindowFontScale(TitleFontScale);
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextStrong))
        {
            ImGui.TextUnformatted(TitleText);
        }

        ImGui.SetWindowFontScale(1f);
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
        {
            ImGui.TextUnformatted(SubtitleText);
        }

        Styling.VSpace(PageHeaderGap);
    }

    private void DrawEntry(int index, bool isLast)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var entry = ChangelogData.Entries[index];
        var isNew = index < unseenAtOpen;
        var isLatest = index == 0;
        var origin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        var railWidth = RailWidth * scale;
        var cardMin = new Vector2(origin.X + railWidth, origin.Y);
        var cardWidth = width - railWidth;
        var padX = CardPadX * scale;
        var padY = CardPadY * scale;
        var textLeft = cardMin.X + padX + BulletColumn * scale;
        var textWidth = MathF.Max(1f, cardMin.X + cardWidth - padX - textLeft);

        ImGui.SetWindowFontScale(VersionFontScale);
        var headerHeight = TextDraw.LineHeight();
        ImGui.SetWindowFontScale(1f);

        var bodyHeight = MeasureBody(entry.Highlights, textWidth);
        var separatorY = cardMin.Y + padY + headerHeight + SeparatorGap * scale;
        var cardHeight = separatorY - cardMin.Y + SeparatorGap * scale + bodyHeight + padY;
        var cardMax = new Vector2(cardMin.X + cardWidth, cardMin.Y + cardHeight);
        var drawList = ImGui.GetWindowDrawList();
        var rounding = Styling.CardRounding * scale;
        var headerMidY = cardMin.Y + padY + headerHeight * 0.5f;

        DrawRail(drawList, origin, headerMidY, cardHeight, isNew || isLatest, isLast);

        if (isNew)
        {
            DrawGlow(drawList, cardMin, cardMax, rounding);
        }

        if (isLatest)
        {
            Paint.Fill(drawList, cardMin, cardMax, Styling.CardBg, rounding);
            Paint.Fill(drawList, cardMin, cardMax, Styling.WithAlpha(Styling.AccentRed, 0.08f), rounding);
            Paint.TopLight(drawList, cardMin, cardMax, rounding);
            Paint.Stroke(drawList, cardMin, cardMax, Styling.WithAlpha(Styling.AccentRed, isNew ? 0.55f : 0.30f), rounding);
        }
        else
        {
            Paint.Surface(drawList, cardMin, cardMax, rounding, Styling.WithAlpha(Styling.CardBg, 0.75f), Styling.WithAlpha(Styling.BorderDim, 0.5f));
        }

        var x = cardMin.X + padX;
        ImGui.SetWindowFontScale(VersionFontScale);
        TextDraw.At(versionLabels[index], new Vector2(x, cardMin.Y + padY), isLatest ? Styling.AccentRedBright : Styling.TextStrong);
        x += TextDraw.Measure(versionLabels[index]).X + PillGap * scale;
        ImGui.SetWindowFontScale(1f);

        if (isNew || isLatest)
        {
            x += DrawPill(drawList, isNew ? NewPillText : LatestPillText, x, headerMidY, isNew) + PillGap * scale;
        }

        var meta = metaLabels[index];
        var metaSize = TextDraw.Measure(meta);
        var metaX = cardMax.X - padX - metaSize.X;
        if (metaX > x)
        {
            TextDraw.At(meta, new Vector2(metaX, headerMidY - metaSize.Y * 0.5f), Styling.TextMuted);
        }

        Paint.Hairline(drawList, new Vector2(cardMin.X + padX, separatorY), new Vector2(cardMax.X - padX, separatorY));
        DrawBody(drawList, entry.Highlights, cardMin.X + padX, textLeft, textWidth, separatorY + SeparatorGap * scale);

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(width, cardHeight + (isLast ? 0f : CardGap * scale)));
    }

    private static void DrawGlow(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding)
    {
        var spread = GlowSpread * ImGuiHelpers.GlobalScale;
        for (var layer = GlowLayers; layer >= 1; layer--)
        {
            var fraction = layer / (float)GlowLayers;
            var grow = new Vector2(spread * fraction, spread * fraction);
            var alpha = 0.10f * (1.2f - fraction);
            Paint.Fill(drawList, min - grow, max + grow, Styling.WithAlpha(Styling.AccentRed, alpha), rounding + grow.X);
        }
    }

    private static void DrawRail(ImDrawListPtr drawList, Vector2 origin, float dotY, float cardHeight, bool highlighted, bool isLast)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var center = new Vector2(origin.X + RailWidth * scale * 0.5f - 2f * scale, dotY);
        var radius = RailDotRadius * scale;
        if (!isLast)
        {
            var lineBottom = origin.Y + cardHeight + CardGap * scale + (dotY - origin.Y) - radius;
            drawList.AddLine(center + new Vector2(0f, radius + 2f * scale), new Vector2(center.X, lineBottom),
                Paint.Col(Styling.WithAlpha(Styling.BorderDim, 0.8f)), 1.5f * scale);
        }

        if (highlighted)
        {
            drawList.AddCircleFilled(center, radius * RailHaloScale, Paint.Col(Styling.WithAlpha(Styling.AccentRed, 0.28f)));
            drawList.AddCircleFilled(center, radius, Paint.Col(Styling.AccentRed));
            return;
        }

        drawList.AddCircle(center, radius, Paint.Col(Styling.WithAlpha(Styling.TextDim, 0.8f)), 0, 1.5f * scale);
    }

    private static float DrawPill(ImDrawListPtr drawList, string text, float x, float midY, bool filled)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var size = TextDraw.Measure(text);
        var min = new Vector2(x, midY - size.Y * 0.5f - PillPadY * scale);
        var max = new Vector2(x + size.X + PillPadX * 2f * scale, midY + size.Y * 0.5f + PillPadY * scale);
        var textPosition = new Vector2(min.X + PillPadX * scale, midY - size.Y * 0.5f);
        if (filled)
        {
            Paint.Pill(drawList, min, max, Styling.AccentRed, Styling.WithAlpha(Styling.AccentRedBright, 0.6f));
            TextDraw.At(text, textPosition, Styling.TextStrong);
        }
        else
        {
            Paint.Pill(drawList, min, max, Styling.WithAlpha(Styling.AccentRed, 0.16f), Styling.WithAlpha(Styling.AccentRed, 0.4f));
            TextDraw.At(text, textPosition, Styling.AccentRedBright);
        }

        return max.X - min.X;
    }

    private static float MeasureBody(string[] highlights, float textWidth)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var height = 0f;
        for (var index = 0; index < highlights.Length; index++)
        {
            if (index > 0)
            {
                height += BulletGap * scale;
            }

            height += TextDraw.MeasureWrapped(highlights[index], textWidth).Y;
        }

        return height;
    }

    private static void DrawBody(ImDrawListPtr drawList, string[] highlights, float bulletX, float textLeft, float textWidth, float top)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var lineHeight = TextDraw.LineHeight();
        var y = top;
        for (var index = 0; index < highlights.Length; index++)
        {
            if (index > 0)
            {
                y += BulletGap * scale;
            }

            var text = highlights[index];
            drawList.AddCircleFilled(new Vector2(bulletX + BulletRadius * scale, y + lineHeight * 0.5f), BulletRadius * scale, Paint.Col(Styling.AccentRed));
            TextDraw.Wrapped(text, new Vector2(textLeft, y), textWidth, Styling.TextSecondary);
            y += TextDraw.MeasureWrapped(text, textWidth).Y;
        }
    }

    private static string FormatDate(string isoDate)
        => DateTime.TryParse(isoDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed.ToString(DateFormat, CultureInfo.InvariantCulture)
            : isoDate;
}
