using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using System.Numerics;

namespace PvpAutoLb.Windows.Components;

internal static class ProgressRing
{
    private const float Top = -MathF.PI / 2f;

    private static Vector2 Dir(float angle) => new(MathF.Cos(angle), MathF.Sin(angle));

    private static void Arc(Vector2 center, float radius, float thickness, float startAngle, float endAngle, uint color)
    {
        var drawList = ImGui.GetWindowDrawList();
        var span = MathF.Abs(endAngle - startAngle);
        var segments = Math.Max(2, (int)MathF.Ceiling(span / (MathF.PI / 48f)));
        var previous = center + Dir(startAngle) * radius;
        for (var segment = 1; segment <= segments; segment++)
        {
            var angle = startAngle + (endAngle - startAngle) * (segment / (float)segments);
            var current = center + Dir(angle) * radius;
            drawList.AddLine(previous, current, color, thickness);
            previous = current;
        }

        var cap = thickness * 0.5f;
        drawList.AddCircleFilled(center + Dir(startAngle) * radius, cap, color);
        drawList.AddCircleFilled(center + Dir(endAngle) * radius, cap, color);
    }

    public static void Glow(Vector2 center, float radius, Vector4 color, float intensity)
    {
        var drawList = ImGui.GetWindowDrawList();
        for (var layer = 4; layer >= 1; layer--)
        {
            var layerRadius = radius * (0.72f + layer * 0.17f);
            var alpha = Math.Clamp(intensity * 0.05f * (5 - layer), 0f, 0.5f);
            drawList.AddCircleFilled(center, layerRadius, Paint.Col(Styling.WithAlpha(color, alpha)));
        }
    }

    public static void Disc(Vector2 center, float radius, Vector4 color)
        => ImGui.GetWindowDrawList().AddCircleFilled(center, radius, Paint.Col(color));

    public static void Track(Vector2 center, float radius, float thickness, Vector4 color)
        => Arc(center, radius, thickness, Top, Top + MathF.PI * 2f, Paint.Col(color));

    public static void Fill(Vector2 center, float radius, float thickness, float fraction, Vector4 color)
    {
        fraction = Math.Clamp(fraction, 0f, 1f);
        if (fraction <= 0.0001f)
        {
            return;
        }

        Arc(center, radius, thickness, Top, Top + fraction * MathF.PI * 2f, Paint.Col(color));
    }

    public static void Sweep(Vector2 center, float radius, float thickness, Vector4 color, double periodMs, float arcLength, float headAlpha)
    {
        var drawList = ImGui.GetWindowDrawList();
        var head = Top + Styling.Phase(periodMs) * MathF.PI * 2f;
        var tail = head - arcLength;
        var steps = Math.Max(10, (int)MathF.Ceiling(arcLength / (MathF.PI / 36f)));
        var previous = center + Dir(tail) * radius;
        for (var step = 1; step <= steps; step++)
        {
            var progress = step / (float)steps;
            var angle = tail + (head - tail) * progress;
            var current = center + Dir(angle) * radius;
            drawList.AddLine(previous, current, Paint.Col(Styling.WithAlpha(color, headAlpha * progress * progress)), thickness);
            previous = current;
        }

        drawList.AddCircleFilled(center + Dir(head) * radius, thickness * 0.62f, Paint.Col(Styling.WithAlpha(color, headAlpha)));
    }

    // A glyph fitted to a shape is drawn through the draw list at an explicit size, from the icon tier
    // that already covers the target, so the window font scale is never touched.
    public static void CenterIcon(Vector2 center, FontAwesomeIcon icon, Vector4 color, float targetHeight)
    {
        var glyph = icon.ToIconString();
        using var font = Fonts.PushIconFor(targetHeight);
        var naturalSize = ImGui.CalcTextSize(glyph);
        if (naturalSize.Y <= 0f)
        {
            return;
        }

        var fit = targetHeight / naturalSize.Y;
        var size = naturalSize * fit;
        ImGui.GetWindowDrawList().AddText(ImGui.GetFont(), ImGui.GetFontSize() * fit, center - size * 0.5f, Paint.Col(color), glyph, 0f);
    }
}
