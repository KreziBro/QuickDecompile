using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace QuickDecompile.UI.ViewModels;

public sealed partial class EntryNodeViewModel : ObservableObject
{
    [ObservableProperty] private bool _isExpanded;

    public required string  DisplayName { get; init; }
    public required string  EntryPath   { get; init; }
    public required bool    IsContainer { get; init; }

    public ObservableCollection<EntryNodeViewModel> Children { get; } = [];

    public string Icon => IsContainer ? "📁" : "📄";
}
