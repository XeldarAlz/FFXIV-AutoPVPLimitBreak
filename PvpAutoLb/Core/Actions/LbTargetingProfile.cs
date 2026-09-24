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
    LbCastShape Shape)
{
    public static LbTargetingProfile None => new(0, 0, 0, LbCastShape.Unknown);

    public bool IsAoe => Shape is
        LbCastShape.CircleAroundCaster or
        LbCastShape.CircleAroundTarget or
        LbCastShape.Cone or
        LbCastShape.Line or
        LbCastShape.GroundCircle or
        LbCastShape.Donut or
        LbCastShape.Cross;

    public bool RequiresEnemyTarget => Shape is
        LbCastShape.SingleTarget or
        LbCastShape.CircleAroundTarget or
        LbCastShape.Cone or
        LbCastShape.Line or
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
        var shape = MapShape(row.Value.CastType);
        return new LbTargetingProfile(actionId, range, effect, shape);
    }

    private static LbCastShape MapShape(byte castType) => castType switch
    {
        1 => LbCastShape.SingleTarget,
        2 => LbCastShape.CircleAroundCaster,
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
