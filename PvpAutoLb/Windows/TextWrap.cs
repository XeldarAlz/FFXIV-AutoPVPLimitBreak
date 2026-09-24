using Dalamud.Bindings.ImGui;
using System.Text;

namespace PvpAutoLb.Windows;

internal static class TextWrap
{
    // Breaks at spaces with the current font, and mid-word only when a single word is wider than the line.
    public static string Hard(string text, float width)
    {
        if (width <= 0f || ImGui.CalcTextSize(text).X <= width && text.IndexOf('\n') < 0)
        {
            return text;
        }

        var builder = new StringBuilder(text.Length + text.Length / 32);
        var lineStart = 0;
        while (lineStart <= text.Length)
        {
            var lineEnd = text.IndexOf('\n', lineStart);
            if (lineEnd < 0)
            {
                lineEnd = text.Length;
            }

            var sourceLine = text.AsSpan(lineStart, lineEnd - lineStart).TrimEnd('\r');
            AppendWrapped(builder, sourceLine, width);
            if (lineEnd >= text.Length)
            {
                break;
            }

            builder.Append('\n');
            lineStart = lineEnd + 1;
        }

        return builder.ToString();
    }

    private static void AppendWrapped(StringBuilder builder, ReadOnlySpan<char> line, float width)
    {
        while (ImGui.CalcTextSize(line).X > width)
        {
            var cut = FitLength(line, width);
            var space = line[..cut].LastIndexOf(' ');
            if (space > 0 && cut < line.Length)
            {
                cut = space;
            }

            builder.Append(line[..cut]).Append('\n');
            line = line[cut..].TrimStart(' ');
        }

        builder.Append(line);
    }

    private static int FitLength(ReadOnlySpan<char> line, float width)
    {
        var low = 1;
        var high = line.Length;
        while (low < high)
        {
            var middle = (low + high + 1) / 2;
            if (ImGui.CalcTextSize(line[..middle]).X <= width)
            {
                low = middle;
            }
            else
            {
                high = middle - 1;
            }
        }

        return low;
    }
}
