using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class PerJobOverrideSection
{
    public static void Draw(Configuration cfg)
    {
        var jobId = JobLookup.CurrentJobId;
        if (jobId == 0)
        {
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
                ImGui.TextWrapped("Log into a job to set a per-job override. The threshold you set here applies only while that job is active.");
            return;
        }

        var jobName = JobLookup.Name(jobId);
        var hasOverride = cfg.HasJobOverride(jobId);

        if (SettingsRow.Toggle("##joboverride", $"Override for {jobName}",
                $"When on, {jobName} fires at its own threshold below instead of the global one.",
                ref hasOverride))
        {
            if (hasOverride) cfg.EnsureJobOverride(jobId);
            else cfg.ClearJobOverride(jobId);
            cfg.Save();
        }

        if (!cfg.HasJobOverride(jobId)) return;

        DrawControls(cfg, jobId);
    }

    private static void DrawControls(Configuration cfg, uint jobId)
    {
        var j = cfg.EnsureJobOverride(jobId);

        var mode = j.Mode;
        if (ThresholdWidgets.DrawModeToggle("job", ref mode, segmentHeightDip: Layout.SegmentHeightCompact))
        {
            j.Mode = mode;
            cfg.Save();
        }

        ImGui.Spacing();

        var pct = j.Percent;
        var abs = j.Absolute;
        if (ThresholdWidgets.DrawValueControl("job", j.Mode, ref pct, ref abs))
        {
            j.Percent = pct;
            j.Absolute = abs;
            cfg.SaveDebounced();
        }

        ImGui.Spacing();
        ImGui.Spacing();
        ThresholdWidgets.DrawPreview(j.Mode, j.Percent, j.Absolute);
    }
}
