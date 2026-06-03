using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;

namespace PvpAutoLb.Windows.Sections;

internal static class Footer
{
    public static void Draw(AutoLbController ctrl, Configuration cfg)
    {
        ImGui.Separator();
        DrawStatsRow($"Session:  {ctrl.Stats.TotalFires:N0} fires · {ctrl.Stats.KillsAttributed:N0} kills · {ctrl.Stats.EnemiesAffectedTotal:N0} hits",
            "Reset session##stats_session", ctrl.Stats.ResetSession);
        DrawStatsRow($"Lifetime: {cfg.LifetimeFires:N0} fires · {cfg.LifetimeKills:N0} kills · {cfg.LifetimeEnemiesAffected:N0} hits",
            "Reset lifetime##stats_lifetime", ctrl.Stats.ResetLifetime);
        DrawLastFiredLine(ctrl);
    }

    private static void DrawStatsRow(string text, string buttonId, Action onReset)
    {
        ImGui.AlignTextToFramePadding();
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
            ImGui.TextUnformatted(text);

        var labelOnly = buttonId.Substring(0, buttonId.IndexOf("##", StringComparison.Ordinal));
        var btnW = ImGui.CalcTextSize(labelOnly).X + ImGui.GetStyle().FramePadding.X * 2;
        Styling.SameLineRightAligned(btnW);
        if (ImGui.Button(buttonId)) onReset();
    }

    private static void DrawLastFiredLine(AutoLbController ctrl)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextMuted))
        {
            var fired = ctrl.LastFiredUtc is { } ts
                ? $"Last fired {(DateTime.UtcNow - ts).TotalSeconds:F1}s ago"
                : "Last fired: never";
            var build = $"build {typeof(Footer).Assembly.GetName().Version}";
            ImGui.TextUnformatted(fired);
            var buildW = ImGui.CalcTextSize(build).X;
            Styling.SameLineRightAligned(buildW);
            ImGui.TextUnformatted(build);
        }
    }
}
