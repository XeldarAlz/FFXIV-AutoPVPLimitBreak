using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using System.Numerics;

namespace PvpAutoLb.Windows.Pages;

internal sealed partial class AboutPage
{
    private readonly record struct CommunityLink(string Id, FontAwesomeIcon Icon, LocString Title, LocString Body, string Url, Vector4 Accent);

    private const float TileHeight = 96f;
    private const float TileGap = 14f;
    private const float TileStackBelow = 540f;
    private const float TilePad = 18f;
    private const float TileMedallion = 26f;
    private const float TileTextGap = 16f;
    private const float TileLift = 3f;
    private const float ArrowSlide = 5f;

    private static readonly CommunityLink[] communityLinks =
    [
        new("##palb_about_discord", FontAwesomeIcon.Comments, L.About.DiscordTitle, L.About.DiscordBody, DiscordUrl, Styling.AccentDiscord),
        new("##palb_about_github", FontAwesomeIcon.CodeBranch, L.About.GitHubTitle, L.About.GitHubBody, RepoUrl, Styling.AccentRed),
    ];

    private void DrawCommunity()
    {
        var scale = ImGuiHelpers.GlobalScale;
        SectionHeader(FontAwesomeIcon.Users, Loc.T(L.About.Community), Styling.AccentDiscord, columnWidth);

        var origin = new Vector2(columnX, ImGui.GetCursorScreenPos().Y);
        var gap = TileGap * scale;
        var stacked = columnWidth < TileStackBelow * scale;
        var tileSize = new Vector2(stacked ? columnWidth : (columnWidth - gap) * 0.5f, TileHeight * scale);
        for (var index = 0; index < communityLinks.Length; index++)
        {
            var offset = stacked ? new Vector2(0f, index * (tileSize.Y + gap)) : new Vector2(index * (tileSize.X + gap), 0f);
            DrawTile(communityLinks[index], origin + offset, tileSize);
        }

        var rows = stacked ? communityLinks.Length : 1;
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
        TextDraw.IconCentered(link.Icon, medallionCenter, Styling.ForegroundOn(accent), 1.1f + 0.12f * hover);

        var arrowSize = TextDraw.IconSize(FontAwesomeIcon.ArrowRight);
        var arrowX = max.X - pad - arrowSize.X - ArrowSlide * scale * (1f - hover);
        TextDraw.Icon(FontAwesomeIcon.ArrowRight, new Vector2(arrowX, min.Y + (size.Y - arrowSize.Y) * 0.5f),
            Vector4.Lerp(Styling.WithAlpha(Styling.TextDim, 0.7f), Styling.Lighten(accent, 0.35f), hover));

        var textX = medallionCenter.X + radius + TileTextGap * scale;
        var textWidth = MathF.Max(1f, arrowX - TileTextGap * scale - textX);
        var title = Loc.T(link.Title);
        var body = Loc.T(link.Body);
        float titleHeight;
        using (Fonts.PushHeadline())
        {
            titleHeight = TextDraw.LineHeight();
        }

        float bodyHeight;
        using (Fonts.PushCaption())
        {
            bodyHeight = TextDraw.MeasureWrapped(body, textWidth).Y;
        }

        var textY = min.Y + (size.Y - titleHeight - 4f * scale - bodyHeight) * 0.5f;
        using (Fonts.PushHeadline())
        {
            TextDraw.At(TextDraw.Truncate(title, textWidth), new Vector2(textX, textY), Styling.TextStrong);
        }

        using (Fonts.PushCaption())
        {
            TextDraw.Wrapped(body, new Vector2(textX, textY + titleHeight + 4f * scale), textWidth, Vector4.Lerp(Styling.TextDim, Styling.TextSecondary, hover));
        }
    }
}
