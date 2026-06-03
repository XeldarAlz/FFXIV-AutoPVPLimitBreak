using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class FeedbackSection
{
    public static void Draw(Configuration cfg)
    {
        var sound = cfg.PlaySoundOnFire;
        if (SettingsRow.Toggle("##playsound", "Play sound on fire",
                "Plays a chat sound effect (the same set as /se1–/se16) each time the LB fires.",
                ref sound))
        {
            cfg.PlaySoundOnFire = sound;
            cfg.Save();
        }

        using (ImRaii.Disabled(!cfg.PlaySoundOnFire))
        {
            var id = cfg.FireSoundId;
            ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
            if (ImGui.SliderInt("##soundid", ref id, 1, 16, "Sound ID: %d"))
            {
                cfg.FireSoundId = id;
                cfg.SaveDebounced();
            }

            ImGui.SameLine();
            bool test;
            using (ImRaii.PushFont(UiBuilder.IconFont))
                test = ImGui.Button(FontAwesomeIcon.Play.ToIconString() + "##testsound");
            Tooltip.OnHover("Play this sound");
            if (test) Feedback.PlaySound(id);
        }

        ImGui.Spacing();
        ImGui.Spacing();

        var chat = cfg.LogFireToChat;
        if (SettingsRow.Toggle("##logchat", "Log to chat on fire",
                "Prints a line to chat when an LB fires, e.g. \"fired Seiton Tenchu on Striking Dummy\".",
                ref chat))
        {
            cfg.LogFireToChat = chat;
            cfg.Save();
        }
    }
}
