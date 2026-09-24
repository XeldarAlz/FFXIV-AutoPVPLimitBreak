using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using PvpAutoLb.Core;
using PvpAutoLb.Core.Localization;
using LuminaAction = Lumina.Excel.Sheets.Action;

namespace PvpAutoLb.Windows.Sections;

internal enum LiveKind : byte
{
    Offline,
    NoLimitBreak,
    NotInPvp,
    DutyOff,
    Support,
    NoEnemies,
    NoTarget,
    Waiting,
    Blocked,
    Charging,
    Firing,
}

// Everything the Live page, the mini player, the header and the combat HUD show is resolved here once per
// frame, so the windows share one scan of the object table instead of each walking it on their own.
internal sealed class LiveSnapshot
{
    private enum ChipKind : byte { Disarmed, Offline, NoLimitBreak, Standby, OffHere, Firing, Ready, Armed }

    private const float ReadyGauge = 0.999f;
    private const int NameCacheLimit = 128;

    private static readonly LiveSnapshot shared = new();

    private readonly List<IBattleChara> candidates = new(32);
    private readonly List<IBattleChara> allies = new(8);
    private readonly Dictionary<ulong, string> names = new();
    private readonly LbRule globalRule = new();
    private readonly CachedText statusText = new();
    private readonly CachedText chipText = new();
    private readonly CachedText gaugeText = new();
    private readonly CachedText thresholdText = new();
    private readonly CachedText ruleText = new();
    private readonly CachedText shapeText = new();
    private readonly CachedText supportRuleText = new();
    private readonly CachedText subtitleText = new();

    private int frame = -1;
    private uint territory;
    private uint jobNameFor = uint.MaxValue;
    private uint actionInfoFor = uint.MaxValue;

    public bool Armed { get; private set; }
    public LiveKind Kind { get; private set; }
    public uint JobId { get; private set; }
    public string JobName { get; private set; } = string.Empty;
    public uint ActionId { get; private set; }
    public string LimitBreakName { get; private set; } = string.Empty;
    public uint IconId { get; private set; }
    public LbTargetingProfile Profile { get; private set; } = LbTargetingProfile.None;
    public LbRule Rule { get; private set; } = new();
    public bool PerJob { get; private set; }
    public DutyMask Duty { get; private set; }
    public float Gauge { get; private set; }
    public bool LimitBreakReady { get; private set; }
    public IBattleChara? Target { get; private set; }
    public string TargetName { get; private set; } = string.Empty;
    public float TargetDistance { get; private set; }
    public float TargetHp { get; private set; }
    public float TargetShield { get; private set; }
    public float TargetThreshold { get; private set; }
    public FireBlock Block { get; private set; }
    public int SupportAllies { get; private set; }
    public int SupportEnemies { get; private set; }
    public bool SupportMet { get; private set; }
    public Vector4 Accent { get; private set; }
    public Vector4 AccentSoft { get; private set; }
    public FontAwesomeIcon Icon { get; private set; }

    public IReadOnlyList<IBattleChara> Candidates => candidates;

    public bool IsLive => Kind >= LiveKind.Support;

    public bool Firing => Armed && (Kind == LiveKind.Firing || (Kind == LiveKind.Support && SupportMet && LimitBreakReady));

    public float RingFill => LimitBreakReady ? 1f : Gauge;

    public string Status => statusText.Text;

    public string Chip => chipText.Text;

    public string GaugeLabel => gaugeText.Text;

    public string ThresholdLabel => thresholdText.Text;

    public string ShapeLabel => shapeText.Text;

    public string ActiveRule => Rule.Mode == LbFireMode.Offensive ? ruleText.Text : supportRuleText.Text;

    public string Subtitle => subtitleText.Text;

    public static LiveSnapshot Resolve(Plugin plugin)
    {
        var current = ImGui.GetFrameCount();
        if (shared.frame == current)
        {
            return shared;
        }

        shared.frame = current;
        shared.Refresh(plugin);
        return shared;
    }

