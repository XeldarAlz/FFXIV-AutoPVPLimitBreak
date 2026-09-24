using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections.Config;

internal static class TargetingSettings
{
    private const float MinimumRange = 5f;
    private const float MaximumRange = 50f;
    private const float MeleeRange = 8f;
    private const float MidRange = 20f;
    private const float RangedRange = 35f;

    public static void Draw(Configuration configuration)
    {
        using var group = SettingsGroup.Begin(Loc.T(L.Settings.TargetingGroup));

        var autoSelect = configuration.AutoSelectLowestHp;
        if (SettingsControls.ToggleRow(Loc.T(L.Settings.AutoSelect), Loc.T(L.Settings.AutoSelectHelp), "##palb_auto_select", ref autoSelect))
        {
            configuration.AutoSelectLowestHp = autoSelect;
        }

        using (ImRaii.Disabled(!configuration.AutoSelectLowestHp))
        {
            var range = configuration.AutoSelectRangeYalms;
            if (SettingsControls.FloatSliderRow(Loc.T(L.Settings.ScanRange), Loc.T(L.Settings.ScanRangeHelp), "##palb_scan_range",
                    ref range, MinimumRange, MaximumRange, Loc.T(L.Settings.RangeFormat)))
            {
                configuration.AutoSelectRangeYalms = range;
            }

            SettingsRow.Caption(Loc.T(RangeHint(configuration.AutoSelectRangeYalms)));
        }

        if (!configuration.AutoSelectLowestHp)
        {
            SettingsRow.Note(Loc.T(L.Settings.ManualTargetNote));
        }
    }

    private static LocString RangeHint(float range) => range switch
    {
        <= MeleeRange => L.Settings.RangeMelee,
        <= MidRange => L.Settings.RangeMid,
        <= RangedRange => L.Settings.RangeRanged,
        _ => L.Settings.RangeArena,
    };
}
