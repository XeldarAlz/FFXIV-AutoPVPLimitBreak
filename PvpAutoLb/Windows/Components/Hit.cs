using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace PvpAutoLb.Windows.Components;

internal static class Hit
{
    public readonly record struct Result(bool Clicked, bool Hovered, bool Held);

    public static Result Area(string id, Vector2 size, bool enabled = true)
    {
        size = new Vector2(MathF.Max(1f, size.X), MathF.Max(1f, size.Y));
        if (!enabled)
        {
            ImGui.Dummy(size);
            return default;
        }

        var clicked = ImGui.InvisibleButton(id, size);
        var hovered = ImGui.IsItemHovered();
        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        return new Result(clicked, hovered, ImGui.IsItemActive());
    }
}
