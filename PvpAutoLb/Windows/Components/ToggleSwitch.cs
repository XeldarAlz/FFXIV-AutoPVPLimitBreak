using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;

namespace PvpAutoLb.Windows.Components;

internal static class ToggleSwitch
{
    public const float TrackWidthDip = 38f;

    public static bool Draw(string id, ref bool value)
    {
        var accent = Styling.AccentRed;
        var trackWidth = TrackWidthDip * ImGuiHelpers.GlobalScale;
        var trackHeight = 20f * ImGuiHelpers.GlobalScale;
        var knobRadius = (trackHeight - 4f * ImGuiHelpers.GlobalScale) * 0.5f;
        var padX = 3f * ImGuiHelpers.GlobalScale;

        var origin = ImGui.GetCursorScreenPos();
        var end = origin + new Vector2(trackWidth, trackHeight);
        var dl = ImGui.GetWindowDrawList();
        var hovered = ImGui.IsMouseHoveringRect(origin, end);

        var trackColor = value
            ? Vector4.Lerp(accent * 0.7f, accent, hovered ? 0.6f : 0.3f)
            : Vector4.Lerp(Styling.CardBgSoft, Styling.CardBgHover, hovered ? 1f : 0f);
        var knobColor = value ? Styling.TextStrong : Styling.TextSecondary;

        dl.AddRectFilled(origin, end, ImGui.GetColorU32(trackColor), trackHeight * 0.5f);
        dl.AddRect(origin, end, ImGui.GetColorU32(value ? accent : Styling.BorderDim), trackHeight * 0.5f);

        var knobX = value ? end.X - knobRadius - padX : origin.X + knobRadius + padX;
        var knobY = origin.Y + trackHeight * 0.5f;
        dl.AddCircleFilled(new Vector2(knobX, knobY), knobRadius, ImGui.GetColorU32(knobColor));

        ImGui.Dummy(new Vector2(trackWidth, trackHeight));

        if (hovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            value = !value;
            return true;
        }
        if (hovered) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        return false;
    }
}
