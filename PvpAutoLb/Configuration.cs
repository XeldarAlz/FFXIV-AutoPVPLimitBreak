using Dalamud.Configuration;
using ECommons.Throttlers;
using PvpAutoLb.Core;
using System;
using System.Collections.Generic;

namespace PvpAutoLb;

public enum ThresholdMode
{
    Percent,
    Absolute,
}

public class JobThreshold
{
    public ThresholdMode Mode { get; set; } = ThresholdMode.Percent;
    public float Percent { get; set; } = PvpAutoLbConstants.DefaultThresholdPercent;
    public uint Absolute { get; set; } = PvpAutoLbConstants.DefaultThresholdAbsolute;
}

public readonly record struct EffectiveThreshold(ThresholdMode Mode, float Percent, uint Absolute);

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 3;

    public bool Enabled { get; set; } = true;

    public ThresholdMode ThresholdMode { get; set; } = ThresholdMode.Percent;
    public float HpThresholdPercent { get; set; } = PvpAutoLbConstants.DefaultThresholdPercent;
    public uint HpThresholdAbsolute { get; set; } = PvpAutoLbConstants.DefaultThresholdAbsolute;

    public bool AutoSelectLowestHp { get; set; } = true;
    public float AutoSelectRangeYalms { get; set; } = PvpAutoLbConstants.DefaultAutoSelectRangeYalms;

    public Dictionary<uint, JobThreshold> PerJobThresholds { get; set; } = new();

    public Dictionary<uint, LbRule> PerJobRules { get; set; } = new();

    public int PresetVersion { get; set; }

    public bool SkipDoomedTargets { get; set; } = true;
    public bool SkipGuardedTargets { get; set; } = true;

    public bool PlaySoundOnFire { get; set; } = false;
    public int FireSoundId { get; set; } = 7;
    public bool LogFireToChat { get; set; } = false;

    public List<string> NameBlocklist { get; set; } = new();
    public DutyMask EnabledDuties { get; set; } = DutyMask.All;

    public uint LifetimeFires { get; set; }
    public uint LifetimeKills { get; set; }
    public uint LifetimeEnemiesAffected { get; set; }

    public LbRule GlobalOffensiveRule()
        => LbRule.OffensiveDefault(ThresholdMode, HpThresholdPercent, HpThresholdAbsolute);

    public LbRule EffectiveRuleFor(uint jobId)
        => jobId != 0 && PerJobRules.TryGetValue(jobId, out var r) ? r : GlobalOffensiveRule();

    public EffectiveThreshold EffectiveThresholdFor(uint jobId)
        => EffectiveRuleFor(jobId).EnemyThreshold();

    public bool HasJobRule(uint jobId) => jobId != 0 && PerJobRules.ContainsKey(jobId);

    public LbRule EnsureJobRule(uint jobId)
    {
        if (!PerJobRules.TryGetValue(jobId, out var r))
        {
            r = GlobalOffensiveRule();
            r.Source = RuleSource.User;
            PerJobRules[jobId] = r;
        }
        return r;
    }

    public void ClearJobRule(uint jobId) => PerJobRules.Remove(jobId);

    public bool ApplyPresets(IReadOnlyDictionary<uint, LbRule> rules, int presetVersion)
    {
        foreach (var (jobId, incoming) in rules)
        {
            if (jobId == 0) continue;
            if (PerJobRules.TryGetValue(jobId, out var existing) && existing.Source == RuleSource.User)
                continue;
            var copy = incoming.Clone();
            copy.Source = RuleSource.Preset;
            PerJobRules[jobId] = copy;
        }
        PresetVersion = presetVersion;
        Save();
        return true;
    }

    public void MigrateIfNeeded()
    {
        if (Version >= 3)
            return;
        foreach (var (jobId, jt) in PerJobThresholds)
        {
            if (jobId == 0 || PerJobRules.ContainsKey(jobId)) continue;
            var r = LbRule.OffensiveDefault(jt.Mode, jt.Percent, jt.Absolute);
            r.Source = RuleSource.User;
            PerJobRules[jobId] = r;
        }
        Version = 3;
        Save();
    }

    public string FormatEffective(uint jobId, string prefix = "Fires below ")
    {
        var t = EffectiveThresholdFor(jobId);
        var label = t.Mode == ThresholdMode.Percent
            ? $"{prefix}{t.Percent:F0}% HP"
            : $"{prefix}{t.Absolute:N0} HP";
        return HasJobRule(jobId) ? label + " (per-job)" : label;
    }

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);

    public void SaveDebounced()
    {
        if (EzThrottler.Throttle(PvpAutoLbConstants.ThrottleKeys.Save, PvpAutoLbConstants.SaveThrottleMs))
            Save();
    }
}
