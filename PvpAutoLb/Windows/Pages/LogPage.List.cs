using PvpAutoLb.Core;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System.Globalization;
using System.Numerics;

namespace PvpAutoLb.Windows.Pages;

internal sealed partial class LogPage
{
    private readonly record struct Columns(float Time, float Tag, float Source, float MessageX);

    private struct RowText
    {
        public long Sequence;
        public int Repeat;
        public float MessageWidth;
        public float FontSize;
        public string Time;
        public string Message;
        public string? RepeatLabel;
    }

    private const int RowCacheSize = 256;
    private const float ListPadX = 6f;
    private const float ListPadY = 6f;
    private const float RowPadX = 10f;
    private const float RowPadY = 4f;
    private const float ColumnGap = 12f;
    private const float TagPadX = 6f;
    private const float MaxSourceColumn = 150f;
    private const float SelectionBarWidth = 2.5f;
    private const float TrailingGap = 8f;
    private const float JumpInset = 12f;
    private const string ListId = "##palb_log_list";
    private const string MenuId = "##palb_log_menu";
    private const string TimeSample = "00:00:00.000";
    private const string MultilineMark = " …";

    private static readonly string[] tagLabels = ["VERB", "DBUG", "INFO", "WARN", "ERR"];

    private readonly RowText[] rowCache = new RowText[RowCacheSize];
    private readonly Dictionary<string, float> sourceWidths = new(StringComparer.Ordinal);

    private bool follow = true;
    private float lastScrollMax;
    private long detachedTail;
    private int newWhileDetached;
    private int pendingScrollIndex = -1;
    private int contextIndex = -1;
    private float sourceColumn;
    private float measuredFontSize;

    private void DrawList(float height)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var width = ImGui.GetContentRegionAvail().X;
        var origin = ImGui.GetCursorScreenPos();
        Paint.Surface(ImGui.GetWindowDrawList(), origin, origin + new Vector2(width, height), Styling.CardRounding * scale,
            Styling.WithAlpha(Styling.Surface0, 0.7f), Styling.WithAlpha(Styling.BorderDim, 0.5f));

