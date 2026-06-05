using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ECommons;
using PvpAutoLb.Core;
using PvpAutoLb.Windows;

namespace PvpAutoLb;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;

    private const string PrimaryCommand = "/pvpautolb";
    private const string AliasCommand = "/palb";

    internal Configuration Configuration { get; }
    internal WindowSystem WindowSystem { get; } = new("PvpAutoLb");
    internal AutoLbController Controller { get; }

    private readonly PresetIpc presetIpc;
    private readonly ConfigWindow configWindow;
    private readonly MainWindow mainWindow;
    private readonly AboutWindow aboutWindow;

    public Plugin()
    {
        ECommonsMain.Init(PluginInterface, this);

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.MigrateIfNeeded();
        Controller = new AutoLbController(Configuration);
        presetIpc = new PresetIpc(Configuration);

        configWindow = new ConfigWindow(this);
        mainWindow = new MainWindow(this);
        aboutWindow = new AboutWindow();

        WindowSystem.AddWindow(configWindow);
        WindowSystem.AddWindow(mainWindow);
        WindowSystem.AddWindow(aboutWindow);

        CommandManager.AddHandler(PrimaryCommand, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle the Auto PVP LB main window. Use /pvpautolb config to open settings."
        });
        CommandManager.AddHandler(AliasCommand, new CommandInfo(OnCommand)
        {
            HelpMessage = "Alias for /pvpautolb."
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

        WindowSystem.RemoveAllWindows();

        configWindow.Dispose();
        mainWindow.Dispose();
        aboutWindow.Dispose();
        presetIpc.Dispose();
        Controller.Dispose();

        CommandManager.RemoveHandler(PrimaryCommand);
        CommandManager.RemoveHandler(AliasCommand);

        ECommonsMain.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        if (args.Trim().Equals("config", StringComparison.OrdinalIgnoreCase))
            ToggleConfigUi();
        else
            ToggleMainUi();
    }

    public void ToggleConfigUi() => configWindow.Toggle();
    public void ToggleMainUi() => mainWindow.Toggle();
    public void ToggleAboutUi() => aboutWindow.Toggle();
}
