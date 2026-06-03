using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using PvpAutoLb.Core;

namespace PvpAutoLb.Windows;

public sealed class AboutWindow : Window, IDisposable
{
    private const string RepoUrl = "https://github.com/XeldarAlz/FFXIV-AutoPVPLimitBreak";
    private const string IssuesUrl = "https://github.com/XeldarAlz/FFXIV-AutoPVPLimitBreak/issues";
    private const string DiscussionsUrl = "https://github.com/XeldarAlz/FFXIV-AutoPVPLimitBreak/discussions";
    private const string SecurityUrl = "https://github.com/XeldarAlz/FFXIV-AutoPVPLimitBreak/security/advisories/new";
    private const string Author = "XeldarAlz";

    private static readonly Vector4 GreetingColor = new(0.96f, 0.84f, 0.62f, 1.00f);
    private static readonly Vector4 GreetingShimmer = new(1.00f, 0.94f, 0.78f, 1.00f);
    private static readonly string[] GreetingParagraphs =
    {
        "Hello there! I'm a solo developer building FFXIV automation plugins in my free time.",
        "If this one made your day a little easier, the best way to support the project is to share it with other players.",
        "I'd love to hear from you too: bug reports, feature requests, and general feedback are all welcome on GitHub Discussions.",
        "Thanks for trying it out, and have fun out there!",
    };

    private static readonly Dictionary<string, float> linkHoverPulse = new();

    public AboutWindow() : base("Auto PVP LB: About###PvpAutoLbAbout")
    {
        Flags = ImGuiWindowFlags.NoCollapse;
        Size = new Vector2(560, 460);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(360, 360),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 5f);
        style.Push(ImGuiStyleVar.WindowRounding, 7f);
        style.Push(ImGuiStyleVar.ChildRounding, 6f);
        style.Push(ImGuiStyleVar.ItemSpacing, new Vector2(8, 8) * ImGuiHelpers.GlobalScale);

        DrawIcon();
        DrawHeader();
        ImGui.Separator();
        ImGui.Spacing();
        DrawDetailsTable();
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        DrawShimmerGreeting();
    }

    private static void DrawIcon()
    {
        var iconSize = 96f * ImGuiHelpers.GlobalScale;
        var avail = ImGui.GetContentRegionAvail().X;
        if (avail > iconSize)
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (avail - iconSize) * 0.5f);

        var iconPath = Path.Combine(
            Svc.PluginInterface.AssemblyLocation.DirectoryName ?? "",
            "Images", "Icon.png");
        if (!File.Exists(iconPath))
        {
            ImGui.Dummy(new Vector2(iconSize, iconSize));
            return;
        }

        var tex = Svc.Texture.GetFromFile(iconPath).GetWrapOrEmpty();
        if (tex == null)
        {
            ImGui.Dummy(new Vector2(iconSize, iconSize));
            return;
        }

        var alpha = 0.85f + 0.15f * Styling.Pulse(2000.0);
        ImGui.Image(tex.Handle, new Vector2(iconSize, iconSize), Vector2.Zero, Vector2.One, new Vector4(1f, 1f, 1f, alpha));
        ImGui.Spacing();
    }

    private static void DrawHeader()
    {
        var version = typeof(AboutWindow).Assembly.GetName().Version?.ToString() ?? "?";
        var availWidth = ImGui.GetContentRegionAvail().X;
        var label = $"v {version}";
        var textWidth = ImGui.CalcTextSize(label).X;
        if (availWidth > textWidth)
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (availWidth - textWidth) * 0.5f);

        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
            ImGui.TextUnformatted(label);
    }

    private static void DrawDetailsTable()
    {
        if (!ImGui.BeginTable("##about_table", 2,
                ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.PadOuterX))
            return;

        ImGui.TableSetupColumn("##label", ImGuiTableColumnFlags.WidthFixed, 150f * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("##value", ImGuiTableColumnFlags.WidthStretch);

        DrawTextRow("Author", Author);
        DrawLinkRow("GitHub", RepoUrl);
        DrawLinkRow("Report a bug", IssuesUrl);
        DrawLinkRow("Discussions", DiscussionsUrl);
        DrawLinkRow("Security disclosure", SecurityUrl);

        ImGui.EndTable();
    }

    private static void DrawShimmerGreeting()
    {
        ImGui.PushTextWrapPos(0f);
        var availWidth = ImGui.GetContentRegionAvail().X;
        var bandWidth = 140f * ImGuiHelpers.GlobalScale;
        var dl = ImGui.GetWindowDrawList();

        const int loopMs = 5000;
        const int staggerMs = 800;
        var tick = Environment.TickCount;

        for (int i = 0; i < GreetingParagraphs.Length; i++)
        {
            var para = GreetingParagraphs[i];
            var startPos = ImGui.GetCursorScreenPos();

            using (ImRaii.PushColor(ImGuiCol.Text, GreetingColor))
                ImGui.TextUnformatted(para);
            var endPos = ImGui.GetCursorScreenPos();

            int mod = (tick - i * staggerMs) % loopMs;
            if (mod < 0) mod += loopMs;
            float phase = mod / (float)loopMs;
            float bandCenter = startPos.X - bandWidth + phase * (availWidth + bandWidth * 2f);

            dl.PushClipRect(
                new Vector2(bandCenter - bandWidth * 0.5f, startPos.Y),
                new Vector2(bandCenter + bandWidth * 0.5f, endPos.Y),
                true);
            ImGui.SetCursorScreenPos(startPos);
            using (ImRaii.PushColor(ImGuiCol.Text, GreetingShimmer))
                ImGui.TextUnformatted(para);
            ImGui.SetCursorScreenPos(endPos);
            dl.PopClipRect();

            if (i < GreetingParagraphs.Length - 1)
                ImGui.Dummy(new Vector2(0, ImGui.GetTextLineHeight() * 0.5f));
        }

        ImGui.PopTextWrapPos();
    }

    private static void DrawTextRow(string label, string value)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
            ImGui.TextUnformatted(label);
        ImGui.TableSetColumnIndex(1);
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextStrong))
            ImGui.TextUnformatted(value);
    }

    private static void DrawLinkRow(string label, string url)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
            ImGui.TextUnformatted(label);

        ImGui.TableSetColumnIndex(1);

        linkHoverPulse.TryGetValue(url, out var pulse);
        var color = Vector4.Lerp(Styling.AccentBlue, Styling.TextStrong, pulse * 0.55f);

        using (ImRaii.PushColor(ImGuiCol.Text, color))
        {
            ImGui.PushTextWrapPos(ImGui.GetContentRegionMax().X);
            ImGui.TextUnformatted(url);
            ImGui.PopTextWrapPos();
        }

        var hovered = ImGui.IsItemHovered();
        linkHoverPulse[url] = hovered
            ? MathF.Min(pulse + 0.15f, 1f)
            : MathF.Max(pulse - 0.10f, 0f);

        if (!hovered) return;

        using (ImRaii.Tooltip())
            ImGui.TextUnformatted("Click to open · right-click to copy");
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left)) OpenInBrowser(url);
        else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) ImGui.SetClipboardText(url);
    }

    private static void OpenInBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Svc.Log.Warning(ex, PvpAutoLbConstants.LogPrefix + " failed to launch browser for {0}, copied to clipboard instead", url);
            ImGui.SetClipboardText(url);
        }
    }
}
