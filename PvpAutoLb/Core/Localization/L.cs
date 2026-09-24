namespace PvpAutoLb.Core.Localization;

internal static class L
{
    internal static class Common
    {
        public static readonly LocString Close = new("common.close", "Close");
        public static readonly LocString Remove = new("common.remove", "Remove");
        public static readonly LocString Add = new("common.add", "Add");
        public static readonly LocString DragAdjustHint = new("common.dragAdjustHint", "Drag to adjust · Ctrl+click to type");
        public static readonly LocString Test = new("common.test", "Test");
        public static readonly LocString NoMatches = new("common.noMatches", "Nothing matches “{0}”.");
    }

    internal static class Shell
    {
        public static readonly LocString NavLive = new("shell.nav.live", "Live");
        public static readonly LocString NavSettings = new("shell.nav.settings", "Settings");
        public static readonly LocString NavLog = new("shell.nav.log", "Console");
        public static readonly LocString NavChangelog = new("shell.nav.changelog", "Changelog");
        public static readonly LocString NavAbout = new("shell.nav.about", "About");
        public static readonly LocString StatusArmed = new("shell.status.armed", "Armed");
        public static readonly LocString StatusReady = new("shell.status.ready", "Ready");
        public static readonly LocString StatusFiring = new("shell.status.firing", "Firing");
        public static readonly LocString StatusDisarmed = new("shell.status.disarmed", "Disarmed");
        public static readonly LocString StatusStandby = new("shell.status.standby", "Standby");
        public static readonly LocString StatusOffHere = new("shell.status.offHere", "Off here");
        public static readonly LocString StatusNoLimitBreak = new("shell.status.noLimitBreak", "No LB");
        public static readonly LocString StatusOffline = new("shell.status.offline", "Offline");
        public static readonly LocString ChipArm = new("shell.chipArm", "Click to arm auto LB");
        public static readonly LocString ChipDisarm = new("shell.chipDisarm", "Click to disarm auto LB");
        public static readonly LocString Minimize = new("shell.minimize", "Minimize to the title strip");
        public static readonly LocString Restore = new("shell.restore", "Restore the window");
        public static readonly LocString ShowHud = new("shell.showHud", "Show the combat HUD");
        public static readonly LocString HideHud = new("shell.hideHud", "Hide the combat HUD");
    }

    internal static class Live
    {
        public static readonly LocString Enable = new("live.enable", "Enable auto LB");
        public static readonly LocString EnableSub = new("live.enableSub", "Nothing fires until you turn it on");
        public static readonly LocString Disable = new("live.disable", "Disable auto LB");
        public static readonly LocString ChipDisarmed = new("live.chipDisarmed", "Auto LB off");

        public static readonly LocString GaugeReady = new("live.gauge.ready", "Ready");
        public static readonly LocString GaugeFiring = new("live.gauge.firing", "Firing");
        public static readonly LocString GaugeCharging = new("live.gauge.charging", "Charging {0}%");
        public static readonly LocString GaugeNone = new("live.gauge.none", "No Limit Break");

        public static readonly LocString NoLimitBreakName = new("live.noLimitBreakName", "No Limit Break");
        public static readonly LocString NoJob = new("live.noJob", "No job");
        public static readonly LocString ThresholdPercent = new("live.thresholdPercent", "{0}% HP");
        public static readonly LocString ThresholdAbsolute = new("live.thresholdAbsolute", "{0} HP");
        public static readonly LocString FiresBelow = new("live.firesBelow", "Fires below {0}");
        public static readonly LocString BadgeGlobal = new("live.badge.global", "Global");
        public static readonly LocString BadgePerJob = new("live.badge.perJob", "Per-job");
        public static readonly LocString BadgePreset = new("live.badge.preset", "Preset");
        public static readonly LocString SupportRuleDefensive = new("live.supportRule.defensive", "Fires when {0} allies within {2}y drop below {1}% HP and {3} enemies are within {4}y");
        public static readonly LocString SupportRuleUtility = new("live.supportRule.utility", "Fires when {0} allies within {1}y and {2} enemies within {3}y are grouped up");

