using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;

namespace PvpAutoLb.Windows.Components;

internal static class HeroCard
{
    public static void Draw(IBattleChara target, bool below, Configuration cfg, LbDrawState state)
    {
        var firing = below && cfg.Enabled && state.CanFire;
        var border = ResolveBorder(below, cfg.Enabled, state.CanFire, firing);

        using (Card.Begin("##hero", Layout.HeroCardHeight * ImGuiHelpers.GlobalScale, Styling.CardBgHero, border, 1.5f))
        {
            ImGui.SetWindowFontScale(1.22f);
            ImGui.TextUnformatted(target.Name.TextValue);
            ImGui.SetWindowFontScale(1.0f);

            var distLabel = $"{Geo.DistanceToPlayer(target):F1} y";
            var distWidth = ImGui.CalcTextSize(distLabel).X;
            Styling.SameLineRightAligned(distWidth);
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
                ImGui.TextUnformatted(distLabel);

            HpBar.Draw(target.CurrentHp, target.MaxHp, HpMath.ShieldHp(target), firing, cfg, state.JobId, Layout.HpBarHeightHero);

            ImGui.Spacing();
            DrawStatusBadge(below, firing, cfg, state);
        }
    }

    private static Vector4 ResolveBorder(bool below, bool enabled, bool canFire, bool firing)
    {
        if (firing) return Styling.PulseColor(Styling.AccentRed, Styling.AccentRedBright, Styling.PulseMedium);
        if (below && canFire) return Styling.BorderWouldFire;
        if (below) return Styling.AccentAmber;
        if (enabled) return Styling.AccentOrange;
        return Styling.CardBorderDim;
    }

    private static void DrawStatusBadge(bool below, bool firing, Configuration cfg, LbDrawState state)
    {
        FontAwesomeIcon icon;
        Vector4 color;
        string text;
        if (firing)
        {
            icon = FontAwesomeIcon.BoltLightning;
            color = Styling.PulseColor(Styling.AccentRed, Styling.AccentRedBright, Styling.PulseFast);
            text = "Firing — target below threshold";
        }
        else if (below && !cfg.Enabled)
        {
            icon = FontAwesomeIcon.Pause;
            color = Styling.BadgeWouldFire;
            text = "Paused — would fire if plugin enabled";
        }
        else if (below && state.IsSupport)
        {
            icon = FontAwesomeIcon.InfoCircle;
            color = Styling.AccentAmber;
            text = state.ModeBlurb;
        }
        else if (below && !state.ActionReady)
        {
            icon = FontAwesomeIcon.Hourglass;
            color = Styling.AccentAmber;
            text = "Below threshold — LB not ready (gauge / cooldown)";
        }
        else if (cfg.Enabled)
        {
            icon = FontAwesomeIcon.Hourglass;
            color = Styling.AccentOrange;
            text = cfg.FormatEffective(state.JobId, "Waiting — drop below ");
        }
        else
        {
            icon = FontAwesomeIcon.Pause;
            color = Styling.TextMuted;
            text = "Plugin disabled";
        }

        using (ImRaii.PushFont(UiBuilder.IconFont))
        using (ImRaii.PushColor(ImGuiCol.Text, color))
            ImGui.TextUnformatted(icon.ToIconString());
        ImGui.SameLine();
        using (ImRaii.PushColor(ImGuiCol.Text, color))
            ImGui.TextUnformatted(text);
    }
}
