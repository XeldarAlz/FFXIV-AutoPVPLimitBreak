using Dalamud.Interface;
using PvpAutoLb.Core;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections.Config;

internal static class PerJobSettings
{
    private const int MinimumCount = 1;
    private const int MaximumCount = 7;
    private const float MinimumAllyHp = 1f;
    private const float MaximumAllyHp = 100f;
    private const float MinimumRadius = 5f;
    private const float MaximumRadius = 40f;

    private static readonly Segmented.Item[] modeItems = new Segmented.Item[3];
    private static readonly CachedText overrideLabel = new();
    private static readonly CachedText overrideHelp = new();

    public static void Draw(Configuration configuration)
    {
        var jobId = JobLookup.CurrentJobId;
        if (jobId == 0)
        {
            using (SettingsGroup.Begin(string.Empty))
            {
                SettingsRow.Note(Loc.T(L.Settings.PerJobNoJob));
            }

            return;
        }

        DrawOverrideGroup(configuration, jobId);
        if (!configuration.PerJobRules.TryGetValue(jobId, out var rule))
        {
            SettingsGroup.Footnote(Loc.T(L.Settings.PerJobUsesGlobal));
            return;
        }

        DrawModeGroup(configuration, rule);
    }

    private static void DrawOverrideGroup(Configuration configuration, uint jobId)
    {
        using var group = SettingsGroup.Begin(Loc.T(L.Settings.PerJobGroup));

        if (!overrideLabel.Matches(jobId))
        {
            var jobName = Formatting.Capitalize(JobLookup.Name(jobId));
            overrideLabel.Store(jobId, Loc.T(L.Settings.PerJobOverride, jobName));
            overrideHelp.Store(jobId, Loc.T(L.Settings.PerJobOverrideHelp, jobName));
        }

        var hasOverride = configuration.HasJobRule(jobId);
        if (!SettingsControls.ToggleRow(overrideLabel.Text, overrideHelp.Text, "##palb_job_override", ref hasOverride))
        {
            return;
        }

        if (hasOverride)
        {
            configuration.EnsureJobRule(jobId);
            return;
        }

        configuration.ClearJobRule(jobId);
    }

    private static void DrawModeGroup(Configuration configuration, LbRule rule)
    {
        using (SettingsGroup.Begin(Loc.T(L.Settings.PerJobRuleGroup)))
        {
            if (rule.Source == RuleSource.Preset)
            {
                SettingsRow.Note(Loc.T(L.Settings.PerJobPresetNote), Styling.AccentBlueSoft);
            }

            modeItems[0] = new Segmented.Item(FontAwesomeIcon.Bolt, Loc.T(L.Settings.ModeOffensive));
            modeItems[1] = new Segmented.Item(FontAwesomeIcon.ShieldAlt, Loc.T(L.Settings.ModeDefensive));
            modeItems[2] = new Segmented.Item(FontAwesomeIcon.UsersCog, Loc.T(L.Settings.ModeUtility));
            var selected = (int)rule.Mode;
            if (SettingsControls.SegmentedRow(Loc.T(L.Settings.LimitBreakMode), Loc.T(L.Settings.LimitBreakModeHelp), "##palb_job_mode", modeItems, ref selected))
            {
                rule.Mode = (LbFireMode)selected;
                rule.Source = RuleSource.User;
            }

            SettingsRow.Caption(Loc.T(ModeBlurb(rule.Mode)));

            switch (rule.Mode)
            {
                case LbFireMode.Offensive:
                    DrawOffensive(rule);
                    break;
                case LbFireMode.Defensive:
                    DrawDefensive(rule);
                    break;
                case LbFireMode.Utility:
                    DrawUtility(rule);
                    break;
            }
        }

        if (rule.Mode != LbFireMode.Offensive)
        {
            return;
        }

        using (SettingsGroup.Begin(Loc.T(L.Settings.PreviewGroup)))
        {
            ThresholdSettings.DrawPreview(rule.EnemyHpMode, rule.EnemyHpPercent, rule.EnemyHpAbsolute);
        }
    }

