using PvpAutoLb.Core.Localization;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System.Numerics;

namespace PvpAutoLb.Windows.Components;

internal static class Stepper
{
    public const float DefaultWidth = 168f;

    public static bool Draw(string id, ref int value, int step, int min, int max, string format, float width = DefaultWidth)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var height = ImGui.GetFrameHeight();
        var size = new Vector2(width * scale, height);
        var origin = ImGui.GetCursorScreenPos();
        var end = origin + size;
        var drawList = ImGui.GetWindowDrawList();
        var rounding = height * 0.5f;

        Paint.Fill(drawList, origin, end, Styling.WithAlpha(Styling.Surface0, 0.9f), rounding);
        Paint.Stroke(drawList, origin, end, Styling.WithAlpha(Styling.BorderDim, 0.6f), rounding);

        var changed = false;
        ImGui.PushID(id);

        ImGui.SetCursorScreenPos(origin);
        if (IconButton.Draw(FontAwesomeIcon.Minus, "##dec", height, enabled: value > min))
        {
            value = Math.Max(min, value - step);
            changed = true;
        }

        ImGui.SetCursorScreenPos(origin + new Vector2(height, 0f));
        ImGui.SetNextItemWidth(size.X - height * 2f);
        using (ImRaii.PushColor(ImGuiCol.FrameBg, Vector4.Zero)
            .Push(ImGuiCol.FrameBgHovered, Styling.WithAlpha(Styling.Surface2, 0.6f))
            .Push(ImGuiCol.FrameBgActive, Styling.WithAlpha(Styling.Surface3, 0.6f))
            .Push(ImGuiCol.Text, Styling.TextStrong))
        using (ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0f))
        {
            var edited = value;
            if (ImGui.DragInt("##value", ref edited, MathF.Max(0.25f, step * 0.1f), min, max, format))
            {
                value = Math.Clamp(edited, min, max);
                changed = true;
            }
        }

        if (ImGui.IsItemHovered())
        {
            Tooltip.Show(Loc.T(L.Common.DragAdjustHint));
        }

        ImGui.SetCursorScreenPos(new Vector2(end.X - height, origin.Y));
        if (IconButton.Draw(FontAwesomeIcon.Plus, "##inc", height, enabled: value < max))
        {
            value = Math.Min(max, value + step);
            changed = true;
        }

        ImGui.PopID();
        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(size);
        return changed;
    }
}
