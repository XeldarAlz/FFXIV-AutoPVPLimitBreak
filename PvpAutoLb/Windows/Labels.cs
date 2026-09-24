using PvpAutoLb.Core.Localization;

namespace PvpAutoLb.Windows;

// Percentages and distances repeat across every enemy row each frame, so each value is formatted once per
// language and served from a table.
internal static class Labels
{
    private const int PercentCount = 101;
    private const int YalmCount = 256;

    private static readonly string?[] percents = new string?[PercentCount];
    private static readonly string?[] yalms = new string?[YalmCount];

    private static LanguageInfo? language;

    public static string Percent(float fraction)
    {
        EnsureLanguage();
        var index = Math.Clamp((int)MathF.Round(fraction * 100f), 0, PercentCount - 1);
        return percents[index] ??= Loc.T(L.Live.Percent, index);
    }

    public static string Yalms(float distance)
    {
        EnsureLanguage();
        var index = Math.Clamp((int)MathF.Round(distance), 0, YalmCount - 1);
        return yalms[index] ??= Loc.T(L.Live.Yalms, index);
    }

    private static void EnsureLanguage()
    {
        if (ReferenceEquals(language, Loc.Current))
        {
            return;
        }

        language = Loc.Current;
        Array.Clear(percents);
        Array.Clear(yalms);
    }
}
