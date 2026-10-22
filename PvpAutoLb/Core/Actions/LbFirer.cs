using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;

namespace PvpAutoLb.Core;

internal sealed class LbFirer
{
    private readonly TargetSwapper swapper;

    private readonly Dictionary<uint, uint> pendingActionBySlot = new();

    public DateTime? LastFiredUtc { get; private set; }

    public LbFirer(TargetSwapper swapper)
    {
        this.swapper = swapper;
    }

    // Resubmit on every ready tick (do NOT throttle) so competing rotation plugins can't overwrite our queued LB.
    // A used limit break turns into its follow-up on the same button (Deathflare, Encore of Light, Flare Star); pending
    // is kept per slot because the button has already changed by the time the sent action is seen to have gone off.
    public bool TryFire(uint jobId, IBattleChara target, out uint firedActionId)
    {
        firedActionId = 0;
        var slotIds = LbCatalog.ResolveActionIds(jobId);
        if (slotIds.Count == 0)
        {
            return false;
        }

        var targetId = (ulong)target.EntityId;
        for (var slotIndex = 0; slotIndex < slotIds.Count; slotIndex++)
        {
            var slotId = slotIds[slotIndex];
            if (pendingActionBySlot.TryGetValue(slotId, out var pendingActionId) && !ActionExec.IsReady(pendingActionId, targetId))
            {
                pendingActionBySlot.Remove(slotId);
                LastFiredUtc = DateTime.UtcNow;
                firedActionId = pendingActionId;
                return true;
            }

            var actionId = ActionExec.ResolveAdjusted(slotId);
            if (IsResolvedByEarlierSlot(slotIds, slotIndex, actionId) || !ActionExec.IsReady(actionId, targetId))
            {
                continue;
            }

            swapper.Swap(target);
            // Summon Bahamut and Mesotes want a placement: on the target for an offensive rule, on the player for a
            // defensive or utility one, which is what the decision hands over as the hard target in either case.
            var used = LbCatalog.ProfileFor(actionId).TargetArea
                ? ActionExec.TryUseAt(actionId, targetId, target.Position)
                : ActionExec.TryUse(actionId, targetId);
            if (used)
            {
                pendingActionBySlot[slotId] = actionId;
            }
        }
        return false;
    }

    // A limit break can resolve to a follow-up the catalog also lists on its own; it is sent once so it counts once.
    private static bool IsResolvedByEarlierSlot(IReadOnlyList<uint> slotIds, int slotIndex, uint actionId)
    {
        for (var earlierIndex = 0; earlierIndex < slotIndex; earlierIndex++)
        {
            if (ActionExec.ResolveAdjusted(slotIds[earlierIndex]) == actionId)
            {
                return true;
            }
        }

        return false;
    }
}
