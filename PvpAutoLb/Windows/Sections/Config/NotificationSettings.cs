using Dalamud.Interface;
using PvpAutoLb.Core;
using PvpAutoLb.Core.Localization;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections.Config;

internal static class NotificationSettings
{
    private const int FirstSoundEffect = 1;
    private const int LastSoundEffect = 16;

    public static void Draw(Configuration configuration)
    {
        DrawSoundGroup(configuration);
        DrawChatGroup(configuration);
    }

    private static void DrawSoundGroup(Configuration configuration)
    {
        using var group = SettingsGroup.Begin(Loc.T(L.Settings.SoundGroup));

        var play = configuration.PlaySoundOnFire;
        if (SettingsControls.ToggleRow(Loc.T(L.Settings.PlaySound), Loc.T(L.Settings.PlaySoundHelp), "##palb_play_sound", ref play))
        {
            configuration.PlaySoundOnFire = play;
        }

        using var section = Motion.PushSection("##palb_sound_rows", configuration.PlaySoundOnFire);
        if (section is null)
        {
            return;
        }

        var sound = Math.Clamp(configuration.FireSoundId, FirstSoundEffect, LastSoundEffect);
        if (SettingsControls.StepperRow(Loc.T(L.Settings.SoundEffect), Loc.T(L.Settings.SoundEffectHelp), "##palb_sound_id",
                ref sound, 1, FirstSoundEffect, LastSoundEffect, Loc.T(L.Settings.SoundEffectFormat)))
        {
            configuration.FireSoundId = sound;
        }

        if (SettingsControls.ButtonRow(Loc.T(L.Settings.SoundPreview), Loc.T(L.Settings.SoundPreviewHelp), "##palb_sound_test",
                Loc.T(L.Common.Test), FontAwesomeIcon.Play, Styling.AccentRed))
        {
            Feedback.PlaySound(configuration.FireSoundId);
        }
    }

    private static void DrawChatGroup(Configuration configuration)
    {
        using var group = SettingsGroup.Begin(Loc.T(L.Settings.ChatGroup));

        var chat = configuration.LogFireToChat;
        if (SettingsControls.ToggleRow(Loc.T(L.Settings.LogToChat), Loc.T(L.Settings.LogToChatHelp), "##palb_log_chat", ref chat))
        {
            configuration.LogFireToChat = chat;
        }
    }
}
