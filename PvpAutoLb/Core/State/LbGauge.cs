using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace PvpAutoLb.Core;

internal static unsafe class LbGauge
{
    // PvP fills a single personal bar while PvE spreads the same units over several, so the fill is read
    // against every bar the controller reports rather than assuming one.
    public static float Fraction()
    {
        var controller = LimitBreakController.Instance();
        if (controller == null)
        {
            return 0f;
        }

        var capacity = (float)controller->BarUnits * Math.Max((byte)1, controller->BarCount);
        if (capacity <= 0f)
        {
            return 0f;
        }

        return Math.Clamp(controller->CurrentUnits / capacity, 0f, 1f);
    }
}