        public static readonly LocString CastRange = new("live.castRange", "{0}y range");
        public static readonly LocString ShapeSingle = new("live.shape.single", "Single target");
        public static readonly LocString ShapeAroundYou = new("live.shape.aroundYou", "{0}y circle around you");
        public static readonly LocString ShapeAroundTarget = new("live.shape.aroundTarget", "{0}y circle around the target");
        public static readonly LocString ShapeGround = new("live.shape.ground", "{0}y ground circle");
        public static readonly LocString ShapeCone = new("live.shape.cone", "{0}y cone");
        public static readonly LocString ShapeLine = new("live.shape.line", "{0}y line");
        public static readonly LocString ShapeDonut = new("live.shape.donut", "{0}y donut");
        public static readonly LocString ShapeCross = new("live.shape.cross", "{0}y cross");
        public static readonly LocString ShapeUnknown = new("live.shape.unknown", "Unknown shape");

        public static readonly LocString TargetTitle = new("live.target.title", "Target");
        public static readonly LocString TargetingAuto = new("live.target.auto", "Auto-select, lowest HP within {0}y");
        public static readonly LocString TargetingManual = new("live.target.manual", "Manual, your own target");
        public static readonly LocString Hp = new("live.target.hp", "{0} / {1} HP");
        public static readonly LocString HpWithShield = new("live.target.hpShield", "{0} (+{1} shield) / {2} HP");
        public static readonly LocString Percent = new("live.percent", "{0}%");
        public static readonly LocString Yalms = new("live.yalms", "{0}y");

        public static readonly LocString StatusOffline = new("live.status.offline", "Log in to a job to see the live view.");
        public static readonly LocString StatusNoLimitBreak = new("live.status.noLimitBreak", "{0} has no PvP Limit Break. Switch to a combat job.");
        public static readonly LocString StatusNotInPvp = new("live.status.notInPvp", "Auto LB watches enemies once you enter a PvP duty.");
        public static readonly LocString StatusDutyOff = new("live.status.dutyOff", "Auto LB is switched off for {0}. Turn it on in Settings, Filters.");
        public static readonly LocString StatusNoEnemies = new("live.status.noEnemies", "No enemies within {0}y yet. Scanning.");
        public static readonly LocString StatusNoTarget = new("live.status.noTarget", "Auto-select is off, so target an enemy yourself.");
        public static readonly LocString StatusWaiting = new("live.status.waiting", "Waiting for {0} to drop below {1}");
        public static readonly LocString StatusCharging = new("live.status.charging", "{0} is below the threshold. Waiting for the LB gauge.");
        public static readonly LocString StatusFiring = new("live.status.firing", "Firing on {0}");
        public static readonly LocString StatusWouldFire = new("live.status.wouldFire", "Would fire on {0}, but auto LB is off");
        public static readonly LocString BlockDoomed = new("live.block.doomed", "Skipping {0}: they will die before the LB lands");
        public static readonly LocString BlockGuarded = new("live.block.guarded", "Skipping {0}: Guard is up");
        public static readonly LocString BlockInvulnerable = new("live.block.invulnerable", "Skipping {0}: immune to damage");
        public static readonly LocString BlockBlocklisted = new("live.block.blocklisted", "Skipping {0}: on your blocklist");
        public static readonly LocString BlockOutOfRange = new("live.block.outOfRange", "{0} is out of range at {1}y");
        public static readonly LocString SupportProgressDefensive = new("live.support.progressDefensive", "{0} of {1} hurt allies, {2} of {3} enemies nearby");
        public static readonly LocString SupportProgressUtility = new("live.support.progressUtility", "{0} of {1} allies, {2} of {3} enemies nearby");
        public static readonly LocString SupportCharging = new("live.support.charging", "waiting for the LB gauge");
        public static readonly LocString SupportFiring = new("live.support.firing", "firing");
        public static readonly LocString SupportWouldFire = new("live.support.wouldFire", "would fire, but auto LB is off");

        public static readonly LocString EmptyOffline = new("live.empty.offline", "Waiting for your character");
        public static readonly LocString EmptyNoLimitBreak = new("live.empty.noLimitBreak", "No PvP Limit Break");
        public static readonly LocString EmptyNotInPvp = new("live.empty.notInPvp", "Standing by");
        public static readonly LocString EmptyDutyOff = new("live.empty.dutyOff", "Off in this duty");
        public static readonly LocString EmptySupportDefensive = new("live.empty.supportDefensive", "Defensive Limit Break");
        public static readonly LocString EmptySupportUtility = new("live.empty.supportUtility", "Utility Limit Break");
        public static readonly LocString EmptyNoEnemies = new("live.empty.noEnemies", "No enemies in range");
        public static readonly LocString EmptyNoTarget = new("live.empty.noTarget", "No target");

