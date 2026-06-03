using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace PvpAutoLb.Windows.Components;

internal static class ThresholdWidgets
{
    private const uint SamplePreviewMaxHp = 75_000u;

    public static bool DrawModeToggle(string idScope, ref ThresholdMode mode, float segmentHeightDip = Layout.SegmentHeightDefault)
    {
        var avail = ImGui.GetContentRegionAvail().X;
        var half = (avail - ImGui.GetStyle().ItemSpacing.X) / 2f;
        var size = new Vector2(half, segmentHeightDip * ImGuiHelpers.GlobalScale);

        var changed = false;
        if (SegmentedControl.DrawSegment("Percent of max", idScope + "_pct", mode == ThresholdMode.Percent, size))
        {
            mode = ThresholdMode.Percent;
            changed = true;
        }
        ImGui.SameLine();
        if (SegmentedControl.DrawSegment("Absolute HP", idScope + "_abs", mode == ThresholdMode.Absolute, size))
        {
            mode = ThresholdMode.Absolute;
            changed = true;
        }
        return changed;
    }

    public static bool DrawValueControl(string idScope, ThresholdMode mode, ref float percent, ref uint absolute)
    {
        ImGui.SetNextItemWidth(-1);
        if (mode == ThresholdMode.Percent)
        {
            var p = percent;
            if (ImGui.SliderFloat("##" + idScope + "_pctv", ref p, 1f, 99f, "%.0f%% of max HP"))
            {
                percent = p;
                return true;
            }
        }
        else
        {
            var a = (int)absolute;
            if (ImGui.DragInt("##" + idScope + "_absv", ref a, 100f, 1, 500_000, "%d HP"))
            {
                absolute = (uint)Math.Max(1, a);
                return true;
            }
        }
        return false;
    }

    // A sample HP bar with the fire threshold marked, so the value reads as "this much HP = will fire"
    // rather than an abstract number. Shared by the global and per-job pages so both look the same.
    public static void DrawPreview(ThresholdMode mode, float percent, uint absolute)
    {
        var frac = mode == ThresholdMode.Percent
            ? Math.Clamp(percent / 100f, 0.01f, 0.99f)
            : Math.Clamp((float)absolute / SamplePreviewMaxHp, 0.01f, 0.99f);

        var barHeight = Layout.PreviewBarHeight * ImGuiHelpers.GlobalScale;
        using (ImRaii.PushColor(ImGuiCol.PlotHistogram, Styling.AccentGreen))
        using (ImRaii.PushColor(ImGuiCol.FrameBg, new Vector4(0.06f, 0.07f, 0.08f, 0.90f)))
            ImGui.ProgressBar(1.0f, new Vector2(-1, barHeight), string.Empty);

        var rectMin = ImGui.GetItemRectMin();
        var rectMax = ImGui.GetItemRectMax();
        var x = rectMin.X + (rectMax.X - rectMin.X) * frac;
        var draw = ImGui.GetWindowDrawList();

        var overlay = ImGui.GetColorU32(new Vector4(Styling.AccentRed.X, Styling.AccentRed.Y, Styling.AccentRed.Z, 0.32f));
        draw.AddRectFilled(rectMin, new Vector2(x, rectMax.Y), overlay, 3f);
        draw.AddLine(new Vector2(x, rectMin.Y - 2), new Vector2(x, rectMax.Y + 2),
            ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.90f)), 1.8f);

        var markerLabel = mode == ThresholdMode.Percent ? $"{percent:F0}%" : $"{absolute:N0}";
        var labelSize = ImGui.CalcTextSize(markerLabel);
        var labelY = rectMin.Y + (barHeight - labelSize.Y) * 0.5f;
        var labelX = x - labelSize.X - 5f >= rectMin.X ? x - labelSize.X - 5f : x + 5f;
        draw.AddText(new Vector2(labelX, labelY), ImGui.GetColorU32(Styling.TextStrong), markerLabel);

        ImGui.Spacing();
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextMuted))
        {
            ImGui.TextUnformatted("empty");
            var fullW = ImGui.CalcTextSize("full").X;
            Styling.SameLineRightAligned(fullW);
            ImGui.TextUnformatted("full");
        }

        ImGui.Spacing();
        var sentence = mode == ThresholdMode.Percent
            ? $"Fires when target drops below {percent:F0}% of its max HP."
            : $"Fires when target drops below {absolute:N0} HP.";
        using (ImRaii.PushFont(UiBuilder.IconFont))
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.AccentViolet))
            ImGui.TextUnformatted(FontAwesomeIcon.InfoCircle.ToIconString());
        ImGui.SameLine();
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextSecondary))
            ImGui.TextWrapped(sentence);
    }
}
