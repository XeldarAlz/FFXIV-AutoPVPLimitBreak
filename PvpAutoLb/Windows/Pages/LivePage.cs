using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using PvpAutoLb.Core;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;
using PvpAutoLb.Windows.Sections;

namespace PvpAutoLb.Windows.Pages;

internal sealed class LivePage
{
    private const int MaxCandidateRows = 8;
    private const float SectionGap = 20f;
    private const float CardPadX = 18f;
    private const float ConfirmWindowMs = 3000f;
    private const string LifetimeResetId = "##palb_live_reset_lifetime";
    private const string SessionResetId = "##palb_live_reset_session";

    private readonly CachedText hpText = new();
    private readonly CachedText candidateCountText = new();
    private readonly CachedText moreText = new();
    private readonly CachedText targetingText = new();
    private readonly CachedText sessionForText = new();
    private readonly CachedText lastFiredText = new();
    private readonly CachedText[] statTexts = [new(), new(), new(), new(), new(), new()];

    private long lifetimeResetArmedAt = long.MinValue;
    private long sessionResetArmedAt = long.MinValue;

    public void Draw(Plugin plugin)
    {
        var snapshot = LiveSnapshot.Resolve(plugin);
        DrawHero(plugin.Configuration, snapshot);
        Styling.VSpace(SectionGap);
        DrawTargetSection(plugin.Configuration, snapshot);
        Styling.VSpace(SectionGap);
        DrawCandidates(snapshot);
        Styling.VSpace(SectionGap);
        DrawStats(plugin.Controller, plugin.Configuration);
        Styling.VSpace(12f);
    }

    private static void DrawHero(Configuration configuration, LiveSnapshot snapshot)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var size = new Vector2(ImGui.GetContentRegionAvail().X, Layout.LiveHeroHeight * scale);
        var origin = ImGui.GetCursorScreenPos();
        var end = origin + size;
        var drawList = ImGui.GetWindowDrawList();
        var rounding = Styling.PanelRounding * scale;
        var accent = ReadinessRing.RingAccent(snapshot);

        Paint.Glass(drawList, origin, end, rounding, accent, snapshot.Armed ? 0.10f : 0.03f, 0f, true);
        if (snapshot.Firing)
        {
            Paint.Stroke(drawList, origin, end, Styling.PulseColor(Styling.WithAlpha(accent, 0.5f), Styling.AccentRedBright, Styling.PulseFast), rounding, 1.6f * scale);
        }

        var paddingX = CardPadX * scale;
        var ringRadius = size.Y * 0.5f - 22f * scale;
        var ringCenter = new Vector2(origin.X + paddingX + ringRadius + 4f * scale, origin.Y + size.Y * 0.5f);
        ReadinessRing.Draw("##palb_live_ring", ringCenter, ringRadius, 7f * scale, snapshot);

        var columnX = ringCenter.X + ringRadius + 26f * scale;
        var columnWidth = end.X - paddingX - columnX;
        var y = origin.Y + 18f * scale;

        var chipHeight = DrawChip(drawList, columnX, y, snapshot.GaugeLabel, accent, snapshot.LimitBreakReady);
        if (!snapshot.Armed)
        {
            DrawChip(drawList, columnX + ChipWidth(snapshot.GaugeLabel) + 8f * scale, y, Loc.T(L.Live.ChipDisarmed), Styling.TextDim, false);
        }

        y += chipHeight + 10f * scale;

        using (Fonts.PushTitle())
        {
            var name = snapshot.LimitBreakName.Length > 0 ? snapshot.LimitBreakName : Loc.T(L.Live.NoLimitBreakName);
            var clipped = TextDraw.Truncate(name, columnWidth);
            TextDraw.At(clipped, new Vector2(columnX, y), Styling.TextStrong);
            y += TextDraw.Measure(clipped).Y + 4f * scale;
        }

        using (Fonts.PushCaption())
        {
            var clipped = TextDraw.Truncate(snapshot.Subtitle, columnWidth);
            TextDraw.At(clipped, new Vector2(columnX, y), Styling.TextDim);
            y += TextDraw.Measure(clipped).Y + 10f * scale;
        }

        if (snapshot.ActionId != 0)
        {
            DrawRuleRow(drawList, configuration, snapshot, columnX, y, columnWidth);
        }

