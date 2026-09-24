using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace PvpAutoLb.Windows;

internal static class Paint
{
    private const int ShadowLayers = 5;

    public static uint Col(Vector4 color) => ImGui.GetColorU32(color);

    public static void Fill(ImDrawListPtr drawList, Vector2 min, Vector2 max, Vector4 color, float rounding, ImDrawFlags flags = ImDrawFlags.RoundCornersAll)
        => drawList.AddRectFilled(min, max, Col(color), rounding, flags);

    public static void Stroke(ImDrawListPtr drawList, Vector2 min, Vector2 max, Vector4 color, float rounding, float thickness = 1f)
        => drawList.AddRect(min, max, Col(color), rounding, ImDrawFlags.RoundCornersAll, thickness);

    public static void TopLight(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding, float alpha = 0.075f)
        => drawList.AddLine(new Vector2(min.X + rounding, min.Y + 1f), new Vector2(max.X - rounding, min.Y + 1f), Col(new Vector4(1f, 1f, 1f, alpha)), 1f);

    public static void Surface(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding, Vector4 fill, Vector4 border)
    {
        Fill(drawList, min, max, fill, rounding);
        TopLight(drawList, min, max, rounding);
        Stroke(drawList, min, max, border, rounding);
    }

    public static void Pill(ImDrawListPtr drawList, Vector2 min, Vector2 max, Vector4 fill, Vector4 border)
    {
        var rounding = (max.Y - min.Y) * 0.5f;
        Fill(drawList, min, max, fill, rounding);
        Stroke(drawList, min, max, border, rounding);
    }

    public static void Shadow(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding, float spread, float alpha)
    {
        var offset = new Vector2(0f, spread * 0.4f);
        for (var layer = ShadowLayers; layer >= 1; layer--)
        {
            var fraction = layer / (float)ShadowLayers;
            var grow = new Vector2(spread * fraction, spread * fraction);
            var layerAlpha = alpha / ShadowLayers * (1.25f - fraction);
            drawList.AddRectFilled(min - grow + offset, max + grow + offset, Col(new Vector4(0f, 0f, 0f, layerAlpha)), rounding + grow.X);
        }
    }

    public static void Hairline(ImDrawListPtr drawList, Vector2 from, Vector2 to)
        => drawList.AddLine(from, to, Col(Styling.Hairline), 1f);
}
