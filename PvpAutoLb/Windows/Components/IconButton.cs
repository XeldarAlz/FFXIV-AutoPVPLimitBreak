using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace PvpAutoLb.Windows.Components;

internal static class IconButton
{
    public static bool Draw(FontAwesomeIcon icon, string id, float size, Vector4 color, string? tooltip = null)
    {
        var origin = ImGui.GetCursorScreenPos();
        var box = new Vector2(size, size);
        var hit = Hit.Area(id, box);
        var hover = Motion.Hover(Motion.Key(id), hit.Hovered);
        var center = origin + box * 0.5f;

        if (hover > 0.01f)
        {
            var alpha = hover * (hit.Held ? 1f : 0.85f);
            ImGui.GetWindowDrawList().AddCircleFilled(center, size * 0.5f, Paint.Col(Styling.WithAlpha(Styling.CardBorderDim, alpha)));
        }

        TextDraw.IconCentered(icon, center, Vector4.Lerp(color, Styling.TextStrong, hover * 0.55f));

        if (hit.Hovered && tooltip is not null)
        {
            Tooltip.Show(tooltip);
        }

        return hit.Clicked;
    }
}
