using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;

namespace PvpAutoLb.Core;

internal sealed record FireDecision(IBattleChara HardTarget, int EnemiesAffected);

internal static class FireDecisionMaker
{
    public static FireDecision? Decide(LbTargetingProfile profile, LbRule rule, Configuration cfg,
        IReadOnlyList<IBattleChara> hostiles, IReadOnlyList<IBattleChara> allies, HpTracker tracker)
    {
        if (!Player.Available || profile.ActionId == 0) return null;

        return rule.Mode switch
        {
            LbFireMode.Defensive => AimSupport(DecideDefensive(rule, hostiles, allies), profile, cfg, hostiles, tracker),
            LbFireMode.Utility => AimSupport(DecideUtility(rule, hostiles, allies), profile, cfg, hostiles, tracker),
            _ => DecideOffensive(profile, cfg, hostiles, tracker),
        };
    }

    // A team-state rule says when to fire, but an action that only lands on an enemy (Encore of Light, Flare Star) cannot
    // be cast on the player, so it takes an enemy by its own reach with no HP threshold.
    private static FireDecision? AimSupport(FireDecision? decision, LbTargetingProfile profile, Configuration cfg,
        IReadOnlyList<IBattleChara> hostiles, HpTracker tracker)
    {
        if (decision == null || !profile.RequiresEnemyTarget)
        {
            return decision;
        }

        return PickTarget(profile, cfg, hostiles, tracker);
    }

    private static FireDecision? DecideDefensive(LbRule rule, IReadOnlyList<IBattleChara> hostiles, IReadOnlyList<IBattleChara> allies)
    {
        if (CountWithin(hostiles, rule.EnemyRadiusYalms) < rule.EnemyCountNear) return null;

        var hurt = 0;
        for (var i = 0; i < allies.Count; i++)
        {
            var a = allies[i];
            if (Geo.DistanceToPlayer(a) > rule.AllyRadiusYalms) continue;
            if (a.MaxHp == 0) continue;
            if (100f * HpMath.EffectiveHp(a) / a.MaxHp < rule.AllyHpPercent) hurt++;
        }
        if (hurt < rule.AllyCountNear) return null;

        return new FireDecision(Player.Object!, hurt);
    }

    private static FireDecision? DecideUtility(LbRule rule, IReadOnlyList<IBattleChara> hostiles, IReadOnlyList<IBattleChara> allies)
    {
        if (CountWithin(hostiles, rule.EnemyRadiusYalms) < rule.EnemyCountNear) return null;
        var alliesNear = CountWithin(allies, rule.AllyRadiusYalms);
        if (alliesNear < rule.AllyCountNear) return null;

        return new FireDecision(Player.Object!, alliesNear);
    }

    private static int CountWithin(IReadOnlyList<IBattleChara> list, float radius)
    {
        var n = 0;
        for (var i = 0; i < list.Count; i++)
            if (Geo.DistanceToPlayer(list[i]) <= radius) n++;
        return n;
    }

    private static FireDecision? DecideOffensive(LbTargetingProfile profile, Configuration cfg, IReadOnlyList<IBattleChara> hostiles, HpTracker tracker)
    {
        if (hostiles.Count == 0) return null;

        var jobId = Player.Object!.ClassJob.RowId;
        return PickTarget(profile, cfg, FilterBelowThreshold(hostiles, cfg, jobId), tracker);
    }

    private static FireDecision? PickTarget(LbTargetingProfile profile, Configuration cfg, IReadOnlyList<IBattleChara> candidates, HpTracker tracker)
    {
        if (candidates.Count == 0) return null;

        if (cfg.SkipDoomedTargets) candidates = FilterDoomed(candidates, tracker);
        if (candidates.Count == 0) return null;

        if (cfg.SkipGuardedTargets) candidates = FilterGuarded(candidates);
        if (candidates.Count == 0) return null;

        if (cfg.SkipInvulnerableTargets) candidates = FilterImmune(candidates);
        if (candidates.Count == 0) return null;

        if (profile.CentersOnCaster) return DecidePbAoe(profile, candidates);

        return profile.Shape switch
        {
            LbCastShape.CircleAroundTarget
                or LbCastShape.GroundCircle
                => DecideCircleAroundTarget(profile, cfg, candidates),

            _ => DecideSingleTarget(profile, cfg, candidates),
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

    private static List<IBattleChara> FilterDoomed(IReadOnlyList<IBattleChara> below, HpTracker tracker)
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

    private static List<IBattleChara> FilterGuarded(IReadOnlyList<IBattleChara> below)
    {
        var result = new List<IBattleChara>(below.Count);
        for (var i = 0; i < below.Count; i++)
        {
            if (StatusFilter.IsGuarded(below[i])) continue;
            result.Add(below[i]);
        }
        return result;
    }

    private static List<IBattleChara> FilterImmune(IReadOnlyList<IBattleChara> below)
    {
        var result = new List<IBattleChara>(below.Count);
        for (var i = 0; i < below.Count; i++)
        {
            if (StatusFilter.IsImmuneToLb(below[i])) continue;
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
