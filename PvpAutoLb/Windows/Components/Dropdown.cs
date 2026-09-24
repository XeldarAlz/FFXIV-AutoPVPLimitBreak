using PvpAutoLb.Core.Localization;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System.Numerics;

namespace PvpAutoLb.Windows.Components;

// A hand drawn replacement for ImGui.Combo. The closed field, the panel that unfurls under it and
// every option row are painted here, so a picker carries the same gradients, accents and motion as
// the rest of the shell instead of the flat default frame.
internal static class Dropdown
{
    private const float TriggerPadX = 11f;
    private const float CaretHalf = 4.5f;
    private const float CaretThickness = 1.7f;
    private const float CaretGap = 9f;

    private const float PanelGap = 6f;
    private const float PanelPad = 6f;
    private const float PanelMaxList = 300f;
    private const float PanelCollapsed = 0.42f;
    private const float PanelShadow = 14f;
    private const float ViewportMargin = 6f;

    private const float RowPadX = 11f;
    private const float RowPadY = 8f;
    private const float RowGap = 2f;
    private const float RowRounding = 8f;
    private const float RowSlide = 9f;
    private const float RailWidth = 3f;
    private const float RailInset = 4f;
    private const float DetailGap = 3f;
    private const float CheckSize = 9f;

    private const float SearchHeight = 30f;
    private const float SearchPadX = 9f;
    private const float SearchIconGap = 7f;
    private const int FilterMaxLength = 64;

    private const float RevealMs = 200f;
    private const float StaggerMs = 14f;
    private const int StaggerRows = 10;

    private const string ListId = "##palb_dropdown_list";
    private const string RowId = "##palb_dropdown_row";
    private const string SearchId = "##palb_dropdown_search";

    private const ImGuiWindowFlags PanelFlags = ImGuiWindowFlags.NoMove
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoSavedSettings
        | ImGuiWindowFlags.NoScrollbar
        | ImGuiWindowFlags.NoScrollWithMouse;

    private sealed class State
    {
        public State(string popupId)
        {
            PopupId = popupId;
        }

        public readonly string PopupId;
        public long OpenedTick;
        public string Filter = string.Empty;
        public int Highlight;
        public bool ScrollToHighlight;
    }

    private static readonly Dictionary<string, State> states = new(StringComparer.Ordinal);

    private static int[] rowIndices = new int[64];
    private static float[] rowHeights = new float[64];

    public static bool Draw(string id, ReadOnlySpan<string> labels, ref int selected, float width,
        float panelWidth = 0f, bool searchable = false, string? searchHint = null)
        => DrawCore(id, labels, default, ref selected, width, panelWidth, searchable, searchHint);

    private static bool DrawCore(string id, ReadOnlySpan<string> labels, ReadOnlySpan<string> details,
        ref int selected, float width, float panelWidth, bool searchable, string? searchHint)
    {
        if (labels.Length == 0)
        {
            return false;
        }

        selected = Math.Clamp(selected, 0, labels.Length - 1);

        var state = StateFor(id);
        var open = ImGui.IsPopupOpen(state.PopupId);
        var size = new Vector2(width * ImGuiHelpers.GlobalScale, ImGui.GetFrameHeight());
        var origin = ImGui.GetCursorScreenPos();

        // ImGui closes a popup at end of frame when a press lands outside it, so the panel is still
        // open here on the press that dismisses it and the field reads as a toggle.
        if (DrawTrigger(id, labels[selected], origin, size, open) && !open)
        {
            ImGui.OpenPopup(state.PopupId);
            state.OpenedTick = Environment.TickCount64;
            state.Filter = string.Empty;
            state.Highlight = selected;
            state.ScrollToHighlight = true;
            open = true;
        }

        return open && DrawPanel(state, labels, details, ref selected, origin, size, panelWidth, searchable, searchHint);
    }

    private static State StateFor(string id)
    {
        if (states.TryGetValue(id, out var state))
        {
            return state;
        }

        state = new State(string.Concat(id, "_panel"));
        states[id] = state;
        return state;
    }

