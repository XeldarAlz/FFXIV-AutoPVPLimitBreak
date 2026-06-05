using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;

namespace PvpAutoLb.Core;

internal sealed class AutoLbController : IDisposable
{
    private static readonly IReadOnlyList<IBattleChara> EmptyAllies = Array.Empty<IBattleChara>();

    private readonly Configuration cfg;
    private readonly TargetSwapper swapper = new();
    private readonly LbFirer firer;

    public HpTracker HpTracker { get; } = new();
    public SessionStats Stats { get; }

    public DateTime? LastFiredUtc => firer.LastFiredUtc;
    public IBattleChara? LastResolvedTarget { get; private set; }
    public LbTargetingProfile LastProfile { get; private set; } = LbTargetingProfile.None;
    public int LastEnemiesAffected { get; private set; }

    public AutoLbController(Configuration cfg)
    {
        this.cfg = cfg;
        Stats = new SessionStats(cfg);
        firer = new LbFirer(swapper);
        Svc.Framework.Update += OnTick;
        Svc.Log.Info(PvpAutoLbConstants.LogPrefix + " controller online — build " + typeof(AutoLbController).Assembly.GetName().Version);
    }

    public void Dispose()
    {
        Svc.Framework.Update -= OnTick;
    }

    private void OnTick(IFramework _)
    {
        try
        {
            Tick();
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, $"{PvpAutoLbConstants.LogPrefix} tick failed");
        }
    }

    private void Tick()
    {
        Stats.Tick();
        swapper.TryRestore();

        if (!cfg.Enabled) { ClearState(); return; }
        if (!Player.Available) { ClearState(); return; }
        if (!IsDutyAllowed()) { ClearState(); return; }
        if (!EzThrottler.Throttle(PvpAutoLbConstants.ThrottleKeys.Tick, PvpAutoLbConstants.TickThrottleMs)) return;

        var jobId = Player.Object!.ClassJob.RowId;
        var profile = LbCatalog.ResolveProfile(jobId);
        LastProfile = profile;
        if (profile.ActionId == 0) { ClearState(); return; }

        var rule = cfg.EffectiveRuleFor(jobId);

        if (rule.Mode == LbFireMode.Offensive)
        {
            var hostiles = TargetSelector.ScanHostiles(cfg.AutoSelectRangeYalms);
            for (var i = 0; i < hostiles.Count; i++) HpTracker.Sample(hostiles[i]);

            if (cfg.AutoSelectLowestHp)
                TickAutoSelect(jobId, profile, rule, hostiles);
            else
                TickManualTarget(jobId);
            return;
        }

        TickTeamState(jobId, profile, rule);
    }

    private void TickAutoSelect(uint jobId, LbTargetingProfile profile, LbRule rule, IReadOnlyList<IBattleChara> hostiles)
    {
        var decision = FireDecisionMaker.Decide(profile, rule, cfg, hostiles, EmptyAllies, HpTracker);
        LastResolvedTarget = decision?.HardTarget;
        LastEnemiesAffected = decision?.EnemiesAffected ?? 0;
        if (decision == null) return;
        if (BlocklistFilter.IsBlocked(decision.HardTarget, cfg.NameBlocklist)) return;
        Fire(jobId, decision.HardTarget, LastEnemiesAffected, offensive: true);
    }

    private void TickTeamState(uint jobId, LbTargetingProfile profile, LbRule rule)
    {
        var scanRange = Math.Max(cfg.AutoSelectRangeYalms, Math.Max(rule.EnemyRadiusYalms, rule.AllyRadiusYalms));
        var hostiles = TargetSelector.ScanHostiles(scanRange);
        var allies = TargetSelector.ScanAllies(scanRange, includeSelf: true);

        var decision = FireDecisionMaker.Decide(profile, rule, cfg, hostiles, allies, HpTracker);
        LastResolvedTarget = decision?.HardTarget;
        LastEnemiesAffected = 0;
        if (decision == null) return;

        Fire(jobId, decision.HardTarget, 0, offensive: false);
    }

    private void TickManualTarget(uint jobId)
    {
        if (Svc.Targets.Target is not IBattleChara manual || manual.IsDead)
        {
            LastResolvedTarget = null;
            LastEnemiesAffected = 0;
            return;
        }
        LastResolvedTarget = manual;
        if (!HpMath.IsBelowThreshold(manual, cfg, jobId))
        {
            LastEnemiesAffected = 0;
            return;
        }
        if (cfg.SkipGuardedTargets && StatusFilter.IsGuarded(manual)) { LastEnemiesAffected = 0; return; }
        if (BlocklistFilter.IsBlocked(manual, cfg.NameBlocklist)) { LastEnemiesAffected = 0; return; }
        LastEnemiesAffected = 1;
        Fire(jobId, manual, 1, offensive: true);
    }

    private void Fire(uint jobId, IBattleChara target, int enemiesAffected, bool offensive)
    {
        if (!firer.TryFire(jobId, target, out var actionId)) return;
        if (offensive) Stats.RecordFire(target, enemiesAffected);
        else Stats.RecordSupportFire();
        Feedback.OnFire(cfg, target, LbCatalog.GetActionName(actionId));
        Svc.Log.Info($"{PvpAutoLbConstants.LogPrefix} fired {actionId} on 0x{target.EntityId:X} (caught {enemiesAffected})");
    }

    private bool IsDutyAllowed()
    {
        var current = DutyDetector.Current();
        if (current == DutyMask.None) return true;
        return (cfg.EnabledDuties & current) != 0;
    }

    private void ClearState()
    {
        LastResolvedTarget = null;
        LastEnemiesAffected = 0;
    }
}
