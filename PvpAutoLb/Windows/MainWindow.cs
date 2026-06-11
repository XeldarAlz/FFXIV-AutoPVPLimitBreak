using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using PvpAutoLb.Core;
using PvpAutoLb.Windows.Components;
using PvpAutoLb.Windows.Sections;

namespace PvpAutoLb.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin) : base("Auto PVP LB###PvpAutoLbMain")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(320, 280),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
        Size = new Vector2(420, 440);
        SizeCondition = ImGuiCond.FirstUseEver;
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var cfg = plugin.Configuration;
        var ctrl = plugin.Controller;
        var state = LbDrawState.Resolve(ctrl, cfg);

        using var style = Styling.PushWindowStyle();

        TopToolbar.Draw(plugin);
        MasterButton.Draw(cfg);
        ImGui.Spacing();

        StatusLine.Draw(cfg, ctrl, state);
        ImGui.Spacing();

        if (cfg.Enabled)
        {
            TargetSection.Draw(cfg, ctrl, state);
            ImGui.Spacing();
        }

        LbCard.Draw(cfg, ctrl, state);
        ImGui.Spacing();

        Footer.Draw(ctrl, cfg);
    }
}
