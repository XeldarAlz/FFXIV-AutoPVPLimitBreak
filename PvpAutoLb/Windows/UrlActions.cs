using Dalamud.Bindings.ImGui;
using System.Diagnostics;

namespace PvpAutoLb.Windows;

// Opens a URL in the default browser, copying it to the clipboard if the launch fails. The optional
// onError hook lets a caller log the failure without baking any one caller's logging into the helper.
internal static class UrlActions
{
    public static void OpenInBrowser(string url, Action<Exception>? onError = null)
    {
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception exception)
        {
            ImGui.SetClipboardText(url);
            onError?.Invoke(exception);
        }
    }
}
