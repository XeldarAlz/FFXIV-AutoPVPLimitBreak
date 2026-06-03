using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class Footer
{
    private const float ValueScale = 1.35f;

    public static void Draw(AutoLbController ctrl, Configuration cfg)
    {
        DrawHeader("Session", "##reset_session", "Reset session stats", ctrl.Stats.ResetSession);

        var lineH = ImGui.GetTextLineHeight();
        var gap = 2f * ImGuiHelpers.GlobalScale;
        var padY = 8f * ImGuiHelpers.GlobalScale;
        var cardH = lineH * ValueScale + gap + lineH + padY * 2;

        using (Card.Begin("##statscard", cardH, Styling.CardBgSoft, Styling.CardBorderDim))
            DrawTiles(ctrl, gap);

        ImGui.Spacing();
        DrawLifetimeLine(ctrl, cfg);
        DrawLastFiredLine(ctrl);
    }

    private static void DrawTiles(AutoLbController ctrl, float gap)
    {
        var s = ctrl.Stats;
        (string Label, long Value)[] tiles =
        {
            ("Fires", s.TotalFires),
            ("Kills", s.KillsAttributed),
            ("Hits",  s.EnemiesAffectedTotal),
        };

        var lineH = ImGui.GetTextLineHeight();
        var startX = ImGui.GetCursorPosX();
        var startY = ImGui.GetCursorPosY();
        var screen0 = ImGui.GetCursorScreenPos();
        var colW = ImGui.GetContentRegionAvail().X / 3f;

        var dl = ImGui.GetWindowDrawList();
        var divCol = ImGui.GetColorU32(new Vector4(
            Styling.CardBorderDim.X, Styling.CardBorderDim.Y, Styling.CardBorderDim.Z, 0.45f));
        var top = screen0.Y;
        var bottom = screen0.Y + lineH * ValueScale + gap + lineH;
        for (var i = 1; i < 3; i++)
            dl.AddLine(new Vector2(screen0.X + colW * i, top), new Vector2(screen0.X + colW * i, bottom), divCol, 1f);

        ImGui.SetWindowFontScale(ValueScale);
        for (var i = 0; i < 3; i++)
        {
            var text = tiles[i].Value.ToString("N0");
            ImGui.SetCursorPosX(startX + colW * i + (colW - ImGui.CalcTextSize(text).X) * 0.5f);
            ImGui.SetCursorPosY(startY);
            using (ImRaii.PushColor(ImGuiCol.Text, tiles[i].Value > 0 ? Styling.TextStrong : Styling.TextDim))
                ImGui.TextUnformatted(text);
        }
        ImGui.SetWindowFontScale(1f);

        var labelY = startY + lineH * ValueScale + gap;
        for (var i = 0; i < 3; i++)
        {
            var text = tiles[i].Label;
            ImGui.SetCursorPosX(startX + colW * i + (colW - ImGui.CalcTextSize(text).X) * 0.5f);
            ImGui.SetCursorPosY(labelY);
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextMuted))
                ImGui.TextUnformatted(text);
        }
    }

    private static void DrawHeader(string label, string idSuffix, string tooltip, Action onReset)
    {
        ImGui.AlignTextToFramePadding();
        Styling.SectionLabel(label);
        Styling.SameLineRightAligned(ResetButtonWidth());
        GhostButton("Reset" + idSuffix, tooltip, onReset);
    }

    private static void DrawLifetimeLine(AutoLbController ctrl, Configuration cfg)
    {
        ImGui.AlignTextToFramePadding();
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextMuted))
            ImGui.TextUnformatted(
                $"Lifetime  {cfg.LifetimeFires:N0} fires · {cfg.LifetimeKills:N0} kills · {cfg.LifetimeEnemiesAffected:N0} hits");

        Styling.SameLineRightAligned(ResetButtonWidth());
        GhostButton("Reset##reset_lifetime", "Reset lifetime stats", ctrl.Stats.ResetLifetime);
    }

    private static void DrawLastFiredLine(AutoLbController ctrl)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextMuted))
        {
            var fired = ctrl.LastFiredUtc is { } ts
                ? $"Last fired {(DateTime.UtcNow - ts).TotalSeconds:F1}s ago"
                : "Last fired: never";
            var build = $"build {typeof(Footer).Assembly.GetName().Version}";
            ImGui.TextUnformatted(fired);
            Styling.SameLineRightAligned(ImGui.CalcTextSize(build).X);
            ImGui.TextUnformatted(build);
        }
    }

    private static float ResetButtonWidth()
        => ImGui.CalcTextSize("Reset").X + ImGui.GetStyle().FramePadding.X * 2;

    private static void GhostButton(string idLabel, string tooltip, Action onClick)
    {
        bool clicked;
        using (ImRaii.PushColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0)))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, Styling.CardBgHover))
        using (ImRaii.PushColor(ImGuiCol.ButtonActive, Styling.CardBgHover))
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
            clicked = ImGui.Button(idLabel);
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(tooltip);
        if (clicked) onClick();
    }
}
