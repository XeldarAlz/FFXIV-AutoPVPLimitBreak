using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace PvpAutoLb.Windows.Shell;

internal static class Dock
{
    public static void Background(ImDrawListPtr drawList, Vector2 origin, Vector2 end, float windowRounding)
    {
        Paint.Gradient(drawList, origin, end, Styling.WithAlpha(Styling.Surface1, 0.97f), Styling.WithAlpha(Styling.Surface0, 0.97f), windowRounding, ImDrawFlags.RoundCornersBottom);
        Paint.Hairline(drawList, origin, new Vector2(end.X, origin.Y));
    }
}
