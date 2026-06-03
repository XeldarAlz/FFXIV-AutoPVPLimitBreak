using Dalamud.Bindings.ImGui;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class ThresholdSection
{
    public static void Draw(Configuration cfg)
    {
        DrawModeToggle(cfg);
        ImGui.Spacing();
        DrawValueControl(cfg);
        ImGui.Spacing();
        ImGui.Spacing();
        ThresholdWidgets.DrawPreview(cfg.ThresholdMode, cfg.HpThresholdPercent, cfg.HpThresholdAbsolute);
    }

    private static void DrawModeToggle(Configuration cfg)
    {
        var mode = cfg.ThresholdMode;
        if (ThresholdWidgets.DrawModeToggle("thresh", ref mode))
        {
            cfg.ThresholdMode = mode;
            cfg.Save();
        }
    }

    private static void DrawValueControl(Configuration cfg)
    {
        var pct = cfg.HpThresholdPercent;
        var abs = cfg.HpThresholdAbsolute;
        if (ThresholdWidgets.DrawValueControl("thresh", cfg.ThresholdMode, ref pct, ref abs))
        {
            cfg.HpThresholdPercent = pct;
            cfg.HpThresholdAbsolute = abs;
            cfg.SaveDebounced();
        }
    }
}