        public static readonly LocString CandidatesTitle = new("live.candidates.title", "Nearby enemies");
        public static readonly LocPlural EnemiesInRange = new("live.candidates.inRange", "{0} in range, lowest HP first", "{0} in range, lowest HP first");
        public static readonly LocPlural MoreEnemies = new("live.candidates.more", "{0} more enemy further down the list", "{0} more enemies further down the list");
        public static readonly LocString CandidatesEmpty = new("live.candidates.empty", "No enemies in range right now.");
        public static readonly LocString CandidatesIdle = new("live.candidates.idle", "Enemies in range show up here during a PvP duty.");

        public static readonly LocString SessionTitle = new("live.stats.session", "Session");
        public static readonly LocString SessionFor = new("live.stats.sessionFor", "Running for {0}");
        public static readonly LocString LifetimeTitle = new("live.stats.lifetime", "Lifetime");
        public static readonly LocString StatFires = new("live.stats.fires", "Fires");
        public static readonly LocString StatKills = new("live.stats.kills", "Kills");
        public static readonly LocString StatEnemiesHit = new("live.stats.enemiesHit", "Enemies hit");
        public static readonly LocString ResetSession = new("live.stats.resetSession", "Reset session stats");
        public static readonly LocString ResetLifetime = new("live.stats.resetLifetime", "Reset lifetime stats");
        public static readonly LocString ConfirmReset = new("live.stats.confirmReset", "Click again to reset");
        public static readonly LocString NeverFired = new("live.stats.neverFired", "never fired");
        public static readonly LocString AgoSeconds = new("live.ago.seconds", "{0}s ago");
        public static readonly LocString AgoMinutes = new("live.ago.minutes", "{0}m ago");
        public static readonly LocString AgoHours = new("live.ago.hours", "{0}h ago");
        public static readonly LocString DurationHours = new("live.duration.hours", "{0}h {1:D2}m");
        public static readonly LocString DurationMinutes = new("live.duration.minutes", "{0}m {1:D2}s");
    }

    internal static class Duty
    {
        public static readonly LocString CrystallineConflict = new("duty.crystallineConflict", "Crystalline Conflict");
        public static readonly LocString Frontline = new("duty.frontline", "Frontline");
        public static readonly LocString RivalWings = new("duty.rivalWings", "Rival Wings");
        public static readonly LocString CustomMatch = new("duty.customMatch", "Custom Match");
        public static readonly LocString Other = new("duty.other", "Other PvP");
    }

    internal static class Hud
    {
        public static readonly LocString LockTooltip = new("hud.lockTooltip", "Lock the HUD in place. Locked, it lets clicks pass through to the game. Unlock it in Settings, Combat HUD.");
    }

    internal static class Feedback
    {
        public static readonly LocString ChatFired = new("feedback.chatFired", "fired {0} on {1}");
    }

