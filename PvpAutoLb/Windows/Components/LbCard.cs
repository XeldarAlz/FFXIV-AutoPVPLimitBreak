using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.DalamudServices;
using PvpAutoLb.Core;
using LuminaAction = Lumina.Excel.Sheets.Action;

namespace PvpAutoLb.Windows.Components;

internal static class LbCard
{
    public static void Draw(Configuration cfg, AutoLbController ctrl, LbDrawState state)
    {
        Styling.SectionLabel("Limit Break");

        if (state.ActionId == 0)
        {
            EmptyCard.Draw("lb", "No PvP Limit Break for this job — switch to a combat job in a PvP duty.", Dalamud.Interface.FontAwesomeIcon.Ban);
            return;
        }

        var sheet = Svc.Data.GetExcelSheet<LuminaAction>();
        var row = sheet?.GetRowOrDefault(state.ActionId);
        var actionName = row?.Name.ToString() ?? $"Action {state.ActionId}";
        var iconId = row?.Icon ?? 0;

        var target = ctrl.LastResolvedTarget;
        var wouldFire = !state.IsSupport && state.ActionReady && target != null && !target.IsDead
            && HpMath.IsBelowThreshold(target, cfg, state.JobId);
        var firing = wouldFire && cfg.Enabled;

        var border = CardBorders.Resolve(state.ActionReady, cfg.Enabled, wouldFire, firing);
        var iconSize = Layout.LbIconSize * ImGuiHelpers.GlobalScale;
        var lineSpacing = ImGui.GetTextLineHeightWithSpacing();
        var bodyLines = state.IsSupport ? 1 : 2;
        var contentH = MathF.Max(iconSize, lineSpacing) + bodyLines * lineSpacing;
        var paddingY = 8f * 2f * ImGuiHelpers.GlobalScale;
        var cardHeight = contentH + paddingY;

        using (Card.Begin("##lbcard", cardHeight, Styling.CardBg, border, firing ? 1.5f : 1.0f))
        {
            var rowTopY = ImGui.GetCursorPosY();
            DrawHeaderRow(state, cfg, iconId, iconSize, actionName, firing, wouldFire, rowTopY);
            DrawSubtitle(state, cfg, ctrl, firing, rowTopY, iconSize, lineSpacing);
        }
    }

    private static void DrawHeaderRow(LbDrawState state, Configuration cfg, uint iconId, float iconSize,
        string actionName, bool firing, bool wouldFire, float rowTopY)
    {
        DrawIcon(iconId, iconSize, firing, wouldFire, state.ActionReady, cfg.Enabled);
        ImGui.SameLine();

        var nameY = rowTopY + (iconSize - ImGui.GetTextLineHeight()) * 0.5f;
        ImGui.SetCursorPosY(nameY);
        ImGui.SetWindowFontScale(1.10f);
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextStrong))
            ImGui.TextUnformatted(actionName);
        ImGui.SetWindowFontScale(1.0f);

        DrawPillRightAligned(state, firing, wouldFire, cfg.Enabled, nameY);
    }

    private static void DrawSubtitle(LbDrawState state, Configuration cfg, AutoLbController ctrl,
        bool firing, float rowTopY, float iconSize, float lineSpacing)
    {
        ImGui.SetCursorPosY(rowTopY + MathF.Max(iconSize, lineSpacing) + 2f * ImGuiHelpers.GlobalScale);
        var threshLabel = state.IsSupport ? state.ModeBlurb : cfg.FormatEffective(state.JobId);
        using (ImRaii.PushColor(ImGuiCol.Text, state.IsSupport ? Styling.AccentAmber : Styling.TextDim))
            ImGui.TextUnformatted(threshLabel);

        if (!state.IsSupport)
        {
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextMuted))
                ImGui.TextUnformatted(state.Profile.Describe(ctrl.LastEnemiesAffected, firing));
        }
    }

    private static void DrawIcon(uint iconId, float size, bool firing, bool wouldFire, bool ready, bool enabled)
    {
        var pos = ImGui.GetCursorScreenPos();
        var draw = ImGui.GetWindowDrawList();

        if (firing)
        {
            var alpha = 0.35f + 0.55f * Styling.Pulse(Styling.PulseFast);
            var glow = new Vector4(Styling.AccentRed.X, Styling.AccentRed.Y, Styling.AccentRed.Z, alpha);
            draw.AddRectFilled(pos - new Vector2(6, 6), pos + new Vector2(size + 6, size + 6),
                ImGui.GetColorU32(glow), 10f);
        }

        var border = CardBorders.Resolve(ready, enabled, wouldFire, firing);
        draw.AddRectFilled(pos - new Vector2(3, 3), pos + new Vector2(size + 3, size + 3),
            ImGui.GetColorU32(border), 6f);

        if (iconId != 0)
        {
            var tex = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(iconId)).GetWrapOrDefault();
            if (tex != null)
            {
                ImGui.Image(tex.Handle, new Vector2(size, size));
                return;
            }
        }
        ImGui.Dummy(new Vector2(size, size));
    }

    private static void DrawPillRightAligned(LbDrawState state, bool firing, bool wouldFire, bool enabled, float yPos)
    {
        var text = state.IsSupport
            ? state.ModeLabel
            : StatusPill.Resolve(firing, wouldFire, state.Readiness, enabled).Text;
        var pillW = StatusPill.MeasureWidth(text);
        var pillX = ImGui.GetWindowContentRegionMax().X - pillW;

        ImGui.SameLine(pillX);
        ImGui.SetCursorPosY(yPos);
        if (state.IsSupport) StatusPill.DrawSupport(text);
        else StatusPill.Draw(firing, wouldFire, state.Readiness, enabled);
    }
}
