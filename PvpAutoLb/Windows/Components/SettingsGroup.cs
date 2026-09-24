using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System.Numerics;

namespace PvpAutoLb.Windows.Components;

internal sealed class SettingsGroup : IDisposable
{
    private const float PaddingX = 14f;
    private const float PaddingY = 8f;
    private const float GroupGap = 18f;
    private const float FootnotePullUp = 6f;
    private const float FootnoteIndent = 4f;

    internal static float ContentRightEdge { get; private set; }
    internal static bool RowDrawnInGroup;

    private readonly Vector2 cardOrigin;
    private readonly float cardWidth;

    public static SettingsGroup Begin(string title)
    {
        if (title.Length > 0)
        {
            Styling.SectionLabel(title);
            Styling.VSpace(6f);
        }

        return new SettingsGroup();
    }

    private SettingsGroup()
    {
        var scale = ImGuiHelpers.GlobalScale;
        cardOrigin = ImGui.GetCursorScreenPos();
        cardWidth = ImGui.GetContentRegionAvail().X;
        ContentRightEdge = cardOrigin.X + cardWidth - PaddingX * scale;
        RowDrawnInGroup = false;

        var drawList = ImGui.GetWindowDrawList();
        drawList.ChannelsSplit(2);
        drawList.ChannelsSetCurrent(1);

        ImGui.SetCursorScreenPos(cardOrigin + new Vector2(PaddingX, PaddingY) * scale);
        ImGui.BeginGroup();
    }

    public void Dispose()
    {
        ImGui.EndGroup();
        var scale = ImGuiHelpers.GlobalScale;
        var cardEnd = new Vector2(cardOrigin.X + cardWidth, ImGui.GetItemRectMax().Y + PaddingY * scale);

        var drawList = ImGui.GetWindowDrawList();
        drawList.ChannelsSetCurrent(0);
        var rounding = Styling.CardRounding * scale;
        Paint.Surface(drawList, cardOrigin, cardEnd, rounding, Styling.WithAlpha(Styling.Surface1, 0.55f), Styling.WithAlpha(Styling.BorderDim, 0.5f));
        drawList.ChannelsMerge();

        ImGui.SetCursorScreenPos(new Vector2(cardOrigin.X, cardEnd.Y));
        ImGui.Dummy(new Vector2(cardWidth, 0f));
        Styling.VSpace(GroupGap);
    }

    public static void Footnote(string text)
    {
        var scale = ImGuiHelpers.GlobalScale;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - FootnotePullUp * scale);
        ImGui.Indent(FootnoteIndent * scale);
        using (Fonts.PushCaption())
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextMuted))
        {
            ImGui.TextWrapped(text);
        }

        ImGui.Unindent(FootnoteIndent * scale);
        Styling.VSpace(GroupGap);
    }

}
