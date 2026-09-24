using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;
using PvpAutoLb.Windows.Sections.Config;

namespace PvpAutoLb.Windows.Pages;

internal sealed class SettingsPage
{
    private enum Tab { General, WhenToFire, Targeting, PerJob, Filters, Blocklist, Notifications, CombatHud }

    private readonly record struct Entry(Tab Tab, LocString Label, FontAwesomeIcon Icon, LocString Subtitle);

    private static readonly Entry[] entries =
    [
        new(Tab.General,       L.Settings.CatGeneral,       FontAwesomeIcon.Cog,         L.Settings.CatGeneralSub),
        new(Tab.WhenToFire,    L.Settings.CatWhenToFire,    FontAwesomeIcon.HeartBroken, L.Settings.CatWhenToFireSub),
        new(Tab.Targeting,     L.Settings.CatTargeting,     FontAwesomeIcon.Bullseye,    L.Settings.CatTargetingSub),
        new(Tab.PerJob,        L.Settings.CatPerJob,        FontAwesomeIcon.UserCog,     L.Settings.CatPerJobSub),
        new(Tab.Filters,       L.Settings.CatFilters,       FontAwesomeIcon.Filter,      L.Settings.CatFiltersSub),
        new(Tab.Blocklist,     L.Settings.CatBlocklist,     FontAwesomeIcon.UserSlash,   L.Settings.CatBlocklistSub),
        new(Tab.Notifications, L.Settings.CatNotifications, FontAwesomeIcon.Bell,        L.Settings.CatNotificationsSub),
        new(Tab.CombatHud,     L.Settings.CatCombatHud,     FontAwesomeIcon.Crosshairs,  L.Settings.CatCombatHudSub),
    ];

    private Tab activeTab = Tab.General;
    private bool resetScroll;

    public void Draw(Plugin plugin)
    {
        var configuration = plugin.Configuration;
        var scale = ImGuiHelpers.GlobalScale;
        var navWidth = Layout.SettingsNavWidth * scale;

        using (ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero))
        {
            using (var nav = ImRaii.Child("##palb_settings_nav", new Vector2(navWidth, -1f), false, ImGuiWindowFlags.NoScrollbar))
            {
                if (nav)
                {
                    DrawNav();
                }
            }

            ImGui.SameLine(0f, 18f * scale);

            using (var content = ImRaii.Child("##palb_settings_content", new Vector2(-1f, -1f), false, ImGuiWindowFlags.None))
            {
                if (content)
                {
                    DrawContent(plugin, configuration);
                }
            }
        }

        SettingsControls.Flush(configuration);
    }

    private void DrawNav()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var origin = ImGui.GetCursorScreenPos();
        var title = Loc.T(L.Settings.Title);
        using (Fonts.PushTitle())
        {
            TextDraw.At(title, new Vector2(origin.X + 6f * scale, origin.Y), Styling.TextStrong);
            ImGui.Dummy(new Vector2(ImGui.GetContentRegionAvail().X, TextDraw.Measure(title).Y + 10f * scale));
        }

        for (var index = 0; index < entries.Length; index++)
        {
            var entry = entries[index];
            if (SidebarTab.Draw(Loc.T(entry.Label), entry.Icon, Styling.AccentRed, activeTab == entry.Tab))
            {
                Select(entry.Tab);
            }
        }
    }

    private void Select(Tab tab)
    {
        if (activeTab == tab)
        {
            return;
        }

        activeTab = tab;
        resetScroll = true;
    }

    private void DrawContent(Plugin plugin, Configuration configuration)
    {
        if (resetScroll)
        {
            ImGui.SetScrollY(0f);
            resetScroll = false;
        }

        var entry = entries[(int)activeTab];
        var scale = ImGuiHelpers.GlobalScale;

        using var reveal = Motion.PushSwitch("##palb_settings_tab", (int)activeTab);
        using var group = ImRaii.Group();
        ImGui.Dummy(new Vector2(0f, 2f * scale));
        PageHeader.Draw(Loc.T(entry.Label), Loc.T(entry.Subtitle));

        switch (activeTab)
        {
            case Tab.WhenToFire:
                ThresholdSettings.Draw(configuration);
                break;
            case Tab.Targeting:
                TargetingSettings.Draw(configuration);
                break;
            case Tab.PerJob:
                PerJobSettings.Draw(configuration);
                break;
            case Tab.Filters:
                FilterSettings.Draw(configuration);
                break;
            case Tab.Blocklist:
                BlocklistSettings.Draw(configuration);
                break;
            case Tab.Notifications:
                NotificationSettings.Draw(configuration);
                break;
            case Tab.CombatHud:
                HudSettings.Draw(plugin, configuration);
                break;
            case Tab.General:
                GeneralSettings.Draw(configuration);
                break;
        }
    }
}
