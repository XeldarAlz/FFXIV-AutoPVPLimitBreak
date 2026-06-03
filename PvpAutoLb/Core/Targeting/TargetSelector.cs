using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;

namespace PvpAutoLb.Core;

internal static class TargetSelector
{
    public static IReadOnlyList<IBattleChara> ScanHostiles(float rangeYalms)
    {
        if (!Player.Available) return Array.Empty<IBattleChara>();
        var me = Player.Object!;
        var meId = me.GameObjectId;
        var mePos = me.Position;
        var rangeSq = rangeYalms * rangeYalms;

        var result = new List<IBattleChara>(16);
        foreach (var o in Svc.Objects)
        {
            if (o is not IBattleChara b) continue;
            if (b.GameObjectId == meId) continue;
            if (b.IsDead || !b.IsTargetable) continue;
            if (!b.IsHostile()) continue;

            var dx = b.Position.X - mePos.X;
            var dz = b.Position.Z - mePos.Z;
            if (dx * dx + dz * dz > rangeSq) continue;

            var i = result.Count;
            result.Add(b);
            while (i > 0 && result[i - 1].CurrentHp > b.CurrentHp)
            {
                result[i] = result[i - 1];
                i--;
            }
            result[i] = b;
        }
        return result;
    }
}
