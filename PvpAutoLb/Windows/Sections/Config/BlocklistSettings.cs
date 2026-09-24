using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections.Config;

internal static class BlocklistSettings
{
    private const int MaximumNameLength = 64;
    private const float InputWidth = 320f;

    private static readonly CachedText blockedTitle = new();

    private static string draft = string.Empty;

    public static void Draw(Configuration configuration)
    {
        DrawAddGroup(configuration);
        DrawListGroup(configuration);
    }

    private static void DrawAddGroup(Configuration configuration)
    {
        using var group = SettingsGroup.Begin(Loc.T(L.Settings.BlocklistAddGroup));

        SettingsRow.BlockHeader(Loc.T(L.Settings.BlocklistAdd), Loc.T(L.Settings.BlocklistAddHelp));
        bool entered;
        using (SettingsControls.PushFrameColors())
        {
            ImGui.SetNextItemWidth(InputWidth * ImGuiHelpers.GlobalScale);
            entered = ImGui.InputTextWithHint("##palb_blocklist_input", Loc.T(L.Settings.BlocklistHint), ref draft, MaximumNameLength, ImGuiInputTextFlags.EnterReturnsTrue);
        }

        ImGui.SameLine();
        var clicked = PillButton.Draw("##palb_blocklist_add", Loc.T(L.Common.Add), Styling.AccentRed, PillButton.Emphasis.Tinted, FontAwesomeIcon.Plus,
            draft.AsSpan().Trim().Length > 0, ImGui.GetFrameHeight() / ImGuiHelpers.GlobalScale);
        if (entered || clicked)
        {
            AddName(configuration, draft);
            draft = string.Empty;
        }

        SettingsRow.BlockEnd();
    }

    private static void AddName(Configuration configuration, string raw)
    {
        var name = raw.Trim();
        if (name.Length == 0 || Contains(configuration.NameBlocklist, name))
        {
            return;
        }

        configuration.NameBlocklist.Add(name);
        configuration.Save();
    }

    private static bool Contains(List<string> names, string name)
    {
        for (var index = 0; index < names.Count; index++)
        {
            if (string.Equals(names[index], name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void DrawListGroup(Configuration configuration)
    {
        var names = configuration.NameBlocklist;
        var title = blockedTitle.Matches(names.Count)
            ? blockedTitle.Text
            : blockedTitle.Store(names.Count, Loc.Plural(L.Settings.BlocklistCount, names.Count));
        using var group = SettingsGroup.Begin(title);

        if (names.Count == 0)
        {
            SettingsRow.Note(Loc.T(L.Settings.BlocklistEmpty));
            return;
        }

        var removeIndex = -1;
        var buttonSize = ImGui.GetFrameHeight();
        var buttonUnits = buttonSize / ImGuiHelpers.GlobalScale;
        for (var index = 0; index < names.Count; index++)
        {
            ImGui.PushID(index);
            var row = SettingsRow.Begin(names[index], null, buttonUnits, buttonUnits);
            if (IconButton.Draw(FontAwesomeIcon.Times, "##palb_blocklist_remove", buttonSize, Styling.AccentRose, Loc.T(L.Common.Remove)))
            {
                removeIndex = index;
            }

            SettingsRow.End(row);
            ImGui.PopID();
        }

        if (removeIndex < 0)
        {
            return;
        }

        names.RemoveAt(removeIndex);
        configuration.Save();
    }
}
