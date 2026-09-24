using PvpAutoLb.Core;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections.Config;

internal static class FilterSettings
{
    public static void Draw(Configuration configuration)
    {
        DrawSkipGroup(configuration);
        DrawDutyGroup(configuration);
    }

    private static void DrawSkipGroup(Configuration configuration)
    {
        using var group = SettingsGroup.Begin(Loc.T(L.Settings.SkipGroup));

        var doomed = configuration.SkipDoomedTargets;
        if (SettingsControls.ToggleRow(Loc.T(L.Settings.SkipDoomed), Loc.T(L.Settings.SkipDoomedHelp), "##palb_skip_doomed", ref doomed))
        {
            configuration.SkipDoomedTargets = doomed;
        }

        var guarded = configuration.SkipGuardedTargets;
        if (SettingsControls.ToggleRow(Loc.T(L.Settings.SkipGuarded), Loc.T(L.Settings.SkipGuardedHelp), "##palb_skip_guarded", ref guarded))
        {
            configuration.SkipGuardedTargets = guarded;
        }

        var invulnerable = configuration.SkipInvulnerableTargets;
        if (SettingsControls.ToggleRow(Loc.T(L.Settings.SkipInvulnerable), Loc.T(L.Settings.SkipInvulnerableHelp), "##palb_skip_invulnerable", ref invulnerable))
        {
            configuration.SkipInvulnerableTargets = invulnerable;
        }
    }

    private static void DrawDutyGroup(Configuration configuration)
    {
        using (SettingsGroup.Begin(Loc.T(L.Settings.DutyGroup)))
        {
            DrawDutyRow(configuration, DutyMask.CrystallineConflict, L.Duty.CrystallineConflict, "##palb_duty_cc");
            DrawDutyRow(configuration, DutyMask.Frontline, L.Duty.Frontline, "##palb_duty_frontline");
            DrawDutyRow(configuration, DutyMask.RivalWings, L.Duty.RivalWings, "##palb_duty_rival_wings");
            DrawDutyRow(configuration, DutyMask.CustomMatch, L.Duty.CustomMatch, "##palb_duty_custom");
            DrawDutyRow(configuration, DutyMask.Other, L.Duty.Other, "##palb_duty_other");
        }

        SettingsGroup.Footnote(Loc.T(L.Settings.DutyFootnote));
    }

    private static void DrawDutyRow(Configuration configuration, DutyMask duty, LocString label, string id)
    {
        var enabled = (configuration.EnabledDuties & duty) != 0;
        if (!SettingsControls.ToggleRow(Loc.T(label), null, id, ref enabled))
        {
            return;
        }

        configuration.EnabledDuties = enabled ? configuration.EnabledDuties | duty : configuration.EnabledDuties & ~duty;
    }
}