    private static bool DrawTrigger(string id, string label, Vector2 origin, Vector2 size, bool open)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var hit = Hit.Area(id, size);
        var hover = Motion.Hover(Motion.Key(id), hit.Hovered);
        var unfurl = Motion.Approach(Motion.Key(id, 1), open ? 1f : 0f, 16f);
        var lift = MathF.Max(hover * 0.55f, unfurl);

        var drawList = ImGui.GetWindowDrawList();
        var end = origin + size;
        var rounding = Styling.FrameRounding * scale;

        if (unfurl > 0.01f)
        {
            Paint.Glow(drawList, origin, end, rounding, Styling.AccentRed, unfurl);
        }

        var top = Styling.Tint(Vector4.Lerp(Styling.Surface2, Styling.Surface3, hover), Styling.AccentRed, unfurl * 0.22f);
        var bottom = Styling.Tint(Vector4.Lerp(Styling.Surface1, Styling.Surface2, hover), Styling.AccentRed, unfurl * 0.12f);
        if (hit.Held)
        {
            top = Styling.Darken(top, 0.10f);
            bottom = Styling.Darken(bottom, 0.10f);
        }

        Paint.Gradient(drawList, origin, end, top, bottom, rounding);
        Paint.TopLight(drawList, origin, end, rounding);
        Paint.Stroke(drawList, origin, end,
            Vector4.Lerp(Styling.WithAlpha(Styling.BorderDim, 0.85f), Styling.WithAlpha(Styling.AccentRedBright, 0.90f), lift), rounding);

        var padX = TriggerPadX * scale;
        var caretHalf = CaretHalf * scale;
        var caretCenter = new Vector2(end.X - padX - caretHalf, origin.Y + size.Y * 0.5f);
        var textLimit = caretCenter.X - caretHalf - CaretGap * scale - origin.X - padX;
        var text = TextDraw.Truncate(label, textLimit);
        var textSize = TextDraw.Measure(text);

        TextDraw.At(text, new Vector2(origin.X + padX, origin.Y + (size.Y - textSize.Y) * 0.5f),
            Vector4.Lerp(Styling.TextSecondary, Styling.TextStrong, lift));
        DrawCaret(drawList, caretCenter, caretHalf, unfurl,
            Vector4.Lerp(Styling.TextDim, Styling.AccentRedBright, lift), CaretThickness * scale);

