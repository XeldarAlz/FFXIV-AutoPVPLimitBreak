using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class TargetingSection
{
    public static void Draw(Configuration cfg)
    {
        var auto = cfg.AutoSelectLowestHp;
        if (SettingsRow.Toggle("##autoselect", "Auto-select lowest-HP hostile",
                "Continuously scans every visible hostile and targets the one with the lowest HP, overriding your manual hard target.",
                ref auto))
        {
            cfg.AutoSelectLowestHp = auto;
            cfg.Save();
        }

        using (ImRaii.Disabled(!cfg.AutoSelectLowestHp))
        {
            SettingsRow.DrawTitle("Scan range");
            SettingsRow.DrawHelper("How far out to look for hostiles when auto-selecting.");
            ImGui.Spacing();

            var range = cfg.AutoSelectRangeYalms;
            ImGui.SetNextItemWidth(-1);
            if (ImGui.SliderFloat("##range", ref range, 5f, 50f, "%.0f y"))
            {
                cfg.AutoSelectRangeYalms = range;
                cfg.SaveDebounced();
            }

            ImGui.Spacing();
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
                ImGui.TextUnformatted(RangeHint(range));
        }
    }

    private static string RangeHint(float range) => range switch
    {
        <= 8f => "≈ melee range",
        <= 20f => "≈ mid range",
        <= 35f => "≈ ranged combat",
        _ => "≈ whole arena",
    };
}
