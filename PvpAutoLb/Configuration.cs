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
    public int Version { get; set; } = 2;

    public bool Enabled { get; set; } = true;

    public ThresholdMode ThresholdMode { get; set; } = ThresholdMode.Percent;
    public float HpThresholdPercent { get; set; } = PvpAutoLbConstants.DefaultThresholdPercent;
    public uint HpThresholdAbsolute { get; set; } = PvpAutoLbConstants.DefaultThresholdAbsolute;

    public bool AutoSelectLowestHp { get; set; } = true;
    public float AutoSelectRangeYalms { get; set; } = PvpAutoLbConstants.DefaultAutoSelectRangeYalms;

    public Dictionary<uint, JobThreshold> PerJobThresholds { get; set; } = new();

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

    public EffectiveThreshold EffectiveThresholdFor(uint jobId)
        => PerJobThresholds.TryGetValue(jobId, out var j)
            ? new EffectiveThreshold(j.Mode, j.Percent, j.Absolute)
            : new EffectiveThreshold(ThresholdMode, HpThresholdPercent, HpThresholdAbsolute);

    public bool HasJobOverride(uint jobId) => jobId != 0 && PerJobThresholds.ContainsKey(jobId);

    public JobThreshold EnsureJobOverride(uint jobId)
    {
        if (!PerJobThresholds.TryGetValue(jobId, out var j))
        {
            j = new JobThreshold
            {
                Mode = ThresholdMode,
                Percent = HpThresholdPercent,
                Absolute = HpThresholdAbsolute,
            };
            PerJobThresholds[jobId] = j;
        }
        return j;
    }

    public void ClearJobOverride(uint jobId) => PerJobThresholds.Remove(jobId);

    public string FormatEffective(uint jobId, string prefix = "Fires below ")
    {
        var t = EffectiveThresholdFor(jobId);
        var label = t.Mode == ThresholdMode.Percent
            ? $"{prefix}{t.Percent:F0}% HP"
            : $"{prefix}{t.Absolute:N0} HP";
        return HasJobOverride(jobId) ? label + " (per-job)" : label;
    }

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);

    public void SaveDebounced()
    {
        if (EzThrottler.Throttle(PvpAutoLbConstants.ThrottleKeys.Save, PvpAutoLbConstants.SaveThrottleMs))
            Save();
    }
}
