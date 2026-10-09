using Microsoft.Win32;

namespace QuickDecompile.Shell;

public sealed class WindowsShellIntegration : IShellIntegration
{
    private const string MenuName    = "QuickDecompile";
    private const string MenuLabel   = "Open with QuickDecompile";
    private const string RegistryBase = @"Software\Classes\SystemFileAssociations";

    private static readonly string[] SupportedExtensions =
        [".class", ".jar", ".war", ".dex", ".apk", ".aab", ".dll", ".exe"];

    public void InstallContextMenu(string executablePath)
    {
        foreach (var ext in SupportedExtensions)
        {
            string keyPath = $@"{RegistryBase}\{ext}\shell\{MenuName}";
            using var key = Registry.CurrentUser.CreateSubKey(keyPath, writable: true);
            key.SetValue("", MenuLabel);
            key.SetValue("Icon", $"\"{executablePath}\",0");

            using var cmdKey = key.CreateSubKey("command");
            cmdKey.SetValue("", $"\"{executablePath}\" \"%1\"");
        }
    }

    public void UninstallContextMenu()
    {
        foreach (var ext in SupportedExtensions)
        {
            string keyPath = $@"{RegistryBase}\{ext}\shell\{MenuName}";
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(keyPath, throwOnMissingSubKey: false);
            }
            catch
            {
            }
        }
    }

    public bool IsContextMenuInstalled()
    {
        string keyPath = $@"{RegistryBase}\.class\shell\{MenuName}";
        using var key = Registry.CurrentUser.OpenSubKey(keyPath);
        return key != null;
    }
}
