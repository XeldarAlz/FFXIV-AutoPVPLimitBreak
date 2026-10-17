using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;

namespace PvpAutoLb.Core;

internal sealed class LbFirer
{
    private readonly TargetSwapper swapper;

    private readonly Dictionary<uint, bool> pendingByAction = new();

    public DateTime? LastFiredUtc { get; private set; }

    public LbFirer(TargetSwapper swapper)
    {
        this.swapper = swapper;
    }

    // Resubmit on every ready tick (do NOT throttle) so competing rotation plugins can't overwrite our queued LB.
    public bool TryFire(uint jobId, IBattleChara target, out uint firedActionId)
    {
        firedActionId = 0;
        var actionIds = LbCatalog.ResolveActionIds(jobId);
        if (actionIds.Count == 0) return false;

        var targetId = (ulong)target.EntityId;
        foreach (var actionId in actionIds)
        {
            var ready = ActionExec.IsReady(actionId, targetId);
            var wasPending = pendingByAction.TryGetValue(actionId, out var p) && p;

            if (wasPending && !ready)
            {
                pendingByAction[actionId] = false;
                LastFiredUtc = DateTime.UtcNow;
                firedActionId = actionId;
                return true;
            }

            if (!ready) continue;

            swapper.Swap(target);
            // Summon Bahamut and Mesotes want a placement: on the target for an offensive rule, on the player for a
            // defensive or utility one, which is what the decision hands over as the hard target in either case.
            var used = LbCatalog.ProfileFor(actionId).TargetArea
                ? ActionExec.TryUseAt(actionId, targetId, target.Position)
                : ActionExec.TryUse(actionId, targetId);
            if (used)
                pendingByAction[actionId] = true;
        }
        return false;
    }
}