    internal static class Log
    {
        public static readonly LocString Title = new("log.title", "Console");
        public static readonly LocPlural Entries = new("log.entries", "{0} line in the buffer", "{0} lines in the buffer");
        public static readonly LocString Empty = new("log.empty", "Nothing logged yet. Enter a PvP duty and every step the plugin takes shows up here.");
        public static readonly LocString NoMatches = new("log.noMatches", "No lines match the current filters.");
        public static readonly LocString Footer = new("log.footer", "Every line also goes to the Dalamud log (/xllog) with the {0} prefix. When reporting a bug, press Copy log and paste the result into the issue.");
        public static readonly LocString SearchHint = new("log.searchHint", "Search messages and sources");
        public static readonly LocString CopyAll = new("log.copyAll", "Copy log");
        public static readonly LocPlural CopyFiltered = new("log.copyFiltered", "Copy {0} line", "Copy {0} lines");
        public static readonly LocString CopyTooltip = new("log.copyTooltip", "Copies the lines shown below with a header naming the plugin version, Dalamud version and zone, ready to paste into a bug report.");
        public static readonly LocString Copied = new("log.copied", "Copied");
        public static readonly LocPlural CopiedLines = new("log.copiedLines", "Copied {0} line to the clipboard", "Copied {0} lines to the clipboard");
        public static readonly LocString Clear = new("log.clear", "Clear");
        public static readonly LocString ConfirmClear = new("log.confirmClear", "Click again to clear");
        public static readonly LocString Close = new("log.close", "Close");
        public static readonly LocString LevelVerbose = new("log.level.verbose", "Verbose");
        public static readonly LocString LevelDebug = new("log.level.debug", "Debug");
        public static readonly LocString LevelInfo = new("log.level.info", "Info");
        public static readonly LocString LevelWarning = new("log.level.warning", "Warnings");
        public static readonly LocString LevelError = new("log.level.error", "Errors");
        public static readonly LocString LevelTooltip = new("log.levelTooltip", "Click to show or hide these lines. Shift-click to show only this level.");
        public static readonly LocString SourceChip = new("log.sourceChip", "Source: {0}");
        public static readonly LocString SourceChipTooltip = new("log.sourceChipTooltip", "Click to stop filtering by source.");
        public static readonly LocString JumpLatest = new("log.jumpLatest", "Jump to latest");
        public static readonly LocPlural NewLines = new("log.newLines", "{0} new line", "{0} new lines");
        public static readonly LocString Showing = new("log.showing", "Showing {0} of {1}");
        public static readonly LocString ResetFilters = new("log.resetFilters", "Reset filters");
        public static readonly LocString Shortcuts = new("log.shortcuts", "Ctrl+F search · Ctrl+C copy · Shift-click selects a range · Double-click copies a line");
        public static readonly LocPlural Selected = new("log.selected", "{0} line selected", "{0} lines selected");
        public static readonly LocString CopySelection = new("log.copySelection", "Copy selection");
        public static readonly LocString ClearSelection = new("log.clearSelection", "Clear selection");
        public static readonly LocString CopyLine = new("log.copyLine", "Copy line");
        public static readonly LocString CopyToEnd = new("log.copyToEnd", "Copy from here to the end");
        public static readonly LocString OnlySource = new("log.onlySource", "Show only {0}");
        public static readonly LocString Repeated = new("log.repeated", "Repeated {0} times in a row");
        public static readonly LocString HasDetails = new("log.hasDetails", "Has a stack trace. Select the line to read it.");
    }

    internal static class Changelog
    {
        public static readonly LocString Title = new("changelog.title", "What's new");
        public static readonly LocString Subtitle = new("changelog.subtitle", "Every update, newest first.");
        public static readonly LocString Version = new("changelog.version", "Version {0}");
        public static readonly LocString Latest = new("changelog.latest", "Latest");
        public static readonly LocString New = new("changelog.new", "New");
        public static readonly LocPlural Changes = new("changelog.changes", "{0} change", "{0} changes");

        public static readonly LocString[] Release1200 =
        [
            new("changelog.r1200.1", "Added the Console page to view, filter and copy the plugin's logs"),
            new("changelog.r1200.2", "Added this changelog, with a badge whenever an update brings something new"),
            new("changelog.r1200.3", "Overhauled the About page, with the updated Discord link"),
            new("changelog.r1200.4", "Rebuilt the whole interface: a new main window, a combat HUD for matches, and support for 9 languages"),
        ];
    }

    internal static class About
    {
        public static readonly LocString SupportTitle = new("about.support.title", "Made with love and care");
        public static readonly LocString SupportBody = new("about.support.body", "This plugin is a one-person project, built in my free time because I love this game and its community. Keeping it updated takes a lot of those hours. If it has helped you, supporting me on Patreon means I can keep giving it that time. Thank you for being here.");
        public static readonly LocString SupportButton = new("about.support.button", "Support on Patreon");
        public static readonly LocString PatreonHint = new("about.support.hint", "Open Patreon · right-click to copy");
        public static readonly LocString LinkHint = new("about.linkHint", "Click to open · right-click to copy");
        public static readonly LocString MadeBy = new("about.madeBy", "Made by {0}");
        public static readonly LocString Version = new("about.version", "v {0}");
        public static readonly LocString Community = new("about.community", "Community");
        public static readonly LocString DiscordTitle = new("about.discordTitle", "Join the Discord");
        public static readonly LocString DiscordBody = new("about.discordBody", "Get help, report bugs, share ideas and hear about updates first.");
        public static readonly LocString GitHubTitle = new("about.githubTitle", "View on GitHub");
        public static readonly LocString GitHubBody = new("about.githubBody", "Browse the source code and every release.");
        public static readonly LocString ReminderTitle = new("about.reminder.title", "A little reminder");
        public static readonly LocString FactsTitle = new("about.facts.title", "Did you know?");
        public static readonly LocString QuotesTitle = new("about.quotes.title", "Words to live by");
        public static readonly LocString JokesTitle = new("about.jokes.title", "Just for fun");

