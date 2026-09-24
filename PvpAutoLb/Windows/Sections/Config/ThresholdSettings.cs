using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using PvpAutoLb.Core;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections.Config;

internal static class ThresholdSettings
{
    private const float MinimumPercent = 1f;
    private const float MaximumPercent = 99f;
    private const int MinimumAbsolute = 1;
    private const int MaximumAbsolute = 500_000;
    private const int AbsoluteStep = 500;
    // A typical PvP max HP, so an absolute threshold previews at a believable spot on the sample bar.
    private const uint SamplePreviewMaxHp = 75_000u;
    private const float PreviewBarHeight = 16f;

    private static readonly Segmented.Item[] modeItems = new Segmented.Item[2];
    private static readonly CachedText previewText = new();

    public static void Draw(Configuration configuration)
    {
        using (SettingsGroup.Begin(Loc.T(L.Settings.ThresholdGroup)))
        {
            var mode = configuration.ThresholdMode;
            if (DrawModeRow("##palb_threshold_mode", ref mode))
            {
                configuration.ThresholdMode = mode;
            }

            var percent = configuration.HpThresholdPercent;
            var absolute = configuration.HpThresholdAbsolute;
            if (DrawValueRow("##palb_threshold_value", configuration.ThresholdMode, ref percent, ref absolute))
            {
                configuration.HpThresholdPercent = percent;
                configuration.HpThresholdAbsolute = absolute;
            }
        }

        using (SettingsGroup.Begin(Loc.T(L.Settings.PreviewGroup)))
        {
            DrawPreview(configuration.ThresholdMode, configuration.HpThresholdPercent, configuration.HpThresholdAbsolute);
        }
    }

    public static bool DrawModeRow(string id, ref ThresholdMode mode)
    {
        modeItems[0] = new Segmented.Item(FontAwesomeIcon.Percent, Loc.T(L.Settings.ModePercent));
        modeItems[1] = new Segmented.Item(FontAwesomeIcon.Heart, Loc.T(L.Settings.ModeAbsolute));
        var selected = (int)mode;
        if (!SettingsControls.SegmentedRow(Loc.T(L.Settings.ThresholdType), Loc.T(L.Settings.ThresholdTypeHelp), id, modeItems, ref selected))
        {
            return false;
        }

        mode = (ThresholdMode)selected;
        return true;
    }

    public static bool DrawValueRow(string id, ThresholdMode mode, ref float percent, ref uint absolute)
    {
        if (mode == ThresholdMode.Percent)
        {
            return SettingsControls.FloatSliderRow(Loc.T(L.Settings.FireBelow), Loc.T(L.Settings.FireBelowPercentHelp), id,
                ref percent, MinimumPercent, MaximumPercent, Loc.T(L.Settings.PercentFormat));
        }

        var value = (int)Math.Min(absolute, MaximumAbsolute);
        if (!SettingsControls.StepperRow(Loc.T(L.Settings.FireBelow), Loc.T(L.Settings.FireBelowAbsoluteHelp), id,
                ref value, AbsoluteStep, MinimumAbsolute, MaximumAbsolute, Loc.T(L.Settings.AbsoluteFormat)))
        {
            return false;
        }

        absolute = (uint)Math.Max(MinimumAbsolute, value);
        return true;
    }

    // A sample HP bar with the fire zone shaded, so the value reads as "this much HP left means it fires"
    // rather than as an abstract number. Shared by the global and per-job pages so both look the same.
    public static void DrawPreview(ThresholdMode mode, float percent, uint absolute)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var fraction = mode == ThresholdMode.Percent
            ? Math.Clamp(percent / 100f, 0.01f, 0.99f)
            : Math.Clamp((float)absolute / SamplePreviewMaxHp, 0.01f, 0.99f);

        Styling.VSpace(8f);
        var origin = ImGui.GetCursorScreenPos();
        var width = SettingsGroup.ContentRightEdge - origin.X;
        var height = PreviewBarHeight * scale;
        var drawList = ImGui.GetWindowDrawList();
        var marker = Motion.Approach(Motion.Key("##palb_threshold_preview"), fraction, 14f);
        HpBar.Draw(drawList, origin, width, height, 1f, 0f, marker, Styling.AccentMint);
        Paint.Fill(drawList, origin, new Vector2(origin.X + width * marker, origin.Y + height), Styling.WithAlpha(Styling.AccentRed, 0.75f), height * 0.5f);
        HpBar.DrawMarker(drawList, origin, width, height, marker);
        ImGui.Dummy(new Vector2(width, height));

        Styling.VSpace(4f);
        using (Fonts.PushCaption())
        {
            var labelsOrigin = ImGui.GetCursorScreenPos();
            TextDraw.At(Loc.T(L.Settings.PreviewEmpty), labelsOrigin, Styling.TextMuted);
            TextDraw.Right(Loc.T(L.Settings.PreviewFull), labelsOrigin.X + width, labelsOrigin.Y, Styling.TextMuted);
            ImGui.Dummy(new Vector2(width, TextDraw.LineHeight()));
        }

        SettingsRow.Note(PreviewSentence(mode, percent, absolute), Styling.TextSecondary);
    }

    private static string PreviewSentence(ThresholdMode mode, float percent, uint absolute)
    {
        var rounded = (int)MathF.Round(percent);
        var key = mode == ThresholdMode.Percent ? rounded : -(long)absolute - 1;
        if (previewText.Matches(key))
        {
            return previewText.Text;
        }

        var text = mode == ThresholdMode.Percent
            ? Loc.T(L.Settings.PreviewPercent, rounded)
            : Loc.T(L.Settings.PreviewAbsolute, Formatting.Count(absolute));
        return previewText.Store(key, text);
    }
}
