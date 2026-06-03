using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace PvpAutoLb.Windows.Components;

internal static class SidebarTab
{
    public static bool Draw(string label, FontAwesomeIcon icon, Vector4 accent, bool selected)
    {
        var height = 38f * ImGuiHelpers.GlobalScale;
        var width = ImGui.GetContentRegionAvail().X;

        var origin = ImGui.GetCursorScreenPos();
        var end = origin + new Vector2(width, height);
        var dl = ImGui.GetWindowDrawList();
        var hovered = ImGui.IsMouseHoveringRect(origin, end);

        var bg = selected
            ? Vector4.Lerp(Styling.CardBg, accent, 0.18f)
            : hovered ? Styling.CardBgHover : new Vector4(0, 0, 0, 0);

        dl.AddRectFilled(origin, end, ImGui.GetColorU32(bg), 6f);
        if (selected)
            dl.AddRectFilled(origin, new Vector2(origin.X + 3f * ImGuiHelpers.GlobalScale, end.Y),
                ImGui.GetColorU32(accent), 1f);

        var padX = 13f * ImGuiHelpers.GlobalScale;
        var iconStr = icon.ToIconString();
        Vector2 iconSize;
        using (ImRaii.PushFont(UiBuilder.IconFont))
            iconSize = ImGui.CalcTextSize(iconStr);

        var iconPos = new Vector2(origin.X + padX, origin.Y + (height - iconSize.Y) * 0.5f);
        ImGui.SetCursorScreenPos(iconPos);
        using (ImRaii.PushFont(UiBuilder.IconFont))
        using (ImRaii.PushColor(ImGuiCol.Text, selected ? accent : Styling.TextSecondary))
            ImGui.TextUnformatted(iconStr);

        var labelSize = ImGui.CalcTextSize(label);
        var labelPos = new Vector2(
            origin.X + padX + iconSize.X + 10f * ImGuiHelpers.GlobalScale,
            origin.Y + (height - labelSize.Y) * 0.5f);
        ImGui.SetCursorScreenPos(labelPos);
        using (ImRaii.PushColor(ImGuiCol.Text, selected ? Styling.TextStrong : Styling.TextSecondary))
            ImGui.TextUnformatted(label);

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(width, height));

        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left)) return true;
        }
        return false;
    }
}
