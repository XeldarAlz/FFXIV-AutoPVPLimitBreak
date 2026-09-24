using PvpAutoLb.Core.Localization;

namespace PvpAutoLb.Windows;

internal static class Formatting
{
    private const int SecondsPerMinute = 60;
    private const int SecondsPerHour = 3600;

    public static string Elapsed(int totalSeconds)
    {
        var seconds = Math.Max(0, totalSeconds);
        return seconds >= SecondsPerHour
            ? Loc.T(L.Live.DurationHours, seconds / SecondsPerHour, seconds % SecondsPerHour / SecondsPerMinute)
            : Loc.T(L.Live.DurationMinutes, seconds / SecondsPerMinute, seconds % SecondsPerMinute);
    }

    public static string Ago(int totalSeconds)
    {
        var seconds = Math.Max(0, totalSeconds);
        if (seconds < SecondsPerMinute)
        {
            return Loc.T(L.Live.AgoSeconds, seconds);
        }

        return seconds < SecondsPerHour
            ? Loc.T(L.Live.AgoMinutes, seconds / SecondsPerMinute)
            : Loc.T(L.Live.AgoHours, seconds / SecondsPerHour);
    }

    public static string Count(long value) => value.ToString("N0", Loc.Culture);

    // English job names are lowercase in the game sheets.
    public static string Capitalize(string text)
        => text.Length == 0 ? text : char.ToUpper(text[0], Loc.Culture) + text[1..];
}
