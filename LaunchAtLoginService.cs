using Microsoft.Win32;

namespace ClaudeLight;

/// <summary>
/// Manages the "Launch at Login" registry entry under HKCU.
/// Equivalent to macOS SMAppService.mainApp.register().
/// </summary>
static class LaunchAtLoginService
{
    private const string RegistryKey  = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName      = "ClaudeLight";

    public static bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, writable: false);
            return key?.GetValue(AppName) is string val && val == ExePath;
        }
    }

    public static void SetEnabled(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, writable: true)
            ?? throw new InvalidOperationException("Cannot open Run registry key.");

        if (enable)
            key.SetValue(AppName, ExePath);
        else
            key.DeleteValue(AppName, throwOnMissingValue: false);
    }

    private static string ExePath =>
        $"\"{System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "ClaudeLight.exe"}\"";
}
