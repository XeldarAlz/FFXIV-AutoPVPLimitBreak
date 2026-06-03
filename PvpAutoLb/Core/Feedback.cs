using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace PvpAutoLb.Core;

internal static class Feedback
{
    public static void OnFire(Configuration cfg, IBattleChara target, string actionName)
    {
        if (cfg.PlaySoundOnFire)
            PlaySound(cfg.FireSoundId);
        if (cfg.LogFireToChat)
        {
            Svc.Chat.Print($"{PvpAutoLbConstants.LogPrefix} fired {actionName} on {target.Name.TextValue}");
        }
    }

    public static void PlaySound(int soundId)
    {
        unsafe { UIGlobals.PlayChatSoundEffect((uint)soundId); }
    }
}
