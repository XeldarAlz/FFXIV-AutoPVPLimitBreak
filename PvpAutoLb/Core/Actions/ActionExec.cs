using FFXIVClientStructs.FFXIV.Client.Game;
using System.Numerics;

namespace PvpAutoLb.Core;

internal static unsafe class ActionExec
{
    public static bool TryUse(uint actionId, ulong targetId = PvpAutoLbConstants.NoTargetEntityId)
    {
        if (actionId == 0) return false;
        var am = ActionManager.Instance();
        if (am == null) return false;
        return am->UseAction(ActionType.Action, actionId, targetId);
    }

    // A ground-targeted action fired through UseAction only opens the placement cursor; the location variant lands it.
    public static bool TryUseAt(uint actionId, ulong targetId, Vector3 location)
    {
        if (actionId == 0) return false;
        var am = ActionManager.Instance();
        if (am == null) return false;
        return am->UseActionLocation(ActionType.Action, actionId, targetId, &location);
    }

    public static uint ResolveAdjusted(uint actionId)
    {
        if (actionId == 0)
        {
            return 0;
        }

        var actionManager = ActionManager.Instance();
        return actionManager == null ? actionId : actionManager->GetAdjustedActionId(actionId);
    }

    public static bool IsReady(uint actionId, ulong targetId = PvpAutoLbConstants.NoTargetEntityId)
    {
        if (actionId == 0) return false;
        var am = ActionManager.Instance();
        return am != null && am->GetActionStatus(ActionType.Action, actionId, targetId) == 0;
    }
}