        using (ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(ListPadX, ListPadY) * scale))
        using (var child = ImRaii.Child(ListId, new Vector2(width, height), false, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.AlwaysUseWindowPadding))
        {
            if (!child)
            {
                return;
            }

            using (Fonts.PushCaption())
            {
                if (viewCount == 0)
                {
                    DrawEmpty();
                    return;
                }

                UpdateFollow();
                DrawRows();
            }
        }
    }

    private void UpdateFollow()
    {
        var scrollY = ImGui.GetScrollY();
        var scrollMax = ImGui.GetScrollMaxY();
        var wheel = ImGui.GetIO().MouseWheel;
        if (follow)
        {
            var scrolledUp = ImGui.IsWindowHovered() && wheel > 0f;
            var movedWithoutGrowth = scrollMax <= lastScrollMax + 0.5f && scrollY < scrollMax - 2f;
            if (scrolledUp || movedWithoutGrowth)
            {
                Detach();
            }
        }
        else if (scrollY >= scrollMax - 2f && wheel <= 0f && pendingScrollIndex < 0)
        {
            follow = true;
        }
    }

    private void Detach()
    {
        follow = false;
        detachedTail = viewCount > 0 ? view[viewCount - 1].Sequence : 0;
        newWhileDetached = 0;
    }

    private void DrawRows()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var lineHeight = TextDraw.LineHeight();
        var rowHeight = lineHeight + RowPadY * 2f * scale;
        var contentOrigin = ImGui.GetCursorScreenPos();
        var innerWidth = ImGui.GetContentRegionAvail().X;
        var scrollY = ImGui.GetScrollY();
        var visibleHeight = ImGui.GetWindowHeight();
        var first = Math.Clamp((int)(scrollY / rowHeight) - 1, 0, viewCount);
        var last = Math.Min(viewCount, first + (int)(visibleHeight / rowHeight) + 3);

        if (measuredFontSize != ImGui.GetFontSize())
        {
            measuredFontSize = ImGui.GetFontSize();
            sourceWidths.Clear();
            sourceColumn = 0f;
        }

        for (var index = first; index < last; index++)
        {
            sourceColumn = MathF.Max(sourceColumn, SourceWidth(view[index].Source));
        }

        var columns = MeasureColumns(scale);
        var hoveredIndex = HoveredRow(contentOrigin, innerWidth, rowHeight);
        var drawList = ImGui.GetWindowDrawList();
        for (var index = first; index < last; index++)
        {
            var rowMin = new Vector2(contentOrigin.X, contentOrigin.Y + index * rowHeight);
            DrawRow(drawList, index, rowMin, new Vector2(innerWidth, rowHeight), columns, index == hoveredIndex);
        }

        DrawJumpPill(scrollY, visibleHeight, innerWidth);

        ImGui.SetCursorScreenPos(contentOrigin);
        ImGui.Dummy(new Vector2(innerWidth, viewCount * rowHeight));
        HandleRowMouse(hoveredIndex);
        ScrollAfterDraw(rowHeight, visibleHeight);
        DrawContextMenu();
        lastScrollMax = ImGui.GetScrollMaxY();
    }

    private Columns MeasureColumns(float scale)
    {
        var time = TextDraw.Measure(TimeSample).X;
        var tag = TextDraw.Measure(tagLabels[(int)RunLogLevel.Warning]).X + TagPadX * 2f * scale;
        var source = MathF.Min(sourceColumn, MaxSourceColumn * scale);
        var gap = ColumnGap * scale;
        return new Columns(time, tag, source, RowPadX * scale + time + gap + tag + gap + source + gap);
    }

    private float SourceWidth(string name)
    {
        if (sourceWidths.TryGetValue(name, out var width))
        {
            return width;
        }

        width = TextDraw.Measure(name).X;
        sourceWidths[name] = width;
        return width;
    }

    private int HoveredRow(Vector2 contentOrigin, float innerWidth, float rowHeight)
    {
        if (!ImGui.IsWindowHovered())
        {
            return -1;
        }

        var mouse = ImGui.GetMousePos();
        if (mouse.X < contentOrigin.X || mouse.X > contentOrigin.X + innerWidth)
        {
            return -1;
        }

        var index = (int)MathF.Floor((mouse.Y - contentOrigin.Y) / rowHeight);
        return index >= 0 && index < viewCount ? index : -1;
    }

    private void DrawRow(ImDrawListPtr drawList, int index, Vector2 rowMin, Vector2 rowSize, Columns columns, bool hovered)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var line = view[index];
        var rowMax = rowMin + rowSize;
        var selected = IsSelected(line.Sequence);
        var rounding = 5f * scale;

        if (selected)
        {
            Paint.Fill(drawList, rowMin, rowMax, Styling.WithAlpha(Styling.AccentRed, hovered ? 0.22f : 0.16f), rounding);
            Paint.Fill(drawList, rowMin, new Vector2(rowMin.X + SelectionBarWidth * scale, rowMax.Y), Styling.AccentRed, rounding);
        }
        else if (hovered)
        {
            Paint.Fill(drawList, rowMin, rowMax, Styling.WithAlpha(Styling.Surface2, 0.6f), rounding);
        }
        else if (line.Level == RunLogLevel.Error)
        {
            Paint.Fill(drawList, rowMin, rowMax, Styling.WithAlpha(Styling.AccentRose, 0.07f), rounding);
        }
        else if (line.Level == RunLogLevel.Warning)
        {
            Paint.Fill(drawList, rowMin, rowMax, Styling.WithAlpha(Styling.AccentAmber, 0.05f), rounding);
        }
        else if ((index & 1) == 1)
        {
            Paint.Fill(drawList, rowMin, rowMax, Styling.WithAlpha(Styling.Surface1, 0.3f), rounding);
        }

        var lineHeight = TextDraw.LineHeight();
        var textY = rowMin.Y + (rowSize.Y - lineHeight) * 0.5f;
        var gap = ColumnGap * scale;
        var x = rowMin.X + RowPadX * scale;
        var trailing = TrailingWidth(line, scale);
        var messageWidth = MathF.Max(1f, rowMax.X - RowPadX * scale - trailing - (rowMin.X + columns.MessageX));
        ref var text = ref RowTextFor(line, messageWidth);

        TextDraw.At(text.Time, new Vector2(x, textY), Styling.TextMuted);
        x += columns.Time + gap;

        DrawTag(drawList, line.Level, new Vector2(x, rowMin.Y + RowPadY * scale * 0.5f), new Vector2(columns.Tag, rowSize.Y - RowPadY * scale));
        x += columns.Tag + gap;

        drawList.PushClipRect(new Vector2(x, rowMin.Y), new Vector2(x + columns.Source, rowMax.Y), true);
        TextDraw.At(line.Source, new Vector2(x, textY), Styling.TextDim);
        drawList.PopClipRect();

        TextDraw.At(text.Message, new Vector2(rowMin.X + columns.MessageX, textY), MessageColor(line.Level));
        DrawTrailing(drawList, line, text.RepeatLabel, rowMax, rowSize.Y, hovered);
    }

    private static void DrawTag(ImDrawListPtr drawList, RunLogLevel level, Vector2 min, Vector2 size)
    {
        var accent = LevelColor(level);
        Paint.Fill(drawList, min, min + size, Styling.WithAlpha(accent, 0.14f), size.Y * 0.5f);
        TextDraw.Middle(tagLabels[(int)level], min, min + size, Styling.Lighten(accent, 0.15f));
    }

    private float TrailingWidth(in RunLogLine line, float scale)
    {
        var width = 0f;
        if (line.Repeat > 1)
        {
            width += RepeatPillWidth(line.Repeat, scale) + TrailingGap * scale;
        }

        if (line.Detail is not null)
        {
            width += TextDraw.IconSize(FontAwesomeIcon.AlignLeft).X + TrailingGap * scale;
        }

        return width;
    }

    private static float RepeatPillWidth(int repeat, float scale)
    {
        var digits = repeat < 10 ? 1 : repeat < 100 ? 2 : repeat < 1000 ? 3 : 4;
        return TextDraw.Measure("×").X + TextDraw.Measure("0").X * digits + TagPadX * 2f * scale;
    }

    private void DrawTrailing(ImDrawListPtr drawList, in RunLogLine line, string? repeatLabel, Vector2 rowMax, float rowHeight, bool hovered)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var right = rowMax.X - RowPadX * scale;
        var midY = rowMax.Y - rowHeight * 0.5f;

        if (repeatLabel is not null)
        {
            var width = RepeatPillWidth(line.Repeat, scale);
            var min = new Vector2(right - width, rowMax.Y - rowHeight + RowPadY * scale * 0.5f);
            var max = new Vector2(right, rowMax.Y - RowPadY * scale * 0.5f);
            Paint.Fill(drawList, min, max, Styling.WithAlpha(Styling.Surface3, 0.9f), (max.Y - min.Y) * 0.5f);
            TextDraw.Middle(repeatLabel, min, max, Styling.TextSecondary);
            if (hovered && ImGui.IsMouseHoveringRect(min, max))
            {
                Tooltip.Show(Loc.T(L.Log.Repeated, line.Repeat));
            }

            right = min.X - TrailingGap * scale;
        }

        if (line.Detail is null)
        {
            return;
        }

        var iconSize = TextDraw.IconSize(FontAwesomeIcon.AlignLeft);
        var iconMin = new Vector2(right - iconSize.X, midY - iconSize.Y * 0.5f);
        TextDraw.Icon(FontAwesomeIcon.AlignLeft, iconMin, Styling.WithAlpha(LevelColor(line.Level), 0.85f));
        if (hovered && ImGui.IsMouseHoveringRect(iconMin, iconMin + iconSize))
        {
            Tooltip.Show(Loc.T(L.Log.HasDetails));
        }
    }

    // Drawing a row needs its clock text, its message cut to the column and its repeat badge. Those only change when the
    // line repeats or the column resizes, so each is built once per line instead of once per frame.
    private ref RowText RowTextFor(in RunLogLine line, float messageWidth)
    {
        ref var cached = ref rowCache[(int)(line.Sequence & (RowCacheSize - 1))];
        var fontSize = ImGui.GetFontSize();
        if (cached.Sequence == line.Sequence && cached.Repeat == line.Repeat && cached.MessageWidth == messageWidth && cached.FontSize == fontSize)
        {
            return ref cached;
        }

        var message = line.Message;
        var newline = message.IndexOfAny(['\r', '\n']);
        if (newline >= 0)
        {
            message = string.Concat(message.AsSpan(0, newline), MultilineMark);
        }

        cached.Sequence = line.Sequence;
        cached.Repeat = line.Repeat;
        cached.MessageWidth = messageWidth;
        cached.FontSize = fontSize;
        cached.Time = RunLog.Time(line.AtUtc);
        cached.Message = TextDraw.Truncate(message, messageWidth);
        cached.RepeatLabel = line.Repeat > 1 ? string.Concat("×", line.Repeat.ToString(CultureInfo.InvariantCulture)) : null;
        return ref cached;
    }

    private void DrawJumpPill(float scrollY, float visibleHeight, float innerWidth)
    {
        if (follow)
        {
            return;
        }

        var scale = ImGuiHelpers.GlobalScale;
        var label = newWhileDetached > 0
            ? string.Concat(Loc.T(L.Log.JumpLatest), " · ", Loc.Plural(L.Log.NewLines, newWhileDetached))
            : Loc.T(L.Log.JumpLatest);
        var pillWidth = PillButton.Width(label, FontAwesomeIcon.ArrowDown);
        var pillHeight = Layout.ActionPillHeight * scale;
        var windowTop = ImGui.GetWindowPos().Y;
        var x = ImGui.GetCursorScreenPos().X + (innerWidth - pillWidth) * 0.5f;
        var y = windowTop + visibleHeight - pillHeight - JumpInset * scale;
        ImGui.SetCursorScreenPos(new Vector2(x, y));
        Paint.Shadow(ImGui.GetWindowDrawList(), new Vector2(x, y), new Vector2(x + pillWidth, y + pillHeight), pillHeight * 0.5f, 10f * scale, 0.45f);
        if (PillButton.Draw("##palb_log_jump", label, Styling.AccentRed, PillButton.Emphasis.Filled, FontAwesomeIcon.ArrowDown, height: Layout.ActionPillHeight))
        {
            follow = true;
            newWhileDetached = 0;
        }
    }

    private void HandleRowMouse(int hoveredIndex)
    {
        if (hoveredIndex < 0 || ImGui.IsAnyItemHovered())
        {
            return;
        }

        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
        {
            CopyRange(hoveredIndex, hoveredIndex);
            return;
        }

        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            ClickRow(hoveredIndex, ImGui.GetIO().KeyShift);
        }

        if (!ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            return;
        }

        if (!IsSelected(view[hoveredIndex].Sequence))
        {
            SelectSingle(hoveredIndex);
        }

        contextIndex = hoveredIndex;
        ImGui.OpenPopup(MenuId);
    }

    private void ScrollAfterDraw(float rowHeight, float visibleHeight)
    {
        if (pendingScrollIndex >= 0)
        {
            var scale = ImGuiHelpers.GlobalScale;
            var top = pendingScrollIndex * rowHeight;
            var scrollY = ImGui.GetScrollY();
            var viewport = visibleHeight - ListPadY * 2f * scale;
            if (top < scrollY)
            {
                ImGui.SetScrollY(top);
            }
            else if (top + rowHeight > scrollY + viewport)
            {
                ImGui.SetScrollY(top + rowHeight - viewport);
            }

            pendingScrollIndex = -1;
            return;
        }

        if (follow)
        {
            ImGui.SetScrollHereY(1f);
        }
    }

    private void DrawContextMenu()
    {
        using var menu = ContextMenu.Begin(MenuId);
        if (!menu.Open || contextIndex < 0 || contextIndex >= viewCount)
        {
            return;
        }

        var line = view[contextIndex];
        if (ImGui.MenuItem(Loc.T(L.Log.CopyLine)))
        {
            CopyRange(contextIndex, contextIndex);
        }

        var (low, high) = SelectionIndices();
        if (high > low && ImGui.MenuItem(string.Concat(Loc.T(L.Log.CopySelection), " (", Loc.Plural(L.Log.Selected, high - low + 1), ")")))
        {
            CopyRange(low, high);
        }

        if (ImGui.MenuItem(Loc.T(L.Log.CopyToEnd)))
        {
            CopyRange(contextIndex, viewCount - 1);
        }

        ImGui.Separator();
        if (ImGui.MenuItem(Loc.T(L.Log.OnlySource, line.Source), false, source is null))
        {
            source = line.Source;
            ClearSelection();
        }
    }

    private void DrawEmpty()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var origin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        var pad = RowPadX * scale;
        var textWidth = MathF.Max(1f, width - pad * 2f);
        var y = origin.Y + pad;

        if (bufferedCount > 0)
        {
            var noMatches = Loc.T(L.Log.NoMatches);
            TextDraw.Wrapped(noMatches, new Vector2(origin.X + pad, y), textWidth, Styling.TextDim);
            y += TextDraw.MeasureWrapped(noMatches, textWidth).Y + pad;
            ImGui.SetCursorScreenPos(new Vector2(origin.X + pad, y));
            if (PillButton.Draw("##palb_log_reset_empty", Loc.T(L.Log.ResetFilters), Styling.AccentRed, PillButton.Emphasis.Tinted, FontAwesomeIcon.Undo,
                    height: Layout.ActionPillHeight))
            {
                ResetFilters();
            }

            return;
        }

        var empty = Loc.T(L.Log.Empty);
        TextDraw.Wrapped(empty, new Vector2(origin.X + pad, y), textWidth, Styling.TextDim);
        y += TextDraw.MeasureWrapped(empty, textWidth).Y + pad * 0.6f;
        var footer = Loc.T(L.Log.Footer, PvpAutoLbConstants.LogPrefix);
        TextDraw.Wrapped(footer, new Vector2(origin.X + pad, y), textWidth, Styling.TextMuted);
        y += TextDraw.MeasureWrapped(footer, textWidth).Y;
        ImGui.Dummy(new Vector2(width, y - origin.Y));
    }
}
