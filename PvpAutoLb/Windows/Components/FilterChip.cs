using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;

namespace PvpAutoLb.Windows.Components;

internal static class FilterChip
{
    private const float PadX = 11f;
    private const float DotRadius = 3.5f;
    private const float Gap = 7f;
    private const float ToggleSpeed = 18f;

    public static float Width(string label, string? count, FontAwesomeIcon? trailingIcon = null)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var width = PadX * 2f * scale + DotRadius * 2f * scale + Gap * scale + TextDraw.Measure(label).X;
        if (count is not null)
        {
            width += Gap * scale + TextDraw.Measure(count).X;
        }

        if (trailingIcon is { } glyph)
        {
            width += Gap * scale + TextDraw.IconSize(glyph).X;
        }

        return width;
    }

    public static bool Draw(string id, string label, string? count, Vector4 accent, bool active, float height,
        FontAwesomeIcon? trailingIcon, string tooltip)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var size = new Vector2(Width(label, count, trailingIcon), height);
        var origin = ImGui.GetCursorScreenPos();
        var end = origin + size;
        var hit = Hit.Area(id, size);
        var hover = Motion.Hover(Motion.Key(id), hit.Hovered);
        var on = Motion.Approach(Motion.Key(id, 1), active ? 1f : 0f, ToggleSpeed);
        var drawList = ImGui.GetWindowDrawList();

        var fill = Vector4.Lerp(Styling.WithAlpha(Styling.CardBgSoft, 0.5f + 0.4f * hover), Styling.WithAlpha(accent, 0.16f + 0.08f * hover), on);
        var border = Vector4.Lerp(Styling.WithAlpha(Styling.BorderDim, 0.45f + 0.3f * hover), Styling.WithAlpha(accent, 0.5f + 0.25f * hover), on);
        Paint.Pill(drawList, origin, end, fill, border);

        var midY = origin.Y + size.Y * 0.5f;
        var x = origin.X + PadX * scale;
        var radius = DotRadius * scale;
        var dotCenter = new Vector2(x + radius, midY);
        if (active)
        {
            drawList.AddCircleFilled(dotCenter, radius, Paint.Col(accent));
        }
        else
        {
            drawList.AddCircle(dotCenter, radius, Paint.Col(Styling.WithAlpha(accent, 0.7f)), 0, 1.2f * scale);
        }

        x += radius * 2f + Gap * scale;
        var labelColor = Vector4.Lerp(Vector4.Lerp(Styling.TextDim, Styling.TextSecondary, hover), Styling.TextStrong, on);
        var labelSize = TextDraw.Measure(label);
        TextDraw.At(label, new Vector2(x, midY - labelSize.Y * 0.5f), labelColor);
        x += labelSize.X;

        if (count is not null)
        {
            x += Gap * scale;
            var countSize = TextDraw.Measure(count);
            TextDraw.At(count, new Vector2(x, midY - countSize.Y * 0.5f), Vector4.Lerp(Styling.TextMuted, Styling.Lighten(accent, 0.2f), on));
            x += countSize.X;
        }

        if (trailingIcon is { } glyph)
        {
            x += Gap * scale;
            var iconSize = TextDraw.IconSize(glyph);
            TextDraw.Icon(glyph, new Vector2(x, midY - iconSize.Y * 0.5f), Vector4.Lerp(Styling.TextDim, Styling.TextStrong, hover));
        }

        if (hit.Hovered)
        {
            Tooltip.Show(tooltip);
        }

        return hit.Clicked;
    }
}
