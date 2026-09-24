using System;
using Dalamud.Bindings.ImGui;
using PvpAutoLb.Core;

namespace PvpAutoLb.Windows;

public sealed partial class ConsoleWindow
{
    private long anchorSequence;
    private long focusSequence;

    private bool HasSelection => anchorSequence != 0;

    private bool IsSelected(long sequence)
        => HasSelection && sequence >= Math.Min(anchorSequence, focusSequence) && sequence <= Math.Max(anchorSequence, focusSequence);

    private void ClearSelection()
    {
        anchorSequence = 0;
        focusSequence = 0;
    }

    private void SelectSingle(int index)
    {
        anchorSequence = view[index].Sequence;
        focusSequence = anchorSequence;
    }

    private void ClickRow(int index, bool extend)
    {
        var sequence = view[index].Sequence;
        if (extend && HasSelection)
        {
            focusSequence = sequence;
            return;
        }

        if (anchorSequence == sequence && focusSequence == sequence)
        {
            ClearSelection();
            return;
        }

        SelectSingle(index);
    }

    private void SelectAll()
    {
        if (viewCount == 0)
        {
            return;
        }

        anchorSequence = view[0].Sequence;
        focusSequence = view[viewCount - 1].Sequence;
    }

    private void MoveSelection(int delta, bool extend)
    {
        if (viewCount == 0)
        {
            return;
        }

        var target = HasSelection ? Math.Clamp(LowerBound(focusSequence) + delta, 0, viewCount - 1)
            : delta < 0 ? viewCount - 1 : 0;
        if (extend && HasSelection)
        {
            focusSequence = view[target].Sequence;
        }
        else
        {
            SelectSingle(target);
        }

        pendingScrollIndex = target;
        if (target < viewCount - 1 && follow)
        {
            Detach();
        }
    }

    // The view is ordered by sequence, so a selection range maps to one contiguous run of rows.
    private (int Low, int High) SelectionIndices()
    {
        if (!HasSelection)
        {
            return (0, -1);
        }

        var low = LowerBound(Math.Min(anchorSequence, focusSequence));
        var high = LowerBound(Math.Max(anchorSequence, focusSequence) + 1) - 1;
        return (low, high);
    }

    private int LowerBound(long sequence)
    {
        var low = 0;
        var high = viewCount;
        while (low < high)
        {
            var middle = (low + high) >> 1;
            if (view[middle].Sequence < sequence)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }

        return low;
    }

    private int SingleSelectionIndex()
    {
        if (!HasSelection || anchorSequence != focusSequence)
        {
            return -1;
        }

        var index = LowerBound(anchorSequence);
        return index < viewCount && view[index].Sequence == anchorSequence ? index : -1;
    }

    private void OnViewRebuilt(long previousTail, bool filterChanged)
    {
        if (filterChanged)
        {
            sourceColumn = 0f;
        }

        if (HasSelection)
        {
            var (low, high) = SelectionIndices();
            if (high < low)
            {
                ClearSelection();
            }
        }

        if (follow || viewCount == 0 || view[viewCount - 1].Sequence == previousTail)
        {
            return;
        }

        var fresh = 0;
        for (var index = viewCount - 1; index >= 0 && view[index].Sequence > detachedTail; index--)
        {
            fresh++;
        }

        newWhileDetached = fresh;
    }

    private void HandleShortcuts()
    {
        if (!ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows))
        {
            return;
        }

        var io = ImGui.GetIO();
        if (io.KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.F))
        {
            focusSearch = true;
            return;
        }

        if (io.WantTextInput || searchFocused)
        {
            return;
        }

        if (io.KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.C))
        {
            if (HasSelection)
            {
                var (low, high) = SelectionIndices();
                CopyRange(low, high);
            }
            else
            {
                CopyView();
            }

            return;
        }

        if (io.KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.A))
        {
            SelectAll();
            return;
        }

        if (ImGui.IsKeyPressed(ImGuiKey.UpArrow))
        {
            MoveSelection(-1, io.KeyShift);
        }
        else if (ImGui.IsKeyPressed(ImGuiKey.DownArrow))
        {
            MoveSelection(1, io.KeyShift);
        }
    }

    private void CopyView()
    {
        if (viewCount == 0)
        {
            return;
        }

        ImGui.SetClipboardText(RunLogReport.WithHeader(view.AsSpan(0, viewCount), builtFilter, bufferedCount));
        copiedAtMs = Environment.TickCount64;
        ShowNotice(copiedLinesText.For(viewCount));
    }

    private void CopyRange(int low, int high)
    {
        if (low < 0 || high < low || high >= viewCount)
        {
            return;
        }

        var count = high - low + 1;
        ImGui.SetClipboardText(RunLogReport.Plain(view.AsSpan(low, count)));
        ShowNotice(copiedLinesText.For(count));
    }
}
