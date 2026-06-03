using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace PvpAutoLb.Windows;

internal static class Styling
{
    public static readonly Vector4 AccentRed       = new(0.95f, 0.25f, 0.30f, 1.00f);
    public static readonly Vector4 AccentRedBright = new(1.00f, 0.50f, 0.55f, 1.00f);
    public static readonly Vector4 AccentOrange    = new(0.98f, 0.62f, 0.24f, 1.00f);
    public static readonly Vector4 AccentAmber     = new(0.90f, 0.72f, 0.30f, 1.00f);
    public static readonly Vector4 AccentGreen     = new(0.34f, 0.78f, 0.44f, 1.00f);
    public static readonly Vector4 AccentBlue      = new(0.38f, 0.62f, 0.95f, 1.00f);
    public static readonly Vector4 AccentViolet    = new(0.68f, 0.52f, 0.92f, 1.00f);

    public static readonly Vector4 CardBg          = new(0.08f, 0.09f, 0.11f, 0.82f);
    public static readonly Vector4 CardBgHero      = new(0.12f, 0.08f, 0.09f, 0.90f);
    public static readonly Vector4 CardBgSoft      = new(0.10f, 0.11f, 0.13f, 0.60f);
    public static readonly Vector4 CardBorderDim   = new(0.22f, 0.24f, 0.28f, 1.00f);
    public static readonly Vector4 BorderWouldFire = new(0.45f, 0.22f, 0.24f, 1.00f);
    public static readonly Vector4 BadgeWouldFire  = new(0.75f, 0.45f, 0.48f, 1.00f);

    public static readonly Vector4 TextStrong      = new(0.96f, 0.96f, 0.96f, 1.00f);
    public static readonly Vector4 TextSecondary   = new(0.78f, 0.78f, 0.82f, 1.00f);
    public static readonly Vector4 TextDim         = new(0.55f, 0.55f, 0.60f, 1.00f);
    public static readonly Vector4 TextMuted       = new(0.40f, 0.40f, 0.44f, 1.00f);

    public const double PulseFast = 600.0;
    public const double PulseMedium = 800.0;
    public const double PulseDefault = 900.0;

    public static float Pulse(double periodMs = PulseDefault)
    {
        var t = (Environment.TickCount % periodMs) / periodMs;
        return (float)((Math.Sin(t * Math.PI * 2.0) + 1.0) * 0.5);
    }

    public static Vector4 PulseColor(Vector4 a, Vector4 b, double periodMs = PulseDefault)
        => Vector4.Lerp(a, b, Pulse(periodMs));

    public static IDisposable PushCardStyle()
    {
        var p = ImRaii.PushStyle(ImGuiStyleVar.ChildRounding, 6f * ImGuiHelpers.GlobalScale);
        p.Push(ImGuiStyleVar.ChildBorderSize, 1f);
        p.Push(ImGuiStyleVar.WindowPadding, new Vector2(10, 8) * ImGuiHelpers.GlobalScale);
        return p;
    }

    public static void SectionLabel(string label)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, TextDim))
            ImGui.TextUnformatted(label.ToUpperInvariant());
    }

    public static void SameLineRightAligned(float itemWidth)
        => ImGui.SameLine(ImGui.GetContentRegionAvail().X + ImGui.GetCursorPosX() - itemWidth);
}