    public string NameOf(IBattleChara character)
    {
        if (names.TryGetValue(character.GameObjectId, out var name))
        {
            return name;
        }

        if (names.Count >= NameCacheLimit)
        {
            names.Clear();
        }

        name = character.Name.TextValue;
        names[character.GameObjectId] = name;
        return name;
    }

    public bool IsBelowThreshold(IBattleChara character) => HpMath.IsBelowThreshold(character, Rule.EnemyThreshold());

    public float ThresholdFractionFor(IBattleChara character)
    {
        if (Rule.EnemyHpMode == ThresholdMode.Percent)
        {
            return Math.Clamp(Rule.EnemyHpPercent / 100f, 0f, 1f);
        }

        return character.MaxHp == 0 ? 0f : Math.Clamp((float)Rule.EnemyHpAbsolute / character.MaxHp, 0f, 1f);
    }

    private void Refresh(Plugin plugin)
    {
        var configuration = plugin.Configuration;
        ForgetStaleNames();
        ClearTarget();
        candidates.Clear();

        Armed = configuration.Enabled;
        JobId = Player.Available ? Player.Object!.ClassJob.RowId : 0u;
        RefreshJobName();
        RefreshActionInfo();
        Rule = ResolveRule(configuration);
        PerJob = configuration.HasJobRule(JobId);
        Duty = DutyDetector.Current();
        Gauge = ActionId == 0 || Duty == DutyMask.None ? 0f : LbGauge.Fraction();
        LimitBreakReady = ActionId != 0 && Gauge >= ReadyGauge;

        Kind = ResolveKind(plugin, configuration);
        RefreshTexts(configuration);
        RefreshPalette();
    }

    // Mirrors Configuration.EffectiveRuleFor without building a fresh global rule every frame.
    private LbRule ResolveRule(Configuration configuration)
    {
        if (JobId != 0 && configuration.PerJobRules.TryGetValue(JobId, out var rule))
        {
            return rule;
        }

        globalRule.Mode = LbFireMode.Offensive;
        globalRule.EnemyHpMode = configuration.ThresholdMode;
        globalRule.EnemyHpPercent = configuration.HpThresholdPercent;
        globalRule.EnemyHpAbsolute = configuration.HpThresholdAbsolute;
        return globalRule;
    }

    private LiveKind ResolveKind(Plugin plugin, Configuration configuration)
    {
        if (JobId == 0)
        {
            return LiveKind.Offline;
        }

        if (ActionId == 0)
        {
            return LiveKind.NoLimitBreak;
        }

        if (Duty == DutyMask.None)
        {
            return LiveKind.NotInPvp;
        }

        if ((configuration.EnabledDuties & Duty) == 0)
        {
            return LiveKind.DutyOff;
        }

        return Rule.Mode == LbFireMode.Offensive
            ? ResolveOffensive(plugin.Controller, configuration)
            : ResolveSupport(configuration);
    }

    private LiveKind ResolveOffensive(AutoLbController controller, Configuration configuration)
    {
        TargetSelector.ScanHostiles(configuration.AutoSelectRangeYalms, candidates);
        var target = PickTarget(controller, configuration);
        if (target is null)
        {
            return configuration.AutoSelectLowestHp ? LiveKind.NoEnemies : LiveKind.NoTarget;
        }

        Target = target;
        TargetName = NameOf(target);
        TargetDistance = Geo.DistanceToPlayer(target);
        TargetHp = target.MaxHp == 0 ? 0f : Math.Clamp((float)target.CurrentHp / target.MaxHp, 0f, 1f);
        TargetShield = Math.Clamp(target.ShieldPercentage / 100f, 0f, 1f - TargetHp);
        TargetThreshold = ThresholdFractionFor(target);
        Block = TargetVerdict.Resolve(target, configuration, Rule.EnemyThreshold(), Profile, controller.HpTracker, TargetDistance);
        LimitBreakReady |= ActionExec.IsReady(ActionId, target.EntityId);

        if (Block == FireBlock.AboveThreshold)
        {
            return LiveKind.Waiting;
        }

        if (Block != FireBlock.None)
        {
            return LiveKind.Blocked;
        }

        return LimitBreakReady ? LiveKind.Firing : LiveKind.Charging;
    }

