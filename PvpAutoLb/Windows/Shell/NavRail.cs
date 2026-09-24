using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using PvpAutoLb.Core;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;
using PvpAutoLb.Windows.Sections;

namespace PvpAutoLb.Windows.Shell;

internal static class NavRail
{
    private readonly record struct Entry(AppWindow.Page Page, FontAwesomeIcon Icon, string Id, LocString Label);

    private const float TopPad = 12f;
    private const float Gap = 8f;

    private static readonly Entry[] entries =
    [
        new(AppWindow.Page.Live,      FontAwesomeIcon.Bolt,       "##palb_nav_live",      L.Shell.NavLive),
        new(AppWindow.Page.Settings,  FontAwesomeIcon.SlidersH,   "##palb_nav_settings",  L.Shell.NavSettings),
        new(AppWindow.Page.Console,   FontAwesomeIcon.Terminal,   "##palb_nav_log",       L.Shell.NavLog),
        new(AppWindow.Page.Changelog, FontAwesomeIcon.Newspaper,  "##palb_nav_changelog", L.Shell.NavChangelog),
        new(AppWindow.Page.About,     FontAwesomeIcon.InfoCircle, "##palb_nav_about",     L.Shell.NavAbout),
    ];

    public static AppWindow.Page? Draw(AppWindow.Page current, Plugin plugin)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var button = Layout.RailButton * scale;
        var gap = Gap * scale;
        var railOrigin = ImGui.GetCursorScreenPos();
        var available = ImGui.GetContentRegionAvail().X;
        var x = railOrigin.X + (available - button) * 0.5f;
        var startY = railOrigin.Y + TopPad * scale;
        var drawList = ImGui.GetWindowDrawList();

        var selectedIndex = 0;
        for (var index = 0; index < entries.Length; index++)
        {
            if (entries[index].Page == current)
            {
                selectedIndex = index;
            }
        }

        var indicator = Motion.Approach(Motion.Key("##palb_rail_indicator"), selectedIndex, 16f);
        var indicatorY = startY + (button + gap) * indicator;
        var indicatorMin = new Vector2(x, indicatorY);
        var indicatorMax = indicatorMin + new Vector2(button, button);
        Paint.Glass(drawList, indicatorMin, indicatorMax, 12f * scale, Styling.AccentRed, 0.30f);
        Paint.Fill(drawList, new Vector2(railOrigin.X, indicatorY + button * 0.25f), new Vector2(railOrigin.X + 3f * scale, indicatorY + button * 0.75f),
            Styling.AccentRed, 2f * scale);

        var firing = LiveSnapshot.Resolve(plugin).Firing;
        AppWindow.Page? clicked = null;

        for (var index = 0; index < entries.Length; index++)
        {
            var entry = entries[index];
            var y = startY + (button + gap) * index;
            ImGui.SetCursorScreenPos(new Vector2(x, y));
            var hit = Hit.Area(entry.Id, new Vector2(button, button));
            var hover = Motion.Hover(Motion.Key(entry.Id), hit.Hovered);
            var selected = index == selectedIndex;

            if (!selected && hover > 0.01f)
            {
                Paint.Fill(drawList, new Vector2(x, y), new Vector2(x + button, y + button), Styling.WithAlpha(Styling.Surface2, 0.8f * hover), 12f * scale);
            }

            var center = new Vector2(x + button * 0.5f, y + button * 0.5f);
            var color = selected ? Styling.TextStrong : Vector4.Lerp(Styling.TextDim, Styling.TextSecondary, hover);
            TextDraw.IconCentered(entry.Icon, center, color);

            DrawBadge(drawList, entry.Page, current, center, button, firing, plugin.Configuration.HasUnseenChangelog);

            if (hit.Hovered)
            {
                Tooltip.Show(Loc.T(entry.Label));
            }

            if (hit.Clicked)
            {
                clicked = entry.Page;
            }
        }

        ImGui.SetCursorScreenPos(railOrigin);
        ImGui.Dummy(new Vector2(available, TopPad * scale + (button + gap) * entries.Length));
        return clicked;
    }

    private static void DrawBadge(ImDrawListPtr drawList, AppWindow.Page page, AppWindow.Page current, Vector2 center, float button, bool firing, bool unseenChangelog)
    {
        if (page == current)
        {
            return;
        }

        var color = page switch
        {
            AppWindow.Page.Live when firing => Styling.PulseColor(Styling.AccentRed, Styling.AccentRedBright, Styling.PulseFast),
            AppWindow.Page.Changelog when unseenChangelog => Styling.PulseColor(Styling.AccentRed, Styling.AccentRedBright, Styling.PulseMedium),
            AppWindow.Page.Console when RunLog.Unseen is { } unseen => unseen == RunLogLevel.Error ? Styling.AccentRose : Styling.AccentAmber,
            _ => Vector4.Zero,
        };

        if (color.W <= 0f)
        {
            return;
        }

        var scale = ImGuiHelpers.GlobalScale;
        var badgeCenter = center + new Vector2(button * 0.30f, -button * 0.30f);
        var radius = 3.5f * scale;
        drawList.AddCircleFilled(badgeCenter, radius + 1.5f * scale, Paint.Col(Styling.WindowBg));
        drawList.AddCircleFilled(badgeCenter, radius, Paint.Col(color));
    }
}
