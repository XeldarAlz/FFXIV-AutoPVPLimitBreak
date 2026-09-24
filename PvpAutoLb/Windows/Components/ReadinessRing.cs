using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using PvpAutoLb.Windows.Sections;

namespace PvpAutoLb.Windows.Components;

// The LB gauge as a ring around the Limit Break's own icon: amber while it charges, mint once it can fire,
// and a breathing red ripple while the plugin is actually firing it.
internal static class ReadinessRing
{
    private const double RippleMs = 1400.0;
    private const float RippleGrowth = 0.28f;
    private const float FillSpeed = 7f;

    public static void Draw(string id, Vector2 center, float radius, float thickness, LiveSnapshot snapshot)
    {
        var drawList = ImGui.GetWindowDrawList();
        var scale = ImGuiHelpers.GlobalScale;
        var fill = Motion.Approach(Motion.Key(id), snapshot.RingFill, FillSpeed);
        var accent = RingAccent(snapshot);
        var ready = snapshot.LimitBreakReady;

        if (ready)
        {
            var intensity = snapshot.Firing ? 1.4f + Styling.Pulse(Styling.PulseFast) : 0.7f + 0.5f * Styling.Pulse(Styling.PulseBreath);
            ProgressRing.Glow(center, radius, accent, intensity);
            DrawRipple(drawList, center, radius, accent, scale);
        }

        ProgressRing.Disc(center, radius - thickness * 0.5f, Styling.WithAlpha(Styling.Surface0, 0.95f));
        ProgressRing.Track(center, radius, thickness, Styling.WithAlpha(Styling.BorderDim, 0.7f));
        ProgressRing.Fill(center, radius, thickness, fill, accent);
        if (snapshot.Firing)
        {
            ProgressRing.Sweep(center, radius, thickness, Styling.AccentRedBright, Styling.PulseOrbit, MathF.PI * 0.6f, 1f);
        }

        var iconHalf = (radius - thickness * 1.6f) * 0.72f;
        var iconMin = center - new Vector2(iconHalf, iconHalf);
        var iconMax = center + new Vector2(iconHalf, iconHalf);
        var alpha = ready ? 1f : 0.55f + 0.45f * fill;
        if (!GameIcon.Draw(drawList, snapshot.IconId, iconMin, iconMax, iconHalf * 0.3f, alpha))
        {
            ProgressRing.CenterIcon(center, snapshot.ActionId == 0 ? FontAwesomeIcon.Ban : FontAwesomeIcon.Bolt, Styling.WithAlpha(accent, alpha), iconHalf * 1.2f);
        }
    }

    public static Vector4 RingAccent(LiveSnapshot snapshot)
    {
        if (snapshot.Firing)
        {
            return Styling.PulseColor(Styling.AccentRed, Styling.AccentRedBright, Styling.PulseFast);
        }

        if (snapshot.ActionId == 0)
        {
            return Styling.TextMuted;
        }

        return snapshot.LimitBreakReady ? Styling.AccentMint : Styling.AccentAmber;
    }

    private static void DrawRipple(ImDrawListPtr drawList, Vector2 center, float radius, Vector4 accent, float scale)
    {
        if (Motion.Reduced)
        {
            return;
        }

        var phase = Styling.Phase(RippleMs);
        var rippleRadius = radius * (1f + RippleGrowth * phase);
        drawList.AddCircle(center, rippleRadius, Paint.Col(Styling.WithAlpha(accent, 0.45f * (1f - phase))), 64, 1.6f * scale);
    }
}
