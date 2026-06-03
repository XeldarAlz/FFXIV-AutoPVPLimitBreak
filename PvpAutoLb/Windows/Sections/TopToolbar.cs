using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;

namespace PvpAutoLb.Windows.Sections;

internal static class TopToolbar
{
    public static void Draw(Plugin plugin)
    {
        ImGui.AlignTextToFramePadding();
        var jobName = JobLookup.Abbreviation(JobLookup.CurrentJobId);
        var hasJob = !string.IsNullOrEmpty(jobName);
        using (ImRaii.PushColor(ImGuiCol.Text, hasJob ? Styling.TextDim : Styling.TextMuted))
            ImGui.TextUnformatted(hasJob ? jobName : "offline");

        var infoLabel = FontAwesomeIcon.InfoCircle.ToIconString();
        var gearLabel = FontAwesomeIcon.Cog.ToIconString();

        float framePadX = ImGui.GetStyle().FramePadding.X;
        float spacingX = ImGui.GetStyle().ItemSpacing.X;
        float gearW, infoW;
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            gearW = ImGui.CalcTextSize(gearLabel).X + framePadX * 2;
            infoW = ImGui.CalcTextSize(infoLabel).X + framePadX * 2;
        }
        ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - gearW - infoW - spacingX);

        bool infoClicked;
        using (ImRaii.PushFont(UiBuilder.IconFont))
            infoClicked = ImGui.Button(infoLabel + "##about");
        HoverTip("About");

        ImGui.SameLine();
        bool gearClicked;
        using (ImRaii.PushFont(UiBuilder.IconFont))
            gearClicked = ImGui.Button(gearLabel + "##gear");
        HoverTip("Settings");

        if (infoClicked) plugin.ToggleAboutUi();
        if (gearClicked) plugin.ToggleConfigUi();

        ImGui.Separator();
    }

    private static void HoverTip(string text)
    {
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(text);
    }
}
