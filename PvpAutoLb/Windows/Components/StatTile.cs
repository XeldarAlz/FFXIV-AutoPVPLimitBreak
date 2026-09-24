using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using System.Numerics;

namespace PvpAutoLb.Windows.Components;

internal static class StatTile
{
    private const float PadX = 13f;
    private const float PadY = 10f;

    public static void Draw(string label, string value, string? sub, Vector4 accent, float width, float height = Layout.StatTileHeight)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var size = new Vector2(width, height * scale);
        var origin = ImGui.GetCursorScreenPos();
        var end = origin + size;
        var drawList = ImGui.GetWindowDrawList();

        Paint.Glass(drawList, origin, end, Styling.CardRounding * scale, accent, 0.05f);

        var padX = PadX * scale;
        var padY = PadY * scale;
        var labelSize = TextDraw.SmallCapsSize(label);
        var dotRadius = 3f * scale;
        drawList.AddCircleFilled(new Vector2(origin.X + padX + dotRadius, origin.Y + padY + labelSize.Y * 0.5f), dotRadius, Paint.Col(accent));
        TextDraw.SmallCaps(label, new Vector2(origin.X + padX + dotRadius * 2f + 6f * scale, origin.Y + padY), Styling.TextDim);

        var subWidth = 0f;
        if (!string.IsNullOrEmpty(sub))
        {
            using (Fonts.PushCaption())
            {
                var subSize = TextDraw.Measure(sub);
                subWidth = subSize.X + 8f * scale;
                TextDraw.At(sub, new Vector2(end.X - padX - subSize.X, end.Y - padY - subSize.Y - 1f * scale), Styling.TextDim);
            }
        }

        using (Fonts.PushHeadline())
        {
            var text = TextDraw.Truncate(value, size.X - padX * 2f - subWidth);
            var valueSize = TextDraw.Measure(text);
            TextDraw.At(text, new Vector2(origin.X + padX, end.Y - padY - valueSize.Y), Styling.TextStrong);
        }

        ImGui.Dummy(size);
    }
}
