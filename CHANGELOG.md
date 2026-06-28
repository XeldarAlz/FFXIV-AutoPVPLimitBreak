# Changelog

All notable changes to Auto PVP LB are documented here.

## v1.0.6.0

New filter: the LB no longer auto-fires on targets made invulnerable by a tank Limit Break. Also corrects the Guard status ID, which never matched the real PvP Guard status.

### Added
- **Skip targets immune to your LB.** Paladin's Phalanx LB grants the caster **Hallowed Ground** (status `1302`) and Dark Knight's Eventide LB grants **Undead Redemption** (status `3039`) — both make the target immune to damage (or unkillable) for 10s, so firing the LB into them wastes the charge. The picker now skips these targets in both the auto-select path (`FireDecisionMaker` → `FilterImmune`) and the manual-target path (`AutoLbController.TickManualTarget`) via `StatusFilter.IsImmuneToLb`. New `Skip targets immune to your LB` toggle in **Settings → Filters**, default on. Resolves #21.

### Fixed
- **Guard skip now matches the real Guard status.** `StatusIds.Guard` was `1302`, which is actually PvP **Hallowed Ground**, not Guard — so "Skip targets using Guard" never skipped Guarded targets and instead happened to skip Paladin-LB targets. Corrected to `3054` (the PvP Guard status, cross-checked against XIVAPI, Garland Tools, and RotationSolver/WrathCombo). With the new invulnerability filter above now covering Hallowed Ground (`1302`), Paladin-LB targets stay skipped, Dark-Knight-LB targets are skipped too, and the Guard toggle finally does what its label says.

## v1.0.5.2

Display-name change only. No behavior changes.

### Changed
- **Renamed the plugin's display name from "PVP Auto LB" to "Auto PVP LB"** in the Dalamud installer listing, the three in-game window titles, and command help. The `InternalName` (`PvpAutoLb`), slash commands (`/pvpautolb`, `/palb`), and window ImGui IDs are unchanged, so saved configs and window layouts carry over.

## v1.0.5.0

Behavior fix: the LB now wins the action-queue race against Rotation Solver and other plugins that submit actions every tick.

### Fixed
- **Removed the 250ms submission throttle in `LbFirer`.** v1.0.2.0 lowered the throttle from 500 ms → 250 ms and the changelog claimed we now "submit every tick", but the throttle was still in place — we submitted at ~4 Hz while Rotation Solver submits at ~30 Hz, so RS overwrote our queued LB roughly 7 out of every 8 times the user's anim-lock cleared. We now submit on every ready tick and confirm a fire by observing the action's `IsReady` flip false on a subsequent tick (the action landed and went on cooldown). The state machine tracks each action's pending status in a small `Dictionary<uint, bool>`. Net effect: when racing with RS, we become the last writer at 30 Hz instead of every-250ms, so the LB lands the moment our threshold is crossed.
- **Stats and feedback now count one fire per actual fire.** Because submissions can be overwritten by competing plugins or fizzle when the target dies in the queue window, counting submissions as fires (previous behavior, gated only by the 250 ms throttle) could over- or under-count. The new confirmation gate means `Stats.RecordFire`, the optional chat/sound feedback, and the `[PvpAutoLb] fired …` log line all fire exactly once per LB that actually landed.

### Removed
- `PvpAutoLbConstants.FireThrottleMs` and `ThrottleKeys.FirePrefix` — both unused after the rewrite.

## v1.0.4.0

Behavior fix: the LB no longer auto-fires on targets using PvP Guard.

### Fixed
- **Skip Guarded targets.** When an enemy is using the PvP Guard action (`#29053`, status `1302`), their incoming damage is reduced by 90% for 5 seconds. The plugin previously treated them as a normal below-threshold pick and burned the LB on them for ~10% effect. We now scan the target's `StatusList` for the Guard status and skip them in both the auto-select path (`FireDecisionMaker.Decide` → `FilterGuarded`) and the manual-target path (`AutoLbController.TickManualTarget`). New `Skip targets using Guard` toggle in **Settings → Filters**, default on.

