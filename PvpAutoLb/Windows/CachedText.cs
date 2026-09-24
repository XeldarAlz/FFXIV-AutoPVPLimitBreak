using PvpAutoLb.Core.Localization;

namespace PvpAutoLb.Windows;

// Lines like "Fired 12s ago" are drawn every frame but only change with their inputs, so the formatted text is
// kept until the key or the language changes.
internal sealed class CachedText
{
    private long key = long.MinValue;
    private LanguageInfo? language;

    public string Text { get; private set; } = string.Empty;

    public bool Matches(long candidate) => key == candidate && ReferenceEquals(language, Loc.Current);

    public string Store(long candidate, string text)
    {
        key = candidate;
        language = Loc.Current;
        Text = text;
        return text;
    }
}
