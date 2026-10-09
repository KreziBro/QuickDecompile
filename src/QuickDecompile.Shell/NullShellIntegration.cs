namespace QuickDecompile.Shell;

public sealed class NullShellIntegration : IShellIntegration
{
    public void InstallContextMenu(string executablePath) {  }
    public void UninstallContextMenu() {  }
    public bool IsContextMenuInstalled() => false;
}
