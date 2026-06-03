using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class TargetSection
{
    private const int MaxRowsBeforeScroll = 6;

    public static void Draw(Configuration cfg, AutoLbController ctrl, LbDrawState state)
    {
        Styling.SectionLabel("Target");

        var candidates = TargetSelector.ScanHostiles(cfg.AutoSelectRangeYalms);
        if (candidates.Count == 0)
        {
            EmptyCard.Draw("target",
                $"Scanning — no hostile targets within {cfg.AutoSelectRangeYalms:F0}y.",
                FontAwesomeIcon.Satellite);
            return;
        }

        var picked = ctrl.LastResolvedTarget ?? candidates[0];
        var pickedBelow = HpMath.IsBelowThreshold(picked, cfg, state.JobId);
        HeroCard.Draw(picked, pickedBelow, cfg, state);

        var others = 0;
        for (var i = 0; i < candidates.Count; i++)
            if (candidates[i].GameObjectId != picked.GameObjectId) others++;

        if (others == 0) return;

        ImGui.Spacing();
        DrawOthers(candidates, picked, others, cfg, state.JobId);
    }

    // The plugin only ever fires on the picked target above; this list is purely situational and would
    // balloon to 30+ rows in Frontline, so it stays collapsed and height-capped behind a header.
    private static void DrawOthers(IReadOnlyList<IBattleChara> candidates, IBattleChara picked, int others,
        Configuration cfg, uint jobId)
    {
        // ### keeps the header's ImGui ID (and its open/closed state) stable even as the count changes.
        bool open;
        using (ImRaii.PushColor(ImGuiCol.Header, Styling.CardBgSoft))
        using (ImRaii.PushColor(ImGuiCol.HeaderHovered, Styling.CardBgHover))
        using (ImRaii.PushColor(ImGuiCol.HeaderActive, Styling.CardBgHover))
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
            open = ImGui.CollapsingHeader($"Nearby hostiles ({others})###nearby_hostiles");

        if (!open) return;

        var rowH = Layout.CandidateRowHeight * ImGuiHelpers.GlobalScale;
        var spacingY = ImGui.GetStyle().ItemSpacing.Y;
        var visibleRows = Math.Min(others, MaxRowsBeforeScroll);
        var listH = visibleRows * (rowH + spacingY);

        ImGui.Spacing();
        using (ImRaii.Child("##cand_list", new Vector2(-1, listH), false))
        {
            for (var i = 0; i < candidates.Count; i++)
            {
                var c = candidates[i];
                if (c.GameObjectId == picked.GameObjectId) continue;
                CandidateRow.Draw(c, cfg, jobId);
            }
        }
    }
}
