using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;

namespace PvpAutoLb.Windows;

internal static class Motion
{
    private const float MaxDeltaTime = 0.05f;
    private const float SettleThreshold = 0.0005f;
    private const float HoverSpeed = 18f;

    private static readonly Dictionary<int, float> values = new();

    public static bool Reduced => Plugin.PluginInterface.UiBuilder.ShouldUseReducedMotion;

    public static int Key(string id) => unchecked((int)ImGui.GetID(id));

    public static int Key(string id, int salt) => HashCode.Combine(ImGui.GetID(id), salt);

    public static float Approach(int key, float target, float speed)
    {
        if (Reduced || !values.TryGetValue(key, out var current))
        {
            values[key] = target;
            return target;
        }

        var deltaTime = MathF.Min(ImGui.GetIO().DeltaTime, MaxDeltaTime);
        var next = current + (target - current) * (1f - MathF.Exp(-speed * deltaTime));
        if (MathF.Abs(next - target) < SettleThreshold)
        {
            next = target;
        }

        values[key] = next;
        return next;
    }

    public static float Hover(int key, bool hovered) => Approach(key, hovered ? 1f : 0f, HoverSpeed);

    public static float EaseOutCubic(float progress)
    {
        var remaining = 1f - progress;
        return 1f - remaining * remaining * remaining;
    }

    public static float Smoothstep(float progress) => progress * progress * (3f - 2f * progress);

    public static float Wave(double periodMs) => MathF.Sin(Styling.Phase(periodMs) * MathF.PI * 2f);
}