    private static LocString ModeBlurb(LbFireMode mode) => mode switch
    {
        LbFireMode.Defensive => L.Settings.ModeDefensiveBlurb,
        LbFireMode.Utility => L.Settings.ModeUtilityBlurb,
        _ => L.Settings.ModeOffensiveBlurb,
    };

    private static void DrawOffensive(LbRule rule)
    {
        var mode = rule.EnemyHpMode;
        if (ThresholdSettings.DrawModeRow("##palb_job_threshold_mode", ref mode))
        {
            rule.EnemyHpMode = mode;
            rule.Source = RuleSource.User;
        }

        var percent = rule.EnemyHpPercent;
        var absolute = rule.EnemyHpAbsolute;
        if (ThresholdSettings.DrawValueRow("##palb_job_threshold_value", rule.EnemyHpMode, ref percent, ref absolute))
        {
            rule.EnemyHpPercent = percent;
            rule.EnemyHpAbsolute = absolute;
            rule.Source = RuleSource.User;
        }
    }

    private static void DrawDefensive(LbRule rule)
    {
        var allyHp = rule.AllyHpPercent;
        if (SettingsControls.FloatSliderRow(Loc.T(L.Settings.AllyHp), Loc.T(L.Settings.AllyHpHelp), "##palb_job_ally_hp",
                ref allyHp, MinimumAllyHp, MaximumAllyHp, Loc.T(L.Settings.AllyHpFormat)))
        {
            rule.AllyHpPercent = allyHp;
            rule.Source = RuleSource.User;
        }

        DrawAllyCount(rule, L.Settings.HurtAllies, L.Settings.HurtAlliesHelp);
        DrawAllyRadius(rule);
        DrawEnemyRows(rule);
    }

    private static void DrawUtility(LbRule rule)
    {
        DrawAllyCount(rule, L.Settings.AlliesNear, L.Settings.AlliesNearHelp);
        DrawAllyRadius(rule);
        DrawEnemyRows(rule);
    }

    private static void DrawAllyCount(LbRule rule, LocString label, LocString help)
    {
        var count = rule.AllyCountNear;
        if (SettingsControls.StepperRow(Loc.T(label), Loc.T(help), "##palb_job_ally_count", ref count, 1, MinimumCount, MaximumCount, "%d"))
        {
            rule.AllyCountNear = count;
            rule.Source = RuleSource.User;
        }
    }

    private static void DrawAllyRadius(LbRule rule)
    {
        var radius = rule.AllyRadiusYalms;
        if (SettingsControls.FloatSliderRow(Loc.T(L.Settings.AllyRadius), Loc.T(L.Settings.AllyRadiusHelp), "##palb_job_ally_radius",
                ref radius, MinimumRadius, MaximumRadius, Loc.T(L.Settings.RangeFormat)))
        {
            rule.AllyRadiusYalms = radius;
            rule.Source = RuleSource.User;
        }
    }

    private static void DrawEnemyRows(LbRule rule)
    {
        var count = rule.EnemyCountNear;
        if (SettingsControls.StepperRow(Loc.T(L.Settings.EnemiesNear), Loc.T(L.Settings.EnemiesNearHelp), "##palb_job_enemy_count", ref count, 1, MinimumCount, MaximumCount, "%d"))
        {
            rule.EnemyCountNear = count;
            rule.Source = RuleSource.User;
        }

        var radius = rule.EnemyRadiusYalms;
        if (SettingsControls.FloatSliderRow(Loc.T(L.Settings.EnemyRadius), Loc.T(L.Settings.EnemyRadiusHelp), "##palb_job_enemy_radius",
                ref radius, MinimumRadius, MaximumRadius, Loc.T(L.Settings.RangeFormat)))
        {
            rule.EnemyRadiusYalms = radius;
            rule.Source = RuleSource.User;
        }
    }
}