    private IBattleChara? PickTarget(AutoLbController controller, Configuration configuration)
    {
        if (controller.LastResolvedTarget is { IsDead: false } resolved)
        {
            return resolved;
        }

        if (!configuration.AutoSelectLowestHp)
        {
            return Svc.Targets.Target is IBattleChara { IsDead: false } manual ? manual : null;
        }

        return candidates.Count > 0 ? candidates[0] : null;
    }

    private LiveKind ResolveSupport(Configuration configuration)
    {
        var scanRange = MathF.Max(configuration.AutoSelectRangeYalms, MathF.Max(Rule.EnemyRadiusYalms, Rule.AllyRadiusYalms));
        TargetSelector.ScanHostiles(scanRange, candidates);
        TargetSelector.ScanAllies(scanRange, true, allies);

        SupportEnemies = CountWithin(candidates, Rule.EnemyRadiusYalms);
        SupportAllies = Rule.Mode == LbFireMode.Defensive ? CountHurtAllies() : CountWithin(allies, Rule.AllyRadiusYalms);
        SupportMet = SupportEnemies >= Rule.EnemyCountNear && SupportAllies >= Rule.AllyCountNear;
        LimitBreakReady |= ActionExec.IsReady(ActionId, PvpAutoLbConstants.NoTargetEntityId);
        return LiveKind.Support;
    }

    private int CountHurtAllies()
    {
        var hurt = 0;
        for (var index = 0; index < allies.Count; index++)
        {
            var ally = allies[index];
            if (ally.MaxHp == 0 || Geo.DistanceToPlayer(ally) > Rule.AllyRadiusYalms)
            {
                continue;
            }

            if (100f * HpMath.EffectiveHp(ally) / ally.MaxHp < Rule.AllyHpPercent)
            {
                hurt++;
            }
        }

        return hurt;
    }

    private static int CountWithin(List<IBattleChara> characters, float radius)
    {
        var count = 0;
        for (var index = 0; index < characters.Count; index++)
        {
            if (Geo.DistanceToPlayer(characters[index]) <= radius)
            {
                count++;
            }
        }

        return count;
    }

    private void ClearTarget()
    {
        Target = null;
        TargetName = string.Empty;
        TargetDistance = 0f;
        TargetHp = 0f;
        TargetShield = 0f;
        TargetThreshold = 0f;
        Block = FireBlock.None;
        SupportAllies = 0;
        SupportEnemies = 0;
        SupportMet = false;
        allies.Clear();
    }

    private void ForgetStaleNames()
    {
        var current = Svc.ClientState.TerritoryType;
        if (current == territory)
        {
            return;
        }

        territory = current;
        names.Clear();
    }

    private void RefreshJobName()
    {
        if (jobNameFor == JobId)
        {
            return;
        }

        jobNameFor = JobId;
        JobName = JobId == 0 ? string.Empty : Formatting.Capitalize(JobLookup.Name(JobId));
    }

    private void RefreshActionInfo()
    {
        var actionIds = LbCatalog.ResolveActionIds(JobId);
        ActionId = actionIds.Count > 0 ? actionIds[0] : 0u;
        if (actionInfoFor == ActionId)
        {
            return;
        }

        actionInfoFor = ActionId;
        Profile = LbTargetingProfile.FromAction(ActionId);
        LimitBreakName = ActionId == 0 ? string.Empty : LbCatalog.GetActionName(ActionId);
        IconId = ActionId == 0 ? 0u : Svc.Data.GetExcelSheet<LuminaAction>().GetRowOrDefault(ActionId)?.Icon ?? 0u;
    }

    private void RefreshTexts(Configuration configuration)
    {
        RefreshThresholdTexts();
        RefreshShapeText();
        RefreshSubtitleText();
        RefreshSupportRuleText();
        RefreshChipText();
        RefreshGaugeText();
        RefreshStatusText(configuration);
    }

