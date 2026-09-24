using System.Globalization;

namespace PvpAutoLb.Windows;

// Labels like "12 lines selected" are drawn every frame but only change with their count, so the text is kept until it does.
internal sealed class CountedText
{
    private readonly string singular;
    private readonly string plural;
    private int count = -1;
    private string text = string.Empty;

    public CountedText(string singular, string plural)
    {
        this.singular = singular;
        this.plural = plural;
    }

    public string For(int value)
    {
        if (value == count)
        {
            return text;
        }

        count = value;
        text = string.Format(CultureInfo.InvariantCulture, value == 1 ? singular : plural, value.ToString("N0", CultureInfo.InvariantCulture));
        return text;
    }
}
