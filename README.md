<p align="center">
  <img src="PvpAutoLb/Images/Icon.png" width="180" alt="Auto PVP LB icon" />
</p>

<h1 align="center">Auto PVP LB</h1>

<p align="center">
  <a href="https://discord.gg/hppkAvdBEE"><img alt="Discord" src="https://img.shields.io/badge/Discord-join-5865F2?style=flat-square&logo=discord&logoColor=white"></a>
  <a href="https://github.com/XeldarAlz/FFXIV-AutoPVPLimitBreak/releases/latest"><img alt="Release" src="https://img.shields.io/github/v/release/XeldarAlz/FFXIV-AutoPVPLimitBreak?style=flat-square&color=blue"></a>
  <a href="https://github.com/XeldarAlz/FFXIV-AutoPVPLimitBreak/releases"><img alt="Downloads" src="https://img.shields.io/github/downloads/XeldarAlz/FFXIV-AutoPVPLimitBreak/total?style=flat-square&color=blue"></a>
  <a href="https://github.com/XeldarAlz/FFXIV-AutoPVPLimitBreak/actions/workflows/release.yml"><img alt="Build" src="https://img.shields.io/github/actions/workflow/status/XeldarAlz/FFXIV-AutoPVPLimitBreak/release.yml?style=flat-square"></a>
  <a href="LICENSE.md"><img alt="License" src="https://img.shields.io/badge/license-AGPL--3.0--or--later-blue?style=flat-square"></a>
</p>

<p align="center">
  <em>Your PvP Limit Break, fired for you. Built on Dalamud.</em>
</p>

---

<p align="center">
  <img src="PvpAutoLb/Images/demo.gif" alt="Auto PVP LB demo" />
</p>

## What it does

Monitors enemy HP during PvP. When the target's HP drops below your configured threshold, fires your job's PvP Limit Break. Great for burst jobs like Ninja or Machinist to lock in kills.

Jobs whose PvP LB is defensive or support-focused (e.g. Paladin's Phalanx) are flagged on the Live page and never auto-fired.

## Features

- **Configurable threshold**: percent of max HP or absolute HP, with per-job overrides.
- **Range- and shape-aware targeting**: single-target LBs respect cast range; circle-around-target LBs prefer clustered enemies; PBAoE LBs fire when any below-threshold enemy is in radius.
- **Shield-aware HP**: threshold checks against effective HP (`CurrentHp + ShieldHp`) so shields don't trigger early fires.
- **Skip doomed targets**: predicts time-to-death and skips enemies that will die before the LB lands.
- **Player blocklist + duty filter**: named players are never targeted; per-mode checkboxes (CC / Frontline / Rival Wings / Custom Match / Other) scope auto-fire.
- **Auto-target**: picks the lowest-effective-HP hostile in range; falls back to manual hard target when off.
- **Live page**: your job's LB with an animated readiness ring for the gauge, the effective rule, the current target with an HP bar, shield overlay and threshold marker, and a plain-language state line (waiting, firing, or why a target is skipped: doomed, Guard, immune, blocklisted, out of range, duty switched off). Nearby enemies are listed the way the selector ranks them.
- **Combat HUD**: a small borderless overlay with the LB gauge, your target's HP against the threshold and the current state. Shows itself in PvP duties, can be locked so clicks pass through, and toggles with `/pvpautolb hud`.
- **Session + lifetime stats**: fires, attributed kills, total enemies hit. Lifetime persists across reloads.
- **Console, changelog and nine languages**: every log line in a filterable console with Copy log for bug reports, a changelog page, and the interface in English, French, German, Turkish, Spanish, Portuguese, Russian, Japanese and Chinese.
- **Optional feedback**: chat sound (`/se1`–`/se16`) and/or chat line on fire.

## Install

In-game: `/xlsettings` → **Experimental** → paste into **Custom Plugin Repositories**:

```
https://raw.githubusercontent.com/XeldarAlz/DalamudPlugins/main/repo.json
```

Tick **Enabled**, click **+**, then **Save and Close**. Open `/xlplugins` → **All Plugins**, search for **Auto PVP LB**, and install.

## Commands

| Command | Action |
|---|---|
| `/pvpautolb` | Toggle the main window |
| `/palb` | Alias for `/pvpautolb` (every subcommand works with both) |
| `/pvpautolb config` or `settings` | Open settings |
| `/pvpautolb log` or `console` | Open the console (every line the plugin logs, with Copy log for bug reports) |
| `/pvpautolb changelog` | Open the changelog (what changed in each update, newest first) |
| `/pvpautolb about` | Open the About page |
| `/pvpautolb hud` | Show or hide the combat HUD until your next zone change |

## Configuration

Open via `/pvpautolb config` or the sliders icon in the main window's left rail. Settings are grouped into Essentials, Tuning, Safety and feedback, and Interface.

- **Threshold**: mode (percent / absolute) and value. Below this, the LB fires.
- **Per-job override**: each job can have its own threshold mode and value.
- **Targeting**: auto-select toggle and scan radius (5–50 yalms). When off, only your hard target is considered.
- **Filters**: skip doomed targets, allowed duty types (CC / Frontline / Rival Wings / Custom / Other).
- **Player blocklist**: names listed here are never auto-targeted.
- **Notifications**: optional chat sound and/or chat line on fire.
- **Combat HUD**: show it automatically in PvP duties, lock it in place (click-through), or show and hide it right now.
- **General**: interface language and whether the main window opens on login.

## Job compatibility

Limit Breaks are resolved from game data, so every job is wired up automatically. The table tracks what has been verified in live PvP matches. If you test a job, please open an issue or PR.

**Legend:** ✅ confirmed · ❔ untested · 🛡 defensive/support: not auto-fired

| Tanks | Status | | Healers | Status |
|---|---|---|---|---|
| Paladin | 🛡 Phalanx | | White Mage | ✅ |
| Warrior | 🛡 Primal Scream | | Scholar | 🛡 Seraphism |
| Dark Knight | ✅ | | Astrologian | 🛡 Celestial River |
| Gunbreaker | ✅ | | Sage | 🛡 Mesotes |

| Melee DPS | Status | | Ranged DPS | Status |
|---|---|---|---|---|
| Monk | ✅ | | Bard | 🛡 Final Fantasia |
| Dragoon | ✅ | | Machinist | ✅ |
| Ninja | ✅ | | Dancer | 🛡 Contradance |
| Samurai | ✅ | | Black Mage | 🛡 Soul Resonance |
| Reaper | 🛡 Tenebrae Lemurum | | Summoner | 🛡 Phoenix & Bahamut |
| Viper | ✅ | | Red Mage | ✅ Southern Cross |
|  |  | | Pictomancer | 🛡 Advent of Chocobastion |

## Community

Questions, ideas, or just want to hang out with other players? Come say hi on Discord.

→ [Join our Discord](https://discord.gg/hppkAvdBEE)

## More from me

If you liked this plugin, take a look at my other Dalamud work. You might find something else there for you.

→ [XeldarAlz Dalamud Plugins](https://github.com/XeldarAlz/DalamudPlugins)

## License

AGPL-3.0-or-later. See [LICENSE.md](LICENSE.md).
