using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using QuickDecompile.UI.ViewModels;

namespace QuickDecompile.UI.Views;

public partial class TabContentView : UserControl
{
    private TextMate.Installation? _tmInstallationMain;
    private TextMate.Installation? _tmInstallationSingle;

    private TextEditor? EditorMain   => this.FindControl<TextEditor>("CodeEditor");
    private TextEditor? EditorSingle => this.FindControl<TextEditor>("CodeEditorSingle");
    private TreeView?   TreeV        => this.FindControl<TreeView>("EntryTreeView");

    public TabContentView()
    {
        AvaloniaXamlLoader.Load(this);

        if (EditorMain != null) AvaloniaEdit.Search.SearchPanel.Install(EditorMain);
        if (EditorSingle != null) AvaloniaEdit.Search.SearchPanel.Install(EditorSingle);

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is TabViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
            SetupSyntaxHighlighting(vm);
            SetupTreeInteraction(vm);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not TabViewModel vm) return;

        switch (e.PropertyName)
        {
            case nameof(TabViewModel.Language):
                SetupSyntaxHighlighting(vm);
                break;
            case nameof(TabViewModel.HasEntryTree):
                SetupTreeInteraction(vm);
                break;
        }
    }

    private void SetupTreeInteraction(TabViewModel vm)
    {
        if (TreeV is { } tree)
        {
            tree.SelectionChanged += async (_, _) =>
            {
                if (tree.SelectedItem is EntryNodeViewModel node && !node.IsContainer)
                {
                    await vm.DecompileEntryAsync(node.EntryPath);
                }
            };
        }
    }

    private void SetupSyntaxHighlighting(TabViewModel vm)
    {
        SetupEditorHighlighting(EditorMain,   ref _tmInstallationMain,   vm.Language);
        SetupEditorHighlighting(EditorSingle, ref _tmInstallationSingle, vm.Language);
    }

    private static void SetupEditorHighlighting(TextEditor? editor, ref TextMate.Installation? installation, string language)
    {
        if (editor == null) return;

        installation?.Dispose();
        var registryOptions = new RegistryOptions(ThemeName.DarkPlus);
        installation = editor.InstallTextMate(registryOptions);

        string scopeName = language switch
        {
            "csharp" => "source.cs",
            "java"   => "source.java",
            _        => "source.txt",
        };

        try { installation.SetGrammar(scopeName); }
        catch {  }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _tmInstallationMain?.Dispose();
        _tmInstallationSingle?.Dispose();
        base.OnUnloaded(e);
    }
}
