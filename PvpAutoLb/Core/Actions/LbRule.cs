using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PvpAutoLb.Core;

public enum LbFireMode
{
    Offensive,
    Defensive,
    Utility,
}

public enum RuleSource
{
    Preset,
    User,
}

public sealed class LbRule
{
    [JsonConverter(typeof(StringEnumConverter))]
    public LbFireMode Mode { get; set; } = LbFireMode.Offensive;

    [JsonConverter(typeof(StringEnumConverter))]
    public RuleSource Source { get; set; } = RuleSource.Preset;

    [JsonConverter(typeof(StringEnumConverter))]
    public ThresholdMode EnemyHpMode { get; set; } = ThresholdMode.Percent;
    public float EnemyHpPercent { get; set; } = PvpAutoLbConstants.DefaultThresholdPercent;
    public uint EnemyHpAbsolute { get; set; } = PvpAutoLbConstants.DefaultThresholdAbsolute;
    public int MinEnemiesInAoe { get; set; } = 1;

    public float AllyHpPercent { get; set; } = PvpAutoLbConstants.DefaultAllyHpPercent;
    public int AllyCountNear { get; set; } = PvpAutoLbConstants.DefaultAllyCountNear;
    public float AllyRadiusYalms { get; set; } = PvpAutoLbConstants.DefaultAllyRadiusYalms;
    public int EnemyCountNear { get; set; } = PvpAutoLbConstants.DefaultEnemyCountNear;
    public float EnemyRadiusYalms { get; set; } = PvpAutoLbConstants.DefaultEnemyRadiusYalms;

    public EffectiveThreshold EnemyThreshold() => new(EnemyHpMode, EnemyHpPercent, EnemyHpAbsolute);

    public LbRule Clone() => (LbRule)MemberwiseClone();

    public static LbRule OffensiveDefault(ThresholdMode mode, float percent, uint absolute) => new()
    {
        Mode = LbFireMode.Offensive,
        EnemyHpMode = mode,
        EnemyHpPercent = percent,
        EnemyHpAbsolute = absolute,
    };
}
