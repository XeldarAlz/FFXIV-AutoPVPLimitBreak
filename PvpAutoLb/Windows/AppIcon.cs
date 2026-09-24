using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using ECommons.DalamudServices;
using System.IO;
using System.Numerics;

namespace PvpAutoLb.Windows;

internal static class AppIcon
{
    private const string FileName = "Icon.png";

    private static string? path;
    private static bool? exists;

    public static void Draw(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding, float alpha = 1f)
    {
        path ??= Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName ?? string.Empty, "Images", FileName);
        exists ??= File.Exists(path);

        if (exists.Value)
        {
            var texture = Svc.Texture.GetFromFile(path).GetWrapOrEmpty();
            drawList.AddImageRounded(texture.Handle, min, max, Vector2.Zero, Vector2.One,
                Paint.Col(new Vector4(1f, 1f, 1f, alpha)), rounding, ImDrawFlags.RoundCornersAll);
            return;
        }

        Paint.Gradient(drawList, min, max, Styling.AccentRedBright, Styling.AccentRed, rounding);
        TextDraw.IconCentered(FontAwesomeIcon.Bolt, (min + max) * 0.5f, Styling.WithAlpha(Styling.TextStrong, alpha));
    }
}
