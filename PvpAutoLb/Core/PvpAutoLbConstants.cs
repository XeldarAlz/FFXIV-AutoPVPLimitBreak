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

    public const float DefaultAllyHpPercent = 50f;
    public const int DefaultAllyCountNear = 2;
    public const float DefaultAllyRadiusYalms = 15f;
    public const int DefaultEnemyCountNear = 1;
    public const float DefaultEnemyRadiusYalms = 20f;

    public const uint LimitBreakCategoryId = 15;

    public const int PresetApiVersion = 1;

    public static class Ipc
    {
        public const string ApiVersion = "PvpAutoLb.Presets.ApiVersion";
        public const string GetVersion = "PvpAutoLb.Presets.GetVersion";
        public const string Apply = "PvpAutoLb.Presets.Apply";
    }

    public static class ThrottleKeys
    {
        public const string Tick = "PvpAutoLb.Tick";
        public const string Save = "PvpAutoLb.ConfigSave";
    }

    public static class StatusIds
    {
        // PvP Guard (action 29053): heavy damage reduction + CC immunity while up.
        public const uint Guard = 3054;

        // Paladin LB Phalanx (action 29069) grants Hallowed Ground to self: impervious to most attacks, 10s.
        public const uint HallowedGround = 1302;

        // Dark Knight LB Eventide grants Undead Redemption: HP cannot drop below 1, 10s.
        public const uint UndeadRedemption = 3039;
    }
}
