using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows;

public sealed partial class AboutWindow
{
    private const string PatreonId = "##palb_about_patreon";
    private const string SupportTitle = "Made with love and care";
    private const string SupportBody = "This plugin is a one-person project, built in my free time because I love this game and its community. Keeping it updated takes a lot of those hours. If it has helped you, supporting me on Patreon means I can keep giving it that time. Thank you for being here.";
    private const string SupportButtonLabel = "Support on Patreon";
    private const string PatreonHint = "Open Patreon · right-click to copy";

    private const float SupportPad = 24f;
    private const float SupportMedallion = 30f;
    private const float SupportTextGap = 20f;
    private const float SupportTitleGap = 6f;
    private const float SupportButtonGap = 20f;
    private const float SupportButtonHeight = 52f;
    private const float ButtonGrow = 3f;
    private const float ButtonContentGap = 10f;
    private const int FloatingHearts = 9;
    private const int CometTrail = 26;
    private const double CometPeriodMs = 5200.0;
    private const double HeartbeatPeriodMs = 1400.0;
    private const int BurstHearts = 14;
    private const float BurstMs = 950f;
    private const float BurstDistance = 90f;
    private const float GoldenRatio = 0.618034f;

    private long burstTick = long.MinValue / 2;

    private void DrawSupport()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var drawList = ImGui.GetWindowDrawList();
        var origin = ImGui.GetCursorScreenPos();
        var pad = SupportPad * scale;
        var radius = SupportMedallion * scale;
        var textX = origin.X + pad + radius * 2f + SupportTextGap * scale;
        var textWidth = MathF.Max(1f, origin.X + columnWidth - pad - textX);

        float titleHeight;
        using (TextDraw.PushScale(HeadlineFontScale))
        {
            titleHeight = TextDraw.LineHeight();
        }

        var titleGap = SupportTitleGap * scale;
        var bodyHeight = TextDraw.MeasureWrapped(SupportBody, textWidth).Y;
        var textHeight = titleHeight + titleGap + bodyHeight;
        var topHeight = MathF.Max(radius * 2f, textHeight);
        var buttonHeight = SupportButtonHeight * scale;
        var height = pad + topHeight + SupportButtonGap * scale + buttonHeight + pad;
        var max = origin + new Vector2(columnWidth, height);
        var rounding = Styling.CardRounding * 1.6f * scale;

        Paint.Shadow(drawList, origin, max, rounding, 16f * scale, 0.5f);
        Paint.Gradient(drawList, origin, max, Styling.Tint(Styling.CardBgHover, Styling.AccentPatreon, 0.16f), Styling.Tint(Styling.CardBg, Styling.AccentPatreon, 0.05f), rounding);
        drawList.PushClipRect(origin, max, true);
        DrawFloatingHearts(origin, max);
        drawList.PopClipRect();
        Paint.TopLight(drawList, origin, max, rounding, 0.14f);
        Paint.Stroke(drawList, origin, max, Styling.WithAlpha(Styling.AccentPatreon, 0.35f), rounding, 1.2f * scale);
        DrawComet(drawList, origin, max, rounding);

        var medallionCenter = new Vector2(origin.X + pad + radius, origin.Y + pad + topHeight * 0.5f);
        var beat = Motion.Reduced ? 0f : Heartbeat(HeartbeatPeriodMs);
        ProgressRing.Glow(medallionCenter, radius, Styling.AccentPatreon, 0.45f + 0.7f * beat);
        drawList.AddCircleFilled(medallionCenter, radius, Paint.Col(Vector4.Lerp(Styling.CardBg with { W = 1f }, Styling.AccentPatreon, 0.30f)), 48);
        ProgressRing.Track(medallionCenter, radius, 1.5f * scale, Styling.WithAlpha(Styling.AccentPatreonSoft, 0.85f));
        ProgressRing.CenterIcon(medallionCenter, FontAwesomeIcon.Heart, Styling.AccentPatreonSoft, radius * (0.82f + 0.22f * beat));

        var textY = origin.Y + pad + (topHeight - textHeight) * 0.5f;
        using (TextDraw.PushScale(HeadlineFontScale))
        {
            TextDraw.At(SupportTitle, new Vector2(textX, textY), Styling.TextStrong);
        }

        TextDraw.Wrapped(SupportBody, new Vector2(textX, textY + titleHeight + titleGap), textWidth, Styling.TextSecondary);

