using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections.Config;

internal static class GeneralSettings
{
    public static void Draw(Configuration configuration)
    {
        using (SettingsGroup.Begin(Loc.T(L.Settings.Language)))
        {
            SettingsControls.LanguageRow(configuration);
        }

        using (SettingsGroup.Begin(Loc.T(L.Settings.WindowGroup)))
        {
            var openOnLogin = configuration.AutoShowOnLogin;
            if (SettingsControls.ToggleRow(Loc.T(L.Settings.OpenOnLogin), Loc.T(L.Settings.OpenOnLoginHelp), "##palb_open_on_login", ref openOnLogin))
            {
                configuration.AutoShowOnLogin = openOnLogin;
            }

            var dragAnywhere = configuration.DragWindowFromAnywhere;
            if (SettingsControls.ToggleRow(Loc.T(L.Settings.DragAnywhere), Loc.T(L.Settings.DragAnywhereHelp), "##palb_drag_anywhere", ref dragAnywhere))
            {
                configuration.DragWindowFromAnywhere = dragAnywhere;
            }
        }
    }
}
