using FFXIVClientStructs.FFXIV.Client.Game;

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

    public static bool IsReady(uint actionId, ulong targetId = PvpAutoLbConstants.NoTargetEntityId)
    {
        if (actionId == 0) return false;
        var am = ActionManager.Instance();
        return am != null && am->GetActionStatus(ActionType.Action, actionId, targetId) == 0;
    }
}
