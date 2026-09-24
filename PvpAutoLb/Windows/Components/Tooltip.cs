using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System.Numerics;

namespace PvpAutoLb.Windows.Components;

// The shell pushes a zero window padding so pages can draw edge to edge, and ImGui builds tooltips
// from that same padding. Every tooltip therefore goes through this scope, which restores the
// padding, rounding and body font a hovering hint needs to stay readable.
internal static class Tooltip
{
    private const float PaddingX = 14f;
    private const float PaddingY = 11f;
    private const float WrapWidth = 320f;
    private const float LineGap = 5f;

    public static void Show(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        using (Begin())
        {
            Text(text);
        }
    }

    public static IDisposable Begin()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(PaddingX, PaddingY) * scale)
            .Push(ImGuiStyleVar.WindowRounding, Styling.CardRounding * scale)
            .Push(ImGuiStyleVar.PopupRounding, Styling.CardRounding * scale)
            .Push(ImGuiStyleVar.WindowBorderSize, 1f)
            .Push(ImGuiStyleVar.PopupBorderSize, 1f)
            .Push(ImGuiStyleVar.ItemSpacing, new Vector2(0f, LineGap) * scale);

        var color = ImRaii.PushColor(ImGuiCol.PopupBg, Styling.WithAlpha(Styling.Surface2, 0.98f))
            .Push(ImGuiCol.Border, Styling.WithAlpha(Styling.BorderDim, 0.85f))
            .Push(ImGuiCol.Text, Styling.TextSecondary);

        return new Scope(Fonts.PushBody(), color, style);
    }

    public static void Text(string text, Vector4? color = null)
    {
        ImGui.PushTextWrapPos(WrapWidth * ImGuiHelpers.GlobalScale);
        using (ImRaii.PushColor(ImGuiCol.Text, color ?? Styling.TextSecondary))
        {
            ImGui.TextUnformatted(text);
        }

        ImGui.PopTextWrapPos();
    }

    private sealed class Scope : IDisposable
    {
        private readonly IDisposable font;
        private readonly IDisposable color;
        private readonly IDisposable style;

        public Scope(IDisposable font, IDisposable color, IDisposable style)
        {
            this.font = font;
            this.color = color;
            this.style = style;
            ImGui.BeginTooltip();
        }

        public void Dispose()
        {
            ImGui.EndTooltip();
            font.Dispose();
            color.Dispose();
            style.Dispose();
        }
    }
}
