using System;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;

namespace PvpAutoLb.Core;

internal sealed class TargetSwapper
{
    private ulong? userOriginalTargetId;
    private ulong? lastOurSwapTargetId;
    private DateTime lastSwapAtUtc;

    public void Swap(IBattleChara target)
    {
        var current = Svc.Targets.Target;
        if (current?.EntityId == target.EntityId) return;

        if (userOriginalTargetId == null && current is IBattleChara prev)
            userOriginalTargetId = prev.EntityId;

        Svc.Targets.Target = target;
        lastOurSwapTargetId = target.EntityId;
        lastSwapAtUtc = DateTime.UtcNow;
    }

    public void TryRestore()
    {
        if (userOriginalTargetId == null) return;
        if ((DateTime.UtcNow - lastSwapAtUtc).TotalMilliseconds < PvpAutoLbConstants.TargetRestoreDelayMs) return;

        if (Svc.Targets.Target?.EntityId != lastOurSwapTargetId)
        {
            ClearRestoreState();
            return;
        }

        var original = Svc.Objects.SearchById(userOriginalTargetId.Value);
        if (original is IBattleChara b && !b.IsDead)
            Svc.Targets.Target = b;
        else
            Svc.Targets.Target = null;

        ClearRestoreState();
    }

    private void ClearRestoreState()
    {
        userOriginalTargetId = null;
        lastOurSwapTargetId = null;
    }
}
