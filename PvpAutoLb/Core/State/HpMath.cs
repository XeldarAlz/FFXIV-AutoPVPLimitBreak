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
    {
        var eff = EffectiveHp(t);
        var th = cfg.EffectiveThresholdFor(jobId);
        if (th.Mode == ThresholdMode.Absolute)
            return eff < th.Absolute;
        if (t.MaxHp == 0) return false;
        return 100f * eff / t.MaxHp < th.Percent;
    }
}
