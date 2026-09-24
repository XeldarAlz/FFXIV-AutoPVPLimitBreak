namespace PvpAutoLb.Core.Localization;

internal static class GlyphRanges
{
    public const int CodepointCount = char.MaxValue + 1;
    public const int FirstNonAsciiCodepoint = 0x0080;

    public static bool MarkText(bool[] present, string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var added = false;
        for (var index = 0; index < text.Length; index++)
        {
            var codepoint = text[index];
            if (codepoint < FirstNonAsciiCodepoint || char.IsSurrogate(codepoint) || present[codepoint])
            {
                continue;
            }

            present[codepoint] = true;
            added = true;
        }

        return added;
    }

    public static void MarkRanges(bool[] present, ushort[]? ranges)
    {
        if (ranges is null)
        {
            return;
        }

        for (var index = 0; index + 1 < ranges.Length; index += 2)
        {
            if (ranges[index] == 0)
            {
                return;
            }

            for (int codepoint = ranges[index]; codepoint <= ranges[index + 1]; codepoint++)
            {
                present[codepoint] = true;
            }
        }
    }

    public static ushort[] ToRanges(bool[] present)
    {
        var ranges = new List<ushort>();
        var runStart = -1;
        for (var codepoint = 1; codepoint < CodepointCount; codepoint++)
        {
            if (present[codepoint])
            {
                if (runStart < 0)
                {
                    runStart = codepoint;
                }

                continue;
            }

            if (runStart < 0)
            {
                continue;
            }

            ranges.Add((ushort)runStart);
            ranges.Add((ushort)(codepoint - 1));
            runStart = -1;
        }

        if (runStart >= 0)
        {
            ranges.Add((ushort)runStart);
            ranges.Add(char.MaxValue);
        }

        ranges.Add(0);
        return [.. ranges];
    }
}
