using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;

namespace PvpAutoLb.Windows.Sections;

internal static class TopToolbar
{
    private const float UnseenDotRadius = 3.5f;
    private const float UnseenDotInset = 4f;
    private const float UnseenDotRing = 1.5f;

    public static void Draw(Plugin plugin)
    {
        ImGui.AlignTextToFramePadding();
        var jobName = JobLookup.Abbreviation(JobLookup.CurrentJobId);
        var hasJob = !string.IsNullOrEmpty(jobName);
        using (ImRaii.PushColor(ImGuiCol.Text, hasJob ? Styling.TextDim : Styling.TextMuted))
            ImGui.TextUnformatted(hasJob ? jobName : "offline");

        var consoleLabel = FontAwesomeIcon.Terminal.ToIconString();
        var infoLabel = FontAwesomeIcon.InfoCircle.ToIconString();
        var gearLabel = FontAwesomeIcon.Cog.ToIconString();

        float framePadX = ImGui.GetStyle().FramePadding.X;
        float spacingX = ImGui.GetStyle().ItemSpacing.X;
        float consoleW, gearW, infoW;
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            consoleW = ImGui.CalcTextSize(consoleLabel).X + framePadX * 2;
            gearW = ImGui.CalcTextSize(gearLabel).X + framePadX * 2;
            infoW = ImGui.CalcTextSize(infoLabel).X + framePadX * 2;
        }
        ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - consoleW - gearW - infoW - spacingX * 2);

        bool consoleClicked;
        using (ImRaii.PushFont(UiBuilder.IconFont))
            consoleClicked = ImGui.Button(consoleLabel + "##console");
        DrawUnseenDot(plugin);
        HoverTip("Console");

        ImGui.SameLine();
        bool infoClicked;
        using (ImRaii.PushFont(UiBuilder.IconFont))
            infoClicked = ImGui.Button(infoLabel + "##about");
        HoverTip("About");

        ImGui.SameLine();
        bool gearClicked;
        using (ImRaii.PushFont(UiBuilder.IconFont))
            gearClicked = ImGui.Button(gearLabel + "##gear");
        HoverTip("Settings");

        if (consoleClicked) plugin.ToggleConsoleUi();
        if (infoClicked) plugin.ToggleAboutUi();
        if (gearClicked) plugin.ToggleConfigUi();

        ImGui.Separator();
    }

    private static void DrawUnseenDot(Plugin plugin)
    {
        if (plugin.ConsoleOpen || RunLog.Unseen is not { } unseen)
        {
            return;
        }

        var scale = ImGuiHelpers.GlobalScale;
        var inset = UnseenDotInset * scale;
        var radius = UnseenDotRadius * scale;
        var center = new Vector2(ImGui.GetItemRectMax().X - inset, ImGui.GetItemRectMin().Y + inset);
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddCircleFilled(center, radius + UnseenDotRing * scale, ImGui.GetColorU32(ImGuiCol.WindowBg));
        drawList.AddCircleFilled(center, radius, Paint.Col(ConsoleWindow.LevelColor(unseen)));
    }

    private static void HoverTip(string text)
    {
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(text);
    }
}
