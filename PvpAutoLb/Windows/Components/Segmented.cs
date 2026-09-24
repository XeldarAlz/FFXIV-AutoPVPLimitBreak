using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using System.Numerics;

namespace PvpAutoLb.Windows.Components;

internal static class Segmented
{
    public readonly record struct Item(FontAwesomeIcon? Icon, string Label);

    private readonly record struct Metrics(Vector2 Icon, Vector2 Label, float Content);

    public const int MaxItems = 8;

    private const float Inset = 3f;
    private const float SegmentPadX = 14f;
    private const float IconGap = 8f;

    private static readonly Metrics[] metrics = new Metrics[MaxItems];
    private static readonly float[] segmentWidths = new float[MaxItems];

    public static bool Draw(string id, ReadOnlySpan<Item> items, ref int selected, bool enabled = true, float height = Layout.SegmentHeight, float width = 0f)
    {
        var count = Math.Min(items.Length, MaxItems);
        if (count == 0)
        {
            return false;
        }

        var scale = ImGuiHelpers.GlobalScale;
        var size = new Vector2(width > 0f ? width : ImGui.GetContentRegionAvail().X, height * scale);
        var origin = ImGui.GetCursorScreenPos();
        var end = origin + size;
        var inset = Inset * scale;
        var rounding = size.Y * 0.5f;
        var drawList = ImGui.GetWindowDrawList();
        var current = Math.Clamp(selected, 0, count - 1);

        ResolveWidths(items, count, size.X - inset * 2f);

        Paint.Fill(drawList, origin, end, Styling.WithAlpha(Styling.Surface0, 0.9f), rounding);
        Paint.Stroke(drawList, origin, end, Styling.WithAlpha(Styling.BorderDim, 0.6f), rounding);

        ImGui.PushID(id);
        var indicatorX = Motion.Approach(Motion.Key("##seg"), SegmentOffset(current), 18f);
        var indicatorWidth = Motion.Approach(Motion.Key("##seg", 1), segmentWidths[current], 18f);
        var indicatorMin = new Vector2(origin.X + inset + indicatorX, origin.Y + inset);
        var indicatorMax = indicatorMin + new Vector2(indicatorWidth, size.Y - inset * 2f);
        var indicatorAccent = enabled ? Styling.AccentRed : Styling.Surface3;
        Paint.Gradient(drawList, indicatorMin, indicatorMax,
            Styling.Tint(Styling.Surface3, indicatorAccent, 0.30f), Styling.Tint(Styling.Surface2, indicatorAccent, 0.22f), rounding - inset);
        Paint.TopLight(drawList, indicatorMin, indicatorMax, rounding - inset, 0.10f);
        Paint.Stroke(drawList, indicatorMin, indicatorMax, Styling.WithAlpha(enabled ? Styling.AccentRedBright : Styling.BorderDim, 0.55f), rounding - inset);

        var changed = false;
        var segmentX = origin.X + inset;
        for (var index = 0; index < count; index++)
        {
            var segmentWidth = segmentWidths[index];
            var segmentMin = new Vector2(segmentX, origin.Y);
            ImGui.SetCursorScreenPos(segmentMin);
            ImGui.PushID((nint)(index + 1));
            var hit = Hit.Area("##segment", new Vector2(segmentWidth, size.Y), enabled);
            var hover = Motion.Hover(Motion.Key("##segment"), hit.Hovered);
            ImGui.PopID();

            if (hit.Clicked && selected != index)
            {
                selected = index;
                changed = true;
            }

            var isSelected = index == current;
            if (!isSelected && hover > 0.01f)
            {
                Paint.Fill(drawList, segmentMin + new Vector2(0f, inset), segmentMin + new Vector2(segmentWidth, size.Y - inset),
                    Styling.WithAlpha(Styling.Surface2, 0.7f * hover), rounding - inset);
            }

            DrawContent(items[index], metrics[index], segmentMin, segmentWidth, size.Y, isSelected, hover, enabled);
            segmentX += segmentWidth;
        }

        ImGui.PopID();
        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(size);
        return changed;
    }

    private static void ResolveWidths(ReadOnlySpan<Item> items, int count, float inner)
    {
        var pad = SegmentPadX * 2f * ImGuiHelpers.GlobalScale;
        var equal = inner / count;
        var widest = 0f;
        var natural = 0f;
        for (var index = 0; index < count; index++)
        {
            metrics[index] = Measure(items[index]);
            widest = MathF.Max(widest, metrics[index].Content);
            natural += metrics[index].Content + pad;
        }

        if (widest + pad <= equal)
        {
            for (var index = 0; index < count; index++)
            {
                segmentWidths[index] = equal;
            }
            return;
        }

        var shrink = natural > inner ? inner / natural : 1f;
        var slack = MathF.Max(0f, inner - natural) / count;
        for (var index = 0; index < count; index++)
        {
            segmentWidths[index] = (metrics[index].Content + pad) * shrink + slack;
        }
    }

    private static float SegmentOffset(int index)
    {
        var offset = 0f;
        for (var segment = 0; segment < index; segment++)
        {
            offset += segmentWidths[segment];
        }
        return offset;
    }

    private static Metrics Measure(Item item)
    {
        var icon = item.Icon is { } glyph ? TextDraw.IconSize(glyph) : Vector2.Zero;
        var label = TextDraw.Measure(item.Label);
        var content = label.X + (item.Icon is not null ? icon.X + IconGap * ImGuiHelpers.GlobalScale : 0f);
        return new Metrics(icon, label, content);
    }

    private static void DrawContent(Item item, Metrics metric, Vector2 segmentMin, float segmentWidth, float height, bool selected, float hover, bool enabled)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var iconSpan = item.Icon is not null ? metric.Icon.X + IconGap * scale : 0f;
        var available = segmentWidth - SegmentPadX * 2f * scale;
        var label = item.Label;
        var labelWidth = metric.Label.X;
        if (metric.Content > available)
        {
            label = TextDraw.Truncate(label, MathF.Max(0f, available - iconSpan));
            labelWidth = TextDraw.Measure(label).X;
        }

        var x = segmentMin.X + (segmentWidth - iconSpan - labelWidth) * 0.5f;
        var midY = segmentMin.Y + height * 0.5f;

        var textColor = !enabled ? Styling.TextMuted
            : selected ? Styling.TextStrong
            : Vector4.Lerp(Styling.TextDim, Styling.TextSecondary, hover);

        if (item.Icon is { } glyph)
        {
            var iconColor = !enabled ? Styling.TextMuted : selected ? Styling.AccentRedBright : textColor;
            TextDraw.Icon(glyph, new Vector2(x, midY - metric.Icon.Y * 0.5f), iconColor);
            x += iconSpan;
        }

        TextDraw.At(label, new Vector2(x, midY - metric.Label.Y * 0.5f), textColor);
    }
}
