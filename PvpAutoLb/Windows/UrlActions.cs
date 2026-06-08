using System;
using System.Diagnostics;
using Dalamud.Bindings.ImGui;

namespace PvpAutoLb.Windows;

internal static class UrlActions
{
    public static void OpenInBrowser(string url, Action<Exception>? onError = null)
    {
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ImGui.SetClipboardText(url);
            onError?.Invoke(ex);
        }
    }
}