    private void RefreshThresholdTexts()
    {
        var key = HashCode.Combine(Rule.EnemyHpMode, (int)MathF.Round(Rule.EnemyHpPercent), Rule.EnemyHpAbsolute);
        if (thresholdText.Matches(key))
        {
            return;
        }

        var threshold = Rule.EnemyHpMode == ThresholdMode.Percent
            ? Loc.T(L.Live.ThresholdPercent, (int)MathF.Round(Rule.EnemyHpPercent))
            : Loc.T(L.Live.ThresholdAbsolute, Rule.EnemyHpAbsolute.ToString("N0", Loc.Culture));
        thresholdText.Store(key, threshold);
        ruleText.Store(key, Loc.T(L.Live.FiresBelow, threshold));
    }

    private void RefreshShapeText()
    {
        if (shapeText.Matches(ActionId))
        {
            return;
        }

        var effect = (int)MathF.Round(Profile.EffectRange);
        var shape = Profile.Shape switch
        {
            LbCastShape.SingleTarget => Loc.T(L.Live.ShapeSingle),
            LbCastShape.CircleAroundCaster => Loc.T(L.Live.ShapeAroundYou, effect),
            LbCastShape.CircleAroundTarget => Loc.T(L.Live.ShapeAroundTarget, effect),
            LbCastShape.GroundCircle => Loc.T(L.Live.ShapeGround, effect),
            LbCastShape.Cone => Loc.T(L.Live.ShapeCone, effect),
            LbCastShape.Line => Loc.T(L.Live.ShapeLine, effect),
            LbCastShape.Donut => Loc.T(L.Live.ShapeDonut, effect),
            LbCastShape.Cross => Loc.T(L.Live.ShapeCross, effect),
            _ => Loc.T(L.Live.ShapeUnknown),
        };

        var text = Profile.Range > 0
            ? string.Concat(Loc.T(L.Live.CastRange, (int)MathF.Round(Profile.Range)), TextDraw.Separator, shape)
            : shape;
        shapeText.Store(ActionId, text);
    }

    private void RefreshSubtitleText()
    {
        var key = HashCode.Combine(JobId, ActionId);
        if (subtitleText.Matches(key))
        {
            return;
        }

        var job = JobName.Length > 0 ? JobName : Loc.T(L.Live.NoJob);
        subtitleText.Store(key, ActionId == 0 ? job : string.Concat(job, TextDraw.Separator, ShapeLabel));
    }

    private void RefreshSupportRuleText()
    {
        var key = HashCode.Combine(Rule.Mode, Rule.AllyCountNear, (int)Rule.AllyHpPercent, (int)Rule.AllyRadiusYalms, Rule.EnemyCountNear, (int)Rule.EnemyRadiusYalms);
        if (supportRuleText.Matches(key))
        {
            return;
        }

        var text = Rule.Mode switch
        {
            LbFireMode.Defensive => Loc.T(L.Live.SupportRuleDefensive, Rule.AllyCountNear, (int)Rule.AllyHpPercent, (int)Rule.AllyRadiusYalms, Rule.EnemyCountNear, (int)Rule.EnemyRadiusYalms),
            LbFireMode.Utility => Loc.T(L.Live.SupportRuleUtility, Rule.AllyCountNear, (int)Rule.AllyRadiusYalms, Rule.EnemyCountNear, (int)Rule.EnemyRadiusYalms),
            _ => string.Empty,
        };
        supportRuleText.Store(key, text);
    }

    private void RefreshChipText()
    {
        var chip = ResolveChip();
        if (chipText.Matches((long)chip))
        {
            return;
        }

        var text = chip switch
        {
            ChipKind.Disarmed => Loc.T(L.Shell.StatusDisarmed),
            ChipKind.Offline => Loc.T(L.Shell.StatusOffline),
            ChipKind.NoLimitBreak => Loc.T(L.Shell.StatusNoLimitBreak),
            ChipKind.Standby => Loc.T(L.Shell.StatusStandby),
            ChipKind.OffHere => Loc.T(L.Shell.StatusOffHere),
            ChipKind.Firing => Loc.T(L.Shell.StatusFiring),
            ChipKind.Ready => Loc.T(L.Shell.StatusReady),
            _ => Loc.T(L.Shell.StatusArmed),
        };
        chipText.Store((long)chip, text);
    }

