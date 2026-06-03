using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class BlocklistSection
{
    private const int MaxRowsBeforeScroll = 6;
    private static string addBuffer = string.Empty;

    public static void Draw(Configuration cfg)
    {
        Styling.SectionLabel("Player blocklist");

        var rows = cfg.NameBlocklist.Count;
        var listH = Math.Min(rows, MaxRowsBeforeScroll) * Layout.BlocklistRowHeight;
        var height = (Layout.BlocklistBaseHeight + listH) * ImGuiHelpers.GlobalScale;

        using (Card.Begin("##blocklist", height, Styling.CardBg, Styling.CardBorderDim))
        {
            DrawAddInput(cfg);
            ImGui.Spacing();
            DrawList(cfg);
        }
    }

    private static void DrawAddInput(Configuration cfg)
    {
        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputTextWithHint("##blocklist_add", "Player Name (Enter to add)", ref addBuffer, 64,
                ImGuiInputTextFlags.EnterReturnsTrue))
        {
            var name = addBuffer.Trim();
            if (name.Length > 0 && !cfg.NameBlocklist.Contains(name))
            {
                cfg.NameBlocklist.Add(name);
                cfg.Save();
            }
            addBuffer = string.Empty;
        }
        Tooltip.OnHover("Names listed here will never be auto-targeted, even when below threshold.");
    }

    private static void DrawList(Configuration cfg)
    {
        if (cfg.NameBlocklist.Count == 0)
        {
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextMuted))
                ImGui.TextUnformatted("No names blocked.");
            return;
        }

        var rows = cfg.NameBlocklist.Count;
        var listH = Math.Min(rows, MaxRowsBeforeScroll) * Layout.BlocklistRowHeight * ImGuiHelpers.GlobalScale;

        using (ImRaii.Child("##blocklist_rows", new Vector2(-1, listH), false))
        {
            var removeIndex = -1;
            for (var i = 0; i < cfg.NameBlocklist.Count; i++)
            {
                ImGui.PushID(i);
                using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextSecondary))
                    ImGui.TextUnformatted(cfg.NameBlocklist[i]);
                ImGui.SameLine();
                var btnLabel = "Remove";
                var btnW = ImGui.CalcTextSize(btnLabel).X + ImGui.GetStyle().FramePadding.X * 2;
                Styling.SameLineRightAligned(btnW);
                if (ImGui.SmallButton(btnLabel + "##rm")) removeIndex = i;
                ImGui.PopID();
            }
            if (removeIndex >= 0)
            {
                cfg.NameBlocklist.RemoveAt(removeIndex);
                cfg.Save();
            }
        }
    }
}
