using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using QuickDecompile.UI.ViewModels;
using QuickDecompile.UI.Views;
using QuickDecompile.Shell;

namespace QuickDecompile.UI;

public partial class App : Application
{
    private SingleInstanceManager? _singleInstance;
    private MainWindowViewModel?   _mainVm;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            string[] args = desktop.Args ?? [];

            _singleInstance = new SingleInstanceManager("QD1");
            if (!_singleInstance.TryBecomePrimary())
            {
                if (args.Length > 0)
                    SingleInstanceManager.SendFilePathAsync("QD1", args[0]).GetAwaiter().GetResult();
                desktop.Shutdown();
                base.OnFrameworkInitializationCompleted();
                return;
            }

            _mainVm = new MainWindowViewModel();

            _mainVm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(MainWindowViewModel.IsDarkTheme))
                    RequestedThemeVariant = _mainVm.IsDarkTheme
                        ? ThemeVariant.Dark
                        : ThemeVariant.Light;
            };

            var mainWindow = new MainWindow { DataContext = _mainVm };
            desktop.MainWindow = mainWindow;

            if (args.Length > 0 && File.Exists(args[0]))
            {
                mainWindow.Loaded += async (_, _) => await _mainVm.OpenFileAsync(args[0]);
            }

            _singleInstance.FileOpenRequested += async path =>
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    mainWindow.Activate();
                    await _mainVm.OpenFileAsync(path);
                });
            };
            _singleInstance.StartListening();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
