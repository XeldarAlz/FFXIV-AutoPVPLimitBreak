using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;

namespace PvpAutoLb.Windows.Components;

// One glanceable line answering "what is the plugin about to do?" — and, just as importantly, "why is
// nothing happening?" (not in PvP, this duty turned off, no LB on this job). The HeroCard never sees those
// cases because it only draws when there are candidates, so this is the window-level anchor.
internal static class StatusLine
{
    private readonly record struct Status(FontAwesomeIcon Icon, Vector4 Color, string Text, bool Pulse);

    public static void Draw(Configuration cfg, AutoLbController ctrl, LbDrawState state)
    {
        var s = Resolve(cfg, ctrl, state);
        var bg = Vector4.Lerp(Styling.CardBgSoft, s.Color, 0.10f);
        var height = ImGui.GetTextLineHeight() + 16f * ImGuiHelpers.GlobalScale;

        using (Card.Begin("##statusline", height, bg, s.Color, s.Pulse ? 1.5f : 1f))
        {
            using (ImRaii.PushFont(UiBuilder.IconFont))
            using (ImRaii.PushColor(ImGuiCol.Text, s.Color))
                ImGui.TextUnformatted(s.Icon.ToIconString());
            ImGui.SameLine();
            using (ImRaii.PushColor(ImGuiCol.Text, s.Color))
                ImGui.TextUnformatted(s.Text);
        }
    }

    private static Status Resolve(Configuration cfg, AutoLbController ctrl, LbDrawState state)
    {
        if (!cfg.Enabled)
            return new(FontAwesomeIcon.Pause, Styling.TextMuted, "Auto-fire is off — turn it on above to begin.", false);

        if (state.JobId == 0)
            return new(FontAwesomeIcon.Hourglass, Styling.TextDim, "Standing by — waiting for your character.", false);

        var duty = DutyDetector.Current();
        if (duty == DutyMask.None)
            return new(FontAwesomeIcon.Satellite, Styling.TextDim, "Not in a PvP duty — standing by.", false);

        if ((cfg.EnabledDuties & duty) == 0)
            return new(FontAwesomeIcon.Filter, Styling.AccentAmber,
                $"Off for {DutyName(duty)} — see Filters in Settings.", false);

        if (state.ActionId == 0)
            return new(FontAwesomeIcon.Ban, Styling.TextDim, "This job has no PvP Limit Break.", false);

        if (state.IsSupport)
            return new(FontAwesomeIcon.InfoCircle, Styling.AccentAmber, "Defensive LB — not auto-fired.", false);

        var target = ctrl.LastResolvedTarget;
        var threshold = cfg.FormatEffective(state.JobId, string.Empty).Trim();

        if (target == null || target.IsDead)
            return new(FontAwesomeIcon.Bullseye, Styling.AccentOrange,
                $"Armed — waiting for a hostile below {threshold}.", false);

        var name = Truncate(target.Name.TextValue, 18);
        var hpPct = target.MaxHp == 0 ? 0f : target.CurrentHp * 100f / target.MaxHp;

        if (!HpMath.IsBelowThreshold(target, cfg, state.JobId))
            return new(FontAwesomeIcon.Hourglass, Styling.AccentOrange,
                $"Watching {name} ({hpPct:F0}%) — fires below {threshold}.", false);

        if (!state.ActionReady)
        {
            var why = state.Readiness == LbReadyReason.OutOfRange ? "out of range" : "gauge not full";
            return new(FontAwesomeIcon.Hourglass, Styling.AccentAmber,
                $"{name} below threshold — LB not ready ({why}).", false);
        }

        return new(FontAwesomeIcon.BoltLightning,
            Styling.PulseColor(Styling.AccentRed, Styling.AccentRedBright, Styling.PulseFast),
            $"Firing on {name} at {hpPct:F0}%.", true);
    }

    private static string DutyName(DutyMask duty) => duty switch
    {
        DutyMask.CrystallineConflict => "Crystalline Conflict",
        DutyMask.Frontline => "Frontline",
        DutyMask.RivalWings => "Rival Wings",
        DutyMask.CustomMatch => "Custom Match",
        _ => "this mode",
    };

    private static string Truncate(string name, int max)
        => name.Length <= max ? name : name[..(max - 1)] + "…";
}
