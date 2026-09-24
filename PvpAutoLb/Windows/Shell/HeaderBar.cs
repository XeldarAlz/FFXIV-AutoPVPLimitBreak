using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;
using PvpAutoLb.Windows.Sections;

namespace PvpAutoLb.Windows.Shell;

internal static class HeaderBar
{
    private const string Title = "Auto PVP LB";
    private const string ChipId = "##palb_header_chip";
    private const float PadX = 16f;
    private const float IconBox = 26f;
    private const float ButtonSize = 30f;
    private const float ButtonGap = 6f;
    private const int ButtonCount = 3;
    private const float CompactBarWidth = 90f;

    public const float MinimumWidth = PadX * 2f + IconBox + 12f + ButtonSize * ButtonCount + ButtonGap * (ButtonCount - 1);

    public static float ButtonsWidth() => (ButtonSize * ButtonCount + ButtonGap * (ButtonCount - 1)) * ImGuiHelpers.GlobalScale;

    public static bool HandleDrag(Vector2 windowPos, float width, float height)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var dragWidth = width - PadX * scale - ButtonsWidth() - 8f * scale;
        ImGui.SetCursorScreenPos(windowPos);
        ImGui.InvisibleButton("##palb_drag", new Vector2(MathF.Max(1f, dragWidth), height));
        ImGui.SetItemAllowOverlap();
        var doubleClicked = ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);
        if (!ImGui.IsItemActive())
        {
            return doubleClicked;
        }

        var delta = ImGui.GetIO().MouseDelta;
        if (delta != Vector2.Zero)
        {
            ImGui.SetWindowPos(ImGui.GetWindowPos() + delta, ImGuiCond.Always);
        }

        return doubleClicked;
    }

    public static void Draw(AppWindow window, Plugin plugin, Vector2 origin, float width, float height, float windowRounding, bool compact)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var drawList = ImGui.GetWindowDrawList();
        var end = origin + new Vector2(width, height);
        var padX = PadX * scale;
        var midY = origin.Y + height * 0.5f;

        Paint.Fill(drawList, origin, end, Styling.WithAlpha(Styling.Surface1, 0.40f), windowRounding,
            compact ? ImDrawFlags.RoundCornersAll : ImDrawFlags.RoundCornersTop);
        if (!compact)
        {
            Paint.Hairline(drawList, new Vector2(origin.X, end.Y - 0.5f), new Vector2(end.X, end.Y - 0.5f));
        }

        var iconBox = IconBox * scale;
        var iconMin = new Vector2(origin.X + padX, midY - iconBox * 0.5f);
        AppIcon.Draw(drawList, iconMin, iconMin + new Vector2(iconBox, iconBox), 7f * scale);

        var buttonsLeft = end.X - padX - ButtonsWidth();
        var x = iconMin.X + iconBox + 12f * scale;
        using (Fonts.PushHeadline())
        {
            var titleSize = TextDraw.Measure(Title);
            if (x + titleSize.X <= buttonsLeft)
            {
                TextDraw.At(Title, new Vector2(x, midY - titleSize.Y * 0.5f), Styling.TextStrong);
                x += titleSize.X + 14f * scale;
            }
        }

        var snapshot = LiveSnapshot.Resolve(plugin);
        var pillEnd = DrawStatusPill(plugin, snapshot, x, buttonsLeft, midY);
        if (pillEnd > x)
        {
            x = pillEnd + 14f * scale;
        }

        if (compact)
        {
            DrawCompactInfo(snapshot, x, buttonsLeft - 14f * scale, midY);
        }

        DrawButtons(window, plugin, end, midY, compact);
    }

    private static void DrawButtons(AppWindow window, Plugin plugin, Vector2 end, float midY, bool compact)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var padX = PadX * scale;
        var buttonSize = ButtonSize * scale;
        var stride = buttonSize + ButtonGap * scale;
        var top = midY - buttonSize * 0.5f;

        ImGui.SetCursorScreenPos(new Vector2(end.X - padX - buttonSize, top));
        if (IconButton.Draw(FontAwesomeIcon.Times, "##palb_close", buttonSize, tooltip: Loc.T(L.Common.Close)))
        {
            window.IsOpen = false;
        }

        ImGui.SetCursorScreenPos(new Vector2(end.X - padX - buttonSize - stride, top));
        if (IconButton.Draw(compact ? FontAwesomeIcon.ChevronUp : FontAwesomeIcon.ChevronDown, "##palb_minimize", buttonSize,
                tooltip: compact ? Loc.T(L.Shell.Restore) : Loc.T(L.Shell.Minimize)))
        {
            window.ToggleCompact();
        }

        var hud = plugin.CombatHud;
        var hudShown = hud.Visible;
        ImGui.SetCursorScreenPos(new Vector2(end.X - padX - buttonSize - stride * 2f, top));
        if (IconButton.Draw(FontAwesomeIcon.Crosshairs, "##palb_hud", buttonSize,
                hudShown ? Styling.AccentRedBright : null, hudShown ? Loc.T(L.Shell.HideHud) : Loc.T(L.Shell.ShowHud)))
        {
            hud.ToggleVisible();
        }
    }

    private static float DrawStatusPill(Plugin plugin, LiveSnapshot snapshot, float x, float rightLimit, float midY)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var drawList = ImGui.GetWindowDrawList();
        var label = snapshot.Chip;
        var padX = 10f * scale;
        var dotRadius = 3.5f * scale;
        var accent = snapshot.Armed ? ChipAccent(snapshot) : Styling.TextDim;
        var accentSoft = snapshot.Armed ? ChipAccentSoft(snapshot) : Styling.TextSecondary;

        using (Fonts.PushCaption())
        {
            var labelSize = TextDraw.Measure(label);
            var pillHeight = labelSize.Y + 8f * scale;
            var pillMin = new Vector2(x, midY - pillHeight * 0.5f);
            var pillSize = new Vector2(padX * 2f + dotRadius * 2f + 6f * scale + labelSize.X, pillHeight);
            var pillMax = pillMin + pillSize;
            if (pillMax.X > rightLimit)
            {
                return x;
            }

            ImGui.SetCursorScreenPos(pillMin);
            var hit = Hit.Area(ChipId, pillSize);
            var hover = Motion.Hover(Motion.Key(ChipId), hit.Hovered);
            Paint.Pill(drawList, pillMin, pillMax, Styling.WithAlpha(accent, 0.16f + 0.10f * hover), Styling.WithAlpha(accent, 0.45f + 0.30f * hover));

            var dotColor = snapshot.Firing ? Styling.PulseColor(accent, accentSoft, Styling.PulseFast)
                : snapshot.Armed ? Styling.PulseColor(accent, accentSoft, Styling.PulseBreath)
                : accent;
            drawList.AddCircleFilled(new Vector2(pillMin.X + padX + dotRadius, midY), dotRadius, Paint.Col(dotColor));
            TextDraw.At(label, new Vector2(pillMin.X + padX + dotRadius * 2f + 6f * scale, midY - labelSize.Y * 0.5f), accentSoft);

            if (hit.Hovered)
            {
                Tooltip.Show(Loc.T(snapshot.Armed ? L.Shell.ChipDisarm : L.Shell.ChipArm));
            }

            if (hit.Clicked)
            {
                ToggleArmed(plugin.Configuration);
            }

            return pillMax.X;
        }
    }

    public static void ToggleArmed(Configuration configuration)
    {
        configuration.Enabled = !configuration.Enabled;
        configuration.Save();
    }

    private static Vector4 ChipAccent(LiveSnapshot snapshot)
    {
        if (snapshot.Firing)
        {
            return Styling.AccentRed;
        }

        if (!snapshot.IsLive)
        {
            return snapshot.Kind == LiveKind.DutyOff ? Styling.AccentAmber : Styling.TextDim;
        }

        return snapshot.LimitBreakReady ? Styling.AccentMint : Styling.AccentAmber;
    }

    private static Vector4 ChipAccentSoft(LiveSnapshot snapshot)
    {
        if (snapshot.Firing)
        {
            return Styling.AccentRedBright;
        }

        if (!snapshot.IsLive)
        {
            return snapshot.Kind == LiveKind.DutyOff ? Styling.AccentAmberSoft : Styling.TextSecondary;
        }

        return snapshot.LimitBreakReady ? Styling.AccentMintSoft : Styling.AccentAmberSoft;
    }

    private static void DrawCompactInfo(LiveSnapshot snapshot, float x, float rightX, float midY)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var drawList = ImGui.GetWindowDrawList();
        if (rightX - x < CompactBarWidth * scale)
        {
            return;
        }

        var barWidth = CompactBarWidth * scale;
        var barX = rightX - barWidth;
        var barHeight = 6f * scale;
        var fill = Motion.Approach(Motion.Key("##palb_compact_gauge"), snapshot.RingFill, 8f);
        Paint.Bar(drawList, new Vector2(barX, midY - barHeight * 0.5f), barWidth, barHeight, fill, snapshot.LimitBreakReady ? Styling.AccentMint : Styling.AccentAmber);

        var textWidth = barX - 12f * scale - x;
        if (textWidth <= 0f)
        {
            return;
        }

        using (Fonts.PushCaption())
        {
            var text = snapshot.Status;
            var textSize = TextDraw.Measure(text);
            TextDraw.At(TextDraw.Truncate(text, textWidth), new Vector2(x, midY - textSize.Y * 0.5f), Styling.TextSecondary);
        }
    }
}