## v1.0.3.0

Internal refactor + ecosystem-aligned tooling. No behavior changes — every feature, threshold, hotkey, and on-screen state is identical to v1.0.2.0.

### Refactor
- **AutoLbController split into a coordinator + collaborators.** The controller went from 218 lines doing 5 concerns to 126 lines of pure orchestration. Action submission and the fire-key throttle cache moved to `Core/Actions/LbFirer.cs`. The hard-target swap/restore state machine moved to `Core/Targeting/TargetSwapper.cs`. The name-blocklist scan moved to `Core/Filters/BlocklistFilter.cs`.
- **`LbClassifier` extracted from `LbCatalog`.** The hardcoded support-job set and `Classify(jobId)` now live in `Core/Actions/LbClassifier.cs`; `LbCatalog` is action-id resolution + profile caching only.
- **`Core/` reorganized into responsibility folders.** 16 flat files → `Actions/`, `Targeting/`, `State/`, `Duty/`, `Filters/` (with `AutoLbController`, `PvpAutoLbConstants`, `JobLookup`, `Feedback` at the top). Folder names answer "where does X live?" at a glance; namespace stays flat (`PvpAutoLb.Core`) to avoid `using`-statement churn across peer classes.
- **`LbCatalog` lazy init via `Lazy<T>`.** Replaced the nullable `actionsByJobId` + `EnsureLoaded` pattern with `static readonly Lazy<Dictionary<uint, uint[]>>`. Thread-safe by library guarantee. Diagnostic logging folded into `BuildIndex` itself.
- **Duplication removed across the UI layer.** Layout literals (148f, 168f, 112f…) centralized in `Windows/Layout.cs`. Threshold mode toggle + value control extracted to `Windows/Components/ThresholdWidgets.cs` (used by both global and per-job sections). `LbCard.ResolveBorder` extracted to `Windows/Components/CardBorders.cs`. Pulse-period magic numbers (600/800/900 ms) replaced with `Styling.PulseFast/PulseMedium/PulseDefault`. Throttle key strings centralized in `PvpAutoLbConstants.ThrottleKeys`.
- **Configuration accessor cleanup.** `EffectiveMode(jobId)` / `EffectivePercent(jobId)` / `EffectiveAbsolute(jobId)` bundled into a single `EffectiveThresholdFor(jobId)` returning a `readonly record struct EffectiveThreshold`. Three call sites simplified.
- **Dead code removed.** Unused `HpMath.EffectiveHpPercent`. Unused `LbCatalog.ResolveActionId` thin wrapper. `Plugin.WindowSystem` and `Plugin.Configuration` tightened from `public` to `internal`.
- **Naming consistency.** `cfg` parameter/field naming standardized everywhere (one `Configuration config` outlier in `AutoLbController` and `ConfigWindow` renamed). `actionsByJob` → `actionsByJobId`. Fully-qualified `Dalamud.Game.ClientState.Objects.Types.IBattleChara` in `LbDrawState` replaced with a `using`.

