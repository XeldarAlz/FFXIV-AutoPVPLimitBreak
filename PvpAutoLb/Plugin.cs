using System.Globalization;
using System.IO;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.DalamudServices;
using PvpAutoLb.Core;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows;
using PvpAutoLb.Windows.Shell;

namespace PvpAutoLb;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;

    private const string PrimaryCommand = "/pvpautolb";
    private const string AliasCommand = "/palb";
    private const string ConfigArgument = "config";
    private const string SettingsArgument = "settings";
    private const string LogArgument = "log";
    private const string ConsoleArgument = "console";
    private const string ChangelogArgument = "changelog";
    private const string AboutArgument = "about";
    private const string HudArgument = "hud";

    internal static Plugin Instance { get; private set; } = null!;

    internal Configuration Configuration { get; }
    internal WindowSystem WindowSystem { get; } = new("PvpAutoLb");
    internal AutoLbController Controller { get; }
    internal CombatHud CombatHud { get; }

    private readonly PresetIpc presetIpc;
    private readonly AppWindow appWindow;
    private readonly CommandInfo primaryCommand;
    private readonly CommandInfo aliasCommand;

    public Plugin()
    {
        Instance = this;
        ECommonsMain.Init(PluginInterface, this);

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.MigrateIfNeeded();
        Controller = new AutoLbController(Configuration);
        presetIpc = new PresetIpc(Configuration);

        InitializeLocalization();
        GameTextGlyphs.Collect();
        Fonts.Initialize(PluginInterface.UiBuilder, PluginDirectory);

        appWindow = new AppWindow(this);
        CombatHud = new CombatHud(this);
        WindowSystem.AddWindow(appWindow);
        WindowSystem.AddWindow(CombatHud);

        primaryCommand = new CommandInfo(OnCommand) { HelpMessage = Loc.T(L.Plugin.CommandHelp) };
        aliasCommand = new CommandInfo(OnCommand) { HelpMessage = Loc.T(L.Plugin.CommandHelpAlias) };
        CommandManager.AddHandler(PrimaryCommand, primaryCommand);
        CommandManager.AddHandler(AliasCommand, aliasCommand);

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        Svc.ClientState.Login += OnLogin;
        if (Svc.ClientState.IsLoggedIn)
        {
            OnLogin();
        }
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        Svc.ClientState.Login -= OnLogin;

        WindowSystem.RemoveAllWindows();
        appWindow.Dispose();
        CombatHud.Dispose();
        Fonts.Dispose();

        presetIpc.Dispose();
        Controller.Dispose();

        CommandManager.RemoveHandler(PrimaryCommand);
        CommandManager.RemoveHandler(AliasCommand);

        ECommonsMain.Dispose();
    }

    public void OnLanguageChanged()
    {
        primaryCommand.HelpMessage = Loc.T(L.Plugin.CommandHelp);
        aliasCommand.HelpMessage = Loc.T(L.Plugin.CommandHelpAlias);
    }

    private void OnCommand(string command, string args)
    {
        var argument = args.Trim();
        if (Matches(argument, ConfigArgument) || Matches(argument, SettingsArgument))
        {
            ToggleConfigUi();
            return;
        }

        if (Matches(argument, LogArgument) || Matches(argument, ConsoleArgument))
        {
            ToggleConsoleUi();
            return;
        }

        if (Matches(argument, ChangelogArgument))
        {
            ToggleChangelogUi();
            return;
        }

        if (Matches(argument, AboutArgument))
        {
            ToggleAboutUi();
            return;
        }

        if (Matches(argument, HudArgument))
        {
            CombatHud.ToggleVisible();
            return;
        }

        ToggleMainUi();
    }

    private static bool Matches(string argument, string expected)
        => argument.Equals(expected, StringComparison.OrdinalIgnoreCase);

    private static string PluginDirectory => PluginInterface.AssemblyLocation.DirectoryName ?? string.Empty;

    private void InitializeLocalization()
    {
        if (string.IsNullOrEmpty(Configuration.Language))
        {
            Configuration.Language = DetectLanguage();
            Configuration.Save();
        }

        Loc.Initialize(Configuration.Language, Path.Combine(PluginDirectory, "Localization"));
    }

    private static string DetectLanguage()
    {
        var dalamudLanguage = PluginInterface.UiLanguage;
        if (Languages.IsKnown(dalamudLanguage))
        {
            return Languages.Resolve(dalamudLanguage).Code;
        }

        switch (Svc.ClientState.ClientLanguage)
        {
            case Dalamud.Game.ClientLanguage.German:
                return Languages.German.Code;
            case Dalamud.Game.ClientLanguage.French:
                return Languages.French.Code;
            case Dalamud.Game.ClientLanguage.Japanese:
                return Languages.Japanese.Code;
        }

        var osLanguage = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
        return Languages.IsKnown(osLanguage) ? Languages.Resolve(osLanguage).Code : Languages.English.Code;
    }

    private void OnLogin()
    {
        if (!Configuration.AutoShowOnLogin)
        {
            return;
        }

        appWindow.Show(AppWindow.Page.Live);
    }

    public void ToggleMainUi() => appWindow.Toggle();
    public void ToggleConfigUi() => appWindow.TogglePage(AppWindow.Page.Settings);
    public void ToggleConsoleUi() => appWindow.TogglePage(AppWindow.Page.Console);
    public void ToggleChangelogUi() => appWindow.TogglePage(AppWindow.Page.Changelog);
    public void ToggleAboutUi() => appWindow.TogglePage(AppWindow.Page.About);
}