    private ChipKind ResolveChip()
    {
        if (!Armed)
        {
            return ChipKind.Disarmed;
        }

        return Kind switch
        {
            LiveKind.Offline => ChipKind.Offline,
            LiveKind.NoLimitBreak => ChipKind.NoLimitBreak,
            LiveKind.NotInPvp => ChipKind.Standby,
            LiveKind.DutyOff => ChipKind.OffHere,
            LiveKind.Firing => ChipKind.Firing,
            LiveKind.Support when Firing => ChipKind.Firing,
            _ => LimitBreakReady ? ChipKind.Ready : ChipKind.Armed,
        };
    }

    private void RefreshGaugeText()
    {
        var percent = LimitBreakReady ? 100 : (int)(Gauge * 100f);
        var key = HashCode.Combine(percent, Firing, ActionId == 0);
        if (gaugeText.Matches(key))
        {
            return;
        }

        var text = ActionId == 0 ? Loc.T(L.Live.GaugeNone)
            : Firing ? Loc.T(L.Live.GaugeFiring)
            : LimitBreakReady ? Loc.T(L.Live.GaugeReady)
            : Loc.T(L.Live.GaugeCharging, percent);
        gaugeText.Store(key, text);
    }

    private void RefreshStatusText(Configuration configuration)
    {
        var targetKey = Target is null ? 0UL : Target.GameObjectId;
        var key = HashCode.Combine(
            HashCode.Combine(Kind, Armed, Block, Duty, JobId),
            targetKey,
            (int)MathF.Round(TargetDistance),
            (int)MathF.Round(TargetHp * 100f),
            HashCode.Combine(SupportAllies, SupportEnemies, SupportMet, LimitBreakReady),
            (int)MathF.Round(configuration.AutoSelectRangeYalms),
            ThresholdLabel);
        if (statusText.Matches(key))
        {
            return;
        }

        statusText.Store(key, ComposeStatus(configuration));
    }

    private string ComposeStatus(Configuration configuration) => Kind switch
    {
        LiveKind.Offline => Loc.T(L.Live.StatusOffline),
        LiveKind.NoLimitBreak => Loc.T(L.Live.StatusNoLimitBreak, JobName),
        LiveKind.NotInPvp => Loc.T(L.Live.StatusNotInPvp),
        LiveKind.DutyOff => Loc.T(L.Live.StatusDutyOff, DutyName(Duty)),
        LiveKind.Support => ComposeSupportStatus(),
        LiveKind.NoEnemies => Loc.T(L.Live.StatusNoEnemies, (int)MathF.Round(configuration.AutoSelectRangeYalms)),
        LiveKind.NoTarget => Loc.T(L.Live.StatusNoTarget),
        LiveKind.Waiting => Loc.T(L.Live.StatusWaiting, TargetName, ThresholdLabel),
        LiveKind.Blocked => ComposeBlocked(),
        LiveKind.Charging => Loc.T(L.Live.StatusCharging, TargetName),
        _ => Armed ? Loc.T(L.Live.StatusFiring, TargetName) : Loc.T(L.Live.StatusWouldFire, TargetName),
    };

    private string ComposeBlocked() => Block switch
    {
        FireBlock.Doomed => Loc.T(L.Live.BlockDoomed, TargetName),
        FireBlock.Guarded => Loc.T(L.Live.BlockGuarded, TargetName),
        FireBlock.Invulnerable => Loc.T(L.Live.BlockInvulnerable, TargetName),
        FireBlock.Blocklisted => Loc.T(L.Live.BlockBlocklisted, TargetName),
        _ => Loc.T(L.Live.BlockOutOfRange, TargetName, (int)MathF.Round(TargetDistance)),
    };