### Tooling
- **`Microsoft.CodeAnalysis.BannedApiAnalyzers` added** (3.3.4) with a curated `BannedSymbols.txt`. Banned-API uses (RS0030) fail the build via `<WarningsAsErrors>RS0030</WarningsAsErrors>`. The list blocks threading APIs that leak into the game process (`Thread` ctor, `Task.Run`, `TaskFactory.StartNew`), framework-blocking calls (`Thread.Sleep`, `Task.Wait`), `Console.WriteLine`/`Write` (no console attached), and the `IBattleChara` cast-state properties known to be unreliable in PvP (carried over from ECommons' curated list). Aligned with the Dalamud ecosystem convention used by BossMod, ECommons, and RotationSolverReborn.
- **`.editorconfig` augmented.** File-scoped namespace enforcement (matches existing codebase), trailing-whitespace trim at the `[*]` level, markdown carve-out, 2-space indent for YAML/JSON.

## v1.0.2.0

Behavior fix: the LB now wins the action-queue race against other rotation plugins, so it fires the instant HP drops below the threshold instead of waiting one or more GCD cycles.

### Fixed
- **Engine-level action queueing.** `ActionExec.TryUse` previously short-circuited on `AnimationLock > 0f` and a redundant `GetActionStatus` check, so we never submitted to `ActionManager.UseAction` during the queue window. That let competing rotation plugins (e.g. Rotation Solver) land in the queue slot first; our LB only fired on the rare tick where both anim-lock and GCD were already clear. We now submit every tick HP is below threshold and let the engine queue the LB — last writer wins the slot, and at 30 Hz that is us. Multi-phase LB selection (Dragoon High Jump → Sky Shatter, etc.) is unchanged: `IsReady` still picks the correct action ID before submission.
- **Faster recovery from fizzled queues.** `FireThrottleMs` lowered from 500 ms → 250 ms so a queued LB that fizzles at flush (target died or moved out of range during the queue window) retargets on the next tick instead of after half a second.

## v1.0.1.0

Compatibility release for Patch 7.50 / Dalamud API 15. No behavior changes.

### Changed
- **Dalamud API 15.** Bumped `Dalamud.NET.Sdk` to `15.0.0` and `DalamudApiLevel` to 15. ECommons submodule advanced to NightmareXIV/ECommons master at the API15 update commit.

### Fixed
- **Card child-region disposal.** `ImRaii.Child(...)` in API 15 now returns a `ChildDisposable` ref struct instead of `IDisposable`, which broke `Card.Begin`. Replaced the heap-allocated `CompositeDisposable` in `Card.cs` with a `CardScope` ref struct that holds the child handle directly and disposes it before the surrounding style/color pushes (preserving the original LIFO order so ImGui state unwinds correctly).

## v1.0.0.0

First stable release. Builds on v0.1.0.0 with five user-facing features and a full architecture refactor.

### Added
- **Skip doomed targets.** When auto-select is on, the picker estimates each enemy's HP-per-second loss over a 1-second window and skips targets whose predicted time-to-death is shorter than the LB's animation lock (~1.2s). Configurable in **Settings → Filters**.
- **Sound + chat on fire.** Optional. Sound plays one of `/se1`–`/se16`; chat prints `[PvpAutoLb] fired <action> on <target>`. Both off by default. Configurable in **Settings → Feedback**.
- **Player blocklist + duty filter.** Names listed in **Settings → Player blocklist** are never auto-targeted. Allowed-duties checkboxes in **Settings → Filters** scope auto-fire to Crystalline Conflict, Frontline, Rival Wings, Custom Match, or other PvP zones.
- **Session + lifetime stats.** Footer of the main window shows current-session and lifetime totals (fires, attributed kills, total enemies hit). Lifetime persists across reloads. Each line has its own Reset button.
- **Granular readiness pill.** Replaced the single `UNAVAILABLE` state with `OUT OF RANGE` (target out of LB range) and `GAUGE LOW` (LB gauge insufficient).

### Changed
- **About window.** Separate window with author, GitHub repo, issues, discussions, and security advisory links. Open via the info-circle button in the main window's toolbar.
- **UI overhaul.** Compact LB card with icon + name + status pill on the top row and threshold/profile descriptor below. Plugin icons removed from window title bars (the OS-level title already shows the plugin name). Target section is hidden when the plugin is disabled. Master enable/disable lives only on the main window.
- **Save debouncing.** Slider drags no longer hit disk every frame.

### Fixed
- **Hard-target restoration.** When the auto-picker swapped your hard target to fire on a different enemy, your original target was lost. The plugin now snapshots the previous target and restores it ~700 ms after the swap (long enough for the LB animation to land). If you've manually moved off our pick during the window, the restore is skipped — your manual choice wins.

### Architecture
- 35 source files, none over ~120 lines. `MainWindow` and `ConfigWindow` are pure composition (~50 / ~40 lines).
- `Core/` exposes a single `LbCatalog` (replaces the old `IJobLimitBreakModule` registry indirection) plus small focused helpers: `Geo`, `JobLookup`, `HpMath`, `HpTracker`, `SessionStats`, `Feedback`, `DutyDetector`.
- `Windows/Components/` and `Windows/Sections/` separate reusable visual pieces from page composition.
- All magic numbers consolidated in `Core/PvpAutoLbConstants.cs`.
- Hot-path allocations eliminated: per-action throttle keys are cached, `TargetSelector` uses a manual loop with insertion-sort instead of LINQ chains, `FireDecisionMaker` filters with index loops.
- `OnTick` wraps in `try/catch` + logs so a tick-time exception cannot silently unsubscribe the framework callback.

## v0.1.0.0

Renamed from **LastHit** to **PVP Auto LB**. The plugin's `InternalName` changed (`LastHitPlugin` → `PvpAutoLb`), so existing users will need to install fresh — old saved configs do not migrate. Slash commands, window titles, and log prefixes were all updated.

### Added
- **Range-aware targeting.** The picker now reads each LB's `Range`, `EffectRange`, and cast shape from the game's Action sheet:
  - Single-target LBs only consider enemies within the LB's actual cast range, not just the user's scan radius. A 25y LB no longer wastes ticks trying to fire on a 50y target.
  - Circle-around-caster / ground-circle / donut LBs fire when any below-threshold enemy is in the AoE radius.
  - Circle-around-target LBs (e.g. Sky Shatter) pick the candidate whose AoE catches the most below-threshold enemies, breaking ties on lowest effective HP. Cluster matters now.
  - Cone / line / cross LBs fall back to single-target picking — geometric facing isn't modeled, but the LB still fires and catches what the player is facing.
- **Per-job HP thresholds.** Each PvP job can now have its own threshold mode + value, overriding the global setting. Configure under the **Per-job override** card in settings; the active job's override is keyed off whatever you're currently logged into. Falls back to the global threshold when no override exists.
- **Shield-aware HP.** Threshold checks now compare against effective HP (`CurrentHp + ShieldHp`), so the LB doesn't fire prematurely on a target whose shield will eat the damage. The hero card's HP bar shows the shield as a gold strip stacked past the green segment.
- **Status window LB card** now shows the LB's range and shape (e.g. `25y · single-target` or `PBAoE 5y · catches 3`) under the threshold line. When firing on an AoE LB, it reports the count of enemies caught.

### Changed
- `Configuration.Version` bumped from 1 to 2 to track the per-job-thresholds field. Existing v1 configs deserialize with `PerJobThresholds` defaulting to empty; no manual migration required.
- The hero card now reflects the controller's actual fire pick (relevant for AoE LBs that prefer cluster size over lowest HP).

## v0.0.0.5

### Added
- **Defensive/support LB detection.** Jobs whose PvP Limit Break is defensive or support-focused (Paladin, Warrior, Bard, Black Mage, Astrologian, Dancer, Reaper, Sage, Pictomancer) are now recognized as out of scope. The Limit Break card surfaces an amber `DEFENSIVE` pill with a "Support LB — not auto-fired" note instead of an HP-threshold label, so players don't expect the plugin to fire on those jobs.
- README job tables now include a 🛡 status for support LBs alongside ✅ / ❔.

## v0.0.0.4

### Fixed
- **PvP target detection.** Enemy players in Crystalline Conflict, Frontlines, and Rival Wings were not showing up in the Target section. The hostile filter relied on `StatusFlags.Hostile`, which is driven by PvE aggro state and is not reliably set on opposing-team players in PvP. Replaced with ECommons' nameplate-color `IsHostile()` check, which correctly identifies PvP enemy players as well as hostile BattleNpcs (mechs, NPCs, summons in Frontlines / Rival Wings).

## v0.0.0.3
- Release packaging fixes; download-count workflow; job compatibility notes in README.

## v0.0.0.2
- UI polish: empty-card text clipping at small window sizes.

## v0.0.0.1
- Initial release.
