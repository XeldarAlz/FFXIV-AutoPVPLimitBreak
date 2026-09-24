using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
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
        var result = new List<IBattleChara>(16);
        ScanHostiles(rangeYalms, result);
        return result;
    }

    // The window redraws every frame, so it scans into a list it keeps instead of a fresh one.
    public static void ScanHostiles(float rangeYalms, List<IBattleChara> result)
    {
        result.Clear();
        if (!Player.Available) return;
        var me = Player.Object!;
        var meId = me.GameObjectId;
        var mePos = me.Position;
        var rangeSq = rangeYalms * rangeYalms;

        foreach (var o in Svc.Objects)
        {
            if (o is not IBattleChara b) continue;
            if (b.GameObjectId == meId) continue;
            if (b.IsDead || !b.IsTargetable) continue;
            if (!b.IsHostile()) continue;
            if (!WithinRange(b, mePos, rangeSq)) continue;

            InsertByHp(result, b);
        }
    }

    public static IReadOnlyList<IBattleChara> ScanAllies(float rangeYalms, bool includeSelf)
    {
        if (!Player.Available) return Array.Empty<IBattleChara>();
        var result = new List<IBattleChara>(8);
        ScanAllies(rangeYalms, includeSelf, result);
        return result;
    }

    public static void ScanAllies(float rangeYalms, bool includeSelf, List<IBattleChara> result)
    {
        result.Clear();
        if (!Player.Available) return;
        var me = Player.Object!;
        var meId = me.GameObjectId;
        var mePos = me.Position;
        var rangeSq = rangeYalms * rangeYalms;

        if (includeSelf) InsertByHp(result, me);

        foreach (var o in Svc.Objects)
        {
            if (o is not IPlayerCharacter b) continue;
            if (b.GameObjectId == meId) continue;
            if (b.IsDead || !b.IsTargetable) continue;
            if (b.IsHostile()) continue;
            if (!WithinRange(b, mePos, rangeSq)) continue;

            InsertByHp(result, b);
        }
    }

    private static bool WithinRange(IBattleChara b, System.Numerics.Vector3 origin, float rangeSq)
    {
        var dx = b.Position.X - origin.X;
        var dz = b.Position.Z - origin.Z;
        return dx * dx + dz * dz <= rangeSq;
    }

    private static void InsertByHp(List<IBattleChara> list, IBattleChara b)
    {
        var i = list.Count;
        list.Add(b);
        while (i > 0 && list[i - 1].CurrentHp > b.CurrentHp)
        {
            list[i] = list[i - 1];
            i--;
        }
        list[i] = b;
    }
}