        public static readonly LocString[] Reminders =
        [
            new("about.reminder.1", "Been at it a while? Roll your shoulders and take one slow breath."),
            new("about.reminder.2", "Hydration check. When did you last drink some water?"),
            new("about.reminder.3", "Blink a few times and let your eyes rest for a moment."),
            new("about.reminder.4", "Stand up, stretch, and shake out your hands. Future you says thanks."),
            new("about.reminder.5", "Sit up and settle in comfortably. Your back will thank you later."),
            new("about.reminder.6", "Remember to eat something today. You matter more than any score."),
            new("about.reminder.7", "Eyes feel tired? Look at something far away for twenty seconds."),
            new("about.reminder.8", "Whatever you're chasing, you're allowed to take a break whenever."),
            new("about.reminder.9", "You're doing great. Be a little kinder to yourself today."),
            new("about.reminder.10", "A glass of water and a quick stretch can reset a long session."),
            new("about.reminder.11", "Unclench your jaw and drop your shoulders. There you go."),
            new("about.reminder.12", "Rest is part of the journey too. Step away whenever you need to."),
        ];

        public static readonly LocString[] Facts =
        [
            new("about.facts.1", "Honey never spoils. Jars over 3,000 years old have been found still edible."),
            new("about.facts.2", "Octopuses have three hearts and blue blood."),
            new("about.facts.3", "A day on Venus is longer than a whole year on Venus."),
            new("about.facts.4", "Bananas are berries, but strawberries aren't."),
            new("about.facts.5", "There are more possible chess games than atoms in the observable universe."),
            new("about.facts.6", "Sharks have been around longer than trees have."),
            new("about.facts.7", "A group of flamingos is called a flamboyance."),
            new("about.facts.8", "Honeybees can recognize individual human faces."),
            new("about.facts.9", "Wombat droppings are cube shaped."),
            new("about.facts.10", "The Eiffel Tower can grow over 15 cm taller on a hot day."),
            new("about.facts.11", "Hot water can sometimes freeze faster than cold water."),
            new("about.facts.12", "A bolt of lightning is roughly five times hotter than the surface of the Sun."),
        ];

        public static readonly LocString[] Quotes =
        [
            new("about.quotes.1", "Done is better than perfect. You can always polish later."),
            new("about.quotes.2", "Small steps every day add up to surprising distances."),
            new("about.quotes.3", "Comparison is the thief of joy. Run your own race."),
            new("about.quotes.4", "Progress, not perfection."),
            new("about.quotes.5", "You don't have to be great to start, but you have to start to be great."),
            new("about.quotes.6", "Be patient with yourself. Growth takes time."),
            new("about.quotes.7", "The best time to begin was yesterday. The second best is right now."),
            new("about.quotes.8", "Celebrate the small wins. They count too."),
            new("about.quotes.9", "Slow progress is still progress."),
            new("about.quotes.10", "Your only real competition is who you were yesterday."),
        ];

        public static readonly LocString[] Jokes =
        [
            new("about.jokes.1", "Why don't scientists trust atoms? Because they make up everything."),
            new("about.jokes.2", "I would tell you a chemistry joke, but I know I wouldn't get a reaction."),
            new("about.jokes.3", "Why did the scarecrow win an award? He was outstanding in his field."),
            new("about.jokes.4", "I'm reading a book about anti-gravity. It's impossible to put down."),
            new("about.jokes.5", "Why don't skeletons fight each other? They don't have the guts."),
            new("about.jokes.6", "What do you call fake spaghetti? An impasta."),
            new("about.jokes.7", "Why did the bicycle fall over? It was two tired."),
            new("about.jokes.8", "What do you call cheese that isn't yours? Nacho cheese."),
            new("about.jokes.9", "I'm on a seafood diet. I see food, and I eat it."),
            new("about.jokes.10", "I only know 25 letters of the alphabet. I don't know y."),
        ];
    }

    internal static class Settings
    {
        public static readonly LocString Title = new("settings.title", "Settings");
        public static readonly LocString SearchHint = new("settings.searchHint", "Search...");
        public static readonly LocString Language = new("settings.language", "Language");
        public static readonly LocString LanguageHelp = new("settings.languageHelp", "The language of this plugin's windows. Job and Limit Break names always follow the game client.");


