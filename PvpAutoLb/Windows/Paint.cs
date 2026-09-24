using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using System.Numerics;

namespace PvpAutoLb.Windows;

internal static class Paint
{
    private const int ShadowLayers = 5;

    public static uint Col(Vector4 color) => ImGui.GetColorU32(color);

    private static uint Opaque(Vector4 color) => ImGui.ColorConvertFloat4ToU32(color with { W = 1f });

    public static void Fill(ImDrawListPtr drawList, Vector2 min, Vector2 max, Vector4 color, float rounding, ImDrawFlags flags = ImDrawFlags.RoundCornersAll)
        => drawList.AddRectFilled(min, max, Col(color), rounding, flags);

    public static void Stroke(ImDrawListPtr drawList, Vector2 min, Vector2 max, Vector4 color, float rounding, float thickness = 1f, ImDrawFlags flags = ImDrawFlags.RoundCornersAll)
        => drawList.AddRect(min, max, Col(color), rounding, flags, thickness);

    public static void Gradient(ImDrawListPtr drawList, Vector2 min, Vector2 max, Vector4 top, Vector4 bottom, float rounding, ImDrawFlags flags = ImDrawFlags.RoundCornersAll)
    {
        var start = drawList.VtxBuffer.Size;
        drawList.AddRectFilled(min, max, Col(new Vector4(1f, 1f, 1f, top.W)), rounding, flags);
        var end = drawList.VtxBuffer.Size;
        ImGuiP.ShadeVertsLinearColorGradientKeepAlpha(drawList, start, end, min, new Vector2(min.X, max.Y), Opaque(top), Opaque(bottom));
    }

    public static void GradientH(ImDrawListPtr drawList, Vector2 min, Vector2 max, Vector4 left, Vector4 right, float rounding, ImDrawFlags flags = ImDrawFlags.RoundCornersAll)
    {
        var start = drawList.VtxBuffer.Size;
        drawList.AddRectFilled(min, max, Col(new Vector4(1f, 1f, 1f, left.W)), rounding, flags);
        var end = drawList.VtxBuffer.Size;
        ImGuiP.ShadeVertsLinearColorGradientKeepAlpha(drawList, start, end, min, new Vector2(max.X, min.Y), Opaque(left), Opaque(right));
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

    public static void Glow(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding, Vector4 color, float intensity)
    {
        var scale = ImGuiHelpers.GlobalScale;
        for (var layer = 3; layer >= 1; layer--)
        {
            var grow = new Vector2(layer * 3f * scale, layer * 3f * scale);
            var alpha = 0.04f * (4 - layer) * intensity;
            drawList.AddRectFilled(min - grow, max + grow, Col(Styling.WithAlpha(color, alpha)), rounding + grow.X);
        }
    }

    public static void TopLight(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding, float alpha = 0.075f)
        => drawList.AddLine(new Vector2(min.X + rounding, min.Y + 1f), new Vector2(max.X - rounding, min.Y + 1f), Col(new Vector4(1f, 1f, 1f, alpha)), 1f);

    public static void Surface(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding, Vector4 fill, Vector4 border, bool topLight = true, float borderThickness = 1f)
    {
        Fill(drawList, min, max, fill, rounding);
        if (topLight)
        {
            TopLight(drawList, min, max, rounding);
        }
        Stroke(drawList, min, max, border, rounding, borderThickness);
    }

    public static void Glass(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding, Vector4 accent, float tint, float hover = 0f, bool elevated = false)
    {
        if (elevated)
        {
            Shadow(drawList, min, max, rounding, 10f * ImGuiHelpers.GlobalScale, 0.5f);
        }

        var top = Vector4.Lerp(Styling.Surface1, accent, tint * 1.3f);
        var bottom = Vector4.Lerp(Styling.Surface0, accent, tint * 0.8f);
        if (hover > 0f)
        {
            top = Vector4.Lerp(top, Styling.Surface2, hover * 0.6f);
            bottom = Vector4.Lerp(bottom, Styling.Surface1, hover * 0.6f);
        }

        Gradient(drawList, min, max, top with { W = 0.97f }, bottom with { W = 0.97f }, rounding);
        TopLight(drawList, min, max, rounding);

        var borderMix = Math.Clamp(tint * 2.2f + hover * 0.4f, 0f, 1f);
        var border = Vector4.Lerp(Styling.WithAlpha(Styling.BorderDim, 0.75f), Styling.WithAlpha(accent, 0.9f), borderMix);
        Stroke(drawList, min, max, border, rounding);
    }

    public static void Pill(ImDrawListPtr drawList, Vector2 min, Vector2 max, Vector4 fill, Vector4 border)
    {
        var rounding = (max.Y - min.Y) * 0.5f;
        Fill(drawList, min, max, fill, rounding);
        Stroke(drawList, min, max, border, rounding);
    }

    public static void Bar(ImDrawListPtr drawList, Vector2 origin, float width, float height, float fraction, Vector4 color)
    {
        var rounding = height * 0.5f;
        var end = origin + new Vector2(width, height);
        Fill(drawList, origin, end, Styling.WithAlpha(Styling.Surface0, 0.9f), rounding);
        Stroke(drawList, origin, end, Styling.WithAlpha(Styling.BorderDim, 0.55f), rounding);

        fraction = Math.Clamp(fraction, 0f, 1f);
        if (fraction <= 0f)
        {
            return;
        }

        var fillWidth = MathF.Max(height, width * fraction);
        var fillEnd = new Vector2(origin.X + fillWidth, end.Y);
        Gradient(drawList, origin, fillEnd, Styling.Lighten(color, 0.22f), color, rounding);
        drawList.AddCircleFilled(new Vector2(fillEnd.X - rounding, origin.Y + rounding), rounding * 1.7f, Col(Styling.WithAlpha(color, 0.22f)));
    }

    public static void Dot(ImDrawListPtr drawList, Vector2 center, float radius, Vector4 color, float haloAlpha = 0.22f)
    {
        drawList.AddCircleFilled(center, radius * 2.3f, Col(Styling.WithAlpha(color, haloAlpha)));
        drawList.AddCircleFilled(center, radius, Col(color));
    }

    public static void Hairline(ImDrawListPtr drawList, Vector2 from, Vector2 to)
        => drawList.AddLine(from, to, Col(Styling.Hairline), 1f);

    public static void Divider(float verticalPadding = 6f)
    {
        Styling.VSpace(verticalPadding);
        var origin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        Hairline(ImGui.GetWindowDrawList(), origin, origin + new Vector2(width, 0f));
        ImGui.Dummy(new Vector2(width, 1f));
        Styling.VSpace(verticalPadding);
    }

    public static void Check(ImDrawListPtr drawList, Vector2 center, float size, Vector4 color, float thickness)
    {
        var start = center + new Vector2(-size * 0.42f, 0f);
        var corner = center + new Vector2(-size * 0.10f, size * 0.32f);
        var tip = center + new Vector2(size * 0.46f, -size * 0.34f);
        var packed = Col(color);
        drawList.AddLine(start, corner, packed, thickness);
        drawList.AddLine(corner, tip, packed, thickness);
    }
}
