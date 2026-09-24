using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;
using PvpAutoLb.Windows.Sections;

namespace PvpAutoLb.Windows.Shell;

internal static class MiniPlayer
{
    private const string OpenId = "##palb_mini_open";
    private const float PadX = 18f;
    private const float ButtonSize = 34f;
    private const float BarWidth = 160f;
    private const float BarHeight = 8f;

    public static bool Draw(Plugin plugin, Vector2 size, float windowRounding)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var origin = ImGui.GetCursorScreenPos();
        var end = origin + size;
        var drawList = ImGui.GetWindowDrawList();
        var snapshot = LiveSnapshot.Resolve(plugin);

        Dock.Background(drawList, origin, end, windowRounding);

        var padX = PadX * scale;
        var buttonSize = ButtonSize * scale;
        ImGui.SetCursorScreenPos(origin);
        var hit = Hit.Area(OpenId, new Vector2(size.X - padX - buttonSize - 8f * scale, size.Y));
        var hover = Motion.Hover(Motion.Key(OpenId), hit.Hovered);
        if (hover > 0.01f)
        {
            Paint.Fill(drawList, origin, end, Styling.WithAlpha(Styling.Surface2, 0.35f * hover), windowRounding, ImDrawFlags.RoundCornersBottom);
        }

        var midY = origin.Y + size.Y * 0.5f;
        var dotColor = Styling.PulseColor(snapshot.Accent, snapshot.AccentSoft, snapshot.Firing ? Styling.PulseFast : Styling.PulseMedium);
        Paint.Dot(drawList, new Vector2(origin.X + padX + 4f * scale, midY), 4f * scale, dotColor);

        var barWidth = BarWidth * scale;
        var barRight = end.X - padX - buttonSize - 16f * scale;
        var barX = barRight - barWidth;
        var barY = midY - BarHeight * scale * 0.5f;
        var fill = Motion.Approach(Motion.Key("##palb_mini_gauge"), snapshot.RingFill, 8f);
        Paint.Bar(drawList, new Vector2(barX, barY), barWidth, BarHeight * scale, fill, snapshot.LimitBreakReady ? Styling.AccentMint : Styling.AccentAmber);

        using (Fonts.PushCaption())
        {
            var gauge = snapshot.GaugeLabel;
            var gaugeSize = TextDraw.Measure(gauge);
            TextDraw.At(gauge, new Vector2(barRight - gaugeSize.X, barY - gaugeSize.Y - 3f * scale), Styling.TextDim);
        }

        var textX = origin.X + padX + 22f * scale;
        var title = snapshot.LimitBreakName.Length > 0 ? snapshot.LimitBreakName : snapshot.Chip;
        var titleSize = TextDraw.SmallCapsSize(title);
        var lineHeight = ImGui.GetTextLineHeight();
        var gap = 2f * scale;
        var top = midY - (titleSize.Y + gap + lineHeight) * 0.5f;
        TextDraw.SmallCaps(TextDraw.Truncate(title, barX - 16f * scale - textX), new Vector2(textX, top), snapshot.AccentSoft);
        TextDraw.At(TextDraw.Truncate(snapshot.Status, barX - 16f * scale - textX), new Vector2(textX, top + titleSize.Y + gap), Styling.TextStrong);

        ImGui.SetCursorScreenPos(new Vector2(end.X - padX - buttonSize, midY - buttonSize * 0.5f));
        if (IconButton.Draw(FontAwesomeIcon.PowerOff, "##palb_mini_disarm", buttonSize, Styling.AccentAmberSoft, Loc.T(L.Live.Disable)))
        {
            HeaderBar.ToggleArmed(plugin.Configuration);
        }

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(size);
        return hit.Clicked;
    }
}
