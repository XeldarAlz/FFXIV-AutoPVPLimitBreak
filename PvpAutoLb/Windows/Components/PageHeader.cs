using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using System.Numerics;

namespace PvpAutoLb.Windows.Components;

internal static class PageHeader
{
    public static void Draw(string title, string subtitle, Vector4? subtitleColor = null)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var origin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;

        Vector2 titleSize;
        using (Fonts.PushTitle())
        {
            titleSize = TextDraw.Measure(title);
            TextDraw.At(title, origin, Styling.TextStrong);
        }

        var subtitleY = origin.Y + titleSize.Y + 5f * scale;
        var subtitleSize = TextDraw.MeasureWrapped(subtitle, width);
        TextDraw.Wrapped(subtitle, new Vector2(origin.X, subtitleY), width, subtitleColor ?? Styling.TextDim);

        var totalHeight = titleSize.Y + 5f * scale + subtitleSize.Y;
        ImGui.Dummy(new Vector2(width, totalHeight));
        Paint.Divider(12f);
    }
}
