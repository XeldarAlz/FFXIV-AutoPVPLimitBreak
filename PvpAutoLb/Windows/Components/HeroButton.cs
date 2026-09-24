using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using System.Numerics;

namespace PvpAutoLb.Windows.Components;

internal static class HeroButton
{
    private const float Rounding = 14f;
    private const float PadX = 20f;
    private const float SubGap = 24f;

    public static bool Draw(FontAwesomeIcon icon, string title, string? sublabel, Vector4 accent, bool enabled, string? disabledReason = null, float width = 0f)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var height = Layout.HeroButtonHeight * scale;
        if (width <= 0f)
        {
            width = ImGui.GetContentRegionAvail().X;
        }
        var size = new Vector2(width, height);
        var origin = ImGui.GetCursorScreenPos();

        ImGui.PushID((nint)(int)icon);
        var hit = Hit.Area("##hero", size, enabled);
        var hover = Motion.Hover(Motion.Key("##hero"), hit.Hovered);
        var press = Motion.Approach(Motion.Key("##hero", 1), hit.Held ? 1f : 0f, 30f);
        ImGui.PopID();

        var lift = enabled ? (hover * 2f - press * 2f) * scale : 0f;
        var min = origin - new Vector2(0f, lift);
        var max = min + size;
        var rounding = Rounding * scale;
        var drawList = ImGui.GetWindowDrawList();

        if (enabled)
        {
            var pulse = 0.5f + 0.5f * Styling.Pulse(Styling.PulseBreath);
            Paint.Glow(drawList, min, max, rounding, accent, 0.35f + 0.35f * pulse + hover * 0.6f);
            Paint.Shadow(drawList, min, max, rounding, 8f * scale, 0.5f);
            var top = Vector4.Lerp(Styling.Lighten(accent, 0.18f), Styling.Lighten(accent, 0.30f), hover);
            var bottom = Vector4.Lerp(Styling.Darken(accent, 0.14f), accent, hover);
            Paint.Gradient(drawList, min, max, top, bottom, rounding);
            Paint.TopLight(drawList, min, max, rounding, 0.28f);
            Paint.Stroke(drawList, min, max, Styling.WithAlpha(Styling.Lighten(accent, 0.5f), 0.55f + hover * 0.3f), rounding);
        }
        else
        {
            Paint.Fill(drawList, min, max, Styling.WithAlpha(Styling.Surface1, 0.8f), rounding);
            Paint.Stroke(drawList, min, max, Styling.WithAlpha(Styling.BorderDim, 0.7f), rounding);
        }

        var padX = PadX * scale;
        var midY = min.Y + height * 0.5f;
        var glyph = enabled ? icon : FontAwesomeIcon.Lock;
        var textColor = enabled ? Styling.ForegroundOn(accent) : Styling.TextMuted;

        var iconSize = TextDraw.IconSize(glyph);
        TextDraw.Icon(glyph, new Vector2(min.X + padX, midY - iconSize.Y * 0.5f), textColor);
        var titleX = min.X + padX + iconSize.X + 12f * scale;
        float titleEnd;
        using (Fonts.PushHeadline())
        {
            var titleSize = TextDraw.Measure(title);
            TextDraw.At(title, new Vector2(titleX, midY - titleSize.Y * 0.5f), textColor);
            titleEnd = titleX + titleSize.X;
        }

        var sub = enabled ? sublabel : disabledReason ?? sublabel;
        if (!string.IsNullOrEmpty(sub))
        {
            var clipped = TextDraw.Truncate(sub, max.X - padX - titleEnd - SubGap * scale);
            var subSize = TextDraw.Measure(clipped);
            TextDraw.At(clipped, new Vector2(max.X - padX - subSize.X, midY - subSize.Y * 0.5f),
                enabled ? Styling.WithAlpha(textColor, 0.8f) : Styling.TextDim);
        }

        return hit.Clicked;
    }
}
