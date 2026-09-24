using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;
using PvpAutoLb.Windows.Sections;

namespace PvpAutoLb.Windows;

// A glanceable strip for mid-match: the LB gauge, the enemy it is watching and what it is waiting for. It is
// drawn entirely through the draw list over one invisible drag handle, so a frame costs a handful of shapes.
public sealed class CombatHud : Window, IDisposable
{
    private const string DragId = "##palb_hud_drag";
    private const string LockId = "##palb_hud_lock";
    private const float Padding = 12f;
    private const float RingRadius = 20f;
    private const float RingThickness = 4f;
    private const float RowGap = 9f;
    private const float BarHeight = 7f;
    private const float LockButtonSize = 22f;
    // First appearance sits centered just below the character, above the default hotbar rows.
    private const float FirstUseHeightShare = 0.62f;
    private const ImGuiWindowFlags BaseFlags =
        ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse
        | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav;
    private const ImGuiWindowFlags LockedFlags = BaseFlags | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoMove;

    private readonly Plugin plugin;

    private bool? forced;
    private uint territory;
    private IDisposable? chrome;
    private IDisposable? bodyFont;
    private IDisposable? frame;

    public CombatHud(Plugin plugin) : base("Auto PVP LB HUD###PvpAutoLbHud", BaseFlags)
    {
        this.plugin = plugin;
        IsOpen = true;
        RespectCloseHotkey = false;
        AllowPinning = false;
        AllowClickthrough = false;
        DisableWindowSounds = true;
    }

    public void Dispose() { }

    // An explicit toggle holds until the next zone change, so hiding it mid-match does not bring it back on
    // the following frame, and showing it outside PvP does not leave it stuck on after the next queue.
    public bool Visible
    {
        get
        {
            ForgetToggleOnZoneChange();
            if (forced is { } value)
            {
                return value;
            }

            return plugin.Configuration.ShowCombatHud && Svc.ClientState.IsPvPExcludingDen;
        }
    }

    public void ToggleVisible() => forced = !Visible;

    public override bool DrawConditions() => Visible;

    public override void PreDraw()
    {
        Flags = plugin.Configuration.CombatHudLocked ? LockedFlags : BaseFlags;
        var viewport = ImGui.GetMainViewport();
        var firstPosition = new Vector2(viewport.Pos.X + (viewport.Size.X - Layout.HudWidth * ImGuiHelpers.GlobalScale) * 0.5f, viewport.Pos.Y + viewport.Size.Y * FirstUseHeightShare);
        ImGui.SetNextWindowPos(firstPosition, ImGuiCond.FirstUseEver);
        bodyFont = Fonts.PushBody();
        chrome = Styling.PushChrome(new Vector2(Padding, Padding));
        frame = ImRaii.PushColor(ImGuiCol.WindowBg, Vector4.Zero).Push(ImGuiCol.Border, Vector4.Zero);
    }

    public override void PostDraw()
    {
        frame?.Dispose();
        frame = null;
        chrome?.Dispose();
        chrome = null;
        bodyFont?.Dispose();
        bodyFont = null;
    }

    public override void Draw()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var snapshot = LiveSnapshot.Resolve(plugin);
        var drawList = ImGui.GetWindowDrawList();
        var windowPos = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();
        var rounding = Styling.PanelRounding * scale;

        Paint.Glass(drawList, windowPos, windowPos + windowSize, rounding, snapshot.Accent, snapshot.Firing ? 0.12f : 0.05f);
        if (snapshot.Firing)
        {
            Paint.Stroke(drawList, windowPos, windowPos + windowSize,
                Styling.PulseColor(Styling.WithAlpha(Styling.AccentRed, 0.5f), Styling.AccentRedBright, Styling.PulseFast), rounding, 1.6f * scale);
        }

        var width = Layout.HudWidth * scale;
        var origin = ImGui.GetCursorScreenPos();
        var height = ContentHeight(snapshot);
        ImGui.InvisibleButton(DragId, new Vector2(width, height));
        ImGui.SetItemAllowOverlap();
        var hovered = ImGui.IsItemHovered();
        if (ImGui.IsItemActive())
        {
            var delta = ImGui.GetIO().MouseDelta;
            if (delta != Vector2.Zero)
            {
                ImGui.SetWindowPos(ImGui.GetWindowPos() + delta, ImGuiCond.Always);
            }
        }

        var y = DrawHeadline(snapshot, origin, width);
        y = DrawTarget(drawList, snapshot, new Vector2(origin.X, y), width);
        DrawStatus(snapshot, new Vector2(origin.X, y), width);

