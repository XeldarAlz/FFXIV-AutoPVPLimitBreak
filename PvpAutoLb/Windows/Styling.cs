using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System.Numerics;

namespace PvpAutoLb.Windows;

internal static class Styling
{
    public static readonly Vector4 AccentRed        = new(0.95f, 0.25f, 0.30f, 1.00f);
    public static readonly Vector4 AccentRedBright  = new(1.00f, 0.50f, 0.55f, 1.00f);
    public static readonly Vector4 AccentPink       = new(0.95f, 0.45f, 0.78f, 1.00f);
    public static readonly Vector4 AccentMint       = new(0.46f, 0.86f, 0.66f, 1.00f);
    public static readonly Vector4 AccentMintSoft   = new(0.66f, 0.96f, 0.80f, 1.00f);
    public static readonly Vector4 AccentAmber      = new(0.92f, 0.74f, 0.34f, 1.00f);
    public static readonly Vector4 AccentAmberSoft  = new(1.00f, 0.86f, 0.52f, 1.00f);
    public static readonly Vector4 AccentRose       = new(0.93f, 0.42f, 0.50f, 1.00f);
    public static readonly Vector4 AccentRoseSoft   = new(1.00f, 0.62f, 0.68f, 1.00f);
    public static readonly Vector4 AccentBlue       = new(0.40f, 0.68f, 0.98f, 1.00f);
    public static readonly Vector4 AccentBlueSoft   = new(0.62f, 0.82f, 1.00f, 1.00f);
    public static readonly Vector4 AccentDiscord    = new(0.345f, 0.396f, 0.949f, 1.00f);
    public static readonly Vector4 AccentPatreon    = new(1.000f, 0.259f, 0.302f, 1.00f);
    public static readonly Vector4 AccentPatreonSoft = new(1.000f, 0.580f, 0.600f, 1.00f);

    public static readonly Vector4 WindowBg = new(0.050f, 0.054f, 0.076f, 0.985f);
    public static readonly Vector4 Surface0 = new(0.082f, 0.090f, 0.118f, 1.00f);
    public static readonly Vector4 Surface1 = new(0.108f, 0.118f, 0.152f, 1.00f);
    public static readonly Vector4 Surface2 = new(0.142f, 0.155f, 0.196f, 1.00f);
    public static readonly Vector4 Surface3 = new(0.180f, 0.196f, 0.244f, 1.00f);

    public static readonly Vector4 CardBgHover = new(0.142f, 0.155f, 0.196f, 0.95f);
    public static readonly Vector4 SliderBg    = new(0.160f, 0.175f, 0.220f, 1.00f);
    public static readonly Vector4 BorderDim   = new(0.235f, 0.262f, 0.330f, 1.00f);

    public static readonly Vector4 TextStrong    = new(0.965f, 0.965f, 0.975f, 1.00f);
    public static readonly Vector4 InkOnAccent   = new(0.133f, 0.047f, 0.059f, 1.00f);
    public static readonly Vector4 TextSecondary = new(0.780f, 0.800f, 0.840f, 1.00f);
    public static readonly Vector4 TextDim       = new(0.560f, 0.590f, 0.640f, 1.00f);
    public static readonly Vector4 TextMuted     = new(0.400f, 0.420f, 0.470f, 1.00f);

    public static readonly Vector4 Hairline  = new(1f, 1f, 1f, 0.055f);
    public static readonly Vector4 Highlight = new(1f, 1f, 1f, 0.075f);

    public const float WindowRounding = 14f;
    public const float PanelRounding = 12f;
    public const float CardRounding = 10f;
    public const float FrameRounding = 7f;

    public const double PulseFast = 600.0;
    public const double PulseMedium = 800.0;
    public const double PulseBreath = 2600.0;
    public const double PulseOrbit = 3400.0;

    public static float Pulse(double periodMs = PulseMedium)
    {
        var phase = (Environment.TickCount % periodMs) / periodMs;
        return (float)((Math.Sin(phase * Math.PI * 2.0) + 1.0) * 0.5);
    }

    public static Vector4 PulseColor(Vector4 from, Vector4 to, double periodMs = PulseMedium)
        => Vector4.Lerp(from, to, Pulse(periodMs));

    public static float Phase(double periodMs)
        => (float)((Environment.TickCount % periodMs) / periodMs);

    public static Vector4 WithAlpha(Vector4 color, float alpha) => color with { W = alpha };

    // Only a pale accent crosses 0.65; the brand accents keep white text like the sibling plugins.
    public static Vector4 ForegroundOn(Vector4 fill) => Luminance(fill) > 0.65f ? InkOnAccent : TextStrong;

    // WCAG relative luminance of an sRGB color.
    private static float Luminance(Vector4 color) => 0.2126f * Linear(color.X) + 0.7152f * Linear(color.Y) + 0.0722f * Linear(color.Z);

