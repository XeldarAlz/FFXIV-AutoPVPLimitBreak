using System.Diagnostics;
using ECommons.DalamudServices;
using Lumina.Excel.Sheets;

namespace PvpAutoLb.Core.Localization;

// Job and Limit Break names follow the client language, which is picked independently of the plugin
// language, so their glyphs come from the game sheets rather than from the active catalog.
internal static class GameTextGlyphs
{
    private static readonly bool[] present = new bool[GlyphRanges.CodepointCount];

    public static int Version { get; private set; }

    public static ReadOnlySpan<bool> Present => present;

    public static void Collect()
    {
        var watch = Stopwatch.StartNew();
        try
        {
            ScanJobs();
        }
        catch (Exception exception)
        {
            RunLog.Error(exception, "Failed to collect game text glyphs; some job or Limit Break names may render as missing glyphs");
            return;
        }

        RunLog.Info($"Collected {CountGlyphs()} game text glyphs in {watch.ElapsedMilliseconds} ms");
    }

    private static void Add(string? text)
    {
        if (GlyphRanges.MarkText(present, text))
        {
            Version++;
        }
    }

    private static void ScanJobs()
    {
        var jobs = Svc.Data.GetExcelSheet<ClassJob>();
        for (var index = 0; index < jobs.Count; index++)
        {
            var job = jobs.GetRowAt(index);
            Add(job.Name.ExtractText());
            Add(job.Abbreviation.ExtractText());

            var actionIds = LbCatalog.ResolveActionIds(job.RowId);
            for (var actionIndex = 0; actionIndex < actionIds.Count; actionIndex++)
            {
                Add(LbCatalog.GetActionName(actionIds[actionIndex]));
            }
        }
    }

    private static int CountGlyphs()
    {
        var count = 0;
        for (var codepoint = 0; codepoint < present.Length; codepoint++)
        {
            if (present[codepoint])
            {
                count++;
            }
        }

        return count;
    }
}