        public static readonly LocString CatWhenToFire = new("settings.cat.whenToFire", "When to fire");
        public static readonly LocString CatWhenToFireSub = new("settings.cat.whenToFireSub", "How low a target's HP must drop before the LB fires.");
        public static readonly LocString CatTargeting = new("settings.cat.targeting", "Targeting");
        public static readonly LocString CatTargetingSub = new("settings.cat.targetingSub", "Choose who the Limit Break lands on.");
        public static readonly LocString CatPerJob = new("settings.cat.perJob", "Per-job");
        public static readonly LocString CatPerJobSub = new("settings.cat.perJobSub", "Give your current job its own rule, separate from the global threshold.");
        public static readonly LocString CatFilters = new("settings.cat.filters", "Filters");
        public static readonly LocString CatFiltersSub = new("settings.cat.filtersSub", "Don't waste the LB on targets it can't kill, and limit it to the right duties.");
        public static readonly LocString CatBlocklist = new("settings.cat.blocklist", "Blocklist");
        public static readonly LocString CatBlocklistSub = new("settings.cat.blocklistSub", "Names here are never auto-targeted, even when below the threshold.");
        public static readonly LocString CatNotifications = new("settings.cat.notifications", "Notifications");
        public static readonly LocString CatNotificationsSub = new("settings.cat.notificationsSub", "Optional sound and chat cues when the LB fires.");
        public static readonly LocString CatCombatHud = new("settings.cat.combatHud", "Combat HUD");
        public static readonly LocString CatCombatHudSub = new("settings.cat.combatHudSub", "A small overlay with the LB gauge, your target and what the plugin is waiting for.");
        public static readonly LocString CatGeneral = new("settings.cat.general", "General");
        public static readonly LocString CatGeneralSub = new("settings.cat.generalSub", "Window and behavior preferences.");

        public static readonly LocString ThresholdGroup = new("settings.threshold.group", "Threshold");
        public static readonly LocString ThresholdType = new("settings.threshold.type", "Threshold type");
        public static readonly LocString ThresholdTypeHelp = new("settings.threshold.typeHelp", "Percent scales with every target's max HP. Absolute fires below a fixed amount of HP, whatever the target's max.");
        public static readonly LocString ModePercent = new("settings.threshold.modePercent", "Percent of max");
        public static readonly LocString ModeAbsolute = new("settings.threshold.modeAbsolute", "Absolute HP");
        public static readonly LocString FireBelow = new("settings.threshold.fireBelow", "Fire below");
        public static readonly LocString FireBelowPercentHelp = new("settings.threshold.fireBelowPercentHelp", "The LB fires once a target's HP, shields included, drops below this share of its max HP.");
        public static readonly LocString FireBelowAbsoluteHelp = new("settings.threshold.fireBelowAbsoluteHelp", "The LB fires once a target's HP, shields included, drops below this many points.");
        public static readonly LocString PercentFormat = new("settings.threshold.percentFormat", "%.0f%% of max HP");
        public static readonly LocString AbsoluteFormat = new("settings.threshold.absoluteFormat", "%d HP");
        public static readonly LocString PreviewGroup = new("settings.preview.group", "Preview");
        public static readonly LocString PreviewEmpty = new("settings.preview.empty", "Empty");
        public static readonly LocString PreviewFull = new("settings.preview.full", "Full");
        public static readonly LocString PreviewPercent = new("settings.preview.percent", "Fires when the target drops below {0}% of its max HP.");
        public static readonly LocString PreviewAbsolute = new("settings.preview.absolute", "Fires when the target drops below {0} HP.");

        public static readonly LocString TargetingGroup = new("settings.targeting.group", "Target selection");
        public static readonly LocString AutoSelect = new("settings.targeting.autoSelect", "Auto-select lowest-HP hostile");
        public static readonly LocString AutoSelectHelp = new("settings.targeting.autoSelectHelp", "Continuously scans every visible hostile and targets the one with the lowest HP, overriding your manual hard target.");
        public static readonly LocString ScanRange = new("settings.targeting.scanRange", "Scan range");
        public static readonly LocString ScanRangeHelp = new("settings.targeting.scanRangeHelp", "How far out to look for hostiles when auto-selecting.");
        public static readonly LocString RangeFormat = new("settings.targeting.rangeFormat", "%.0f y");
        public static readonly LocString RangeMelee = new("settings.targeting.rangeMelee", "About melee range");
        public static readonly LocString RangeMid = new("settings.targeting.rangeMid", "About mid range");
        public static readonly LocString RangeRanged = new("settings.targeting.rangeRanged", "About ranged combat");
        public static readonly LocString RangeArena = new("settings.targeting.rangeArena", "About the whole arena");
        public static readonly LocString ManualTargetNote = new("settings.targeting.manualNote", "Auto-select is off, so the LB only fires on the enemy you target yourself.");

