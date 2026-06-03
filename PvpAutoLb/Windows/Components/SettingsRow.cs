using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace PvpAutoLb.Windows.Components;

internal static class SettingsRow
{
    public static void Draw(string title, string? helper, Action drawControl)
    {
        DrawTitle(title);
        DrawHelper(helper);
        ImGui.Spacing();
        drawControl();
        Gap();
    }

    public static bool Toggle(string id, string title, string? helper, ref bool value)
    {
        ImGui.SetWindowFontScale(1.05f);
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextStrong))
            ImGui.TextUnformatted(title);
        ImGui.SetWindowFontScale(1.0f);

        Styling.SameLineRightAligned(ToggleSwitch.TrackWidthDip * ImGuiHelpers.GlobalScale);
        var changed = ToggleSwitch.Draw(id, ref value);

        DrawHelper(helper);
        Gap();
        return changed;
    }

    public static void DrawTitle(string title)
    {
        ImGui.SetWindowFontScale(1.05f);
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextStrong))
            ImGui.TextUnformatted(title);
        ImGui.SetWindowFontScale(1.0f);
    }

    public static void DrawHelper(string? helper)
    {
        if (string.IsNullOrEmpty(helper)) return;
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextMuted))
            ImGui.TextWrapped(helper);
    }

    private static void Gap()
    {
        ImGui.Spacing();
        ImGui.Spacing();
    }
}