        var buttonMin = new Vector2(origin.X + pad, max.Y - pad - buttonHeight);
        DrawPatreonButton(buttonMin, new Vector2(columnWidth - pad * 2f, buttonHeight));

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(columnWidth, height));
    }

    private static void DrawFloatingHearts(Vector2 min, Vector2 max)
    {
        if (Motion.Reduced)
        {
            return;
        }

        var width = max.X - min.X;
        var height = max.Y - min.Y;
        var scale = ImGuiHelpers.GlobalScale;
        for (var index = 0; index < FloatingHearts; index++)
        {
            var seed = index * GoldenRatio % 1f;
            var progress = (Styling.Phase(6200.0 + index * 870.0) + seed) % 1f;
            var drift = MathF.Sin(progress * MathF.PI * 2.6f + index) * 10f * scale;
            var position = new Vector2(min.X + width * (0.06f + 0.88f * seed) + drift, max.Y + 12f * scale - progress * (height + 24f * scale));
            var alpha = 0.16f * MathF.Sin(progress * MathF.PI);
            TextDraw.IconCentered(FontAwesomeIcon.Heart, position, Styling.WithAlpha(Styling.AccentPatreonSoft, alpha), 0.55f + 0.45f * (index * 0.37f % 1f));
        }
    }

    // A bright head with a fading tail travels the card's border. The path runs inset from the corners so the
    // trail stays close to the rounded outline without tracing each arc.
    private static void DrawComet(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding)
    {
        if (Motion.Reduced)
        {
            return;
        }

        var scale = ImGuiHelpers.GlobalScale;
        var inset = rounding * 0.29f;
        var innerMin = min + new Vector2(inset, inset);
        var innerMax = max - new Vector2(inset, inset);
        var perimeter = 2f * (innerMax.X - innerMin.X + innerMax.Y - innerMin.Y);
        var head = Styling.Phase(CometPeriodMs) * perimeter;
        var spacing = 5f * scale;
        for (var trailIndex = CometTrail - 1; trailIndex >= 0; trailIndex--)
        {
            var fade = 1f - trailIndex / (float)CometTrail;
            var point = PerimeterPoint(innerMin, innerMax, head - trailIndex * spacing, perimeter);
            drawList.AddCircleFilled(point, (1f + 1.6f * fade) * scale, Paint.Col(Styling.WithAlpha(Styling.AccentPatreonSoft, 0.85f * fade * fade)));
        }

        var headPoint = PerimeterPoint(innerMin, innerMax, head, perimeter);
        drawList.AddCircleFilled(headPoint, 7f * scale, Paint.Col(Styling.WithAlpha(Styling.AccentPatreon, 0.18f)));
    }

    private static Vector2 PerimeterPoint(Vector2 min, Vector2 max, float distance, float perimeter)
    {
        var width = max.X - min.X;
        var height = max.Y - min.Y;
        distance %= perimeter;
        if (distance < 0f)
        {
            distance += perimeter;
        }

        if (distance < width)
        {
            return new Vector2(min.X + distance, min.Y);
        }

        distance -= width;
        if (distance < height)
        {
            return new Vector2(max.X, min.Y + distance);
        }

        distance -= height;
        if (distance < width)
        {
            return new Vector2(max.X - distance, max.Y);
        }

        return new Vector2(min.X, max.Y - (distance - width));
    }

    private void DrawPatreonButton(Vector2 slot, Vector2 size)
    {
        var scale = ImGuiHelpers.GlobalScale;
        ImGui.SetCursorScreenPos(slot);
        var hit = Hit.Area(PatreonId, size);
        var hover = Motion.Hover(Motion.Key(PatreonId), hit.Hovered);
        var press = Motion.Approach(Motion.Key(PatreonId, 1), hit.Held ? 1f : 0f, 30f);
        if (hit.Hovered)
        {
            Tooltip.Show(PatreonHint);
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                ImGui.SetClipboardText(PatreonUrl);
            }
        }

        if (hit.Clicked)
        {
            OpenUrl(PatreonUrl);
            burstTick = Environment.TickCount64;
        }

        var grow = (ButtonGrow * hover - 2f * press) * scale;
        var min = slot - new Vector2(grow, grow);
        var max = slot + size + new Vector2(grow, grow);
        var rounding = (max.Y - min.Y) * 0.5f;
        var drawList = ImGui.GetWindowDrawList();
        var breath = 0.55f + 0.45f * Styling.Pulse(Styling.PulseBreath);

        for (var layer = 4; layer >= 1; layer--)
        {
            var spread = layer * 3f * scale;
            var alpha = 0.05f * layer * breath * (1f + 1.2f * hover);
            drawList.AddRectFilled(min - new Vector2(spread, spread), max + new Vector2(spread, spread),
                Paint.Col(Styling.WithAlpha(Styling.AccentPatreon, alpha)), rounding + spread);
        }

        var left = Styling.Darken(Styling.Lighten(Styling.AccentPatreon, 0.14f * hover), 0.12f * press);
        var right = Styling.Darken(Styling.Lighten(Styling.AccentPink, 0.14f * hover), 0.12f * press);
        Paint.GradientH(drawList, min, max, left, right, rounding);
        Paint.TopLight(drawList, min, max, rounding, 0.30f);
        Sheen(drawList, min, max - min, hover);
        Paint.Stroke(drawList, min, max, new Vector4(1f, 1f, 1f, 0.18f + 0.30f * hover), rounding, 1.2f * scale);

        DrawButtonContent(min, max, hover);
        DrawBurst(drawList, (min + max) * 0.5f);
    }

    private static void DrawButtonContent(Vector2 min, Vector2 max, float hover)
    {
        var scale = ImGuiHelpers.GlobalScale;
        using var font = TextDraw.PushScale(HeadlineFontScale);
        var labelSize = TextDraw.Measure(SupportButtonLabel);
        var heartSize = TextDraw.IconSize(FontAwesomeIcon.HandHoldingHeart);
        var arrowSize = TextDraw.IconSize(FontAwesomeIcon.ArrowRight);
        var gap = ButtonContentGap * scale;
        var contentWidth = heartSize.X + gap + labelSize.X + gap + arrowSize.X;
        var x = (min.X + max.X - contentWidth) * 0.5f;
        var midY = (min.Y + max.Y) * 0.5f;
        var beat = Motion.Reduced ? 0f : Heartbeat(HeartbeatPeriodMs);

        TextDraw.IconCentered(FontAwesomeIcon.HandHoldingHeart, new Vector2(x + heartSize.X * 0.5f, midY), Styling.TextStrong, 1f + 0.12f * beat);
        x += heartSize.X + gap;
        TextDraw.At(SupportButtonLabel, new Vector2(x, midY - labelSize.Y * 0.5f), Styling.TextStrong);
        x += labelSize.X + gap;
        TextDraw.Icon(FontAwesomeIcon.ArrowRight, new Vector2(x + ArrowSlide * scale * hover, midY - arrowSize.Y * 0.5f),
            Styling.WithAlpha(Styling.TextStrong, 0.55f + 0.45f * hover));
    }

    // A slanted band of light crosses the button every few seconds, and keeps crossing while it is hovered.
    private static void Sheen(ImDrawListPtr drawList, Vector2 origin, Vector2 size, float hover)
    {
        var period = hover > 0.5f ? 1400.0 : 3200.0;
        var window = hover > 0.5f ? 0.8f : 0.35f;
        var phase = Styling.Phase(period);
        if (Motion.Reduced || phase > window)
        {
            return;
        }

        var scale = ImGuiHelpers.GlobalScale;
        var sweep = phase / window;
        var slant = size.Y * 0.6f;
        var bandHalf = 18f * scale;
        var centerX = origin.X - bandHalf - slant + sweep * (size.X + slant + bandHalf * 2f);
        drawList.PushClipRect(origin, origin + size, true);
        const int strokes = 18;
        for (var stroke = -strokes; stroke <= strokes; stroke++)
        {
            var alpha = 0.18f * (1f - MathF.Abs(stroke) / (float)strokes);
            var x = centerX + stroke * bandHalf / strokes;
            drawList.AddLine(new Vector2(x + slant, origin.Y), new Vector2(x, origin.Y + size.Y), Paint.Col(new Vector4(1f, 1f, 1f, alpha)), 1.4f * scale);
        }

        drawList.PopClipRect();
    }

    private void DrawBurst(ImDrawListPtr drawList, Vector2 center)
    {
        var elapsed = Environment.TickCount64 - burstTick;
        if (Motion.Reduced || elapsed >= BurstMs)
        {
            return;
        }

        var scale = ImGuiHelpers.GlobalScale;
        var progress = elapsed / BurstMs;
        var eased = Motion.EaseOutCubic(progress);
        for (var index = 0; index < BurstHearts; index++)
        {
            var angle = index * MathF.PI * 2f / BurstHearts + index * 0.35f;
            var distance = BurstDistance * scale * eased * (0.7f + 0.3f * (index * GoldenRatio % 1f));
            var position = center + new Vector2(MathF.Cos(angle) * 1.6f, MathF.Sin(angle)) * distance;
            var color = index % 2 == 0 ? Styling.AccentPatreonSoft : Styling.Lighten(Styling.AccentPink, 0.3f);
            TextDraw.IconCentered(FontAwesomeIcon.Heart, position, Styling.WithAlpha(color, 1f - progress), 0.6f + 0.5f * (1f - progress));
        }
    }

    private static float Heartbeat(double periodMs)
    {
        var phase = Styling.Phase(periodMs);
        return MathF.Max(Bump(phase, 0.06f, 0.06f), Bump(phase, 0.20f, 0.06f) * 0.6f);
    }

    private static float Bump(float phase, float center, float width)
    {
        var distance = (phase - center) / width;
        if (distance < -1f || distance > 1f)
        {
            return 0f;
        }

        return 0.5f * (1f + MathF.Cos(distance * MathF.PI));
    }
}
