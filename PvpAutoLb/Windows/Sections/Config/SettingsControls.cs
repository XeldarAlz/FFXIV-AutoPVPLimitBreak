using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections.Config;

// Each row helper takes the value by ref and reports a change, so a settings section writes the option back
// itself without a closure per row. Saving waits until no control is held, so a slider drag lands on disk
// once, with its final value.
internal static class SettingsControls
{
    public const float ToggleWidth = ToggleSwitch.Width;
    public const float RowSliderWidth = 180f;
    public const float RowComboWidth = 170f;
    public const float RowSegmentWidth = 300f;

    private static readonly string[] languageLabels = BuildLanguageLabels();

    private static bool pendingSave;

    public static bool ToggleRow(string label, string? help, string id, ref bool value)
    {
        var row = SettingsRow.Begin(label, help, ToggleWidth, SettingsRow.ToggleHeight);
        var changed = ToggleSwitch.Draw(id, ref value);
        SettingsRow.End(row);
        return MarkIf(changed);
    }

    public static bool FloatSliderRow(string label, string? help, string id, ref float value, float minimum, float maximum, string format)
    {
        var row = SettingsRow.Begin(label, help, RowSliderWidth);
        ImGui.SetNextItemWidth(RowSliderWidth * ImGuiHelpers.GlobalScale);
        bool changed;
        using (PushFrameColors())
        {
            changed = ImGui.SliderFloat(id, ref value, minimum, maximum, format);
        }

        SettingsRow.End(row);
        value = Math.Clamp(value, minimum, maximum);
        return MarkIf(changed);
    }

    public static bool StepperRow(string label, string? help, string id, ref int value, int step, int minimum, int maximum, string format)
    {
        var row = SettingsRow.Begin(label, help, Stepper.DefaultWidth);
        var changed = Stepper.Draw(id, ref value, step, minimum, maximum, format);
        SettingsRow.End(row);
        return MarkIf(changed);
    }

    public static bool SegmentedRow(string label, string? help, string id, ReadOnlySpan<Segmented.Item> items, ref int selected)
    {
        var height = Layout.SegmentHeight * 0.8f;
        var row = SettingsRow.Begin(label, help, RowSegmentWidth, height);
        var changed = Segmented.Draw(id, items, ref selected, true, height, RowSegmentWidth * ImGuiHelpers.GlobalScale);
        SettingsRow.End(row);
        return MarkIf(changed);
    }

    public static bool ButtonRow(string label, string? help, string id, string buttonLabel, FontAwesomeIcon icon, Vector4 accent)
    {
        var width = PillButton.Width(buttonLabel, icon) / ImGuiHelpers.GlobalScale;
        var row = SettingsRow.Begin(label, help, width, Layout.ActionPillHeight);
        var clicked = PillButton.Draw(id, buttonLabel, accent, PillButton.Emphasis.Tinted, icon, true, Layout.ActionPillHeight);
        SettingsRow.End(row);
        return clicked;
    }

    public static void LanguageRow(Configuration configuration)
    {
        var row = SettingsRow.Begin(Loc.T(L.Settings.Language), Loc.T(L.Settings.LanguageHelp), RowComboWidth);
        var languages = Languages.All;
        var selected = 0;
        for (var index = 0; index < languages.Length; index++)
        {
            if (ReferenceEquals(languages[index], Loc.Current))
            {
                selected = index;
            }
        }

        if (Dropdown.Draw("##palb_language", languageLabels, ref selected, RowComboWidth))
        {
            ApplyLanguage(configuration, languages[selected].Code);
        }

        SettingsRow.End(row);
    }

    public static void Flush(Configuration configuration)
    {
        if (!pendingSave || ImGui.IsAnyItemActive())
        {
            return;
        }

        pendingSave = false;
        configuration.Save();
    }

    public static IDisposable PushFrameColors()
        => ImRaii.PushColor(ImGuiCol.SliderGrab, Styling.AccentRed)
            .Push(ImGuiCol.SliderGrabActive, Styling.AccentRedBright)
            .Push(ImGuiCol.FrameBg, Styling.SliderBg)
            .Push(ImGuiCol.FrameBgHovered, Styling.CardBgHover)
            .Push(ImGuiCol.FrameBgActive, Styling.CardBgHover);

    private static bool MarkIf(bool changed)
    {
        if (changed)
        {
            pendingSave = true;
        }

        return changed;
    }

    private static string[] BuildLanguageLabels()
    {
        var languages = Languages.All;
        var labels = new string[languages.Length];
        for (var index = 0; index < languages.Length; index++)
        {
            labels[index] = languages[index].NativeName;
        }

        return labels;
    }

    private static void ApplyLanguage(Configuration configuration, string code)
    {
        configuration.Language = code;
        configuration.Save();
        Loc.SetLanguage(code);
        Fonts.OnLanguageChanged();
        Plugin.Instance.OnLanguageChanged();
    }
}