        return hit.Hovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left);
    }

    // Flips between a down and an up chevron, passing through a flat line at the halfway point.
    private static void DrawCaret(ImDrawListPtr drawList, Vector2 center, float half, float flip, Vector4 color, float thickness)
    {
        var lift = half * 0.55f * (1f - 2f * flip);
        var left = center + new Vector2(-half, -lift);
        var middle = center + new Vector2(0f, lift);
        var right = center + new Vector2(half, -lift);
        var stroke = Paint.Col(color);
        drawList.AddLine(left, middle, stroke, thickness);
        drawList.AddLine(middle, right, stroke, thickness);
    }

    private static bool DrawPanel(State state, ReadOnlySpan<string> labels, ReadOnlySpan<string> details,
        ref int selected, Vector2 triggerOrigin, Vector2 triggerSize, float panelWidth, bool searchable, string? searchHint)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var pad = PanelPad * scale;
        var width = MathF.Max(triggerSize.X, panelWidth * scale);
        var innerWidth = width - pad * 2f;

        var rows = BuildRows(labels, details, state.Filter, innerWidth - RowPadX * 2f * scale, scale, out var listTotal);
        var searchBlock = searchable ? SearchHeight * scale + pad : 0f;
        var listHeight = MathF.Min(listTotal, PanelMaxList * scale);
        var contentHeight = pad * 2f + searchBlock + listHeight;

        var reveal = Motion.Reveal(state.OpenedTick, RevealMs);
        var height = MathF.Max(1f, contentHeight * (PanelCollapsed + (1f - PanelCollapsed) * reveal));

        ImGui.SetNextWindowPos(PanelPosition(triggerOrigin, triggerSize, width, contentHeight, height, scale));
        ImGui.SetNextWindowSize(new Vector2(width, height));

        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero)
            .Push(ImGuiStyleVar.PopupRounding, Styling.CardRounding * scale)
            .Push(ImGuiStyleVar.PopupBorderSize, 0f)
            .Push(ImGuiStyleVar.ItemSpacing, new Vector2(0f, RowGap * scale))
            .Push(ImGuiStyleVar.Alpha, MathF.Max(0.05f, reveal));
        using var colors = ImRaii.PushColor(ImGuiCol.PopupBg, Vector4.Zero);
        using var popup = ImRaii.Popup(state.PopupId, PanelFlags);
        if (!popup)
        {
            return false;
        }

        PaintPanel(scale);

        var panelOrigin = ImGui.GetWindowPos() + new Vector2(pad, pad);
        if (searchable)
        {
            DrawSearch(state, panelOrigin, innerWidth, searchHint, scale);
        }

        var listOrigin = panelOrigin + new Vector2(0f, searchBlock);
        if (rows == 0)
        {
            DrawNoMatches(state, listOrigin, innerWidth, scale);
            return false;
        }

        return DrawRows(state, labels, details, ref selected, rows, listOrigin, innerWidth, listHeight, scale);
    }

    private static Vector2 PanelPosition(Vector2 triggerOrigin, Vector2 triggerSize, float width,
        float contentHeight, float height, float scale)
    {
        var viewport = ImGui.GetMainViewport();
        var margin = ViewportMargin * scale;
        var gap = PanelGap * scale;

        var x = width > triggerSize.X ? triggerOrigin.X + triggerSize.X - width : triggerOrigin.X;
        x = MathF.Max(viewport.WorkPos.X + margin,
            MathF.Min(x, viewport.WorkPos.X + viewport.WorkSize.X - width - margin));

        var below = triggerOrigin.Y + triggerSize.Y + gap;
        var overflowsBelow = below + contentHeight > viewport.WorkPos.Y + viewport.WorkSize.Y - margin;
        var fitsAbove = triggerOrigin.Y - gap - contentHeight > viewport.WorkPos.Y + margin;
        return new Vector2(x, overflowsBelow && fitsAbove ? triggerOrigin.Y - gap - height : below);
    }

    private static void PaintPanel(float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        var min = ImGui.GetWindowPos();
        var max = min + ImGui.GetWindowSize();
        var rounding = Styling.CardRounding * scale;
        var spread = PanelShadow * scale;

        var bleed = new Vector2(spread * 3f, spread * 3f);
        drawList.PushClipRect(min - bleed, max + bleed, false);
        Paint.Shadow(drawList, min, max, rounding, spread, 0.55f);
        drawList.PopClipRect();

        Paint.Gradient(drawList, min, max, Styling.Surface2 with { W = 0.99f }, Styling.Surface0 with { W = 0.99f }, rounding);
        Paint.TopLight(drawList, min, max, rounding, 0.09f);
        Paint.Stroke(drawList, min, max, Styling.WithAlpha(Styling.AccentRed, 0.42f), rounding);
    }

    private static int BuildRows(ReadOnlySpan<string> labels, ReadOnlySpan<string> details, string filter,
        float wrapWidth, float scale, out float total)
    {
        EnsureCapacity(labels.Length);

        var uniform = RowPadY * 2f * scale + ImGui.GetTextLineHeight();
        var gap = RowGap * scale;
        var count = 0;
        total = 0f;

        using IDisposable? caption = details.IsEmpty ? null : Fonts.PushCaption();
        for (var index = 0; index < labels.Length; index++)
        {
            if (filter.Length > 0 && labels[index].IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            var height = details.IsEmpty
                ? uniform
                : uniform + DetailGap * scale + TextDraw.MeasureWrapped(details[index], wrapWidth).Y;

            rowIndices[count] = index;
            rowHeights[count] = height;
            total += count > 0 ? height + gap : height;
            count++;
        }

        if (count == 0)
        {
            total = uniform;
        }

        return count;
    }

    private static void EnsureCapacity(int count)
    {
        if (rowIndices.Length >= count)
        {
            return;
        }

        var size = Math.Max(count, rowIndices.Length * 2);
        rowIndices = new int[size];
        rowHeights = new float[size];
    }

    private static bool DrawRows(State state, ReadOnlySpan<string> labels, ReadOnlySpan<string> details,
        ref int selected, int rows, Vector2 origin, float innerWidth, float listHeight, float scale)
    {
        state.Highlight = Math.Clamp(state.Highlight, 0, rows - 1);
        var commit = HandleKeys(state, rows);

        ImGui.SetCursorScreenPos(origin);
        using var list = ImRaii.Child(ListId, new Vector2(innerWidth, listHeight), false, ImGuiWindowFlags.NoBackground);
        if (!list)
        {
            return false;
        }

        if (state.ScrollToHighlight)
        {
            ImGui.SetScrollY(HighlightScroll(state.Highlight, listHeight, RowGap * scale));
            state.ScrollToHighlight = false;
        }

        var width = ImGui.GetContentRegionAvail().X;
        var scrollY = ImGui.GetScrollY();
        var picked = -1;

        for (var row = 0; row < rows; row++)
        {
            var height = rowHeights[row];
            var localY = ImGui.GetCursorPosY();
            if (localY + height < scrollY || localY > scrollY + listHeight)
            {
                ImGui.Dummy(new Vector2(width, height));
                continue;
            }

            var option = rowIndices[row];
            var detail = details.IsEmpty ? null : details[option];
            if (DrawRow(state, row, labels[option], detail, option == selected, width, height, scale))
            {
                picked = option;
            }
        }

        if (commit)
        {
            picked = rowIndices[state.Highlight];
        }

        if (picked < 0)
        {
            return false;
        }

        ImGui.CloseCurrentPopup();
        if (picked == selected)
        {
            return false;
        }

        selected = picked;
        return true;
    }

    private static bool HandleKeys(State state, int rows)
    {
        if (ImGui.IsKeyPressed(ImGuiKey.DownArrow, true))
        {
            state.Highlight = (state.Highlight + 1) % rows;
            state.ScrollToHighlight = true;
        }
        else if (ImGui.IsKeyPressed(ImGuiKey.UpArrow, true))
        {
            state.Highlight = (state.Highlight + rows - 1) % rows;
            state.ScrollToHighlight = true;
        }

        return ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsKeyPressed(ImGuiKey.KeypadEnter);
    }

    private static float HighlightScroll(int highlight, float listHeight, float gap)
    {
        var offset = 0f;
        for (var row = 0; row < highlight; row++)
        {
            offset += rowHeights[row] + gap;
        }

        return MathF.Max(0f, offset - (listHeight - rowHeights[highlight]) * 0.5f);
    }

    private static bool DrawRow(State state, int row, string name, string? detail, bool selected,
        float width, float height, float scale)
    {
        var size = new Vector2(width, height);
        var origin = ImGui.GetCursorScreenPos();

        ImGui.PushID(row);
        var hit = Hit.Area(RowId, size);
        ImGui.PopID();

        if (hit.Hovered)
        {
            state.Highlight = row;
        }

        var hover = Motion.Hover(Motion.Key(RowId, row), hit.Hovered || state.Highlight == row);
        var appear = Motion.Reveal(state.OpenedTick, RevealMs, MathF.Min(row, StaggerRows) * StaggerMs);
        var drawList = ImGui.GetWindowDrawList();
        var min = origin + new Vector2((1f - appear) * RowSlide * scale, 0f);
        var max = min + size;

        var fill = selected
            ? Styling.WithAlpha(Styling.AccentRed, (0.17f + 0.10f * hover) * appear)
            : Styling.WithAlpha(Styling.Surface3, 0.75f * hover * appear);
        if (fill.W > 0.004f)
        {
            Paint.Fill(drawList, min, max, fill, RowRounding * scale);
        }

        var padX = RowPadX * scale;
        var padY = RowPadY * scale;
        var lineHeight = ImGui.GetTextLineHeight();
        var checkSize = CheckSize * scale;

        if (selected)
        {
            var inset = RailInset * scale;
            var railMin = new Vector2(min.X + inset, min.Y + inset);
            var railMax = new Vector2(railMin.X + RailWidth * scale, max.Y - inset);
            Paint.Fill(drawList, railMin, railMax, Styling.WithAlpha(Styling.AccentRedBright, appear), RailWidth * scale * 0.5f);
            Paint.Check(drawList, new Vector2(max.X - padX - checkSize * 0.5f, min.Y + padY + lineHeight * 0.5f),
                checkSize, Styling.WithAlpha(Styling.AccentRedBright, appear), 2f * scale);
        }

        var textLeft = min.X + padX;
        var textRight = max.X - padX - (selected ? checkSize * 2f : 0f);
        var nameColor = selected ? Styling.AccentRedBright : Vector4.Lerp(Styling.TextSecondary, Styling.TextStrong, hover);
        TextDraw.At(TextDraw.Truncate(name, textRight - textLeft), new Vector2(textLeft, min.Y + padY),
            Styling.WithAlpha(nameColor, appear));

        if (detail is not null)
        {
            using (Fonts.PushCaption())
            {
                TextDraw.Wrapped(detail, new Vector2(textLeft, min.Y + padY + lineHeight + DetailGap * scale),
                    max.X - padX - textLeft, Styling.WithAlpha(Styling.TextMuted, appear));
            }
        }

        return hit.Clicked;
    }

    private static void DrawSearch(State state, Vector2 origin, float width, string? hint, float scale)
    {
        var height = SearchHeight * scale;
        var padX = SearchPadX * scale;
        var end = origin + new Vector2(width, height);
        var drawList = ImGui.GetWindowDrawList();
        var rounding = Styling.FrameRounding * scale;

        Paint.Fill(drawList, origin, end, Styling.WithAlpha(Styling.Surface0, 0.85f), rounding);
        Paint.Stroke(drawList, origin, end, Styling.WithAlpha(Styling.BorderDim, 0.70f), rounding);

        var iconSize = TextDraw.IconSize(FontAwesomeIcon.Search);
        TextDraw.Icon(FontAwesomeIcon.Search, new Vector2(origin.X + padX, origin.Y + (height - iconSize.Y) * 0.5f), Styling.TextMuted);

        var fieldX = origin.X + padX + iconSize.X + SearchIconGap * scale;
        ImGui.SetCursorScreenPos(new Vector2(fieldX, origin.Y + (height - ImGui.GetFrameHeight()) * 0.5f));
        ImGui.SetNextItemWidth(end.X - fieldX - padX);
        if (ImGui.IsWindowAppearing())
        {
            ImGui.SetKeyboardFocusHere(0);
        }

        var filter = state.Filter;
        using (ImRaii.PushColor(ImGuiCol.FrameBg, Vector4.Zero)
            .Push(ImGuiCol.FrameBgHovered, Vector4.Zero)
            .Push(ImGuiCol.FrameBgActive, Vector4.Zero))
        {
            ImGui.InputTextWithHint(SearchId, hint ?? Loc.T(L.Settings.SearchHint), ref filter, FilterMaxLength);
        }

        if (string.Equals(filter, state.Filter, StringComparison.Ordinal))
        {
            return;
        }

        state.Filter = filter;
        state.Highlight = 0;
        state.ScrollToHighlight = true;
    }

    private static void DrawNoMatches(State state, Vector2 origin, float width, float scale)
    {
        using (Fonts.PushCaption())
        {
            var text = Loc.T(L.Common.NoMatches, state.Filter);
            var padX = RowPadX * scale;
            TextDraw.At(TextDraw.Truncate(text, width - padX * 2f), origin + new Vector2(padX, RowPadY * scale), Styling.TextMuted);
        }
    }
}
