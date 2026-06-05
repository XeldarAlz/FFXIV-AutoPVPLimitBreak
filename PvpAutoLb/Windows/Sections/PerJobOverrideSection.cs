using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class PerJobOverrideSection
{
    private static readonly string[] ModeBlurbs =
    [
        "Fires on a hostile below an HP threshold.",
        "Fires (self-cast) when your team is pressured: enough hurt allies nearby AND enemies present.",
        "Fires (self-cast) in a committed teamfight: enough allies AND enemies clustered, regardless of HP.",
    ];

    public static void Draw(Configuration cfg)
    {
        var jobId = JobLookup.CurrentJobId;
        if (jobId == 0)
        {
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
                ImGui.TextWrapped("Log into a job to set a per-job rule. The rule you set here applies only while that job is active.");
            return;
        }

        var jobName = JobLookup.Name(jobId);
        var hasOverride = cfg.HasJobRule(jobId);

        if (SettingsRow.Toggle("##joboverride", $"Override for {jobName}",
                $"When on, {jobName} uses its own rule below instead of the global threshold.",
                ref hasOverride))
        {
            if (hasOverride) cfg.EnsureJobRule(jobId);
            else cfg.ClearJobRule(jobId);
            cfg.Save();
        }

        if (!cfg.HasJobRule(jobId)) return;

        DrawControls(cfg, jobId);
    }

    private static void DrawControls(Configuration cfg, uint jobId)
    {
        var rule = cfg.EnsureJobRule(jobId);

        var mode = rule.Mode;
        if (DrawModeSelector("jobmode", ref mode))
        {
            rule.Mode = mode;
            rule.Source = RuleSource.User;
            cfg.Save();
        }

        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextMuted))
            ImGui.TextWrapped(ModeBlurbs[(int)rule.Mode]);
        ImGui.Spacing();
        ImGui.Spacing();

        switch (rule.Mode)
        {
            case LbFireMode.Offensive: DrawOffensive(cfg, rule); break;
            case LbFireMode.Defensive: DrawDefensive(cfg, rule); break;
            case LbFireMode.Utility: DrawUtility(cfg, rule); break;
        }
    }

    private static bool DrawModeSelector(string idScope, ref LbFireMode mode)
    {
        var avail = ImGui.GetContentRegionAvail().X;
        var spacing = ImGui.GetStyle().ItemSpacing.X;
        var seg = (avail - spacing * 2f) / 3f;
        var size = new Vector2(seg, Layout.SegmentHeightCompact * ImGuiHelpers.GlobalScale);

        var changed = false;
        if (SegmentedControl.DrawSegment("Offensive", idScope + "_off", mode == LbFireMode.Offensive, size)) { mode = LbFireMode.Offensive; changed = true; }
        ImGui.SameLine();
        if (SegmentedControl.DrawSegment("Defensive", idScope + "_def", mode == LbFireMode.Defensive, size)) { mode = LbFireMode.Defensive; changed = true; }
        ImGui.SameLine();
        if (SegmentedControl.DrawSegment("Utility", idScope + "_util", mode == LbFireMode.Utility, size)) { mode = LbFireMode.Utility; changed = true; }
        return changed;
    }

    private static void DrawOffensive(Configuration cfg, LbRule rule)
    {
        var mode = rule.EnemyHpMode;
        if (ThresholdWidgets.DrawModeToggle("job", ref mode, segmentHeightDip: Layout.SegmentHeightCompact))
        {
            rule.EnemyHpMode = mode;
            rule.Source = RuleSource.User;
            cfg.Save();
        }

        ImGui.Spacing();

        var pct = rule.EnemyHpPercent;
        var abs = rule.EnemyHpAbsolute;
        if (ThresholdWidgets.DrawValueControl("job", rule.EnemyHpMode, ref pct, ref abs))
        {
            rule.EnemyHpPercent = pct;
            rule.EnemyHpAbsolute = abs;
            rule.Source = RuleSource.User;
            cfg.SaveDebounced();
        }

        ImGui.Spacing();
        ImGui.Spacing();
        ThresholdWidgets.DrawPreview(rule.EnemyHpMode, rule.EnemyHpPercent, rule.EnemyHpAbsolute);
    }

    private static void DrawDefensive(Configuration cfg, LbRule rule)
    {
        FloatRow(cfg, rule, "Ally HP %", "An ally counts as hurt below this % of max HP.",
            () => rule.AllyHpPercent, v => rule.AllyHpPercent = v, 1f, 100f, "%.0f%%");
        IntRow(cfg, rule, "Hurt allies", "Fire when at least this many hurt allies are nearby.",
            () => rule.AllyCountNear, v => rule.AllyCountNear = v, 1, 7);
        FloatRow(cfg, rule, "Ally radius", "How close an ally must be to count.",
            () => rule.AllyRadiusYalms, v => rule.AllyRadiusYalms = v, 5f, 40f, "%.0f y");
        IntRow(cfg, rule, "Enemies near", "Also require this many enemies within range.",
            () => rule.EnemyCountNear, v => rule.EnemyCountNear = v, 1, 7);
        FloatRow(cfg, rule, "Enemy radius", "How close an enemy must be to count.",
            () => rule.EnemyRadiusYalms, v => rule.EnemyRadiusYalms = v, 5f, 40f, "%.0f y");
    }

    private static void DrawUtility(Configuration cfg, LbRule rule)
    {
        IntRow(cfg, rule, "Allies near", "Fire when at least this many allies are clustered nearby.",
            () => rule.AllyCountNear, v => rule.AllyCountNear = v, 1, 7);
        FloatRow(cfg, rule, "Ally radius", "How close an ally must be to count.",
            () => rule.AllyRadiusYalms, v => rule.AllyRadiusYalms = v, 5f, 40f, "%.0f y");
        IntRow(cfg, rule, "Enemies near", "Also require this many enemies within range.",
            () => rule.EnemyCountNear, v => rule.EnemyCountNear = v, 1, 7);
        FloatRow(cfg, rule, "Enemy radius", "How close an enemy must be to count.",
            () => rule.EnemyRadiusYalms, v => rule.EnemyRadiusYalms = v, 5f, 40f, "%.0f y");
    }

    private static void IntRow(Configuration cfg, LbRule rule, string label, string tip,
        Func<int> get, Action<int> set, int min, int max)
    {
        DrawRowLabel(label, tip);
        ImGui.SetNextItemWidth(-1);
        var v = get();
        if (ImGui.DragInt("##jr_" + label, ref v, 0.1f, min, max, "%d"))
        {
            set(Math.Clamp(v, min, max));
            rule.Source = RuleSource.User;
            cfg.SaveDebounced();
        }
    }

    private static void FloatRow(Configuration cfg, LbRule rule, string label, string tip,
        Func<float> get, Action<float> set, float min, float max, string fmt)
    {
        DrawRowLabel(label, tip);
        ImGui.SetNextItemWidth(-1);
        var v = get();
        if (ImGui.SliderFloat("##jr_" + label, ref v, min, max, fmt))
        {
            set(v);
            rule.Source = RuleSource.User;
            cfg.SaveDebounced();
        }
    }

    private static void DrawRowLabel(string label, string tip)
    {
        ImGui.AlignTextToFramePadding();
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextSecondary))
            ImGui.TextUnformatted(label);
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(tip);
        ImGui.SameLine(140f * ImGuiHelpers.GlobalScale);
    }
}
