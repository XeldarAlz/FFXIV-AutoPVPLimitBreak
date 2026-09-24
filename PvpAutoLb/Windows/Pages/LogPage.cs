using PvpAutoLb.Core;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using System.Numerics;

namespace PvpAutoLb.Windows.Pages;

internal sealed partial class LogPage
{
    private const float ToolbarGap = 8f;
    private const float SectionGap = 10f;
    private const float MinimumListHeight = 120f;
    private const int NoticeMs = 1800;
    private const int ClearConfirmMs = 3000;

    private static readonly RunLogLevel[] chipLevels = [RunLogLevel.Verbose, RunLogLevel.Debug, RunLogLevel.Info, RunLogLevel.Warning, RunLogLevel.Error];
    private static readonly LocString[] chipLabels = [L.Log.LevelVerbose, L.Log.LevelDebug, L.Log.LevelInfo, L.Log.LevelWarning, L.Log.LevelError];
    private static readonly string[] chipIds = ["##palb_log_verbose", "##palb_log_debug", "##palb_log_info", "##palb_log_warning", "##palb_log_error"];

    private readonly RunLogLine[] view = new RunLogLine[RunLog.Capacity];
    private readonly int[] levelTotals = new int[RunLog.LevelCount];

    private int viewCount;
    private int bufferedCount;
    private int builtVersion = -1;
    private RunLogFilter builtFilter;

    private int levelMask = RunLogFilter.AllLevels;
    private string search = string.Empty;
    private string? source;
    private bool searchFocused;
    private bool focusSearch;

    private long copiedAtMs;
    private long clearArmedAtMs;
    private string? notice;
    private long noticeAtMs;

    private RunLogFilter Filter => new(levelMask, search, source);

    public void Draw()
    {
        RunLog.MarkSeen();
        Refresh();
        PageHeader.Draw(Loc.T(L.Log.Title), Loc.Plural(L.Log.Entries, bufferedCount));

        DrawToolbar();
        Styling.VSpace(ToolbarGap);
        DrawFilters();
        Styling.VSpace(SectionGap);

        var scale = ImGuiHelpers.GlobalScale;
        var spacing = ImGui.GetStyle().ItemSpacing.Y;
        var inspectorHeight = InspectorHeight();
        var inspectorBlock = inspectorHeight > 0f ? SectionGap * scale + spacing + inspectorHeight + spacing : 0f;
        var reserved = spacing + inspectorBlock + StatusGap * scale + spacing + StatusLineHeight();
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

        var copyLabel = now - copiedAtMs < NoticeMs ? Loc.T(L.Log.Copied)
            : builtFilter.IsNarrowed ? Loc.Plural(L.Log.CopyFiltered, viewCount)
            : Loc.T(L.Log.CopyAll);
        var clearArmed = now - clearArmedAtMs < ClearConfirmMs;
        var clearLabel = Loc.T(clearArmed ? L.Log.ConfirmClear : L.Log.Clear);
        var copyWidth = PillButton.Width(copyLabel, FontAwesomeIcon.Copy);
        var clearWidth = PillButton.Width(clearLabel, FontAwesomeIcon.Eraser);
        var searchWidth = MathF.Max(1f, width - copyWidth - clearWidth - gap * 2f);

        ImGui.SetCursorScreenPos(origin);
        if (SearchField.Draw("##palb_log_search", Loc.T(L.Log.SearchHint), ref search, ref searchFocused, searchWidth, height, focusSearch))
        {
            ClearSelection();
        }

        focusSearch = false;

        ImGui.SetCursorScreenPos(origin + new Vector2(searchWidth + gap, 0f));
        if (PillButton.Draw("##palb_log_copy", copyLabel, Styling.AccentRed, PillButton.Emphasis.Filled, FontAwesomeIcon.Copy,
                enabled: viewCount > 0, height: Layout.ActionPillHeight, tooltip: Loc.T(L.Log.CopyTooltip)))
        {
            CopyView();
        }

        ImGui.SetCursorScreenPos(origin + new Vector2(searchWidth + copyWidth + gap * 2f, 0f));
        if (PillButton.Draw("##palb_log_clear", clearLabel, clearArmed ? Styling.AccentRose : Styling.TextSecondary,
                clearArmed ? PillButton.Emphasis.Tinted : PillButton.Emphasis.Ghost, FontAwesomeIcon.Eraser,
                enabled: bufferedCount > 0, height: Layout.ActionPillHeight))
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
            var label = Loc.T(chipLabels[index]);
            var count = levelTotals[(int)level].ToString(Loc.Culture);
            var chipWidth = FilterChip.Width(label, count);
            WrapIfNeeded(ref x, ref y, chipWidth, origin.X, width, height, gap);

            ImGui.SetCursorScreenPos(new Vector2(x, y));
            if (FilterChip.Draw(chipIds[index], label, count, LevelColor(level), builtFilter.Shows(level), height, tooltip: Loc.T(L.Log.LevelTooltip)))
            {
                ToggleLevel(level, ImGui.GetIO().KeyShift);
            }

            x += chipWidth + gap;
        }

        if (source is not null)
        {
            var label = Loc.T(L.Log.SourceChip, source);
            var chipWidth = FilterChip.Width(label, null, FontAwesomeIcon.Times);
            WrapIfNeeded(ref x, ref y, chipWidth, origin.X, width, height, gap);

            ImGui.SetCursorScreenPos(new Vector2(x, y));
            if (FilterChip.Draw("##palb_log_source", label, null, Styling.AccentPink, true, height, FontAwesomeIcon.Times, Loc.T(L.Log.SourceChipTooltip)))
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

    private static Vector4 LevelColor(RunLogLevel level) => level switch
    {
        RunLogLevel.Verbose => Styling.TextDim,
        RunLogLevel.Debug   => Styling.AccentBlue,
        RunLogLevel.Warning => Styling.AccentAmber,
        RunLogLevel.Error   => Styling.AccentRose,
        _                   => Styling.AccentRed,
    };

    private static Vector4 MessageColor(RunLogLevel level) => level switch
    {
        RunLogLevel.Verbose => Styling.TextMuted,
        RunLogLevel.Debug   => Styling.TextDim,
        RunLogLevel.Warning => Styling.AccentAmberSoft,
        RunLogLevel.Error   => Styling.AccentRoseSoft,
        _                   => Styling.TextSecondary,
    };
}