        if (!plugin.Configuration.CombatHudLocked)
        {
            DrawLockButton(origin, width, hovered);
        }
    }

    private float ContentHeight(LiveSnapshot snapshot)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var headline = MathF.Max(RingRadius * 2f * scale, HeadlineLineHeight() + 4f * scale + CaptionLineHeight() + 6f * scale);
        var target = snapshot.Target is null ? 0f : ImGui.GetTextLineHeight() + 6f * scale + BarHeight * scale + RowGap * scale;
        return headline + RowGap * scale + target + CaptionLineHeight();
    }

    private static float HeadlineLineHeight()
    {
        using (Fonts.PushHeadline())
        {
            return TextDraw.LineHeight();
        }
    }

    private static float CaptionLineHeight()
    {
        using (Fonts.PushCaption())
        {
            return TextDraw.LineHeight();
        }
    }

    private static float DrawHeadline(LiveSnapshot snapshot, Vector2 origin, float width)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var radius = RingRadius * scale;
        var headlineHeight = HeadlineLineHeight();
        var chipHeight = CaptionLineHeight() + 6f * scale;
        var rowHeight = MathF.Max(radius * 2f, headlineHeight + 4f * scale + chipHeight);
        var center = new Vector2(origin.X + radius, origin.Y + rowHeight * 0.5f);
        ReadinessRing.Draw("##palb_hud_ring", center, radius, RingThickness * scale, snapshot);

        var textX = center.X + radius + 12f * scale;
        var textWidth = origin.X + width - LockButtonSize * scale - 6f * scale - textX;
        var top = origin.Y + (rowHeight - headlineHeight - 4f * scale - chipHeight) * 0.5f;
        using (Fonts.PushHeadline())
        {
            var name = snapshot.LimitBreakName.Length > 0 ? snapshot.LimitBreakName : Loc.T(L.Live.NoLimitBreakName);
            TextDraw.At(TextDraw.Truncate(name, textWidth), new Vector2(textX, top), Styling.TextStrong);
        }

        DrawGaugePill(snapshot, new Vector2(textX, top + headlineHeight + 4f * scale), chipHeight);
        return origin.Y + rowHeight + RowGap * scale;
    }

    private static void DrawGaugePill(LiveSnapshot snapshot, Vector2 position, float height)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var drawList = ImGui.GetWindowDrawList();
        var accent = ReadinessRing.RingAccent(snapshot);
        var paddingX = 8f * scale;
        using (Fonts.PushCaption())
        {
            var label = TextDraw.Upper(snapshot.Armed ? snapshot.GaugeLabel : snapshot.Chip);
            var labelSize = TextDraw.Measure(label);
            var max = position + new Vector2(labelSize.X + paddingX * 2f, height);
            var fillAlpha = snapshot.LimitBreakReady ? 0.20f + 0.14f * Styling.Pulse(Styling.PulseBreath) : 0.18f;
            var pillAccent = snapshot.Armed ? accent : Styling.TextDim;
            Paint.Pill(drawList, position, max, Styling.WithAlpha(pillAccent, fillAlpha), Styling.WithAlpha(pillAccent, 0.6f));
            TextDraw.At(label, new Vector2(position.X + paddingX, position.Y + (height - labelSize.Y) * 0.5f), Styling.Lighten(pillAccent, 0.3f));
        }
    }

    private static float DrawTarget(ImDrawListPtr drawList, LiveSnapshot snapshot, Vector2 origin, float width)
    {
        if (snapshot.Target is null)
        {
            return origin.Y;
        }

        var scale = ImGuiHelpers.GlobalScale;
        var percent = Labels.Percent(snapshot.TargetHp);
        float percentWidth;
        using (Fonts.PushCaption())
        {
            percentWidth = TextDraw.Measure(percent).X;
            TextDraw.Right(percent, origin.X + width, origin.Y + (ImGui.GetTextLineHeight() - TextDraw.LineHeight()) * 0.5f, Styling.TextSecondary);
        }

        TextDraw.At(TextDraw.Truncate(snapshot.TargetName, width - percentWidth - 10f * scale), origin, Styling.TextStrong);

        var barY = origin.Y + ImGui.GetTextLineHeight() + 6f * scale;
        var fill = snapshot.Block == Core.FireBlock.AboveThreshold ? Styling.AccentMint : Styling.AccentRed;
        HpBar.Draw(drawList, new Vector2(origin.X, barY), width, BarHeight * scale, snapshot.TargetHp, snapshot.TargetShield, snapshot.TargetThreshold, fill);
        return barY + BarHeight * scale + RowGap * scale;
    }

    private static void DrawStatus(LiveSnapshot snapshot, Vector2 origin, float width)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var color = snapshot.Firing ? Styling.PulseColor(snapshot.Accent, snapshot.AccentSoft, Styling.PulseFast) : snapshot.AccentSoft;
        using (Fonts.PushCaption())
        {
            var lineHeight = TextDraw.LineHeight();
            var iconSize = TextDraw.IconSize(snapshot.Icon);
            TextDraw.Icon(snapshot.Icon, new Vector2(origin.X, origin.Y + (lineHeight - iconSize.Y) * 0.5f), color);
            var textX = origin.X + iconSize.X + 7f * scale;
            TextDraw.At(TextDraw.Truncate(snapshot.Status, origin.X + width - textX), new Vector2(textX, origin.Y), color);
        }
    }

    private void DrawLockButton(Vector2 origin, float width, bool dragHovered)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var size = LockButtonSize * scale;
        var reveal = Motion.Hover(Motion.Key(LockId, 1), dragHovered || ImGui.IsWindowHovered());
        if (reveal <= 0.01f)
        {
            return;
        }

        ImGui.SetCursorScreenPos(new Vector2(origin.X + width - size, origin.Y));
        using (Motion.PushAlpha(reveal))
        {
            if (IconButton.Draw(FontAwesomeIcon.Lock, LockId, size, Styling.TextDim, Loc.T(L.Hud.LockTooltip)))
            {
                plugin.Configuration.CombatHudLocked = true;
                plugin.Configuration.Save();
            }
        }
    }

    private void ForgetToggleOnZoneChange()
    {
        var current = Svc.ClientState.TerritoryType;
        if (current == territory)
        {
            return;
        }

        territory = current;
        forced = null;
    }
}
