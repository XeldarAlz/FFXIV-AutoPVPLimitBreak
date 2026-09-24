using Dalamud.Interface;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections.Config;

internal static class HudSettings
{
    public static void Draw(Plugin plugin, Configuration configuration)
    {
        using (SettingsGroup.Begin(Loc.T(L.Settings.HudGroup)))
        {
            var show = configuration.ShowCombatHud;
            if (SettingsControls.ToggleRow(Loc.T(L.Settings.HudShowInPvp), Loc.T(L.Settings.HudShowInPvpHelp), "##palb_hud_show", ref show))
            {
                configuration.ShowCombatHud = show;
            }

            var locked = configuration.CombatHudLocked;
            if (SettingsControls.ToggleRow(Loc.T(L.Settings.HudLock), Loc.T(L.Settings.HudLockHelp), "##palb_hud_lock_toggle", ref locked))
            {
                configuration.CombatHudLocked = locked;
            }

            var visible = plugin.CombatHud.Visible;
            if (SettingsControls.ButtonRow(Loc.T(L.Settings.HudNow), Loc.T(L.Settings.HudNowHelp), "##palb_hud_now",
                    Loc.T(visible ? L.Settings.HudHide : L.Settings.HudShow), visible ? FontAwesomeIcon.EyeSlash : FontAwesomeIcon.Eye, Styling.AccentRed))
            {
                plugin.CombatHud.ToggleVisible();
            }
        }

        SettingsGroup.Footnote(Loc.T(L.Settings.HudFootnote));
    }
}
