using ECommons.DalamudServices;
using LuminaAction = Lumina.Excel.Sheets.Action;

namespace PvpAutoLb.Core;

internal enum LbCastShape
{
    Unknown,
    SingleTarget,
    CircleAroundCaster,
    Cone,
    Line,
    CircleAroundTarget,
    GroundCircle,
    Donut,
    Cross,
}

internal readonly record struct LbTargetingProfile(
    uint ActionId,
    float Range,
    float EffectRange,
    LbCastShape Shape,
    bool TargetArea,
    bool RequiresEnemyTarget)
{
    public static LbTargetingProfile None => new(0, 0, 0, LbCastShape.Unknown, false, false);

    // A ground circle the game asks a placement for (Summon Bahamut, Mesotes) lands where it is placed, up to Range
    // away; one without it is centred on the caster like any other point-blank AoE.
    public bool CentersOnCaster => Shape is LbCastShape.CircleAroundCaster or LbCastShape.Donut
        || (Shape == LbCastShape.GroundCircle && !TargetArea);

    public bool IsAoe => Shape is
        LbCastShape.CircleAroundCaster or
        LbCastShape.CircleAroundTarget or
        LbCastShape.Cone or
        LbCastShape.Line or
        LbCastShape.GroundCircle or
        LbCastShape.Donut or
        LbCastShape.Cross;

    public static LbTargetingProfile FromAction(uint actionId)
    {
        if (actionId == 0) return None;
        var sheet = Svc.Data.GetExcelSheet<LuminaAction>();
        var row = sheet?.GetRowOrDefault(actionId);
        if (row == null) return None;

        // -1 = self/no range → treat as 0.
        var range = row.Value.Range < 0 ? 0f : row.Value.Range;
        var effect = (float)row.Value.EffectRange;
        var shape = MapShape(row.Value.CastType, range);
        // Southern Cross lands on a party member as readily as on an enemy, so only an action that cannot be cast on
        // yourself needs an enemy to aim at.
        var requiresEnemyTarget = row.Value.CanTargetHostile && !row.Value.CanTargetSelf;
        return new LbTargetingProfile(actionId, range, effect, shape, row.Value.TargetArea, requiresEnemyTarget);
    }

    // Cast type 2 is a circle around whatever the action is cast on: the caster when it has no range (Contradance),
    // the enemy when it has one (Zantetsuken, Deathflare, Encore of Light).
    private static LbCastShape MapShape(byte castType, float range) => castType switch
    {
        1 => LbCastShape.SingleTarget,
        2 => range > 0 ? LbCastShape.CircleAroundTarget : LbCastShape.CircleAroundCaster,
        3 => LbCastShape.Cone,
        4 => LbCastShape.Line,
        5 => LbCastShape.CircleAroundTarget,
        7 or 8 => LbCastShape.GroundCircle,
        10 or 13 => LbCastShape.Cross,
        11 => LbCastShape.Donut,
        12 => LbCastShape.Cone,
        _ => LbCastShape.Unknown,
    };
}
