using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuickDecompile.Backends;
using QuickDecompile.Core;
using QuickDecompile.Shell;

namespace QuickDecompile.UI.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly BackendRegistry _registry;
    private readonly DecompileCache  _cache;

    [ObservableProperty] private string _title = "QuickDecompile";

    [ObservableProperty] private string _statusText = "Ready";

    [ObservableProperty] private bool _isDarkTheme = true;

    [ObservableProperty] private bool _isContextMenuInstalled;

    [ObservableProperty] private TabViewModel? _selectedTab;

    public ObservableCollection<TabViewModel> Tabs { get; } = [];

    public IShellIntegration ShellIntegration { get; }

    public MainWindowViewModel()
    {
        _registry = BuildRegistry();
        _cache    = new DecompileCache();

        ShellIntegration = OperatingSystem.IsWindows()
            ? new WindowsShellIntegration()
            : new NullShellIntegration();

        IsContextMenuInstalled = ShellIntegration.IsContextMenuInstalled();
    }

    [RelayCommand]
    public async Task OpenFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            StatusText = $"File not found: {filePath}";
            return;
        }

        var existingTab = Tabs.FirstOrDefault(t => t.FilePath == filePath && t.EntryPath == null);
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        StatusText = $"Detecting format: {Path.GetFileName(filePath)} ...";
        var format = await FileFormatDetector.DetectAsync(filePath);
        var backend = _registry.Resolve(format);

        if (backend == null)
        {
            backend = _registry.Resolve(FileFormat.PlainText)!;
        }

        var tab = new TabViewModel(filePath, null, format, backend, _cache)
        {
            Header = Path.GetFileName(filePath),
        };
        tab.CloseRequested += OnTabCloseRequested;

        Tabs.Add(tab);
        SelectedTab = tab;
        Title = $"QuickDecompile — {Path.GetFileName(filePath)}";

        await tab.LoadAsync();
        StatusText = tab.StatusText;
    }

    [RelayCommand]
    public async Task OpenEntryAsync(EntryOpenRequest request)
    {
        var existingTab = Tabs.FirstOrDefault(t => t.FilePath == request.FilePath && t.EntryPath == request.EntryPath);
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        var format  = await FileFormatDetector.DetectAsync(request.FilePath);
        var backend = _registry.Resolve(format) ?? _registry.Resolve(FileFormat.PlainText)!;

        var tab = new TabViewModel(request.FilePath, request.EntryPath, format, backend, _cache)
        {
            Header = request.DisplayName,
        };
        tab.CloseRequested += OnTabCloseRequested;

        Tabs.Add(tab);
        SelectedTab = tab;

        await tab.LoadAsync();
        StatusText = tab.StatusText;
    }

    [RelayCommand]
    public void CloseTab(TabViewModel tab) => OnTabCloseRequested(tab);

    [RelayCommand]
    public void ToggleTheme() => IsDarkTheme = !IsDarkTheme;

    [RelayCommand]
    public void InstallContextMenu()
    {
        string exe = Process.GetCurrentProcess().MainModule?.FileName ?? AppContext.BaseDirectory;
        ShellIntegration.InstallContextMenu(exe);
        IsContextMenuInstalled = ShellIntegration.IsContextMenuInstalled();
        StatusText = "Context menu installed.";
    }

    [RelayCommand]
    public void UninstallContextMenu()
    {
        ShellIntegration.UninstallContextMenu();
        IsContextMenuInstalled = ShellIntegration.IsContextMenuInstalled();
        StatusText = "Context menu removed.";
    }

    [RelayCommand]
    public async Task OpenFileDialogAsync()
    {
        var topLevel = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime dt
            ? dt.MainWindow as TopLevel
            : null;
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title         = "Open File",
            AllowMultiple = true,
            FileTypeFilter =
            [
                new FilePickerFileType("Supported Files")
                {
                    Patterns = ["*.class", "*.jar", "*.war", "*.dex", "*.apk", "*.aab", "*.dll", "*.exe"]
                },
                new FilePickerFileType("All Files") { Patterns = ["*.*"] },
            ],
        });

        foreach (var f in files)
        {
            string? path = f.TryGetLocalPath();
            if (path != null) await OpenFileAsync(path);
        }
    }

    [RelayCommand]
    public static void Exit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime dt)
            dt.Shutdown();
    }

    [RelayCommand]
    public void CloseSelectedTab()
    {
        if (SelectedTab != null) CloseTab(SelectedTab);
    }

    [RelayCommand]
    public async Task SaveCurrentTabAsync()
    {
        if (SelectedTab == null || string.IsNullOrEmpty(SelectedTab.SourceCode)) return;

        var topLevel = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime dt
            ? dt.MainWindow as TopLevel
            : null;
        if (topLevel == null) return;

        string ext = SelectedTab.Language switch {
            "csharp" => ".cs",
            "java" => ".java",
            _ => ".txt"
        };

        string defaultName = SelectedTab.Header;
        if (!defaultName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            defaultName += ext;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Source Code",
            SuggestedFileName = defaultName
        });

        if (file != null)
        {
            string? localPath = file.TryGetLocalPath();
            if (localPath != null)
            {
                await File.WriteAllTextAsync(localPath, SelectedTab.SourceCode);
                StatusText = $"Saved to {Path.GetFileName(localPath)}";
            }
        }
    }
    private void OnTabCloseRequested(TabViewModel tab)
    {
        int idx = Tabs.IndexOf(tab);
        Tabs.Remove(tab);
        tab.CloseRequested -= OnTabCloseRequested;

        if (Tabs.Count > 0)
            SelectedTab = Tabs[Math.Max(0, idx - 1)];
        else
            SelectedTab = null;
    }

    private static BackendRegistry BuildRegistry()
    {
        var reg = new BackendRegistry();
        reg.Register(new DotNetBackend());
        reg.Register(new JavaBackend
        {
            DecompilerJarPath    = FindDecompilerJar("vineflower") ?? FindDecompilerJar("cfr"),
            DecompilerDisplayName = FindDecompilerJar("vineflower") != null ? "Vineflower" : "CFR",
        });
        reg.Register(new AndroidBackend());
        reg.Register(new PlainTextBackend());
        return reg;
    }

    private static string? FindDecompilerJar(string nameHint)
    {
        string appDir = AppContext.BaseDirectory;
        return Directory.GetFiles(appDir, "*.jar", SearchOption.TopDirectoryOnly)
                        .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f)
                            .Contains(nameHint, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed record EntryOpenRequest(string FilePath, string EntryPath, string DisplayName);
