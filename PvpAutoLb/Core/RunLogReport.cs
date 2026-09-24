using System;
using System.Globalization;
using System.Text;
using Dalamud.Plugin;
using ECommons.DalamudServices;

namespace PvpAutoLb.Core;

public static class RunLogReport
{
    private const int MaxSourceWidth = 24;
    private const string DetailIndent = "    ";
    private const string Separator = " | ";

    private static readonly string[] levelNames = ["Verbose", "Debug", "Info", "Warning", "Error"];

    public static string WithHeader(ReadOnlySpan<RunLogLine> lines, in RunLogFilter filter, int bufferedCount)
    {
        var builder = new StringBuilder(256 + lines.Length * 112);
        AppendEnvironment(builder);
        AppendScope(builder, lines.Length, filter, bufferedCount);
        builder.AppendLine();
        AppendLines(builder, lines);
        return builder.ToString();
    }

    public static string Plain(ReadOnlySpan<RunLogLine> lines)
    {
        var builder = new StringBuilder(lines.Length * 112);
        AppendLines(builder, lines);
        return builder.ToString();
    }

    private static void AppendEnvironment(StringBuilder builder)
    {
        var manifest = Svc.PluginInterface.Manifest;
        var dalamudVersion = typeof(IDalamudPluginInterface).Assembly.GetName().Version;
        var now = DateTimeOffset.Now;
        builder.Append(manifest.Name).Append(' ').Append(manifest.AssemblyVersion)
               .Append(Separator).Append("Dalamud ").Append(dalamudVersion)
               .Append(Separator).Append(now.ToString("yyyy-MM-dd HH:mm:ss 'UTC'zzz", CultureInfo.InvariantCulture))
               .Append(Separator).Append("Territory ").Append(Svc.ClientState.TerritoryType)
               .AppendLine();
    }

    private static void AppendScope(StringBuilder builder, int shown, in RunLogFilter filter, int bufferedCount)
    {
        builder.Append(shown).Append(" of ").Append(bufferedCount).Append(" lines");
        if (filter.LevelMask != RunLogFilter.AllLevels)
        {
            builder.Append(Separator).Append("levels: ");
            var first = true;
            for (var level = 0; level < RunLog.LevelCount; level++)
            {
                if (!filter.Shows((RunLogLevel)level))
                {
                    continue;
                }

                if (!first)
                {
                    builder.Append(", ");
                }

                builder.Append(levelNames[level]);
                first = false;
            }
        }

        if (filter.Search.Length > 0)
        {
            builder.Append(Separator).Append("search: \"").Append(filter.Search).Append('"');
        }

        if (filter.Source is not null)
        {
            builder.Append(Separator).Append("source: ").Append(filter.Source);
        }

        builder.AppendLine();
    }

    private static void AppendLines(StringBuilder builder, ReadOnlySpan<RunLogLine> lines)
    {
        var sourceWidth = 0;
        for (var index = 0; index < lines.Length; index++)
        {
            sourceWidth = Math.Max(sourceWidth, lines[index].Source.Length);
        }

        sourceWidth = Math.Min(sourceWidth, MaxSourceWidth);
        for (var index = 0; index < lines.Length; index++)
        {
            AppendLine(builder, lines[index], sourceWidth);
        }
    }

    private static void AppendLine(StringBuilder builder, in RunLogLine line, int sourceWidth)
    {
        builder.Append(RunLog.Time(line.AtUtc))
               .Append(' ')
               .Append(RunLog.Tag(line.Level))
               .Append(' ')
               .Append(line.Source.PadRight(sourceWidth))
               .Append("  ")
               .Append(line.Message);

        if (line.Repeat > 1)
        {
            builder.Append(" (x").Append(line.Repeat).Append(')');
        }

        builder.AppendLine();
        if (line.Detail is null)
        {
            return;
        }

        var detail = line.Detail.AsSpan();
        foreach (var detailLine in detail.EnumerateLines())
        {
            builder.Append(DetailIndent).Append(detailLine).AppendLine();
        }
    }
}
