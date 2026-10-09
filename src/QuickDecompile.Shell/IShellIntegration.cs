namespace QuickDecompile.Shell;

public interface IShellIntegration
{
    void InstallContextMenu(string executablePath);

    void UninstallContextMenu();

    bool IsContextMenuInstalled();
}
