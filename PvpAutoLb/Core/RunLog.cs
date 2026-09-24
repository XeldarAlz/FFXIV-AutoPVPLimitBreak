using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using ECommons.DalamudServices;

namespace PvpAutoLb.Core;

public enum RunLogLevel : byte { Verbose, Debug, Info, Warning, Error }

public readonly record struct RunLogLine(long Sequence, DateTime AtUtc, RunLogLevel Level, string Source, string Message, string? Detail, int Repeat);

// In-window copy of everything the plugin logs, so a bug report can be pasted straight from the
// console instead of digging the [PvpAutoLb] lines out of dalamud.log. IPC callbacks log off the
// framework thread, so every access goes through the gate.
public static class RunLog
{
    public const int Capacity = 3000;
    public const int LevelCount = 5;

    private const string TimeFormat = "HH:mm:ss.fff";

    private static readonly object gate = new();
    private static readonly RunLogLine[] lines = new RunLogLine[Capacity];
    private static readonly Dictionary<string, string> sourceNames = new(StringComparer.Ordinal);
    private static int start;
    private static int count;
    private static int version;
    private static long nextSequence = 1;
    private static long latestWarningSequence;
    private static long latestErrorSequence;
    private static long seenSequence;

    public static int Version => Volatile.Read(ref version);

    public static int Count
    {
        get
        {
            lock (gate)
            {
                return count;
            }
        }
    }

    public static RunLogLevel? Unseen
    {
        get
        {
            lock (gate)
            {
                if (latestErrorSequence > seenSequence)
                {
                    return RunLogLevel.Error;
                }

                return latestWarningSequence > seenSequence ? RunLogLevel.Warning : null;
            }
        }
    }

    public static void Verbose(string message, [CallerFilePath] string callerFile = "")
    {
        Svc.Log.Verbose($"{PvpAutoLbConstants.LogPrefix} {message}");
        Push(RunLogLevel.Verbose, message, null, callerFile);
    }

    public static void Debug(string message, [CallerFilePath] string callerFile = "")
    {
        Svc.Log.Debug($"{PvpAutoLbConstants.LogPrefix} {message}");
        Push(RunLogLevel.Debug, message, null, callerFile);
    }

    public static void Info(string message, [CallerFilePath] string callerFile = "")
    {
        Svc.Log.Info($"{PvpAutoLbConstants.LogPrefix} {message}");
        Push(RunLogLevel.Info, message, null, callerFile);
    }

    public static void Warning(string message, [CallerFilePath] string callerFile = "")
    {
        Svc.Log.Warning($"{PvpAutoLbConstants.LogPrefix} {message}");
        Push(RunLogLevel.Warning, message, null, callerFile);
    }

    public static void Warning(Exception exception, string message, [CallerFilePath] string callerFile = "")
    {
        Svc.Log.Warning(exception, $"{PvpAutoLbConstants.LogPrefix} {message}");
        Push(RunLogLevel.Warning, message, exception.ToString(), callerFile);
    }

    public static void Error(string message, [CallerFilePath] string callerFile = "")
    {
        Svc.Log.Error($"{PvpAutoLbConstants.LogPrefix} {message}");
        Push(RunLogLevel.Error, message, null, callerFile);
    }

    public static void Error(Exception exception, string message, [CallerFilePath] string callerFile = "")
    {
        Svc.Log.Error(exception, $"{PvpAutoLbConstants.LogPrefix} {message}");
        Push(RunLogLevel.Error, message, exception.ToString(), callerFile);
    }

    // For lines another logger already wrote to dalamud.log, so the console still shows them without a duplicate there.
    public static void Record(RunLogLevel level, string message, [CallerFilePath] string callerFile = "")
        => Push(level, message, null, callerFile);

    public static void Clear()
    {
        lock (gate)
        {
            start = 0;
            count = 0;
            latestWarningSequence = 0;
            latestErrorSequence = 0;
            version++;
        }
    }

    public static void MarkSeen()
    {
        lock (gate)
        {
            seenSequence = nextSequence - 1;
        }
    }

    // Copies every buffered line that passes the filter into the caller's array and tallies each level
    // across the whole buffer, so the view can show both what matches and what the filter hides.
    public static int Snapshot(in RunLogFilter filter, RunLogLine[] destination, Span<int> levelTotals)
    {
        levelTotals.Clear();
        lock (gate)
        {
            var written = 0;
            for (var index = 0; index < count; index++)
            {
                var line = lines[(start + index) % Capacity];
                levelTotals[(int)line.Level]++;
                if (filter.Matches(line))
                {
                    destination[written++] = line;
                }
            }

            return written;
        }
    }

    public static string Time(DateTime atUtc) => atUtc.ToLocalTime().ToString(TimeFormat, CultureInfo.InvariantCulture);

    public static string Tag(RunLogLevel level) => level switch
    {
        RunLogLevel.Verbose => "VERB",
        RunLogLevel.Debug   => "DBUG",
        RunLogLevel.Warning => "WARN",
        RunLogLevel.Error   => "ERR ",
        _                   => "INFO",
    };

    private static void Push(RunLogLevel level, string message, string? detail, string callerFile)
    {
        lock (gate)
        {
            var now = DateTime.UtcNow;
            var source = SourceName(callerFile);
            var sequence = nextSequence++;
            if (level == RunLogLevel.Warning)
            {
                latestWarningSequence = sequence;
            }
            else if (level == RunLogLevel.Error)
            {
                latestErrorSequence = sequence;
            }

            if (count > 0)
            {
                var lastSlot = (start + count - 1) % Capacity;
                var last = lines[lastSlot];
                if (last.Level == level && ReferenceEquals(last.Source, source) && string.Equals(last.Message, message, StringComparison.Ordinal) && string.Equals(last.Detail, detail, StringComparison.Ordinal))
                {
                    lines[lastSlot] = last with { AtUtc = now, Repeat = last.Repeat + 1 };
                    version++;
                    return;
                }
            }

            var slot = (start + count) % Capacity;
            lines[slot] = new RunLogLine(sequence, now, level, source, message, detail, 1);
            if (count < Capacity)
            {
                count++;
            }
            else
            {
                start = (start + 1) % Capacity;
            }

            version++;
        }
    }

    private static string SourceName(string callerFile)
    {
        if (sourceNames.TryGetValue(callerFile, out var name))
        {
            return name;
        }

        name = Path.GetFileNameWithoutExtension(callerFile);
        var partial = name.IndexOf('.');
        if (partial > 0)
        {
            name = name[..partial];
        }

        sourceNames[callerFile] = name;
        return name;
    }
}