    private string ComposeSupportStatus()
    {
        var progress = Rule.Mode == LbFireMode.Defensive
            ? Loc.T(L.Live.SupportProgressDefensive, SupportAllies, Rule.AllyCountNear, SupportEnemies, Rule.EnemyCountNear)
            : Loc.T(L.Live.SupportProgressUtility, SupportAllies, Rule.AllyCountNear, SupportEnemies, Rule.EnemyCountNear);
        if (!SupportMet)
        {
            return progress;
        }

        var verdict = !LimitBreakReady ? Loc.T(L.Live.SupportCharging)
            : Armed ? Loc.T(L.Live.SupportFiring)
            : Loc.T(L.Live.SupportWouldFire);
        return string.Concat(progress, TextDraw.Separator, verdict);
    }

    public static string DutyName(DutyMask duty) => duty switch
    {
        DutyMask.CrystallineConflict => Loc.T(L.Duty.CrystallineConflict),
        DutyMask.Frontline => Loc.T(L.Duty.Frontline),
        DutyMask.RivalWings => Loc.T(L.Duty.RivalWings),
        DutyMask.CustomMatch => Loc.T(L.Duty.CustomMatch),
        _ => Loc.T(L.Duty.Other),
    };

    private void RefreshPalette()
    {
        var (accent, soft, icon) = Kind switch
        {
            LiveKind.Offline => (Styling.TextDim, Styling.TextSecondary, FontAwesomeIcon.Hourglass),
            LiveKind.NoLimitBreak => (Styling.TextDim, Styling.TextSecondary, FontAwesomeIcon.Ban),
            LiveKind.NotInPvp => (Styling.TextDim, Styling.TextSecondary, FontAwesomeIcon.Satellite),
            LiveKind.DutyOff => (Styling.AccentAmber, Styling.AccentAmberSoft, FontAwesomeIcon.Filter),
            LiveKind.Support => SupportPalette(),
            LiveKind.NoEnemies or LiveKind.NoTarget => (Styling.AccentBlue, Styling.AccentBlueSoft, FontAwesomeIcon.Crosshairs),
            LiveKind.Waiting => (Styling.AccentAmber, Styling.AccentAmberSoft, FontAwesomeIcon.Hourglass),
            LiveKind.Blocked => (Styling.AccentRose, Styling.AccentRoseSoft, BlockIcon(Block)),
            LiveKind.Charging => (Styling.AccentAmber, Styling.AccentAmberSoft, FontAwesomeIcon.BatteryHalf),
            _ => Armed
                ? (Styling.AccentRed, Styling.AccentRedBright, FontAwesomeIcon.Bolt)
                : (Styling.AccentMint, Styling.AccentMintSoft, FontAwesomeIcon.Pause),
        };

        Accent = accent;
        AccentSoft = soft;
        Icon = icon;
    }

    private (Vector4 Accent, Vector4 Soft, FontAwesomeIcon Icon) SupportPalette()
    {
        if (!SupportMet)
        {
            return (Styling.AccentBlue, Styling.AccentBlueSoft, FontAwesomeIcon.UsersCog);
        }

        if (LimitBreakReady && Armed)
        {
            return (Styling.AccentRed, Styling.AccentRedBright, FontAwesomeIcon.Bolt);
        }

        return (Styling.AccentAmber, Styling.AccentAmberSoft, FontAwesomeIcon.UsersCog);
    }

    private static FontAwesomeIcon BlockIcon(FireBlock block) => block switch
    {
        FireBlock.Doomed => FontAwesomeIcon.SkullCrossbones,
        FireBlock.Guarded => FontAwesomeIcon.ShieldAlt,
        FireBlock.Invulnerable => FontAwesomeIcon.UserShield,
        FireBlock.Blocklisted => FontAwesomeIcon.UserSlash,
        _ => FontAwesomeIcon.ArrowsAltH,
    };
}
