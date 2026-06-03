using System;
using System.Collections.Generic;
using System.Linq;
using ECommons.DalamudServices;
using LuminaAction = Lumina.Excel.Sheets.Action;
using LuminaActionCategory = Lumina.Excel.Sheets.ActionCategory;
using LuminaClassJob = Lumina.Excel.Sheets.ClassJob;

namespace PvpAutoLb.Core;

internal static class LbCatalog
{
    private static readonly uint[] EmptyIds = Array.Empty<uint>();

    private static readonly Lazy<Dictionary<uint, uint[]>> ActionsByJobId =
        new(BuildIndex, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Dictionary<uint, LbTargetingProfile> ProfileCache = new();

    public static IReadOnlyList<uint> ResolveActionIds(uint classJobId)
        => ActionsByJobId.Value.TryGetValue(classJobId, out var ids) ? ids : EmptyIds;

    public static LbTargetingProfile ResolveProfile(uint classJobId)
    {
        var ids = ResolveActionIds(classJobId);
        var actionId = ids.Count > 0 ? ids[0] : 0u;
        if (actionId == 0) return LbTargetingProfile.None;
        if (!ProfileCache.TryGetValue(actionId, out var p))
        {
            p = LbTargetingProfile.FromAction(actionId);
            ProfileCache[actionId] = p;
        }
        return p;
    }

    public static string GetActionName(uint actionId)
    {
        var sheet = Svc.Data.GetExcelSheet<LuminaAction>();
        return sheet?.GetRowOrDefault(actionId)?.Name.ToString() ?? $"action {actionId}";
    }

    private static Dictionary<uint, uint[]> BuildIndex()
    {
        var sheet = Svc.Data.GetExcelSheet<LuminaAction>();
        if (sheet == null) return new Dictionary<uint, uint[]>();

        var primaries = sheet
            .Where(a => a.IsPvP)
            .Where(a => a.ActionCategory.RowId == PvpAutoLbConstants.LimitBreakCategoryId)
            .Where(a => a.ClassJob.RowId != 0 && a.ClassJob.RowId != uint.MaxValue)
            .GroupBy(a => a.ClassJob.RowId)
            .ToDictionary(g => g.Key, g => g.Select(a => a.RowId).ToList());

        var followUpsByParent = sheet
            .Where(a => a.IsPvP && a.ActionCombo.RowId != 0)
            .GroupBy(a => a.ActionCombo.RowId)
            .ToDictionary(g => g.Key, g => g.Select(a => a.RowId).ToArray());

        var result = new Dictionary<uint, uint[]>(primaries.Count);
        foreach (var (jobId, primaryIds) in primaries)
        {
            var ordered = new List<uint>(primaryIds);
            var seen = new HashSet<uint>(primaryIds);
            var queue = new Queue<uint>(primaryIds);
            while (queue.Count > 0)
            {
                var parent = queue.Dequeue();
                if (!followUpsByParent.TryGetValue(parent, out var followUps)) continue;
                foreach (var id in followUps)
                {
                    if (seen.Add(id))
                    {
                        ordered.Add(id);
                        queue.Enqueue(id);
                    }
                }
            }
            result[jobId] = ordered.ToArray();
        }

        LogResolvedMap(result);
        if (result.Count == 0) DumpDiagnostics();
        return result;
    }

    private static void LogResolvedMap(Dictionary<uint, uint[]> map)
    {
        Svc.Log.Info($"{PvpAutoLbConstants.LogPrefix} resolved PvP LBs for {map.Count} jobs");
        var jobs = Svc.Data.GetExcelSheet<LuminaClassJob>();
        var actions = Svc.Data.GetExcelSheet<LuminaAction>();
        foreach (var (jobId, ids) in map.OrderBy(kv => kv.Key))
        {
            var jobName = jobs?.GetRowOrDefault(jobId)?.Abbreviation.ToString() ?? $"Job{jobId}";
            var parts = string.Join(", ", ids.Select(id =>
            {
                var name = actions?.GetRowOrDefault(id)?.Name.ToString() ?? $"Action{id}";
                return $"{id} ({name})";
            }));
            Svc.Log.Debug($"{PvpAutoLbConstants.LogPrefix}   {jobId} {jobName} -> [{parts}]");
        }
    }

    private static void DumpDiagnostics()
    {
        var sheet = Svc.Data.GetExcelSheet<LuminaAction>();
        if (sheet == null) { Svc.Log.Error($"{PvpAutoLbConstants.LogPrefix} diag: action sheet is null"); return; }

        var categories = Svc.Data.GetExcelSheet<LuminaActionCategory>();
        Svc.Log.Info($"{PvpAutoLbConstants.LogPrefix} diag: IsPvP actions by ActionCategory:");
        foreach (var g in sheet.Where(a => a.IsPvP).GroupBy(a => a.ActionCategory.RowId).OrderByDescending(g => g.Count()))
        {
            var name = categories?.GetRowOrDefault(g.Key)?.Name.ToString() ?? "?";
            Svc.Log.Info($"{PvpAutoLbConstants.LogPrefix}   cat={g.Key} ({name}) count={g.Count()}");
        }
    }
}
