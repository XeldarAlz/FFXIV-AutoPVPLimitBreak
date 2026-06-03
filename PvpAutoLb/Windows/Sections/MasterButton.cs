using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class MasterButton
{
    private static readonly Vector4 BgEnabled = new(0.18f, 0.55f, 0.30f, 0.85f);
    private static readonly Vector4 BgDisabled = new(0.28f, 0.28f, 0.30f, 0.55f);

    public static void Draw(Configuration cfg)
    {
        var bg = cfg.Enabled ? BgEnabled : BgDisabled;
        var bgHover = bg + new Vector4(0.08f, 0.08f, 0.08f, 0f);
        var bgActive = bg - new Vector4(0.04f, 0.04f, 0.04f, 0f);
        var height = Layout.MasterButtonHeight * ImGuiHelpers.GlobalScale;

        using (ImRaii.PushColor(ImGuiCol.Button, bg))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, bgHover))
        using (ImRaii.PushColor(ImGuiCol.ButtonActive, bgActive))
        using (ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 8f))
        {
            var icon = FontAwesomeIcon.PowerOff.ToIconString();
            var label = cfg.Enabled ? "Auto-fire is ON" : "Auto-fire is OFF";
            if (DrawIconButton(icon, label, height))
            {
                cfg.Enabled = !cfg.Enabled;
                cfg.Save();
            }
        }
        Tooltip.OnHover(cfg.Enabled
            ? "Auto-fire is on — the LB fires automatically when a target drops below threshold. Click to pause."
            : "Auto-fire is paused — nothing fires automatically. Click to enable.");
    }

    private static bool DrawIconButton(string icon, string label, float height)
    {
        float iconW, labelW, spacing = ImGui.GetStyle().ItemInnerSpacing.X;
        using (ImRaii.PushFont(UiBuilder.IconFont))
            iconW = ImGui.CalcTextSize(icon).X;
        labelW = ImGui.CalcTextSize(label).X;

        var clicked = ImGui.Button("##master", new Vector2(-1, height));

        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        var totalW = iconW + spacing + labelW;
        var startX = min.X + (max.X - min.X - totalW) * 0.5f;
        var midY = (min.Y + max.Y) * 0.5f;
        var dl = ImGui.GetWindowDrawList();
        var textCol = ImGui.GetColorU32(Styling.TextStrong);

        using (ImRaii.PushFont(UiBuilder.IconFont))
            dl.AddText(new Vector2(startX, midY - ImGui.GetTextLineHeight() * 0.5f), textCol, icon);
        dl.AddText(new Vector2(startX + iconW + spacing, midY - ImGui.GetTextLineHeight() * 0.5f), textCol, label);

        return clicked;
    }
}
