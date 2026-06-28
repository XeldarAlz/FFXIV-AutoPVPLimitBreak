using Dalamud.Game.ClientState.Objects.Types;

namespace PvpAutoLb.Core;

internal static class StatusFilter
{
    public static bool IsGuarded(IBattleChara target)
    {
        var list = target.StatusList;
        for (var i = 0; i < list.Length; i++)
        {
            if (list[i] is { StatusId: PvpAutoLbConstants.StatusIds.Guard }) return true;
        }
        return false;
    }

    public static bool IsImmuneToLb(IBattleChara target)
    {
        var list = target.StatusList;
        for (var i = 0; i < list.Length; i++)
        {
            if (list[i] is { StatusId: PvpAutoLbConstants.StatusIds.HallowedGround
                or PvpAutoLbConstants.StatusIds.UndeadRedemption }) return true;
        }
        return false;
    }
}
