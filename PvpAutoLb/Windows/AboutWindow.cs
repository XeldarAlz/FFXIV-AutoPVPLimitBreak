using System;
using System.IO;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using PvpAutoLb.Core;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows;

public sealed partial class AboutWindow : Window, IDisposable
{
    private const string Name = "Auto PVP LB";
    private const string Author = "XeldarAlz";
    private const string RepoUrl = "https://github.com/XeldarAlz/FFXIV-AutoPVPLimitBreak";
    private const string DiscordUrl = "https://discord.gg/hppkAvdBEE";
    private const string PatreonUrl = "https://www.patreon.com/XeldarAlz";
    private const string IconFile = "Icon.png";
    private const string WindowId = "PvpAutoLbAbout";

    private const string MadeByLabel = "Made by " + Author;
    private const string OpenCopyHint = "Click to open · right-click to copy";

    private const float TitleFontScale = 1.6f;
    private const float HeadlineFontScale = 1.15f;
    private const float CaptionFontScale = 0.9f;

    private const float MaxColumnWidth = 860f;
    private const float SectionGap = 22f;
    private const float RevealMs = 460f;
    private const float RevealStaggerMs = 110f;
    private const float RevealSlide = 14f;

    private const float HeroHeight = 184f;
    private const float HeroPad = 26f;
    private const float HeroIconSize = 108f;
    private const float HeroTextGap = 26f;
    private const float HeroLineGap = 6f;
    private const float ChipPadX = 10f;
    private const float ChipPadY = 4f;
    private const float ChipIconGap = 6f;
    private const float FooterGap = 6f;

    private static readonly Vector2[] BloomOffsets = [new(1.6f, 0f), new(-1.6f, 0f), new(0f, 1.6f), new(0f, -1.6f)];
    private static readonly string VersionLabel = "v " + (typeof(AboutWindow).Assembly.GetName().Version?.ToString() ?? "?");

    private readonly string iconPath;
    private readonly bool hasIcon;
    private readonly string punchline;

    private long openTick = long.MinValue / 2;
    private float columnX;
    private float columnWidth;

    public AboutWindow() : base($"{Name}: About###{WindowId}")
    {
        iconPath = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName ?? string.Empty, "Images", IconFile);
        hasIcon = File.Exists(iconPath);
        punchline = Svc.PluginInterface.Manifest.Punchline ?? string.Empty;

        Size = new Vector2(640, 700);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(460, 560),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public void Dispose() { }

    public override void OnOpen() => openTick = Environment.TickCount64;

    public override void Draw()
    {
        using var style = Styling.PushWindowStyle();
        using (ImRaii.PushStyle(ImGuiStyleVar.Alpha, MathF.Max(0.0001f, Reveal(0))))
        {
            AmbientBackground();
        }

        var available = ImGui.GetContentRegionAvail().X;
        columnWidth = MathF.Min(available, MaxColumnWidth * ImGuiHelpers.GlobalScale);
        columnX = ImGui.GetCursorScreenPos().X + (available - columnWidth) * 0.5f;

        Styling.VSpace(6f);
        using (BeginReveal(0))
        {
            DrawHero();
        }

        Styling.VSpace(SectionGap);
        using (BeginReveal(1))
        {
            DrawCommunity();
        }

        Styling.VSpace(SectionGap);
        using (BeginReveal(2))
        {
            DrawSupport();
        }

        Styling.VSpace(SectionGap);
        using (BeginReveal(3))
        {
            DrawFooter();
        }

        Styling.VSpace(SectionGap);
    }

    private float Reveal(int index)
    {
        if (Motion.Reduced)
        {
            return 1f;
        }

        var elapsed = Environment.TickCount64 - openTick;
        return Motion.EaseOutCubic(Math.Clamp((elapsed - index * RevealStaggerMs) / RevealMs, 0f, 1f));
    }

    private ImRaii.StyleDisposable BeginReveal(int index)
    {
        var progress = Reveal(index);
        var cursorY = ImGui.GetCursorScreenPos().Y + (1f - progress) * RevealSlide * ImGuiHelpers.GlobalScale;
        ImGui.SetCursorScreenPos(new Vector2(columnX, cursorY));
        return ImRaii.PushStyle(ImGuiStyleVar.Alpha, MathF.Max(0.0001f, progress));
    }

