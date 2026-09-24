using System;
using System.Globalization;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using PvpAutoLb.Core;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows;

public sealed partial class ConsoleWindow : Window
{
    private const float ToolbarGap = 8f;
    private const float SectionGap = 10f;
    private const float MinimumListHeight = 120f;
    private const int NoticeMs = 1800;
    private const int ClearConfirmMs = 3000;

    private const string EmptyText = "Nothing logged yet. Enter a PvP duty and every step the plugin takes shows up here.";
    private const string NoMatchesText = "No lines match the current filters.";
    private const string FooterText = "Every line also goes to the Dalamud log (/xllog) with the " + PvpAutoLbConstants.LogPrefix
        + " prefix. When reporting a bug, press Copy log and paste the result into the issue.";
    private const string SearchHintText = "Search messages and sources";
    private const string CopyAllText = "Copy log";
    private const string CopyTooltipText = "Copies the lines shown below with a header naming the plugin version, Dalamud version and zone, ready to paste into a bug report.";
    private const string CopiedText = "Copied";
    private const string ClearText = "Clear";
    private const string ConfirmClearText = "Click again to clear";
    private const string CloseText = "Close";
    private const string LevelTooltipText = "Click to show or hide these lines. Shift-click to show only this level.";
    private const string SourceChipFormat = "Source: {0}";
    private const string SourceChipTooltipText = "Click to stop filtering by source.";
    private const string JumpLatestText = "Jump to latest";
    private const string ShowingFormat = "Showing {0} of {1}";
    private const string ResetFiltersText = "Reset filters";
    private const string ShortcutsText = "Ctrl+F search · Ctrl+C copy · Shift-click selects a range · Double-click copies a line";
    private const string CopySelectionText = "Copy selection";
    private const string ClearSelectionText = "Clear selection";
    private const string CopyLineText = "Copy line";
    private const string CopyToEndText = "Copy from here to the end";
    private const string OnlySourceFormat = "Show only {0}";
    private const string RepeatedFormat = "Repeated {0} times in a row";
    private const string HasDetailsText = "Has a stack trace. Select the line to read it.";

    private static readonly RunLogLevel[] chipLevels = [RunLogLevel.Verbose, RunLogLevel.Debug, RunLogLevel.Info, RunLogLevel.Warning, RunLogLevel.Error];
    private static readonly string[] chipLabels = ["Verbose", "Debug", "Info", "Warnings", "Errors"];
    private static readonly string[] chipIds = ["##pvplb_log_verbose", "##pvplb_log_debug", "##pvplb_log_info", "##pvplb_log_warning", "##pvplb_log_error"];

    private readonly RunLogLine[] view = new RunLogLine[RunLog.Capacity];
    private readonly int[] levelTotals = new int[RunLog.LevelCount];
    private readonly string[] levelTotalLabels = new string[RunLog.LevelCount];
    private readonly CountedText copyFilteredText = new("Copy {0} line", "Copy {0} lines");
    private readonly CountedText copiedLinesText = new("Copied {0} line to the clipboard", "Copied {0} lines to the clipboard");
    private readonly CountedText selectedText = new("{0} line selected", "{0} lines selected");
    private readonly CountedText newLinesText = new(JumpLatestText + " · {0} new line", JumpLatestText + " · {0} new lines");

    private int viewCount;
    private int bufferedCount;
    private int builtVersion = -1;
    private RunLogFilter builtFilter;
    private string showingText = string.Empty;
    private string sourceChipText = string.Empty;

    private int levelMask = RunLogFilter.AllLevels;
    private string search = string.Empty;
    private string? source;
    private bool searchFocused;
    private bool focusSearch;

    private long copiedAtMs;
    private long clearArmedAtMs;
    private string? notice;
    private long noticeAtMs;

    public ConsoleWindow() : base("Auto PVP LB Console###PvpAutoLbConsole")
    {
        Size = new Vector2(720, 520);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(460, 320),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };

        for (var level = 0; level < RunLog.LevelCount; level++)
        {
            levelTotalLabels[level] = "0";
        }
    }

    private RunLogFilter Filter => new(levelMask, search, source);

    internal static Vector4 LevelColor(RunLogLevel level) => level switch
    {
        RunLogLevel.Verbose => Styling.TextDim,
        RunLogLevel.Debug   => Styling.AccentBlue,
        RunLogLevel.Warning => Styling.AccentAmber,
        RunLogLevel.Error   => Styling.AccentRed,
        _                   => Styling.AccentMint,
    };

    public override void Draw()
    {
        using var style = Styling.PushWindowStyle();
        RunLog.MarkSeen();
        Refresh();

        DrawToolbar();
        Styling.VSpace(ToolbarGap);
        DrawFilters();
        Styling.VSpace(SectionGap);

        var scale = ImGuiHelpers.GlobalScale;
        var spacing = ImGui.GetStyle().ItemSpacing.Y;
        var inspectorHeight = InspectorHeight();
        var inspectorBlock = inspectorHeight > 0f ? SectionGap * scale + spacing + inspectorHeight + spacing : 0f;
        var reserved = spacing + inspectorBlock + StatusGap * scale + spacing + TextDraw.LineHeight();
        var listHeight = MathF.Max(MinimumListHeight * scale, ImGui.GetContentRegionAvail().Y - reserved);
        DrawList(listHeight);
        DrawInspector(inspectorHeight);
        DrawStatus();
        HandleShortcuts();
    }

    private void Refresh()
    {
        var filter = Filter;
        var currentVersion = RunLog.Version;
        if (currentVersion == builtVersion && filter == builtFilter)
        {
            return;
        }

        var previousTail = viewCount > 0 ? view[viewCount - 1].Sequence : 0;
        viewCount = RunLog.Snapshot(filter, view, levelTotals);
        bufferedCount = 0;
        for (var level = 0; level < RunLog.LevelCount; level++)
        {
            bufferedCount += levelTotals[level];
            levelTotalLabels[level] = levelTotals[level].ToString(CultureInfo.InvariantCulture);
        }

        showingText = string.Format(CultureInfo.InvariantCulture, ShowingFormat,
            viewCount.ToString("N0", CultureInfo.InvariantCulture), bufferedCount.ToString("N0", CultureInfo.InvariantCulture));
        if (filter.Source is not null && filter.Source != builtFilter.Source)
        {
            sourceChipText = string.Format(CultureInfo.InvariantCulture, SourceChipFormat, filter.Source);
        }

        OnViewRebuilt(previousTail, filter != builtFilter);
        builtVersion = currentVersion;
        builtFilter = filter;
    }

    private void DrawToolbar()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var height = Layout.ActionPillHeight * scale;
        var origin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        var gap = ToolbarGap * scale;
        var now = Environment.TickCount64;

        var copyLabel = now - copiedAtMs < NoticeMs ? CopiedText
            : builtFilter.IsNarrowed ? copyFilteredText.For(viewCount)
            : CopyAllText;
        var clearArmed = now - clearArmedAtMs < ClearConfirmMs;
        var clearLabel = clearArmed ? ConfirmClearText : ClearText;
        var copyWidth = PillButton.Width(copyLabel, FontAwesomeIcon.Copy);
        var clearWidth = PillButton.Width(clearLabel, FontAwesomeIcon.Eraser);
        var searchWidth = MathF.Max(1f, width - copyWidth - clearWidth - gap * 2f);

        ImGui.SetCursorScreenPos(origin);
        if (SearchField.Draw("##pvplb_log_search", SearchHintText, ref search, ref searchFocused, searchWidth, height, focusSearch))
        {
            ClearSelection();
        }

        focusSearch = false;

        ImGui.SetCursorScreenPos(origin + new Vector2(searchWidth + gap, 0f));
        if (PillButton.Draw("##pvplb_log_copy", copyLabel, Styling.AccentRed, PillButton.Emphasis.Filled, FontAwesomeIcon.Copy,
                enabled: viewCount > 0, tooltip: CopyTooltipText))
        {
            CopyView();
        }

        ImGui.SetCursorScreenPos(origin + new Vector2(searchWidth + copyWidth + gap * 2f, 0f));
        if (PillButton.Draw("##pvplb_log_clear", clearLabel, clearArmed ? Styling.AccentRose : Styling.TextSecondary,
                clearArmed ? PillButton.Emphasis.Tinted : PillButton.Emphasis.Ghost, FontAwesomeIcon.Eraser, enabled: bufferedCount > 0))
        {
            if (clearArmed)
            {
                RunLog.Clear();
                ClearSelection();
                clearArmedAtMs = 0;
            }
            else
            {
                clearArmedAtMs = now;
            }
        }

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(width, height));
    }

    private void DrawFilters()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var height = Layout.ConsoleChipHeight * scale;
        var gap = ToolbarGap * scale;
        var origin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        var x = origin.X;
        var y = origin.Y;

        for (var index = 0; index < chipLevels.Length; index++)
        {
            var level = chipLevels[index];
            var label = chipLabels[index];
            var count = levelTotalLabels[(int)level];
            var chipWidth = FilterChip.Width(label, count);
            WrapIfNeeded(ref x, ref y, chipWidth, origin.X, width, height, gap);

            ImGui.SetCursorScreenPos(new Vector2(x, y));
            if (FilterChip.Draw(chipIds[index], label, count, LevelColor(level), builtFilter.Shows(level), height, null, LevelTooltipText))
            {
                ToggleLevel(level, ImGui.GetIO().KeyShift);
            }

            x += chipWidth + gap;
        }

        if (source is not null)
        {
            var chipWidth = FilterChip.Width(sourceChipText, null, FontAwesomeIcon.Times);
            WrapIfNeeded(ref x, ref y, chipWidth, origin.X, width, height, gap);

            ImGui.SetCursorScreenPos(new Vector2(x, y));
            if (FilterChip.Draw("##pvplb_log_source", sourceChipText, null, Styling.AccentViolet, true, height, FontAwesomeIcon.Times, SourceChipTooltipText))
            {
                source = null;
                ClearSelection();
            }
        }

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(width, y - origin.Y + height));
    }

    private static void WrapIfNeeded(ref float x, ref float y, float itemWidth, float left, float width, float height, float gap)
    {
        if (x <= left || x + itemWidth <= left + width)
        {
            return;
        }

        x = left;
        y += height + gap;
    }

    private void ToggleLevel(RunLogLevel level, bool solo)
    {
        var bit = 1 << (int)level;
        if (solo)
        {
            levelMask = levelMask == bit ? RunLogFilter.AllLevels : bit;
        }
        else
        {
            levelMask ^= bit;
        }

        ClearSelection();
    }

    private void ResetFilters()
    {
        levelMask = RunLogFilter.AllLevels;
        search = string.Empty;
        source = null;
        ClearSelection();
    }

    private void ShowNotice(string text)
    {
        notice = text;
        noticeAtMs = Environment.TickCount64;
    }

    private static Vector4 MessageColor(RunLogLevel level) => level switch
    {
        RunLogLevel.Verbose => Styling.TextMuted,
        RunLogLevel.Debug   => Styling.TextDim,
        RunLogLevel.Warning => Styling.AccentAmberSoft,
        RunLogLevel.Error   => Styling.AccentRedBright,
        _                   => Styling.TextSecondary,
    };
}
