using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows;

public sealed partial class AboutWindow
{
    private readonly record struct CommunityLink(string Id, FontAwesomeIcon Icon, string Title, string Body, string Url, Vector4 Accent);

    private const string CommunityLabel = "COMMUNITY";
    private const float TileHeight = 96f;
    private const float TileGap = 14f;
    private const float TileStackBelow = 540f;
    private const float TilePad = 18f;
    private const float TileMedallion = 26f;
    private const float TileTextGap = 16f;
    private const float TileTitleGap = 4f;
    private const float TileLift = 3f;
    private const float ArrowSlide = 5f;

    private static readonly CommunityLink[] CommunityLinks =
    [
        new("##palb_about_discord", FontAwesomeIcon.Comments, "Join the Discord", "Get help, report bugs, share ideas and hear about updates first.", DiscordUrl, Styling.AccentDiscord),
        new("##palb_about_github", FontAwesomeIcon.CodeBranch, "View on GitHub", "Browse the source code and every release.", RepoUrl, Styling.AccentViolet),
    ];

    private void DrawCommunity()
    {
        var scale = ImGuiHelpers.GlobalScale;
        SectionHeader(FontAwesomeIcon.Users, CommunityLabel, Styling.AccentDiscord, columnWidth);

        var origin = new Vector2(columnX, ImGui.GetCursorScreenPos().Y);
        var gap = TileGap * scale;
        var stacked = columnWidth < TileStackBelow * scale;
        var tileSize = new Vector2(stacked ? columnWidth : (columnWidth - gap) * 0.5f, TileHeight * scale);
        for (var index = 0; index < CommunityLinks.Length; index++)
        {
            var offset = stacked ? new Vector2(0f, index * (tileSize.Y + gap)) : new Vector2(index * (tileSize.X + gap), 0f);
            DrawTile(CommunityLinks[index], origin + offset, tileSize);
        }

        var rows = stacked ? CommunityLinks.Length : 1;
        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(columnWidth, rows * tileSize.Y + (rows - 1) * gap));
    }

    private static void DrawTile(in CommunityLink link, Vector2 slot, Vector2 size)
    {
        var scale = ImGuiHelpers.GlobalScale;
        ImGui.SetCursorScreenPos(slot);
        var hit = Hit.Area(link.Id, size);
        var hover = Motion.Hover(Motion.Key(link.Id), hit.Hovered);
        var press = Motion.Approach(Motion.Key(link.Id, 1), hit.Held ? 1f : 0f, 30f);
        OpenOrCopy(hit, link.Url);

        var lift = (TileLift * hover - press * 1.5f) * scale;
        var min = slot - new Vector2(0f, lift);
        var max = min + size;
        var rounding = Styling.CardRounding * 1.4f * scale;
        var drawList = ImGui.GetWindowDrawList();
        var accent = link.Accent;

        if (hover > 0.01f)
        {
            Paint.Shadow(drawList, min, max, rounding, 12f * scale, 0.35f * hover);
            Paint.Glow(drawList, min, max, rounding, accent, hover);
        }

        Paint.Glass(drawList, min, max, rounding, accent, 0.09f + 0.10f * hover);
        Paint.Stroke(drawList, min, max, Styling.WithAlpha(accent, 0.28f + 0.5f * hover), rounding, 1.2f * scale);

        var pad = TilePad * scale;
        var radius = TileMedallion * scale;
        var medallionCenter = new Vector2(min.X + pad + radius, min.Y + size.Y * 0.5f);
        drawList.AddCircleFilled(medallionCenter, radius * (1.25f + 0.1f * hover), Paint.Col(Styling.WithAlpha(accent, 0.10f + 0.10f * hover)), 40);
        drawList.AddCircleFilled(medallionCenter, radius, Paint.Col(Vector4.Lerp(accent, Styling.Lighten(accent, 0.15f), hover)), 40);
        drawList.AddCircle(medallionCenter, radius, Paint.Col(Styling.WithAlpha(Styling.Lighten(accent, 0.5f), 0.55f)), 40, 1.2f * scale);
        TextDraw.IconCentered(link.Icon, medallionCenter, Styling.TextStrong, 1.1f + 0.12f * hover);

        var arrowSize = TextDraw.IconSize(FontAwesomeIcon.ArrowRight);
        var arrowX = max.X - pad - arrowSize.X - ArrowSlide * scale * (1f - hover);
        TextDraw.Icon(FontAwesomeIcon.ArrowRight, new Vector2(arrowX, min.Y + (size.Y - arrowSize.Y) * 0.5f),
            Vector4.Lerp(Styling.WithAlpha(Styling.TextDim, 0.7f), Styling.Lighten(accent, 0.35f), hover));

        var textX = medallionCenter.X + radius + TileTextGap * scale;
        var textWidth = MathF.Max(1f, arrowX - TileTextGap * scale - textX);
        float titleHeight;
        using (TextDraw.PushScale(HeadlineFontScale))
        {
            titleHeight = TextDraw.LineHeight();
        }

        var titleGap = TileTitleGap * scale;
        var bodyHeight = TextDraw.MeasureWrapped(link.Body, textWidth).Y;
        var textY = min.Y + (size.Y - titleHeight - titleGap - bodyHeight) * 0.5f;
        using (TextDraw.PushScale(HeadlineFontScale))
        {
            TextDraw.At(TextDraw.Truncate(link.Title, textWidth), new Vector2(textX, textY), Styling.TextStrong);
        }

        TextDraw.Wrapped(link.Body, new Vector2(textX, textY + titleHeight + titleGap), textWidth, Vector4.Lerp(Styling.TextDim, Styling.TextSecondary, hover));
    }
}
