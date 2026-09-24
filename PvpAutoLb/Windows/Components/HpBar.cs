using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;

namespace PvpAutoLb.Windows.Components;

internal static class HpBar
{
    private static readonly Vector4 ShieldColor = new(0.96f, 0.80f, 0.36f, 0.85f);

    public static void Draw(ImDrawListPtr drawList, Vector2 origin, float width, float height, float hp, float shield, float threshold, Vector4 fill)
    {
        var end = origin + new Vector2(width, height);
        var rounding = height * 0.5f;

        Paint.Fill(drawList, origin, end, Styling.WithAlpha(Styling.Surface0, 0.92f), rounding);
        Paint.Fill(drawList, origin, new Vector2(origin.X + width * threshold, end.Y), Styling.WithAlpha(Styling.AccentRose, 0.10f), rounding);
        Paint.Stroke(drawList, origin, end, Styling.WithAlpha(Styling.BorderDim, 0.55f), rounding);

        var hpWidth = width * Math.Clamp(hp, 0f, 1f);
        if (hpWidth > 0f)
        {
            var hpEnd = new Vector2(origin.X + MathF.Max(height, hpWidth), end.Y);
            Paint.Gradient(drawList, origin, hpEnd, Styling.Lighten(fill, 0.22f), fill, rounding);
        }

        if (shield > 0f)
        {
            var shieldStart = origin.X + hpWidth;
            var shieldEnd = MathF.Min(end.X, shieldStart + width * shield);
            Paint.Fill(drawList, new Vector2(shieldStart, origin.Y), new Vector2(shieldEnd, end.Y), ShieldColor, rounding);
        }

        DrawMarker(drawList, origin, width, height, threshold);
    }

    public static void DrawMarker(ImDrawListPtr drawList, Vector2 origin, float width, float height, float threshold)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var end = origin + new Vector2(width, height);
        var markerX = origin.X + width * Math.Clamp(threshold, 0f, 1f);
        var overhang = 3f * scale;
        var markerColor = Paint.Col(Styling.WithAlpha(Styling.TextStrong, 0.92f));
        drawList.AddLine(new Vector2(markerX, origin.Y - overhang), new Vector2(markerX, end.Y + overhang), markerColor, 2f * scale);
        var notch = 3.5f * scale;
        drawList.AddTriangleFilled(
            new Vector2(markerX - notch, origin.Y - overhang - notch),
            new Vector2(markerX + notch, origin.Y - overhang - notch),
            new Vector2(markerX, origin.Y - overhang),
            markerColor);
    }
}