    private static void AmbientBackground()
    {
        var windowPosition = ImGui.GetWindowPos();
        var min = windowPosition + ImGui.GetWindowContentRegionMin();
        var max = windowPosition + ImGui.GetWindowContentRegionMax();
        var width = max.X - min.X;
        var height = max.Y - min.Y;

        var drawList = ImGui.GetWindowDrawList();
        drawList.PushClipRect(min, max, true);
        SoftBlob(drawList, min + new Vector2(width * (0.26f + 0.12f * Motion.Wave(11000)), height * (0.20f + 0.10f * Motion.Wave(13700))), width * 0.55f, Styling.AccentViolet, 0.075f);
        SoftBlob(drawList, min + new Vector2(width * (0.80f + 0.12f * Motion.Wave(15500)), height * (0.32f + 0.10f * Motion.Wave(9300))), width * 0.48f, Styling.AccentPink, 0.060f);
        SoftBlob(drawList, min + new Vector2(width * (0.55f + 0.14f * Motion.Wave(17900)), height * (0.82f + 0.08f * Motion.Wave(12100))), width * 0.52f, Styling.AccentBlue, 0.050f);
        drawList.PopClipRect();
    }

    private void DrawHero()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var drawList = ImGui.GetWindowDrawList();
        var origin = ImGui.GetCursorScreenPos();
        var pad = HeroPad * scale;
        var iconSize = MathF.Min(HeroIconSize * scale, HeroHeight * scale - pad * 2f);
        var textGap = HeroTextGap * scale;
        var textWidth = MathF.Min(HeroTextWidth(), MathF.Max(1f, columnWidth - pad * 2f - iconSize - textGap));
        var groupX = origin.X + (columnWidth - iconSize - textGap - textWidth) * 0.5f;
        var textX = groupX + iconSize + textGap;

        float titleHeight;
        using (TextDraw.PushScale(TitleFontScale))
        {
            titleHeight = TextDraw.LineHeight();
        }

        var lineGap = HeroLineGap * scale;
        var punchlineHeight = punchline.Length > 0 ? TextDraw.LineHeight() + lineGap : 0f;
        var chipHeight = TextDraw.LineHeight() + ChipPadY * 2f * scale;

        var blockHeight = titleHeight + lineGap + punchlineHeight + lineGap + chipHeight;
        var height = MathF.Max(HeroHeight * scale, blockHeight + pad * 2f);
        var max = origin + new Vector2(columnWidth, height);
        var rounding = Styling.CardRounding * 1.6f * scale;

        Paint.Shadow(drawList, origin, max, rounding, 16f * scale, 0.5f);
        Paint.Gradient(drawList, origin, max, Styling.Tint(Styling.CardBgHover, Styling.AccentRed, 0.20f), Styling.Tint(Styling.CardBg, Styling.AccentRed, 0.05f), rounding);
        DrawAurora(drawList, origin, max);
        Paint.TopLight(drawList, origin, max, rounding, 0.14f);
        Paint.Stroke(drawList, origin, max, Styling.WithAlpha(Styling.AccentRed, 0.35f), rounding, 1.2f * scale);

        var bob = Motion.Reduced ? 0f : Motion.Wave(3200) * 3f * scale;
        DrawHeroIcon(drawList, new Vector2(groupX + iconSize * 0.5f, origin.Y + height * 0.5f + bob), iconSize);

        var y = origin.Y + (height - blockHeight) * 0.5f;
        DrawShimmerTitle(Name, new Vector2(textX, y), textWidth);
        y += titleHeight + lineGap;
        if (punchline.Length > 0)
        {
            TextDraw.At(TextDraw.Truncate(punchline, textWidth), new Vector2(textX, y), Styling.TextSecondary);
            y += punchlineHeight;
        }

