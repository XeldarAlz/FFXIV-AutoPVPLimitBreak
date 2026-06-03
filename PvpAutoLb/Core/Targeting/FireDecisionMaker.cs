using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;

namespace PvpAutoLb.Core;

internal sealed record FireDecision(IBattleChara HardTarget, int EnemiesAffected);

internal static class FireDecisionMaker
{
    public static FireDecision? Decide(LbTargetingProfile profile, Configuration cfg, IReadOnlyList<IBattleChara> hostiles, HpTracker tracker)
    {
        if (!Player.Available || profile.ActionId == 0 || hostiles.Count == 0) return null;

        var jobId = Player.Object!.ClassJob.RowId;
        var below = FilterBelowThreshold(hostiles, cfg, jobId);
        if (below.Count == 0) return null;

        if (cfg.SkipDoomedTargets) below = FilterDoomed(below, tracker);
        if (below.Count == 0) return null;

        if (cfg.SkipGuardedTargets) below = FilterGuarded(below);
        if (below.Count == 0) return null;

        return profile.Shape switch
        {
            LbCastShape.CircleAroundCaster
                or LbCastShape.GroundCircle
                or LbCastShape.Donut
                => DecidePbAoe(profile, below),

            LbCastShape.CircleAroundTarget
                => DecideCircleAroundTarget(profile, cfg, below),

            _ => DecideSingleTarget(profile, cfg, below),
        };
    }

    private static FireDecision? DecideSingleTarget(LbTargetingProfile profile, Configuration cfg, IReadOnlyList<IBattleChara> below)
    {
        var range = EffectiveCastRange(profile, cfg);
        IBattleChara? pick = null;
        var bestHp = uint.MaxValue;
        for (var i = 0; i < below.Count; i++)
        {
            var c = below[i];
            if (Geo.DistanceToPlayer(c) > range) continue;
            var hp = HpMath.EffectiveHp(c);
            if (hp < bestHp) { pick = c; bestHp = hp; }
        }
        return pick == null ? null : new FireDecision(pick, 1);
    }

    private static FireDecision? DecidePbAoe(LbTargetingProfile profile, IReadOnlyList<IBattleChara> below)
    {
        var radius = EffectiveAoeRadius(profile);
        IBattleChara? pick = null;
        var bestHp = uint.MaxValue;
        var count = 0;
        for (var i = 0; i < below.Count; i++)
        {
            var c = below[i];
            if (Geo.DistanceToPlayer(c) > radius) continue;
            count++;
            var hp = HpMath.EffectiveHp(c);
            if (hp < bestHp) { pick = c; bestHp = hp; }
        }
        return pick == null ? null : new FireDecision(pick, count);
    }

    private static FireDecision? DecideCircleAroundTarget(LbTargetingProfile profile, Configuration cfg, IReadOnlyList<IBattleChara> below)
    {
        var castRange = EffectiveCastRange(profile, cfg);
        var aoeRadius = EffectiveAoeRadius(profile);

        IBattleChara? best = null;
        var bestScore = 0;
        var bestHp = uint.MaxValue;

        for (var i = 0; i < below.Count; i++)
        {
            var candidate = below[i];
            if (Geo.DistanceToPlayer(candidate) > castRange) continue;

            var score = 0;
            for (var j = 0; j < below.Count; j++)
            {
                if (Geo.Distance2D(below[j], candidate) <= aoeRadius) score++;
            }

            var effHp = HpMath.EffectiveHp(candidate);
            if (score > bestScore || (score == bestScore && effHp < bestHp))
            {
                best = candidate;
                bestScore = score;
                bestHp = effHp;
            }
        }

        return best == null ? null : new FireDecision(best, bestScore);
    }

    private static List<IBattleChara> FilterBelowThreshold(IReadOnlyList<IBattleChara> hostiles, Configuration cfg, uint jobId)
    {
        var result = new List<IBattleChara>(hostiles.Count);
        for (var i = 0; i < hostiles.Count; i++)
        {
            if (HpMath.IsBelowThreshold(hostiles[i], cfg, jobId)) result.Add(hostiles[i]);
        }
        return result;
    }

    private static List<IBattleChara> FilterDoomed(List<IBattleChara> below, HpTracker tracker)
    {
        var result = new List<IBattleChara>(below.Count);
        for (var i = 0; i < below.Count; i++)
        {
            var ttd = tracker.PredictTimeToDeath(below[i]);
            if (ttd is { } t && t.TotalMilliseconds < PvpAutoLbConstants.DoomedTtdMs) continue;
            result.Add(below[i]);
        }
        return result;
    }

    private static List<IBattleChara> FilterGuarded(List<IBattleChara> below)
    {
        var result = new List<IBattleChara>(below.Count);
        for (var i = 0; i < below.Count; i++)
        {
            if (StatusFilter.IsGuarded(below[i])) continue;
            result.Add(below[i]);
        }
        return result;
    }

    private static float EffectiveCastRange(LbTargetingProfile profile, Configuration cfg)
        => profile.Range > 0
            ? Math.Min(profile.Range, cfg.AutoSelectRangeYalms)
            : cfg.AutoSelectRangeYalms;

    private static float EffectiveAoeRadius(LbTargetingProfile profile)
        => profile.EffectRange > 0 ? profile.EffectRange : PvpAutoLbConstants.UnknownAoeFallbackYalms;
}
