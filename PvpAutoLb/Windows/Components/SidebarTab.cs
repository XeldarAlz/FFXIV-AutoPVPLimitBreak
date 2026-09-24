using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using System.Numerics;

namespace PvpAutoLb.Windows.Components;

internal static class SidebarTab
{
    private const float Height = 40f;
    private const float PadX = 14f;
    private const float IconGap = 11f;

    public static bool Draw(string label, FontAwesomeIcon icon, Vector4 accent, bool selected)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var size = new Vector2(ImGui.GetContentRegionAvail().X, Height * scale);
        var origin = ImGui.GetCursorScreenPos();
        var end = origin + size;
        var hit = Hit.Area(label, size);
        var hover = Motion.Hover(Motion.Key(label), hit.Hovered);
        var active = Motion.Approach(Motion.Key(label, 1), selected ? 1f : 0f, 16f);
        var drawList = ImGui.GetWindowDrawList();
        var rounding = 9f * scale;

        var backgroundAlpha = MathF.Max(hover * 0.7f, active) * 0.9f;
        if (backgroundAlpha > 0.01f)
        {
            Paint.Fill(drawList, origin, end, Styling.WithAlpha(Styling.Tint(Styling.Surface2, accent, active * 0.18f), backgroundAlpha), rounding);
        }

        if (active > 0.01f)
        {
            var barMin = new Vector2(origin.X, origin.Y + size.Y * (0.5f - 0.25f * active));
            var barMax = new Vector2(origin.X + 3f * scale, origin.Y + size.Y * (0.5f + 0.25f * active));
            Paint.Fill(drawList, barMin, barMax, Styling.WithAlpha(accent, active), 2f * scale);
        }

        var iconColor = Vector4.Lerp(Styling.TextDim, accent, active);
        var textColor = Vector4.Lerp(Styling.TextSecondary, Styling.TextStrong, MathF.Max(hover, active));
        var midY = origin.Y + size.Y * 0.5f;
        var iconSize = TextDraw.IconSize(icon);
        TextDraw.Icon(icon, new Vector2(origin.X + PadX * scale, midY - iconSize.Y * 0.5f), iconColor);

        var labelSize = TextDraw.Measure(label);
        TextDraw.At(label, new Vector2(origin.X + PadX * scale + iconSize.X + IconGap * scale, midY - labelSize.Y * 0.5f), textColor);

        return hit.Clicked;
    }
}
