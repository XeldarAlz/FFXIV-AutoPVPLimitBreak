using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;
using PvpAutoLb.Windows.Sections;

namespace PvpAutoLb.Windows.Shell;

internal static class ActionDock
{
    private const float PadX = 18f;

    public static void Draw(Plugin plugin, Vector2 size, float windowRounding)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var origin = ImGui.GetCursorScreenPos();
        var end = origin + size;
        Dock.Background(ImGui.GetWindowDrawList(), origin, end, windowRounding);

        var padX = PadX * scale;
        var buttonHeight = Layout.HeroButtonHeight * scale;
        var innerWidth = size.X - padX * 2f;
        ImGui.SetCursorScreenPos(new Vector2(origin.X + padX, origin.Y + (size.Y - buttonHeight) * 0.5f));

        var configuration = plugin.Configuration;
        var snapshot = LiveSnapshot.Resolve(plugin);
        var clicked = configuration.Enabled
            ? HeroButton.Draw(FontAwesomeIcon.PowerOff, Loc.T(L.Live.Disable), snapshot.ActiveRule, Styling.AccentAmber, true, null, innerWidth)
            : HeroButton.Draw(FontAwesomeIcon.Bolt, Loc.T(L.Live.Enable), Loc.T(L.Live.EnableSub), Styling.AccentRed, true, null, innerWidth);
        if (clicked)
        {
            HeaderBar.ToggleArmed(configuration);
        }

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(size);
    }
}
