using Avalonia;
using System;

namespace QuickDecompile.UI;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        SetupLogging();
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            string crashFile = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "QuickDecompile", "logs", "crash.log");
            System.IO.File.AppendAllText(crashFile, $"[{DateTime.Now}] CRASH:\n{ex}\n\n");
            throw;
        }
    }

    private static void SetupLogging()
    {
        try
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string logDir = System.IO.Path.Combine(appData, "QuickDecompile", "logs");
            System.IO.Directory.CreateDirectory(logDir);
            string logFile = System.IO.Path.Combine(logDir, "app.log");

            var listener = new System.Diagnostics.TextWriterTraceListener(logFile, "FileLog");
            System.Diagnostics.Trace.Listeners.Add(listener);
            System.Diagnostics.Trace.AutoFlush = true;
            System.Diagnostics.Trace.WriteLine($"--- QuickDecompile started at {DateTime.Now} ---");
        }
        catch {  }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
