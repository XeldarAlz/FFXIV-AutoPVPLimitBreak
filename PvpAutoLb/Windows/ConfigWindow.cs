using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using PvpAutoLb.Windows.Components;
using PvpAutoLb.Windows.Sections;

namespace PvpAutoLb.Windows;

public sealed class ConfigWindow : Window, IDisposable
{
    private enum Tab { Threshold, Targeting, PerJob, Filters, Blocklist, Feedback }

    private readonly Configuration cfg;
    private Tab activeTab = Tab.Threshold;

    public ConfigWindow(Plugin plugin) : base("Auto PVP LB — Settings###PvpAutoLbConfig")
    {
        Size = new Vector2(560, 520);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(480, 360),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
        cfg = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var style = Styling.PushWindowStyle();

        var sidebarWidth = 158f * ImGuiHelpers.GlobalScale;
        using (ImRaii.Child("##cfg_sidebar", new Vector2(sidebarWidth, -1), border: false))
            DrawSidebar();

        ImGui.SameLine();

        using (ImRaii.Child("##cfg_content", new Vector2(-1, -1), border: false))
            DrawContent();
    }

    private void DrawSidebar()
    {
        ImGui.Spacing();

        // Grouped so a newcomer can tell the two settings that change behaviour (Essentials) from the
        // optional tuning and the safety nets. Defaults are sane, so everything below Essentials is optional.
        DrawGroupLabel("ESSENTIALS");
        TabButton("When to fire", FontAwesomeIcon.HeartBroken, Tab.Threshold);
        TabButton("Targeting",    FontAwesomeIcon.Bullseye,    Tab.Targeting);

        DrawGroupLabel("TUNING");
        TabButton("Per-job",      FontAwesomeIcon.UserCog,     Tab.PerJob);
        TabButton("Filters",      FontAwesomeIcon.Filter,      Tab.Filters);

        DrawGroupLabel("SAFETY & FEEDBACK");
        TabButton("Blocklist",     FontAwesomeIcon.UserSlash,  Tab.Blocklist);
        TabButton("Notifications", FontAwesomeIcon.Bell,       Tab.Feedback);
    }

    private void TabButton(string label, FontAwesomeIcon icon, Tab tab)
    {
        if (SidebarTab.Draw(label, icon, Styling.AccentRed, activeTab == tab))
            activeTab = tab;
    }

    private static void DrawGroupLabel(string text)
    {
        ImGui.Spacing();
        ImGui.Indent(6f * ImGuiHelpers.GlobalScale);
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextMuted))
            ImGui.TextUnformatted(text);
        ImGui.Unindent(6f * ImGuiHelpers.GlobalScale);
        ImGui.Spacing();
    }

    private void DrawContent()
    {
        ImGui.Spacing();
        switch (activeTab)
        {
            case Tab.Threshold:
                DrawHeader("When to fire", "How low a target's HP must drop before the LB fires.");
                ThresholdSection.Draw(cfg);
                break;
            case Tab.Targeting:
                DrawHeader("Targeting", "Choose who the Limit Break lands on.");
                TargetingSection.Draw(cfg);
                break;
            case Tab.PerJob:
                DrawHeader("Per-job override", "Give your current job its own threshold, separate from the global one.");
                PerJobOverrideSection.Draw(cfg);
                break;
            case Tab.Filters:
                DrawHeader("Filters", "Don't waste the LB on targets it can't kill, and limit it to the right duties.");
                FilterSection.Draw(cfg);
                break;
            case Tab.Blocklist:
                DrawHeader("Player blocklist", "Names here are never auto-targeted, even when below threshold.");
                BlocklistSection.Draw(cfg);
                break;
            case Tab.Feedback:
                DrawHeader("Notifications", "Optional sound and chat cues when the LB fires.");
                FeedbackSection.Draw(cfg);
                break;
        }
    }

    private static void DrawHeader(string title, string subtitle)
    {
        ImGui.SetWindowFontScale(1.5f);
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextStrong))
            ImGui.TextUnformatted(title);
        ImGui.SetWindowFontScale(1.0f);

        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextMuted))
            ImGui.TextWrapped(subtitle);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
    }
}
