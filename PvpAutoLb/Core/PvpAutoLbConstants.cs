namespace PvpAutoLb.Core;

internal static class PvpAutoLbConstants
{
    public const string LogPrefix = "[PvpAutoLb]";

    // FFXIV's "no target" sentinel for ActionManager.
    public const ulong NoTargetEntityId = 0xE000_0000UL;

    public const int TickThrottleMs = 33;
    public const int SaveThrottleMs = 250;

    // ms; below this predicted time-to-death the target dies before our cast lands.
    public const int DoomedTtdMs = 1200;

    public const int TargetRestoreDelayMs = 700;

    // Fallback AoE radius (yalms) when EffectRange is 0.
    public const float UnknownAoeFallbackYalms = 5f;

    public const float DefaultThresholdPercent = 30f;
    public const uint DefaultThresholdAbsolute = 7000;
    public const float DefaultAutoSelectRangeYalms = 30f;

    public const uint LimitBreakCategoryId = 15;

    public static class ThrottleKeys
    {
        public const string Tick = "PvpAutoLb.Tick";
        public const string Save = "PvpAutoLb.ConfigSave";
    }

    public static class StatusIds
    {
        // PvP Guard (from action 29053): 90% damage reduction, 5s.
        public const uint Guard = 1302;
    }
}
