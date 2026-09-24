using Dalamud.Game.ClientState.Objects.Types;

namespace PvpAutoLb.Core;

// Mirrors the checks the controller runs before firing on a target, in the same order, so the window can
// say why a below-threshold enemy is being passed over instead of only that it is.
internal static class TargetVerdict
{
    public static FireBlock Resolve(IBattleChara target, Configuration configuration, EffectiveThreshold threshold, LbTargetingProfile profile, HpTracker tracker, float distance)
    {
        if (!HpMath.IsBelowThreshold(target, threshold))
        {
            return FireBlock.AboveThreshold;
        }

        if (configuration.AutoSelectLowestHp && configuration.SkipDoomedTargets && IsDoomed(target, tracker))
        {
            return FireBlock.Doomed;
        }

        if (configuration.SkipGuardedTargets && StatusFilter.IsGuarded(target))
        {
            return FireBlock.Guarded;
        }

        if (configuration.SkipInvulnerableTargets && StatusFilter.IsImmuneToLb(target))
        {
            return FireBlock.Invulnerable;
        }

        if (BlocklistFilter.IsBlocked(target, configuration.NameBlocklist))
        {
            return FireBlock.Blocklisted;
        }

        return distance > ReachFor(profile, configuration) ? FireBlock.OutOfRange : FireBlock.None;
    }

    private static bool IsDoomed(IBattleChara target, HpTracker tracker)
        => tracker.PredictTimeToDeath(target) is { } timeToDeath && timeToDeath.TotalMilliseconds < PvpAutoLbConstants.DoomedTtdMs;

    private static float ReachFor(LbTargetingProfile profile, Configuration configuration)
    {
        if (profile.Shape is LbCastShape.CircleAroundCaster or LbCastShape.GroundCircle or LbCastShape.Donut)
        {
            return profile.EffectRange > 0 ? profile.EffectRange : PvpAutoLbConstants.UnknownAoeFallbackYalms;
        }

        if (!configuration.AutoSelectLowestHp)
        {
            return profile.Range > 0 ? profile.Range : float.MaxValue;
        }

        return profile.Range > 0 ? MathF.Min(profile.Range, configuration.AutoSelectRangeYalms) : configuration.AutoSelectRangeYalms;
    }
}
