using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace PvpAutoLb.Windows.Shell;

internal static class Ambient
{
    private const int Layers = 5;

    public static void Draw(ImDrawListPtr drawList, Vector2 min, Vector2 max)
    {
        var width = max.X - min.X;
        var height = max.Y - min.Y;

        drawList.PushClipRect(min, max, true);
        Blob(drawList, min + new Vector2(width * (0.20f + 0.08f * Motion.Wave(16000)), height * (0.14f + 0.06f * Motion.Wave(21000))),
            width * 0.45f, Styling.AccentRed, 0.055f);
        Blob(drawList, min + new Vector2(width * (0.86f + 0.06f * Motion.Wave(19000)), height * (0.32f + 0.08f * Motion.Wave(14000))),
            width * 0.40f, Styling.AccentBlue, 0.040f);
        Blob(drawList, min + new Vector2(width * (0.55f + 0.10f * Motion.Wave(23000)), height * (0.96f + 0.05f * Motion.Wave(17000))),
            width * 0.42f, Styling.AccentPink, 0.035f);
        drawList.PopClipRect();
    }

    private static void Blob(ImDrawListPtr drawList, Vector2 center, float radius, Vector4 color, float peak)
    {
        for (var layer = Layers; layer >= 1; layer--)
        {
            var layerRadius = radius * layer / Layers;
            var alpha = peak * (1f - (layer - 1f) / Layers);
            drawList.AddCircleFilled(center, layerRadius, Paint.Col(Styling.WithAlpha(color, alpha)), 48);
        }
    }
}