        public static readonly LocString PerJobNoJob = new("settings.perJob.noJob", "Log into a job to set a per-job rule. The rule you set here applies only while that job is active.");
        public static readonly LocString PerJobGroup = new("settings.perJob.group", "Current job");
        public static readonly LocString PerJobOverride = new("settings.perJob.override", "Override for {0}");
        public static readonly LocString PerJobOverrideHelp = new("settings.perJob.overrideHelp", "When on, {0} uses its own rule below instead of the global threshold.");
        public static readonly LocString PerJobUsesGlobal = new("settings.perJob.usesGlobal", "This job follows the global threshold from When to fire.");
        public static readonly LocString PerJobRuleGroup = new("settings.perJob.ruleGroup", "Rule");
        public static readonly LocString PerJobPresetNote = new("settings.perJob.presetNote", "This rule came from a preset. Any change you make here turns it into your own rule, and presets leave it alone after that.");
        public static readonly LocString LimitBreakMode = new("settings.perJob.mode", "LB mode");
        public static readonly LocString LimitBreakModeHelp = new("settings.perJob.modeHelp", "Offensive LBs fire on a low enemy. Defensive and utility LBs are self-cast and fire on team conditions instead.");
        public static readonly LocString ModeOffensive = new("settings.perJob.modeOffensive", "Offensive");
        public static readonly LocString ModeDefensive = new("settings.perJob.modeDefensive", "Defensive");
        public static readonly LocString ModeUtility = new("settings.perJob.modeUtility", "Utility");
        public static readonly LocString ModeOffensiveBlurb = new("settings.perJob.offensiveBlurb", "Fires on a hostile below an HP threshold.");
        public static readonly LocString ModeDefensiveBlurb = new("settings.perJob.defensiveBlurb", "Fires (self-cast) when your team is pressured: enough hurt allies nearby and enemies present.");
        public static readonly LocString ModeUtilityBlurb = new("settings.perJob.utilityBlurb", "Fires (self-cast) in a committed teamfight: enough allies and enemies clustered, regardless of HP.");
        public static readonly LocString AllyHp = new("settings.perJob.allyHp", "Ally HP");
        public static readonly LocString AllyHpHelp = new("settings.perJob.allyHpHelp", "An ally counts as hurt below this share of max HP.");
        public static readonly LocString AllyHpFormat = new("settings.perJob.allyHpFormat", "%.0f%%");
        public static readonly LocString HurtAllies = new("settings.perJob.hurtAllies", "Hurt allies");
        public static readonly LocString HurtAlliesHelp = new("settings.perJob.hurtAlliesHelp", "Fire when at least this many hurt allies are nearby.");
        public static readonly LocString AlliesNear = new("settings.perJob.alliesNear", "Allies near");
        public static readonly LocString AlliesNearHelp = new("settings.perJob.alliesNearHelp", "Fire when at least this many allies are clustered nearby.");
        public static readonly LocString AllyRadius = new("settings.perJob.allyRadius", "Ally radius");
        public static readonly LocString AllyRadiusHelp = new("settings.perJob.allyRadiusHelp", "How close an ally must be to count.");
        public static readonly LocString EnemiesNear = new("settings.perJob.enemiesNear", "Enemies near");
        public static readonly LocString EnemiesNearHelp = new("settings.perJob.enemiesNearHelp", "Also require this many enemies within range.");
        public static readonly LocString EnemyRadius = new("settings.perJob.enemyRadius", "Enemy radius");
        public static readonly LocString EnemyRadiusHelp = new("settings.perJob.enemyRadiusHelp", "How close an enemy must be to count.");

        public static readonly LocString SkipGroup = new("settings.filters.skipGroup", "Skip targets");
        public static readonly LocString SkipDoomed = new("settings.filters.skipDoomed", "Skip targets that will die first");
        public static readonly LocString SkipDoomedHelp = new("settings.filters.skipDoomedHelp", "If an enemy is losing HP fast enough to die before the LB lands (about a 1.2 s cast lock), skip it so the charge isn't wasted on a kill someone else gets.");
        public static readonly LocString SkipGuarded = new("settings.filters.skipGuarded", "Skip targets using Guard");
        public static readonly LocString SkipGuardedHelp = new("settings.filters.skipGuardedHelp", "Guard reduces incoming damage by 90% for 5 s. The LB would land for about 10% and waste the charge, so guarded targets are skipped.");
        public static readonly LocString SkipInvulnerable = new("settings.filters.skipInvulnerable", "Skip targets immune to your LB");
        public static readonly LocString SkipInvulnerableHelp = new("settings.filters.skipInvulnerableHelp", "Paladin's Phalanx LB (Hallowed Ground) and Dark Knight's Eventide LB (Undead Redemption) make the target immune to damage for 10 s. The LB can't kill them, so these targets are skipped.");
        public static readonly LocString DutyGroup = new("settings.filters.dutyGroup", "Allowed duties");
        public static readonly LocString DutyFootnote = new("settings.filters.dutyFootnote", "Auto-fire only runs in the PvP modes switched on here.");

