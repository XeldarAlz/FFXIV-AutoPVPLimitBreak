using Dalamud.Game.ClientState.Objects.Types;

namespace PvpAutoLb.Core;

internal static class HpMath
{
    // ShieldPercentage is 0–100 (% of MaxHp).
    public static uint ShieldHp(IBattleChara t)
        => (uint)((ulong)t.MaxHp * t.ShieldPercentage / 100UL);

    public static uint EffectiveHp(IBattleChara t)
        => t.CurrentHp + ShieldHp(t);

    public static bool IsBelowThreshold(IBattleChara t, Configuration cfg, uint jobId)
        => IsBelowThreshold(t, cfg.EffectiveThresholdFor(jobId));

    public static bool IsBelowThreshold(IBattleChara target, EffectiveThreshold threshold)
    {
        var effective = EffectiveHp(target);
        if (threshold.Mode == ThresholdMode.Absolute)
        {
            return effective < threshold.Absolute;
        }

        if (target.MaxHp == 0)
        {
            return false;
        }

        return 100f * effective / target.MaxHp < threshold.Percent;
    }
}
