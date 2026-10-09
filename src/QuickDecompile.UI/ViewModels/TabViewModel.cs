using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuickDecompile.Core;

namespace QuickDecompile.UI.ViewModels;

public sealed partial class TabViewModel : ObservableObject
{
    private readonly IDecompilerBackend _backend;
    private readonly DecompileCache     _cache;

    public string FilePath  { get; }
    public string? EntryPath { get; }
    public FileFormat Format { get; }

    [ObservableProperty] private string _header = "Untitled";
    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private string _sourceCode = string.Empty;
    [ObservableProperty] private string _language    = "text";
    [ObservableProperty] private string _statusText  = string.Empty;

    public ObservableCollection<EntryNodeViewModel> EntryTree { get; } = [];

    public bool HasEntryTree => EntryTree.Count > 0;

    public event Action<TabViewModel>? CloseRequested;

    public TabViewModel(string filePath, string? entryPath, FileFormat format,
                        IDecompilerBackend backend, DecompileCache cache)
    {
        FilePath  = filePath;
        EntryPath = entryPath;
        Format    = format;
        _backend  = backend;
        _cache    = cache;
    }

    [RelayCommand]
    public void RequestClose() => CloseRequested?.Invoke(this);

    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading = true;
        try
        {
            var entries = await _backend.ListEntriesAsync(FilePath, ct);
            EntryTree.Clear();
            foreach (var root in entries)
                EntryTree.Add(MapEntry(root));
            OnPropertyChanged(nameof(HasEntryTree));

            if (!HasEntryTree || EntryPath != null)
            {
                await DecompileEntryAsync(EntryPath, ct);
            }
            else
            {
                SourceCode = "
                Language   = "text";
                StatusText = $"Format: {Format}  |  Backend: {_backend.Name}  |  {EntryTree.Count} top-level entries";
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            SourceCode = $"
            Language   = "text";
            StatusText = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task DecompileEntryAsync(string? entryPath, CancellationToken ct = default)
    {
        IsLoading = true;
        try
        {
            if (_cache.TryGet(FilePath, entryPath, out var cached) && cached != null)
            {
                ApplyResult(cached);
                return;
            }

            var result = await _backend.DecompileAsync(FilePath, entryPath, ct);
            _cache.Set(FilePath, entryPath, result);
            ApplyResult(result);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            SourceCode = $"
            Language   = "text";
            StatusText = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyResult(DecompileResult result)
    {
        SourceCode = result.SourceCode ?? result.ErrorMessage ?? string.Empty;
        Language   = result.Language;
        StatusText = $"Format: {Format}  |  Backend: {result.BackendName}  |  {result.Elapsed.TotalMilliseconds:F0} ms";
    }

    private static EntryNodeViewModel MapEntry(ContainerEntry entry)
    {
        var node = new EntryNodeViewModel
        {
            DisplayName  = entry.DisplayName,
            EntryPath    = entry.EntryPath,
            IsContainer  = entry.IsContainer,
        };
        foreach (var child in entry.Children)
            node.Children.Add(MapEntry(child));
        return node;
    }
}
