using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace PvpAutoLb.Windows.Components;

internal static class Tooltip
{
    public static void OnHover(string text)
    {
        if (!ImGui.IsItemHovered())
        {
            return;
        }

        Show(text);
    }

    public static void Show(string text)
    {
        using (ImRaii.Tooltip())
        {
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 24);
            ImGui.TextUnformatted(text);
            ImGui.PopTextWrapPos();
        }
    }
}
