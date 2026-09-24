using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace PvpAutoLb.Windows;

internal static class Motion
{
    public const float SwitchMs = 240f;
    public const float SwitchSlide = 10f;

    private readonly record struct Switch(int State, long Tick);

    private static readonly Dictionary<int, float> values = new();
    private static readonly Dictionary<int, Switch> switches = new();

    public static bool Reduced => Plugin.PluginInterface.UiBuilder.ShouldUseReducedMotion;

    public static float DeltaTime => MathF.Min(ImGui.GetIO().DeltaTime, 0.05f);

    public static int Key(string id) => unchecked((int)ImGui.GetID(id));

    public static int Key(string id, int salt) => HashCode.Combine(ImGui.GetID(id), salt);

    public static int Key(string id, uint salt) => HashCode.Combine(ImGui.GetID(id), salt);

    public static float Approach(int key, float target, float speed = 14f)
    {
        if (Reduced || !values.TryGetValue(key, out var current))
        {
            values[key] = target;
            return target;
        }

        var next = current + (target - current) * (1f - MathF.Exp(-speed * DeltaTime));
        if (MathF.Abs(next - target) < 0.0005f)
        {
            next = target;
        }

        values[key] = next;
        return next;
    }

    public static float Hover(int key, bool hovered) => Approach(key, hovered ? 1f : 0f, 18f);

    public static float Reveal(long startedTick, float durationMs, float delayMs = 0f)
    {
        if (Reduced)
        {
            return 1f;
        }
        var elapsed = Environment.TickCount64 - startedTick - delayMs;
        return EaseOutCubic(Math.Clamp(elapsed / durationMs, 0f, 1f));
    }

    public static float Transition(int key, bool state, float durationMs = SwitchMs) => Transition(key, state ? 1 : 0, durationMs);

    public static float Transition(int key, int state, float durationMs = SwitchMs)
    {
        if (!switches.TryGetValue(key, out var current))
        {
            switches[key] = new Switch(state, 0L);
            return 1f;
        }

        if (current.State != state)
        {
            current = new Switch(state, Environment.TickCount64);
            switches[key] = current;
        }

        return Reveal(current.Tick, durationMs);
    }

    public static ImRaii.StyleDisposable PushAlpha(float progress)
        => ImRaii.PushStyle(ImGuiStyleVar.Alpha, MathF.Max(0.001f, progress * ImGui.GetStyle().Alpha));

    public static ImRaii.StyleDisposable PushReveal(float progress, float slide = SwitchSlide)
    {
        if (progress < 1f)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (1f - progress) * slide * ImGuiHelpers.GlobalScale);
        }

        return PushAlpha(progress);
    }

    public static ImRaii.StyleDisposable PushSwitch(string id, bool state, float durationMs = SwitchMs, float slide = SwitchSlide)
        => PushReveal(Transition(Key(id), state, durationMs), slide);

    public static ImRaii.StyleDisposable PushSwitch(string id, int state, float durationMs = SwitchMs, float slide = SwitchSlide)
        => PushReveal(Transition(Key(id), state, durationMs), slide);

    public static ImRaii.StyleDisposable? PushSection(string id, bool shown, float durationMs = SwitchMs, float slide = SwitchSlide)
    {
        var progress = Transition(Key(id), shown, durationMs);
        return shown ? PushReveal(progress, slide) : null;
    }

    public static float EaseOutCubic(float progress)
    {
        var remaining = 1f - progress;
        return 1f - remaining * remaining * remaining;
    }

    public static float EaseInOutCubic(float progress)
        => progress < 0.5f ? 4f * progress * progress * progress : 1f - MathF.Pow(-2f * progress + 2f, 3f) * 0.5f;

    public static float Smoothstep(float t) => t * t * (3f - 2f * t);

    public static float Wave(double periodMs) => MathF.Sin(Styling.Phase(periodMs) * MathF.PI * 2f);
}
