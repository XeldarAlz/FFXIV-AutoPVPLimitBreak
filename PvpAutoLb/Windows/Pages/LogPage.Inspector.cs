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
    private const float InspectorShare = 0.34f;
    private const float InspectorMinHeight = 120f;
    private const float InspectorMaxHeight = 300f;
    private const float CardPadX = 14f;
    private const float CardPadY = 10f;
    private const float HeaderGap = 8f;
    private const float StatusGap = 8f;
    private const float InspectorIconButton = 26f;
    private const float InspectorWrapSlack = 6f;
    private const string StampFormat = "yyyy-MM-dd HH:mm:ss.fff";
    private const string InspectorDetailId = "##palb_log_detail";
    private const string DetailSeparator = "\n\n";

    private long inspectedSequence;
    private int inspectedRepeat;
    private float inspectedWidth;
    private float inspectedFontSize;
    private string inspectedStamp = string.Empty;
    private string inspectedText = string.Empty;

    private float InspectorHeight()
    {
        var scale = ImGuiHelpers.GlobalScale;
        if (!HasSelection)
        {
            return 0f;
        }

        if (SingleSelectionIndex() < 0)
        {
            return (Layout.ActionPillHeight + CardPadY * 2f) * scale;
        }

        var available = ImGui.GetContentRegionAvail().Y;
        return Math.Clamp(available * InspectorShare, InspectorMinHeight * scale, InspectorMaxHeight * scale);
    }

    private void DrawInspector(float height)
    {
        if (!HasSelection)
        {
            return;
        }

        Styling.VSpace(SectionGap);
        var index = SingleSelectionIndex();
        if (index < 0)
        {
            DrawSelectionBar();
            return;
        }

        DrawLineCard(index, height);
    }

    private void DrawSelectionBar()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var origin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        var height = (Layout.ActionPillHeight + CardPadY * 2f) * scale;
        var drawList = ImGui.GetWindowDrawList();
        Paint.Surface(drawList, origin, origin + new Vector2(width, height), Styling.CardRounding * scale,
            Styling.WithAlpha(Styling.AccentRed, 0.08f), Styling.WithAlpha(Styling.AccentRed, 0.35f));

        var (low, high) = SelectionIndices();
        var label = Loc.Plural(L.Log.Selected, high - low + 1);
        var labelSize = TextDraw.Measure(label);
        TextDraw.At(label, new Vector2(origin.X + CardPadX * scale, origin.Y + (height - labelSize.Y) * 0.5f), Styling.TextStrong);

        var clearLabel = Loc.T(L.Log.ClearSelection);
        var copyLabel = Loc.T(L.Log.CopySelection);
        var clearWidth = PillButton.Width(clearLabel, FontAwesomeIcon.Times);
        var copyWidth = PillButton.Width(copyLabel, FontAwesomeIcon.Copy);
        var right = origin.X + width - CardPadX * scale;
        var buttonY = origin.Y + CardPadY * scale;

        ImGui.SetCursorScreenPos(new Vector2(right - clearWidth, buttonY));
        if (PillButton.Draw("##palb_log_sel_clear", clearLabel, Styling.TextSecondary, PillButton.Emphasis.Ghost, FontAwesomeIcon.Times,
                height: Layout.ActionPillHeight))
        {
            ClearSelection();
        }

        ImGui.SetCursorScreenPos(new Vector2(right - clearWidth - ToolbarGap * scale - copyWidth, buttonY));
        if (PillButton.Draw("##palb_log_sel_copy", copyLabel, Styling.AccentRed, PillButton.Emphasis.Tinted, FontAwesomeIcon.Copy,
                height: Layout.ActionPillHeight))
        {
            CopyRange(low, high);
        }

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(width, height));
    }

    private void DrawLineCard(int index, float height)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var line = view[index];
        var origin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        if (inspectedSequence != line.Sequence || inspectedRepeat != line.Repeat)
        {
            inspectedStamp = line.AtUtc.ToLocalTime().ToString(StampFormat, CultureInfo.InvariantCulture);
        }

        var accent = LevelColor(line.Level);
        var drawList = ImGui.GetWindowDrawList();
        Paint.Surface(drawList, origin, origin + new Vector2(width, height), Styling.CardRounding * scale,
            Styling.WithAlpha(Styling.Surface1, 0.85f), Styling.WithAlpha(accent, 0.45f));
        Paint.Fill(drawList, origin, new Vector2(origin.X + SelectionBarWidth * scale, origin.Y + height), accent, Styling.CardRounding * scale,
            ImDrawFlags.RoundCornersLeft);

        var padX = CardPadX * scale;
        var padY = CardPadY * scale;
        var buttonSize = InspectorIconButton * scale;
        var headerHeight = MathF.Max(buttonSize, TextDraw.LineHeight());
        var x = origin.X + padX;
        var headerMidY = origin.Y + padY + headerHeight * 0.5f;

        using (Fonts.PushCaption())
        {
            var tagSize = new Vector2(TextDraw.Measure(tagLabels[(int)RunLogLevel.Warning]).X + TagPadX * 2f * scale, TextDraw.LineHeight() + RowPadY * scale);
            DrawTag(drawList, line.Level, new Vector2(x, headerMidY - tagSize.Y * 0.5f), tagSize);
            x += tagSize.X + HeaderGap * scale;

            var meta = line.Repeat > 1
                ? string.Concat(inspectedStamp, " · ", line.Source, " · ", Loc.T(L.Log.Repeated, line.Repeat))
                : string.Concat(inspectedStamp, " · ", line.Source);
            var metaWidth = MathF.Max(1f, origin.X + width - padX - buttonSize * 2f - HeaderGap * scale - x);
            TextDraw.At(TextDraw.Truncate(meta, metaWidth), new Vector2(x, headerMidY - TextDraw.LineHeight() * 0.5f), Styling.TextDim);
        }

        var closeX = origin.X + width - padX - buttonSize;
        ImGui.SetCursorScreenPos(new Vector2(closeX, headerMidY - buttonSize * 0.5f));
        if (IconButton.Draw(FontAwesomeIcon.Times, "##palb_log_inspect_close", buttonSize, Styling.TextDim, Loc.T(L.Log.Close)))
        {
            ClearSelection();
        }

        ImGui.SetCursorScreenPos(new Vector2(closeX - buttonSize - HeaderGap * scale * 0.5f, headerMidY - buttonSize * 0.5f));
        if (IconButton.Draw(FontAwesomeIcon.Copy, "##palb_log_inspect_copy", buttonSize, Styling.AccentRedBright, Loc.T(L.Log.CopyLine)))
        {
            CopyRange(index, index);
        }

        var bodyTop = origin.Y + padY + headerHeight + HeaderGap * scale;
        var bodySize = new Vector2(width - padX * 2f, origin.Y + height - padY - bodyTop);
        if (bodySize.Y > 1f)
        {
            ImGui.SetCursorScreenPos(new Vector2(origin.X + padX, bodyTop));
            DrawInspectedBody(line, bodySize);
        }

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(width, height));
    }

    // The body is a read-only text box rather than drawn text so any part of a message or stack trace can be
    // selected with the mouse and copied on its own. ImGui text boxes never wrap, so the text is wrapped up front.
    private void DrawInspectedBody(in RunLogLine line, Vector2 size)
    {
        using var font = Fonts.PushCaption();
        RefreshInspected(line, size.X - ImGui.GetStyle().ScrollbarSize - InspectorWrapSlack * ImGuiHelpers.GlobalScale);
        using var colors = ImRaii.PushColor(ImGuiCol.FrameBg, Vector4.Zero)
            .Push(ImGuiCol.Text, line.Detail is null ? MessageColor(line.Level) : Styling.TextSecondary);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FramePadding, Vector2.Zero);
        ImGui.InputTextMultiline(InspectorDetailId, ref inspectedText, inspectedText.Length + 1, size, ImGuiInputTextFlags.ReadOnly);
    }

    private void RefreshInspected(in RunLogLine line, float wrapWidth)
    {
        var fontSize = ImGui.GetFontSize();
        if (inspectedSequence == line.Sequence && inspectedRepeat == line.Repeat && inspectedWidth == wrapWidth && inspectedFontSize == fontSize)
        {
            return;
        }

        inspectedSequence = line.Sequence;
        inspectedRepeat = line.Repeat;
        inspectedWidth = wrapWidth;
        inspectedFontSize = fontSize;
        var text = line.Detail is null
            ? line.Message
            : string.Concat(line.Message, DetailSeparator, line.Detail);
        inspectedText = TextWrap.Hard(text, wrapWidth);
    }

    private static float StatusLineHeight()
    {
        using (Fonts.PushCaption())
        {
            return TextDraw.LineHeight();
        }
    }

    private void DrawStatus()
    {
        var scale = ImGuiHelpers.GlobalScale;
        Styling.VSpace(StatusGap);
        var origin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        var now = Environment.TickCount64;
        var right = origin.X + width;
        float lineHeight;

        using (Fonts.PushCaption())
        {
            lineHeight = TextDraw.LineHeight();
            float leftEnd;
            if (notice is not null && now - noticeAtMs < NoticeMs)
            {
                var iconSize = TextDraw.IconSize(FontAwesomeIcon.Check);
                TextDraw.Icon(FontAwesomeIcon.Check, new Vector2(origin.X, origin.Y + (lineHeight - iconSize.Y) * 0.5f), Styling.AccentMint);
                var noticeX = origin.X + iconSize.X + HeaderGap * scale;
                TextDraw.At(notice, new Vector2(noticeX, origin.Y), Styling.AccentMintSoft);
                leftEnd = noticeX + TextDraw.Measure(notice).X;
            }
            else
            {
                leftEnd = DrawShowing(origin);
            }

            var hint = Loc.T(L.Log.Shortcuts);
            var hintWidth = TextDraw.Measure(hint).X;
            if (right - hintWidth > leftEnd + ColumnGap * scale * 2f)
            {
                TextDraw.At(hint, new Vector2(right - hintWidth, origin.Y), Styling.TextMuted);
            }
        }

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(width, lineHeight));
    }

    private float DrawShowing(Vector2 origin)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var showing = Loc.T(L.Log.Showing, viewCount.ToString("N0", Loc.Culture), bufferedCount.ToString("N0", Loc.Culture));
        TextDraw.At(showing, origin, Styling.TextDim);
        var end = origin.X + TextDraw.Measure(showing).X;
        if (!builtFilter.IsNarrowed)
        {
            return end;
        }

        var reset = Loc.T(L.Log.ResetFilters);
        var resetSize = TextDraw.Measure(reset);
        var resetMin = new Vector2(end + ColumnGap * scale, origin.Y);
        ImGui.SetCursorScreenPos(resetMin);
        var hit = Hit.Area("##palb_log_reset", resetSize);
        var color = hit.Hovered ? Styling.AccentRedBright : Styling.AccentRed;
        TextDraw.At(reset, resetMin, color);
        if (hit.Hovered)
        {
            Paint.Hairline(ImGui.GetWindowDrawList(), resetMin + new Vector2(0f, resetSize.Y), resetMin + resetSize);
        }

        if (hit.Clicked)
        {
            ResetFilters();
        }

        return resetMin.X + resetSize.X;
    }
}
