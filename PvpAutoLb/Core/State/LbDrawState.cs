using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;

namespace PvpAutoLb.Core;

internal enum LbReadyReason
{
    Ready,
    GaugeLow,
    OutOfRange,
}

internal readonly record struct LbDrawState(
    uint JobId,
    uint ActionId,
    LbFireMode Mode,
    LbReadyReason Readiness,
    LbTargetingProfile Profile)
{
    public bool IsSupport => Mode != LbFireMode.Offensive;
    public bool ActionReady => Readiness == LbReadyReason.Ready;
    public bool CanFire => ActionId != 0 && ActionReady;

    public string ModeLabel => Mode switch
    {
        LbFireMode.Defensive => "DEFENSIVE",
        LbFireMode.Utility => "UTILITY",
        _ => "OFFENSIVE",
    };

    public string ModeBlurb => Mode switch
    {
        LbFireMode.Defensive => "Defensive LB — fires when your team is pressured",
        LbFireMode.Utility => "Utility LB — fires in a teamfight",
        _ => string.Empty,
    };

    public static LbDrawState Resolve(AutoLbController ctrl, Configuration cfg)
    {
        var jobId = Player.Available ? Player.Object!.ClassJob.RowId : 0u;
        var ids = LbCatalog.ResolveActionIds(jobId);
        var actionId = ids.Count > 0 ? ids[0] : 0u;
        if (actionId == 0)
            return new LbDrawState(jobId, 0, LbFireMode.Offensive, LbReadyReason.GaugeLow, LbTargetingProfile.None);

        var mode = cfg.EffectiveRuleFor(jobId).Mode;
        var profile = ctrl.LastProfile.ActionId == actionId
            ? ctrl.LastProfile
            : LbTargetingProfile.FromAction(actionId);

        var targetEntity = ctrl.LastResolvedTarget?.EntityId ?? PvpAutoLbConstants.NoTargetEntityId;
        var ready = ActionExec.IsReady(actionId, targetEntity);
        var readiness = ready ? LbReadyReason.Ready : InferReason(ctrl.LastResolvedTarget, profile);

        return new LbDrawState(jobId, actionId, mode, readiness, profile);
    }

    private static LbReadyReason InferReason(IBattleChara? target, LbTargetingProfile profile)
    {
        if (target != null && profile.Range > 0 && Geo.DistanceToPlayer(target) > profile.Range)
            return LbReadyReason.OutOfRange;
        return LbReadyReason.GaugeLow;
    }
}
