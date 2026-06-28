using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class FilterSection
{
    public static void Draw(Configuration cfg)
    {
        var doomed = cfg.SkipDoomedTargets;
        if (SettingsRow.Toggle("##skipdoomed", "Skip targets that will die first",
                "If an enemy is losing HP fast enough to die before the LB lands (~1.2s cast lock), skip it so the charge isn't wasted on a kill someone else gets.",
                ref doomed))
        {
            cfg.SkipDoomedTargets = doomed;
            cfg.Save();
        }

        var guarded = cfg.SkipGuardedTargets;
        if (SettingsRow.Toggle("##skipguarded", "Skip targets using Guard",
                "Guard reduces incoming damage by 90% for 5s — the LB would land for ~10% and waste the charge, so guarded targets are skipped.",
                ref guarded))
        {
            cfg.SkipGuardedTargets = guarded;
            cfg.Save();
        }

        var immune = cfg.SkipInvulnerableTargets;
        if (SettingsRow.Toggle("##skipinvuln", "Skip targets immune to your LB",
                "Paladin's Phalanx LB (Hallowed Ground) and Dark Knight's Eventide LB (Undead Redemption) make the target immune to damage for 10s — the LB can't kill them, so these targets are skipped.",
                ref immune))
        {
            cfg.SkipInvulnerableTargets = immune;
            cfg.Save();
        }

        DrawDutyMask(cfg);
    }

    private static void DrawDutyMask(Configuration cfg)
    {
        SettingsRow.DrawTitle("Allowed duties");
        SettingsRow.DrawHelper("Auto-fire only runs in the PvP modes you tick here.");
        ImGui.Spacing();

        var mask = cfg.EnabledDuties;
        var changed = false;

        changed |= DrawDutyToggle(ref mask, DutyMask.CrystallineConflict, "Crystalline Conflict");
        ImGui.SameLine();
        changed |= DrawDutyToggle(ref mask, DutyMask.Frontline, "Frontline");

        changed |= DrawDutyToggle(ref mask, DutyMask.RivalWings, "Rival Wings");
        ImGui.SameLine();
        changed |= DrawDutyToggle(ref mask, DutyMask.CustomMatch, "Custom Match");

        changed |= DrawDutyToggle(ref mask, DutyMask.Other, "Other PvP");

        if (changed)
        {
            cfg.EnabledDuties = mask;
            cfg.Save();
        }
    }

    private static bool DrawDutyToggle(ref DutyMask mask, DutyMask flag, string label)
    {
        var on = (mask & flag) != 0;
        if (!ImGui.Checkbox(label, ref on)) return false;
        mask = on ? mask | flag : mask & ~flag;
        return true;
    }
}
