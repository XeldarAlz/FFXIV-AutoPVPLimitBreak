using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace PvpAutoLb.Windows;

internal static class TextDraw
{
    private const string Ellipsis = "…";
    private const int TruncateCacheSize = 16;

    private static readonly TruncatedText[] truncateCache = new TruncatedText[TruncateCacheSize];

    private static int truncateCacheNext;

    private readonly record struct TruncatedText(string Source, float MaxWidth, float FontSize, string Result);

    public static Vector2 Measure(string text) => ImGui.CalcTextSize(text);

    public static Vector2 MeasureWrapped(string text, float wrapWidth) => ImGui.CalcTextSize(text, false, wrapWidth);

    public static float LineHeight() => ImGui.GetTextLineHeight();

    public static void At(string text, Vector2 position, Vector4 color)
        => ImGui.GetWindowDrawList().AddText(position, Paint.Col(color), text);

    public static void Middle(string text, Vector2 min, Vector2 max, Vector4 color)
    {
        var size = Measure(text);
        At(text, new Vector2((min.X + max.X - size.X) * 0.5f, (min.Y + max.Y - size.Y) * 0.5f), color);
    }

    public static void Wrapped(string text, Vector2 position, float wrapWidth, Vector4 color)
        => ImGui.GetWindowDrawList().AddText(ImGui.GetFont(), ImGui.GetFontSize(), position, Paint.Col(color), text, wrapWidth);

    public static Vector2 IconSize(FontAwesomeIcon icon)
    {
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            return Measure(icon.ToIconString());
        }
    }

    public static void Icon(FontAwesomeIcon icon, Vector2 position, Vector4 color)
    {
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            At(icon.ToIconString(), position, color);
        }
    }

    public static void IconCentered(FontAwesomeIcon icon, Vector2 center, Vector4 color)
    {
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            var glyph = icon.ToIconString();
            At(glyph, center - Measure(glyph) * 0.5f, color);
        }
    }

    // A line that overflows its slot overflows on every frame it is drawn, so its cut is cached, and prefixes are measured
    // in place, so finding the cut allocates only the result.
    public static string Truncate(string text, float maxWidth)
    {
        if (string.IsNullOrEmpty(text) || maxWidth <= 0f)
        {
            return string.Empty;
        }

        if (Measure(text).X <= maxWidth)
        {
            return text;
        }

        var fontSize = ImGui.GetFontSize();
        for (var index = 0; index < truncateCache.Length; index++)
        {
            var cached = truncateCache[index];
            if (ReferenceEquals(cached.Source, text) && cached.MaxWidth == maxWidth && cached.FontSize == fontSize)
            {
                return cached.Result;
            }
        }

        var result = Cut(text, maxWidth);
        truncateCache[truncateCacheNext] = new TruncatedText(text, maxWidth, fontSize, result);
        truncateCacheNext = (truncateCacheNext + 1) % TruncateCacheSize;
        return result;
    }

    private static string Cut(string text, float maxWidth)
    {
        var budget = maxWidth - Measure(Ellipsis).X;
        if (budget <= 0f)
        {
            return Ellipsis;
        }

        var low = 1;
        var high = text.Length - 1;
        while (low < high)
        {
            var middle = (low + high + 1) / 2;
            if (ImGui.CalcTextSize(text.AsSpan(0, middle)).X <= budget)
            {
                low = middle;
            }
            else
            {
                high = middle - 1;
            }
        }

        return string.Concat(text.AsSpan(0, low), Ellipsis);
    }
}