        StaticChip(FontAwesomeIcon.Tag, VersionLabel, Styling.AccentRed, new Vector2(textX, y + lineGap), chipHeight);

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(columnWidth, height));
    }

    private float HeroTextWidth()
    {
        float width;
        using (TextDraw.PushScale(TitleFontScale))
        {
            width = TextDraw.Measure(Name).X;
        }

        width = MathF.Max(width, TextDraw.Measure(punchline).X);
        return MathF.Max(width, ChipWidth(FontAwesomeIcon.Tag, VersionLabel));
    }

    private static void DrawAurora(ImDrawListPtr drawList, Vector2 min, Vector2 max)
    {
        var width = max.X - min.X;
        var height = max.Y - min.Y;
        drawList.PushClipRect(min, max, true);
        SoftBlob(drawList, min + new Vector2(width * (0.18f + 0.08f * Motion.Wave(11000)), height * (0.30f + 0.20f * Motion.Wave(13700))), height * 1.3f, Styling.AccentRed, 0.10f);
        SoftBlob(drawList, min + new Vector2(width * (0.72f + 0.10f * Motion.Wave(15500)), height * (0.10f + 0.25f * Motion.Wave(9300))), height * 1.1f, Styling.AccentViolet, 0.08f);
        SoftBlob(drawList, min + new Vector2(width * (0.95f + 0.05f * Motion.Wave(17900)), height * (0.95f + 0.10f * Motion.Wave(12100))), height * 1.2f, Styling.AccentPink, 0.06f);
        drawList.PopClipRect();
    }

    // Many faint rings stacked together fall off smoothly; a handful of stronger ones shows as visible bands.
    private static void SoftBlob(ImDrawListPtr drawList, Vector2 center, float radius, Vector4 color, float peak)
    {
        const int layers = 18;
        var layerAlpha = peak * 2f / layers;
        for (var layer = layers; layer >= 1; layer--)
        {
            var fraction = layer / (float)layers;
            var alpha = layerAlpha * (1f - Motion.Smoothstep(fraction)) + layerAlpha * 0.15f;
            drawList.AddCircleFilled(center, radius * fraction, Paint.Col(Styling.WithAlpha(color, alpha)), 64);
        }
    }

    private void DrawHeroIcon(ImDrawListPtr drawList, Vector2 center, float size)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var half = size * 0.5f;
        ProgressRing.Glow(center, half * 1.05f, Styling.AccentRed, 0.5f + 0.45f * Styling.Pulse(Styling.PulseBreath));
        ProgressRing.Sweep(center, half * 1.28f, 2f * scale, Styling.AccentRedBright, Styling.PulseOrbit, MathF.PI * 0.5f, 0.9f);
        OrbitParticles(drawList, center, half * 1.28f, 3, 4600, 1, Styling.AccentRedBright, 2.2f * scale);

        var iconMin = center - new Vector2(half, half);
        var iconMax = center + new Vector2(half, half);
        var rounding = size * 0.22f;
        if (hasIcon)
        {
            var texture = Svc.Texture.GetFromFile(iconPath).GetWrapOrEmpty();
            drawList.AddImageRounded(texture.Handle, iconMin, iconMax, Vector2.Zero, Vector2.One, Paint.Col(Vector4.One), rounding, ImDrawFlags.RoundCornersAll);
        }
        else
        {
            Paint.Gradient(drawList, iconMin, iconMax, Styling.AccentRedBright, Styling.AccentRed, rounding);
            TextDraw.IconCentered(FontAwesomeIcon.Bolt, center, Styling.TextStrong, 2f);
        }

        Paint.Stroke(drawList, iconMin, iconMax, Styling.WithAlpha(Styling.AccentRedBright, 0.55f), rounding, 1.5f * scale);
        IconEasterEgg(iconMin, iconMax, scale);
    }

    private static void OrbitParticles(ImDrawListPtr drawList, Vector2 center, float radius, int count, double periodMs, int direction, Vector4 color, float dotRadius)
    {
        var baseAngle = -MathF.PI / 2f + direction * Styling.Phase(periodMs) * MathF.PI * 2f;
        for (var index = 0; index < count; index++)
        {
            var angle = baseAngle + index * (MathF.PI * 2f / count);
            var position = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            drawList.AddCircleFilled(position, dotRadius * 2.4f, Paint.Col(Styling.WithAlpha(color, 0.16f)));
            drawList.AddCircleFilled(position, dotRadius, Paint.Col(color));
        }
    }

    private static void DrawShimmerTitle(string text, Vector2 position, float width)
    {
        using var font = TextDraw.PushScale(TitleFontScale);
        var size = TextDraw.Measure(text);
        var drawList = ImGui.GetWindowDrawList();
        drawList.PushClipRect(position, position + new Vector2(width, size.Y), true);
        var bloom = Styling.WithAlpha(Styling.AccentRed, 0.22f);
        for (var index = 0; index < BloomOffsets.Length; index++)
        {
            TextDraw.At(text, position + BloomOffsets[index] * ImGuiHelpers.GlobalScale, bloom);
        }

        TextDraw.At(text, position, Styling.TextStrong);
        var bandWidth = size.X * 0.4f;
        var bandCenter = position.X - bandWidth + Styling.Phase(Styling.PulseOrbit) * (size.X + bandWidth * 2f);
        drawList.PushClipRect(new Vector2(bandCenter - bandWidth * 0.5f, position.Y), new Vector2(bandCenter + bandWidth * 0.5f, position.Y + size.Y), true);
        TextDraw.At(text, position, Styling.AccentRedBright);
        drawList.PopClipRect();
        drawList.PopClipRect();
    }

    private static float ChipWidth(FontAwesomeIcon icon, string label)
    {
        var scale = ImGuiHelpers.GlobalScale;
        return ChipPadX * 2f * scale + TextDraw.IconSize(icon).X + ChipIconGap * scale + TextDraw.Measure(label).X;
    }

    private static void StaticChip(FontAwesomeIcon icon, string label, Vector4 accent, Vector2 origin, float height)
    {
        var max = origin + new Vector2(ChipWidth(icon, label), height);
        Paint.Pill(ImGui.GetWindowDrawList(), origin, max, Styling.WithAlpha(accent, 0.12f), Styling.WithAlpha(accent, 0.38f));
        DrawChipContent(icon, label, accent, Styling.TextSecondary, origin, height);
    }

    private static void DrawChipContent(FontAwesomeIcon icon, string label, Vector4 iconColor, Vector4 textColor, Vector2 origin, float height)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var midY = origin.Y + height * 0.5f;
        var iconSize = TextDraw.IconSize(icon);
        var x = origin.X + ChipPadX * scale;
        TextDraw.Icon(icon, new Vector2(x, midY - iconSize.Y * 0.5f), iconColor);
        x += iconSize.X + ChipIconGap * scale;
        TextDraw.At(label, new Vector2(x, midY - TextDraw.LineHeight() * 0.5f), textColor);
    }

    private void DrawFooter()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var origin = ImGui.GetCursorScreenPos();
        using var font = TextDraw.PushScale(CaptionFontScale);
        var iconSize = TextDraw.IconSize(FontAwesomeIcon.Code);
        var labelSize = TextDraw.Measure(MadeByLabel);
        var gap = FooterGap * scale;
        var x = origin.X + (columnWidth - iconSize.X - gap - labelSize.X) * 0.5f;
        var twinkle = Styling.Pulse(2600.0);
        TextDraw.Icon(FontAwesomeIcon.Code, new Vector2(x, origin.Y + (labelSize.Y - iconSize.Y) * 0.5f),
            Vector4.Lerp(Styling.AccentBlue, Styling.Lighten(Styling.AccentBlueSoft, 0.3f), twinkle));
        TextDraw.At(MadeByLabel, new Vector2(x + iconSize.X + gap, origin.Y), Styling.TextDim);
        ImGui.Dummy(new Vector2(columnWidth, labelSize.Y));
    }

    private static void SectionHeader(FontAwesomeIcon icon, string label, Vector4 accent, float width)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var origin = ImGui.GetCursorScreenPos();
        var iconSize = TextDraw.IconSize(icon);
        var labelSize = TextDraw.Measure(label);
        var midY = origin.Y + iconSize.Y * 0.5f;
        TextDraw.Icon(icon, origin, accent);
        var labelX = origin.X + iconSize.X + 8f * scale;
        TextDraw.At(label, new Vector2(labelX, midY - labelSize.Y * 0.5f), Styling.TextDim);
        var lineStart = labelX + labelSize.X + 12f * scale;
        Paint.GradientH(ImGui.GetWindowDrawList(), new Vector2(lineStart, midY), new Vector2(origin.X + width, midY + 1f),
            Styling.WithAlpha(accent, 0.5f), Styling.WithAlpha(accent, 0f), 0f);
        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(width, iconSize.Y));
        Styling.VSpace(4f);
    }

    private static void OpenOrCopy(Hit.Result hit, string url)
    {
        if (!hit.Hovered)
        {
            return;
        }

        Tooltip.Show(OpenCopyHint);
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            ImGui.SetClipboardText(url);
        }

        if (hit.Clicked)
        {
            OpenUrl(url);
        }
    }

    private static void OpenUrl(string url)
        => UrlActions.OpenInBrowser(url, exception =>
            RunLog.Warning(exception, $"failed to launch browser for {url}, copied to clipboard instead"));
}
