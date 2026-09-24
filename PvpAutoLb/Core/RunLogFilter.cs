using System;

namespace PvpAutoLb.Core;

public readonly record struct RunLogFilter(int LevelMask, string Search, string? Source)
{
    public const int AllLevels = (1 << RunLog.LevelCount) - 1;

    public static readonly RunLogFilter Everything = new(AllLevels, string.Empty, null);

    public bool Shows(RunLogLevel level) => (LevelMask & (1 << (int)level)) != 0;

    public bool IsNarrowed => LevelMask != AllLevels || Search.Length > 0 || Source is not null;

    public bool Matches(in RunLogLine line)
    {
        if (!Shows(line.Level))
        {
            return false;
        }

        if (Source is not null && !string.Equals(line.Source, Source, StringComparison.Ordinal))
        {
            return false;
        }

        if (Search.Length == 0)
        {
            return true;
        }

        return line.Message.Contains(Search, StringComparison.OrdinalIgnoreCase)
            || line.Source.Contains(Search, StringComparison.OrdinalIgnoreCase)
            || (line.Detail is not null && line.Detail.Contains(Search, StringComparison.OrdinalIgnoreCase));
    }
}