        public static readonly LocString BlocklistAddGroup = new("settings.blocklist.addGroup", "Add a player");
        public static readonly LocString BlocklistAdd = new("settings.blocklist.add", "Player name");
        public static readonly LocString BlocklistAddHelp = new("settings.blocklist.addHelp", "Type an exact player name and press Enter. Blocked players are never auto-targeted.");
        public static readonly LocString BlocklistHint = new("settings.blocklist.hint", "Player name, then Enter");
        public static readonly LocPlural BlocklistCount = new("settings.blocklist.count", "{0} blocked player", "{0} blocked players");
        public static readonly LocString BlocklistEmpty = new("settings.blocklist.empty", "No names blocked.");

        public static readonly LocString SoundGroup = new("settings.notifications.soundGroup", "Sound");
        public static readonly LocString PlaySound = new("settings.notifications.playSound", "Play sound on fire");
        public static readonly LocString PlaySoundHelp = new("settings.notifications.playSoundHelp", "Plays a chat sound effect (the same set as /se1 to /se16) each time the LB fires.");
        public static readonly LocString SoundEffect = new("settings.notifications.soundEffect", "Sound effect");
        public static readonly LocString SoundEffectHelp = new("settings.notifications.soundEffectHelp", "Which of the sixteen chat sound effects to play.");
        public static readonly LocString SoundEffectFormat = new("settings.notifications.soundEffectFormat", "<se.%d>");
        public static readonly LocString SoundPreview = new("settings.notifications.soundPreview", "Preview");
        public static readonly LocString SoundPreviewHelp = new("settings.notifications.soundPreviewHelp", "Play the selected sound once.");
        public static readonly LocString ChatGroup = new("settings.notifications.chatGroup", "Chat");
        public static readonly LocString LogToChat = new("settings.notifications.logToChat", "Log to chat on fire");
        public static readonly LocString LogToChatHelp = new("settings.notifications.logToChatHelp", "Prints a line to chat when an LB fires, for example \"fired Seiton Tenchu on Striking Dummy\".");

        public static readonly LocString HudGroup = new("settings.hud.group", "Combat HUD");
        public static readonly LocString HudShowInPvp = new("settings.hud.showInPvp", "Show during PvP duties");
        public static readonly LocString HudShowInPvpHelp = new("settings.hud.showInPvpHelp", "Open the HUD automatically whenever you enter a PvP duty, and hide it again when you leave.");
        public static readonly LocString HudLock = new("settings.hud.lock", "Lock in place");
        public static readonly LocString HudLockHelp = new("settings.hud.lockHelp", "A locked HUD ignores the mouse, so clicks pass through to the game. Unlock it to drag it somewhere else.");
        public static readonly LocString HudNow = new("settings.hud.now", "Right now");
        public static readonly LocString HudNowHelp = new("settings.hud.nowHelp", "Show or hide the HUD until your next zone change, in or out of PvP.");
        public static readonly LocString HudShow = new("settings.hud.show", "Show HUD");
        public static readonly LocString HudHide = new("settings.hud.hide", "Hide HUD");
        public static readonly LocString HudFootnote = new("settings.hud.footnote", "Type /pvpautolb hud to show or hide it from chat.");

        public static readonly LocString WindowGroup = new("settings.general.window", "Window");
        public static readonly LocString OpenOnLogin = new("settings.general.openOnLogin", "Open on login");
        public static readonly LocString OpenOnLoginHelp = new("settings.general.openOnLoginHelp", "Pop the main window automatically the next time you log in.");
    }

    internal static class Plugin
    {
        public static readonly LocString CommandHelp = new("plugin.commandHelp", "Toggle the Auto PVP LB window. /pvpautolb config | log | changelog | about | hud (show or hide the combat HUD).");
        public static readonly LocString CommandHelpAlias = new("plugin.commandHelpAlias", "Alias for /pvpautolb.");
    }
}