        ImGui.Dummy(size);
    }

    private static void DrawRuleRow(ImDrawListPtr drawList, Configuration configuration, LiveSnapshot snapshot, float x, float y, float width)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var support = snapshot.Rule.Mode != LbFireMode.Offensive;
        var icon = support ? FontAwesomeIcon.UsersCog : FontAwesomeIcon.HeartBroken;
        var text = snapshot.ActiveRule;
        var lineHeight = ImGui.GetTextLineHeight();
        var iconSize = TextDraw.IconSize(icon);
        TextDraw.Icon(icon, new Vector2(x, y + (lineHeight - iconSize.Y) * 0.5f), Styling.AccentRedBright);
        x += iconSize.X + 8f * scale;

        var badge = BadgeText(configuration, snapshot);
        var badgeWidth = ChipWidth(badge) + 10f * scale;
        var clipped = TextDraw.Truncate(text, MathF.Max(0f, width - iconSize.X - 8f * scale - badgeWidth));
        TextDraw.At(clipped, new Vector2(x, y), Styling.TextSecondary);
        x += TextDraw.Measure(clipped).X + 10f * scale;

        var badgeAccent = snapshot.PerJob ? Styling.AccentBlue : Styling.TextDim;
        using (Fonts.PushCaption())
        {
            var badgeHeight = TextDraw.LineHeight() + 6f * scale;
            DrawChip(drawList, x, y + (lineHeight - badgeHeight) * 0.5f, badge, badgeAccent, false);
        }
    }

    private static string BadgeText(Configuration configuration, LiveSnapshot snapshot)
    {
        if (!snapshot.PerJob)
        {
            return Loc.T(L.Live.BadgeGlobal);
        }

        return configuration.PerJobRules.TryGetValue(snapshot.JobId, out var rule) && rule.Source == RuleSource.Preset
            ? Loc.T(L.Live.BadgePreset)
            : Loc.T(L.Live.BadgePerJob);
    }

    private static float ChipWidth(string text)
    {
        using (Fonts.PushCaption())
        {
            return TextDraw.Measure(TextDraw.Upper(text)).X + 18f * ImGuiHelpers.GlobalScale;
        }
    }

    private static float DrawChip(ImDrawListPtr drawList, float x, float y, string text, Vector4 accent, bool pulse)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var paddingX = 9f * scale;
        var paddingY = 3f * scale;
        using (Fonts.PushCaption())
        {
            var label = TextDraw.Upper(text);
            var textSize = TextDraw.Measure(label);
            var chipMin = new Vector2(x, y);
            var chipMax = chipMin + new Vector2(paddingX * 2f + textSize.X, textSize.Y + paddingY * 2f);
            var fillAlpha = pulse ? 0.20f + 0.14f * Styling.Pulse(Styling.PulseBreath) : 0.22f;
            Paint.Pill(drawList, chipMin, chipMax, Styling.WithAlpha(accent, fillAlpha), Styling.WithAlpha(accent, 0.65f));
            TextDraw.At(label, new Vector2(x + paddingX, y + paddingY), Styling.Lighten(accent, 0.3f));
            return chipMax.Y - chipMin.Y;
        }
    }

    private void DrawTargetSection(Configuration configuration, LiveSnapshot snapshot)
    {
        DrawSectionHeader(Loc.T(L.Live.TargetTitle), TargetingCaption(configuration));
        if (snapshot.Target is { } target)
        {
            DrawTargetCard(target, snapshot);
            return;
        }

        DrawEmptyTarget(snapshot);
    }

    private string TargetingCaption(Configuration configuration)
    {
        var range = (int)MathF.Round(configuration.AutoSelectRangeYalms);
        var key = configuration.AutoSelectLowestHp ? range : -1;
        if (targetingText.Matches(key))
        {
            return targetingText.Text;
        }

        return targetingText.Store(key, configuration.AutoSelectLowestHp ? Loc.T(L.Live.TargetingAuto, range) : Loc.T(L.Live.TargetingManual));
    }

    private void DrawTargetCard(IBattleChara target, LiveSnapshot snapshot)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var size = new Vector2(ImGui.GetContentRegionAvail().X, Layout.TargetCardHeight * scale);
        var origin = ImGui.GetCursorScreenPos();
        var end = origin + size;
        var drawList = ImGui.GetWindowDrawList();
        var rounding = Styling.PanelRounding * scale;
        var paddingX = CardPadX * scale;
        var width = size.X - paddingX * 2f;

        Paint.Glass(drawList, origin, end, rounding, snapshot.Accent, 0.07f);
        if (snapshot.Firing)
        {
            Paint.Stroke(drawList, origin, end, Styling.PulseColor(Styling.WithAlpha(Styling.AccentRed, 0.5f), Styling.AccentRedBright, Styling.PulseFast), rounding, 1.6f * scale);
        }

        var y = origin.Y + 16f * scale;
        var distance = Labels.Yalms(snapshot.TargetDistance);
        Vector2 distanceSize;
        using (Fonts.PushCaption())
        {
            distanceSize = TextDraw.Measure(distance);
        }

        using (Fonts.PushHeadline())
        {
            var name = TextDraw.Truncate(snapshot.TargetName, width - distanceSize.X - 16f * scale);
            var nameSize = TextDraw.Measure(name);
            TextDraw.At(name, new Vector2(origin.X + paddingX, y), Styling.TextStrong);
            using (Fonts.PushCaption())
            {
                TextDraw.Right(distance, end.X - paddingX, y + (nameSize.Y - distanceSize.Y) * 0.5f, Styling.TextDim);
            }

            y += nameSize.Y + 12f * scale;
        }

        var barHeight = 14f * scale;
        var hp = Motion.Approach(Motion.Key("##palb_live_target_hp", target.EntityId), snapshot.TargetHp, 12f);
        var fill = snapshot.Block == FireBlock.AboveThreshold ? Styling.AccentMint : Styling.AccentRed;
        HpBar.Draw(drawList, new Vector2(origin.X + paddingX, y), width, barHeight, hp, snapshot.TargetShield, snapshot.TargetThreshold, fill);
        y += barHeight + 7f * scale;

        using (Fonts.PushCaption())
        {
            TextDraw.At(HpLine(target), new Vector2(origin.X + paddingX, y), Styling.TextDim);
            TextDraw.Right(Labels.Percent(snapshot.TargetHp), end.X - paddingX, y, Styling.TextSecondary);
            y += TextDraw.LineHeight() + 12f * scale;
        }

        DrawStatusLine(snapshot, new Vector2(origin.X + paddingX, y), width);
        ImGui.Dummy(size);
    }

    private string HpLine(IBattleChara target)
    {
        var shield = HpMath.ShieldHp(target);
        var key = HashCode.Combine(target.CurrentHp, target.MaxHp, shield);
        if (hpText.Matches(key))
        {
            return hpText.Text;
        }

        var current = Formatting.Count(target.CurrentHp);
        var maximum = Formatting.Count(target.MaxHp);
        var text = shield > 0
            ? Loc.T(L.Live.HpWithShield, current, Formatting.Count(shield), maximum)
            : Loc.T(L.Live.Hp, current, maximum);
        return hpText.Store(key, text);
    }

    private static void DrawStatusLine(LiveSnapshot snapshot, Vector2 origin, float width)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var lineHeight = ImGui.GetTextLineHeight();
        var iconSize = TextDraw.IconSize(snapshot.Icon);
        var color = snapshot.Firing ? Styling.PulseColor(snapshot.Accent, snapshot.AccentSoft, Styling.PulseFast) : snapshot.AccentSoft;
        TextDraw.Icon(snapshot.Icon, new Vector2(origin.X, origin.Y + (lineHeight - iconSize.Y) * 0.5f), color);
        var textX = origin.X + iconSize.X + 9f * scale;
        TextDraw.At(TextDraw.Truncate(snapshot.Status, width - (textX - origin.X)), new Vector2(textX, origin.Y), color);
    }

    private static void DrawEmptyTarget(LiveSnapshot snapshot)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var size = new Vector2(ImGui.GetContentRegionAvail().X, Layout.TargetCardHeight * scale);
        var origin = ImGui.GetCursorScreenPos();
        var end = origin + size;
        var drawList = ImGui.GetWindowDrawList();
        var paddingX = CardPadX * scale;
        var width = size.X - paddingX * 2f;

        Paint.Glass(drawList, origin, end, Styling.PanelRounding * scale, snapshot.Accent, 0.04f);

        var badgeRadius = 22f * scale;
        var badgeCenter = new Vector2(origin.X + paddingX + badgeRadius, origin.Y + size.Y * 0.5f);
        ProgressRing.Disc(badgeCenter, badgeRadius, Styling.WithAlpha(snapshot.Accent, 0.14f));
        drawList.AddCircle(badgeCenter, badgeRadius, Paint.Col(Styling.WithAlpha(snapshot.Accent, 0.45f)), 48, 1.2f * scale);
        ProgressRing.CenterIcon(badgeCenter, snapshot.Icon, snapshot.AccentSoft, badgeRadius * 0.9f);

        var support = snapshot.Kind == LiveKind.Support;
        var meterWidth = SupportMeterWidth(width);
        var textX = badgeCenter.X + badgeRadius + 18f * scale;
        var textWidth = end.X - paddingX - textX - (support ? meterWidth + 20f * scale : 0f);
        var title = EmptyTitle(snapshot);
        float titleHeight;
        using (Fonts.PushHeadline())
        {
            titleHeight = TextDraw.LineHeight();
        }

        var detailSize = TextDraw.MeasureWrapped(snapshot.Status, textWidth);
        var top = origin.Y + (size.Y - titleHeight - 6f * scale - detailSize.Y) * 0.5f;
        using (Fonts.PushHeadline())
        {
            TextDraw.At(TextDraw.Truncate(title, textWidth), new Vector2(textX, top), Styling.TextStrong);
        }

        TextDraw.Wrapped(snapshot.Status, new Vector2(textX, top + titleHeight + 6f * scale), textWidth, Styling.TextDim);

        if (support)
        {
            DrawSupportMeters(drawList, snapshot, new Vector2(end.X - paddingX, origin.Y + size.Y * 0.5f), meterWidth);
        }

        ImGui.Dummy(size);
    }

    private static string EmptyTitle(LiveSnapshot snapshot) => snapshot.Kind switch
    {
        LiveKind.Offline => Loc.T(L.Live.EmptyOffline),
        LiveKind.NoLimitBreak => Loc.T(L.Live.EmptyNoLimitBreak),
        LiveKind.NotInPvp => Loc.T(L.Live.EmptyNotInPvp),
        LiveKind.DutyOff => Loc.T(L.Live.EmptyDutyOff),
        LiveKind.Support => snapshot.Rule.Mode == LbFireMode.Defensive ? Loc.T(L.Live.EmptySupportDefensive) : Loc.T(L.Live.EmptySupportUtility),
        LiveKind.NoTarget => Loc.T(L.Live.EmptyNoTarget),
        _ => Loc.T(L.Live.EmptyNoEnemies),
    };

    private static float SupportMeterWidth(float width) => MathF.Min(160f * ImGuiHelpers.GlobalScale, width * 0.3f);

    private static void DrawSupportMeters(ImDrawListPtr drawList, LiveSnapshot snapshot, Vector2 rightMiddle, float barWidth)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var barHeight = 6f * scale;
        var x = rightMiddle.X - barWidth;
        var allyFraction = snapshot.Rule.AllyCountNear <= 0 ? 1f : (float)snapshot.SupportAllies / snapshot.Rule.AllyCountNear;
        var enemyFraction = snapshot.Rule.EnemyCountNear <= 0 ? 1f : (float)snapshot.SupportEnemies / snapshot.Rule.EnemyCountNear;
        Paint.Bar(drawList, new Vector2(x, rightMiddle.Y - barHeight - 5f * scale), barWidth, barHeight, allyFraction, Styling.AccentMint);
        Paint.Bar(drawList, new Vector2(x, rightMiddle.Y + 5f * scale), barWidth, barHeight, enemyFraction, Styling.AccentRose);
    }

    private void DrawCandidates(LiveSnapshot snapshot)
    {
        var candidates = snapshot.Candidates;
        var count = candidates.Count;
        var caption = candidateCountText.Matches(count)
            ? candidateCountText.Text
            : candidateCountText.Store(count, Loc.Plural(L.Live.EnemiesInRange, count));
        DrawSectionHeader(Loc.T(L.Live.CandidatesTitle), count > 0 ? caption : null);

        if (count == 0)
        {
            TextDraw.Hint(Loc.T(snapshot.IsLive ? L.Live.CandidatesEmpty : L.Live.CandidatesIdle));
            return;
        }

        var rows = Math.Min(count, MaxCandidateRows);
        for (var index = 0; index < rows; index++)
        {
            DrawCandidateRow(snapshot, candidates[index], index);
        }

        if (count <= MaxCandidateRows)
        {
            return;
        }

        var hidden = count - MaxCandidateRows;
        TextDraw.Hint(moreText.Matches(hidden) ? moreText.Text : moreText.Store(hidden, Loc.Plural(L.Live.MoreEnemies, hidden)));
    }

    private static void DrawCandidateRow(LiveSnapshot snapshot, IBattleChara candidate, int index)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var size = new Vector2(ImGui.GetContentRegionAvail().X, Layout.CandidateRowHeight * scale);
        var origin = ImGui.GetCursorScreenPos();
        var end = origin + size;
        var drawList = ImGui.GetWindowDrawList();
        var rounding = Styling.CardRounding * scale;
        var isTarget = snapshot.Target is { } target && target.GameObjectId == candidate.GameObjectId;
        var below = snapshot.IsBelowThreshold(candidate);
        var accent = below ? Styling.AccentRed : Styling.AccentMint;

        ImGui.PushID(index);
        var hover = Motion.Hover(Motion.Key("##palb_candidate"), Hit.HoveringRect(origin, end));
        ImGui.PopID();
        Paint.Glass(drawList, origin, end, rounding, isTarget ? snapshot.Accent : accent, isTarget ? 0.10f : 0.02f, hover);
        if (isTarget)
        {
            Paint.Fill(drawList, new Vector2(origin.X, origin.Y + size.Y * 0.2f), new Vector2(origin.X + 3f * scale, origin.Y + size.Y * 0.8f), snapshot.AccentSoft, 2f * scale);
        }

        var paddingX = 14f * scale;
        var midY = origin.Y + size.Y * 0.5f;
        var percent = Labels.Percent(candidate.MaxHp == 0 ? 0f : (float)candidate.CurrentHp / candidate.MaxHp);
        var distance = Labels.Yalms(Geo.DistanceToPlayer(candidate));
        var barWidth = MathF.Min(260f * scale, size.X * 0.36f);
        var barHeight = 8f * scale;

        Vector2 percentSize;
        Vector2 distanceSize;
        using (Fonts.PushCaption())
        {
            percentSize = TextDraw.Measure(Labels.Percent(1f));
            distanceSize = TextDraw.Measure(distance);
            TextDraw.Right(percent, end.X - paddingX, midY - TextDraw.Measure(percent).Y * 0.5f, below ? Styling.AccentRedBright : Styling.TextSecondary);
        }

        var barX = end.X - paddingX - percentSize.X - 12f * scale - barWidth;
        var fraction = candidate.MaxHp == 0 ? 0f : Math.Clamp((float)candidate.CurrentHp / candidate.MaxHp, 0f, 1f);
        var shield = Math.Clamp(candidate.ShieldPercentage / 100f, 0f, 1f - fraction);
        HpBar.Draw(drawList, new Vector2(barX, midY - barHeight * 0.5f), barWidth, barHeight, fraction, shield, snapshot.ThresholdFractionFor(candidate), accent);

        var distanceX = barX - 16f * scale - distanceSize.X;
        using (Fonts.PushCaption())
        {
            TextDraw.At(distance, new Vector2(distanceX, midY - distanceSize.Y * 0.5f), Styling.TextDim);
        }

        var nameX = origin.X + paddingX + (isTarget ? 4f * scale : 0f);
        var name = TextDraw.Truncate(snapshot.NameOf(candidate), distanceX - 12f * scale - nameX);
        var nameSize = TextDraw.Measure(name);
        TextDraw.At(name, new Vector2(nameX, midY - nameSize.Y * 0.5f), isTarget ? Styling.TextStrong : Styling.TextSecondary);

        ImGui.Dummy(size);
        Styling.VSpace(2f);
    }

    private void DrawStats(AutoLbController controller, Configuration configuration)
    {
        var stats = controller.Stats;
        var sessionSeconds = (int)(DateTime.UtcNow - stats.StartedUtc).TotalSeconds;
        var sessionFor = sessionForText.Matches(sessionSeconds)
            ? sessionForText.Text
            : sessionForText.Store(sessionSeconds, Loc.T(L.Live.SessionFor, Formatting.Elapsed(sessionSeconds)));
        if (ConfirmedReset(Loc.T(L.Live.SessionTitle), sessionFor, SessionResetId, Loc.T(L.Live.ResetSession), ref sessionResetArmedAt))
        {
            stats.ResetSession();
        }

        var lastFired = LastFired(controller);
        DrawStatRow(0, stats.TotalFires, stats.KillsAttributed, stats.EnemiesAffectedTotal, lastFired);
        Styling.VSpace(SectionGap);

        if (ConfirmedReset(Loc.T(L.Live.LifetimeTitle), null, LifetimeResetId, Loc.T(L.Live.ResetLifetime), ref lifetimeResetArmedAt))
        {
            stats.ResetLifetime();
        }

        DrawStatRow(3, configuration.LifetimeFires, configuration.LifetimeKills, configuration.LifetimeEnemiesAffected, null);
    }

    private string LastFired(AutoLbController controller)
    {
        if (controller.LastFiredUtc is not { } firedAt)
        {
            return lastFiredText.Matches(-1) ? lastFiredText.Text : lastFiredText.Store(-1, Loc.T(L.Live.NeverFired));
        }

        var seconds = (int)(DateTime.UtcNow - firedAt).TotalSeconds;
        return lastFiredText.Matches(seconds) ? lastFiredText.Text : lastFiredText.Store(seconds, Formatting.Ago(seconds));
    }

    private void DrawStatRow(int firstSlot, long fires, long kills, long enemies, string? firesSub)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var gap = 10f * scale;
        var tileWidth = (ImGui.GetContentRegionAvail().X - gap * 2f) / 3f;

        StatTile.Draw(Loc.T(L.Live.StatFires), StatText(firstSlot, fires), firesSub, Styling.AccentRed, tileWidth);
        ImGui.SameLine(0f, gap);
        StatTile.Draw(Loc.T(L.Live.StatKills), StatText(firstSlot + 1, kills), null, Styling.AccentMint, tileWidth);
        ImGui.SameLine(0f, gap);
        StatTile.Draw(Loc.T(L.Live.StatEnemiesHit), StatText(firstSlot + 2, enemies), null, Styling.AccentAmber, tileWidth);
    }

    private string StatText(int slot, long value)
    {
        var cache = statTexts[slot];
        return cache.Matches(value) ? cache.Text : cache.Store(value, Formatting.Count(value));
    }

    private static void DrawSectionHeader(string title, string? caption)
        => DrawSectionHeader(title, caption, null, false, null);

    // Resetting stats cannot be undone, so the first click only arms the button and a second click within the
    // confirm window does the reset.
    private static bool ConfirmedReset(string title, string? caption, string id, string tooltip, ref long armedAt)
    {
        var armed = Environment.TickCount64 - armedAt < ConfirmWindowMs;
        if (!DrawSectionHeader(title, caption, id, armed, tooltip))
        {
            return false;
        }

        if (!armed)
        {
            armedAt = Environment.TickCount64;
            return false;
        }

        armedAt = long.MinValue;
        return true;
    }

    private static bool DrawSectionHeader(string title, string? caption, string? buttonId, bool armed, string? buttonTooltip)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var origin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        var titleSize = TextDraw.SectionTitleSize(title);
        TextDraw.SectionTitle(title, origin, Styling.TextStrong);

        var right = origin.X + width;
        var clicked = false;
        if (buttonId is not null)
        {
            var buttonSize = titleSize.Y + 6f * scale;
            if (armed)
            {
                var label = Loc.T(L.Live.ConfirmReset);
                var pillWidth = PillButton.Width(label, FontAwesomeIcon.Undo);
                ImGui.SetCursorScreenPos(new Vector2(right - pillWidth, origin.Y + (titleSize.Y - buttonSize) * 0.5f));
                clicked = PillButton.Draw(buttonId, label, Styling.AccentRose, PillButton.Emphasis.Tinted, FontAwesomeIcon.Undo,
                    height: buttonSize / scale);
                right -= pillWidth + 8f * scale;
            }
            else
            {
                ImGui.SetCursorScreenPos(new Vector2(right - buttonSize, origin.Y + (titleSize.Y - buttonSize) * 0.5f));
                clicked = IconButton.Draw(FontAwesomeIcon.Undo, buttonId, buttonSize, Styling.TextDim, buttonTooltip);
                right -= buttonSize + 8f * scale;
            }
        }

        if (caption is not null)
        {
            using (Fonts.PushCaption())
            {
                var captionSize = TextDraw.Measure(caption);
                var available = right - (origin.X + titleSize.X + 16f * scale);
                var clipped = TextDraw.Truncate(caption, available);
                TextDraw.Right(clipped, right, origin.Y + (titleSize.Y - captionSize.Y) * 0.5f, Styling.TextDim);
            }
        }

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(width, titleSize.Y + 10f * scale));
        return clicked;
    }
}
