using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;

namespace PvpAutoLb.Windows.Components;

internal static class GameIcon
{
    public static bool Draw(ImDrawListPtr drawList, uint iconId, Vector2 min, Vector2 max, float rounding, float alpha = 1f)
    {
        if (iconId == 0)
        {
            return false;
        }

        var texture = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(iconId)).GetWrapOrDefault();
        if (texture is null)
        {
            return false;
        }

        drawList.AddImageRounded(texture.Handle, min, max, Vector2.Zero, Vector2.One, Paint.Col(new Vector4(1f, 1f, 1f, alpha)), rounding, ImDrawFlags.RoundCornersAll);
        return true;
    }
}
