using System;
using System.Collections.Generic;
using Dalamud.Plugin.Ipc;
using ECommons.DalamudServices;
using Newtonsoft.Json;

namespace PvpAutoLb.Core;

internal sealed class PresetIpc : IDisposable
{
    private readonly Configuration cfg;
    private readonly ICallGateProvider<int> apiVersion;
    private readonly ICallGateProvider<int> getVersion;
    private readonly ICallGateProvider<string, int, bool> apply;

    public PresetIpc(Configuration cfg)
    {
        this.cfg = cfg;

        apiVersion = Svc.PluginInterface.GetIpcProvider<int>(PvpAutoLbConstants.Ipc.ApiVersion);
        getVersion = Svc.PluginInterface.GetIpcProvider<int>(PvpAutoLbConstants.Ipc.GetVersion);
        apply = Svc.PluginInterface.GetIpcProvider<string, int, bool>(PvpAutoLbConstants.Ipc.Apply);

        apiVersion.RegisterFunc(() => PvpAutoLbConstants.PresetApiVersion);
        getVersion.RegisterFunc(() => cfg.PresetVersion);
        apply.RegisterFunc(Apply);
    }

    private bool Apply(string rulesJson, int presetVersion)
    {
        try
        {
            var rules = JsonConvert.DeserializeObject<Dictionary<uint, LbRule>>(rulesJson);
            if (rules == null || rules.Count == 0) return false;
            cfg.ApplyPresets(rules, presetVersion);
            Svc.Log.Info($"{PvpAutoLbConstants.LogPrefix} applied {rules.Count} preset rules (v{presetVersion})");
            return true;
        }
        catch (Exception ex)
        {
            Svc.Log.Warning(ex, $"{PvpAutoLbConstants.LogPrefix} preset apply failed");
            return false;
        }
    }

    public void Dispose()
    {
        apiVersion.UnregisterFunc();
        getVersion.UnregisterFunc();
        apply.UnregisterFunc();
    }
}
