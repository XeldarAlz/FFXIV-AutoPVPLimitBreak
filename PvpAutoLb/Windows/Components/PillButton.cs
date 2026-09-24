using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;

namespace PvpAutoLb.Windows.Components;

internal static class PillButton
{
    public enum Emphasis { Ghost, Tinted, Filled }

    private const float PadX = 13f;
    private const float IconGap = 6f;

    public static float Width(string label, FontAwesomeIcon icon)
    {
        var scale = ImGuiHelpers.GlobalScale;
        return PadX * 2f * scale + TextDraw.IconSize(icon).X + IconGap * scale + TextDraw.Measure(label).X;
    }

    public static bool Draw(string id, string label, Vector4 accent, Emphasis emphasis, FontAwesomeIcon icon, bool enabled = true, string? tooltip = null)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var size = new Vector2(Width(label, icon), Layout.ActionPillHeight * scale);
        var origin = ImGui.GetCursorScreenPos();
        var hit = Hit.Area(id, size, enabled);
        var hover = Motion.Hover(Motion.Key(id), hit.Hovered);
        var drawList = ImGui.GetWindowDrawList();
        var end = origin + size;

        var (fill, border, text) = Palette(accent, emphasis, hover, enabled);
        if (hit.Held)
        {
            fill = Styling.Darken(fill, 0.12f);
        }

        Paint.Pill(drawList, origin, end, fill, border);
        if (emphasis == Emphasis.Filled && enabled)
        {
            Paint.TopLight(drawList, origin, end, size.Y * 0.5f, 0.22f);
        }

        var midY = origin.Y + size.Y * 0.5f;
        var x = origin.X + PadX * scale;
        var iconSize = TextDraw.IconSize(icon);
        TextDraw.Icon(icon, new Vector2(x, midY - iconSize.Y * 0.5f), text);
        x += iconSize.X + IconGap * scale;

        var labelSize = TextDraw.Measure(label);
        TextDraw.At(label, new Vector2(x, midY - labelSize.Y * 0.5f), text);

        if (hit.Hovered && tooltip is not null)
        {
            Tooltip.Show(tooltip);
        }

        return hit.Clicked;
    }

    private static (Vector4 Fill, Vector4 Border, Vector4 Text) Palette(Vector4 accent, Emphasis emphasis, float hover, bool enabled)
    {
        if (!enabled)
        {
            return (Styling.WithAlpha(Styling.CardBgSoft, 0.6f), Styling.WithAlpha(Styling.BorderDim, 0.5f), Styling.TextMuted);
        }

        return emphasis switch
        {
            Emphasis.Filled => (
                Vector4.Lerp(accent, Styling.Lighten(accent, 0.15f), hover),
                Styling.WithAlpha(Styling.Lighten(accent, 0.5f), 0.6f),
                Styling.TextStrong),
            Emphasis.Tinted => (
                Styling.WithAlpha(accent, 0.16f + 0.12f * hover),
                Styling.WithAlpha(accent, 0.45f + 0.30f * hover),
                Vector4.Lerp(Styling.Lighten(accent, 0.25f), Styling.TextStrong, hover * 0.5f)),
            _ => (
                Styling.WithAlpha(Styling.CardBgHover, 0.9f * hover),
                Styling.WithAlpha(Styling.BorderDim, 0.5f + 0.3f * hover),
                Vector4.Lerp(Styling.TextSecondary, Styling.TextStrong, hover)),
        };
    }
}