    private static float Linear(float channel) => channel <= 0.04045f ? channel / 12.92f : MathF.Pow((channel + 0.055f) / 1.055f, 2.4f);

    public static Vector4 Lighten(Vector4 color, float amount) => Vector4.Lerp(color, Vector4.One, amount) with { W = color.W };

    public static Vector4 Darken(Vector4 color, float amount) => Vector4.Lerp(color, Vector4.Zero, amount) with { W = color.W };

    public static Vector4 Tint(Vector4 baseColor, Vector4 accent, float amount)
        => Vector4.Lerp(baseColor, accent, amount) with { W = baseColor.W };

    public static void VSpace(float pixels)
        => ImGui.Dummy(new Vector2(0, pixels * ImGuiHelpers.GlobalScale));

    public static void SectionLabel(string label)
    {
        using (Fonts.PushHeadline())
        using (ImRaii.PushColor(ImGuiCol.Text, TextStrong))
        {
            ImGui.TextUnformatted(label);
        }
    }

    public static IDisposable PushChrome(Vector2 windowPadding)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var style = ImRaii.PushStyle(ImGuiStyleVar.WindowRounding, WindowRounding * scale)
            .Push(ImGuiStyleVar.WindowBorderSize, 1f)
            .Push(ImGuiStyleVar.WindowPadding, windowPadding * scale)
            .Push(ImGuiStyleVar.ChildRounding, CardRounding * scale)
            .Push(ImGuiStyleVar.ChildBorderSize, 0f)
            .Push(ImGuiStyleVar.PopupRounding, CardRounding * scale)
            .Push(ImGuiStyleVar.PopupBorderSize, 1f)
            .Push(ImGuiStyleVar.FrameRounding, FrameRounding * scale)
            .Push(ImGuiStyleVar.FramePadding, new Vector2(10f, 6f) * scale)
            .Push(ImGuiStyleVar.FrameBorderSize, 0f)
            .Push(ImGuiStyleVar.ItemSpacing, new Vector2(10f, 8f) * scale)
            .Push(ImGuiStyleVar.ItemInnerSpacing, new Vector2(6f, 4f) * scale)
            .Push(ImGuiStyleVar.ScrollbarSize, 9f * scale)
            .Push(ImGuiStyleVar.ScrollbarRounding, 9f * scale)
            .Push(ImGuiStyleVar.GrabRounding, 6f * scale)
            .Push(ImGuiStyleVar.GrabMinSize, 12f * scale);

        var color = ImRaii.PushColor(ImGuiCol.WindowBg, WindowBg)
            .Push(ImGuiCol.ChildBg, Vector4.Zero)
            .Push(ImGuiCol.PopupBg, Surface1 with { W = 0.985f })
            .Push(ImGuiCol.Border, new Vector4(1f, 1f, 1f, 0.09f))
            .Push(ImGuiCol.BorderShadow, Vector4.Zero)
            .Push(ImGuiCol.FrameBg, SliderBg)
            .Push(ImGuiCol.FrameBgHovered, Surface2)
            .Push(ImGuiCol.FrameBgActive, Surface3)
            .Push(ImGuiCol.ScrollbarBg, Vector4.Zero)
            .Push(ImGuiCol.ScrollbarGrab, new Vector4(1f, 1f, 1f, 0.12f))
            .Push(ImGuiCol.ScrollbarGrabHovered, new Vector4(1f, 1f, 1f, 0.20f))
            .Push(ImGuiCol.ScrollbarGrabActive, new Vector4(1f, 1f, 1f, 0.28f))
            .Push(ImGuiCol.Button, Surface1)
            .Push(ImGuiCol.ButtonHovered, Surface2)
            .Push(ImGuiCol.ButtonActive, Tint(Surface2, AccentRed, 0.35f))
            .Push(ImGuiCol.Header, Tint(Surface1, AccentRed, 0.30f))
            .Push(ImGuiCol.HeaderHovered, Surface2)
            .Push(ImGuiCol.HeaderActive, Tint(Surface2, AccentRed, 0.40f))
            .Push(ImGuiCol.CheckMark, AccentRedBright)
            .Push(ImGuiCol.SliderGrab, AccentRed)
            .Push(ImGuiCol.SliderGrabActive, AccentRedBright)
            .Push(ImGuiCol.Text, TextStrong)
            .Push(ImGuiCol.TextDisabled, TextMuted)
            .Push(ImGuiCol.Separator, Hairline)
            .Push(ImGuiCol.ResizeGrip, Vector4.Zero)
            .Push(ImGuiCol.ResizeGripHovered, Vector4.Zero)
            .Push(ImGuiCol.ResizeGripActive, Vector4.Zero)
            .Push(ImGuiCol.TextSelectedBg, WithAlpha(AccentRed, 0.35f));

        return new ChromeScope(style, color);
    }

    private sealed class ChromeScope(IDisposable style, IDisposable color) : IDisposable
    {
        public void Dispose()
        {
            color.Dispose();
            style.Dispose();
        }
    }
}
