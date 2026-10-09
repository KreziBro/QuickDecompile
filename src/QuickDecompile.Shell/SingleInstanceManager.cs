using System.IO.Pipes;

namespace QuickDecompile.Shell;

public sealed class SingleInstanceManager : IDisposable
{
    private readonly string _mutexName;
    private readonly string _pipeName;
    private Mutex? _mutex;
    private CancellationTokenSource? _cts;

    public event Action<string>? FileOpenRequested;

    public SingleInstanceManager(string appName)
    {
        _mutexName = $"QuickDecompile_{appName}_Mutex";
        _pipeName  = $"QuickDecompile_{appName}_Pipe";
    }

    public bool TryBecomePrimary()
    {
        _mutex = new Mutex(initiallyOwned: true, _mutexName, out bool createdNew);
        return createdNew;
    }

    public void StartListening()
    {
        _cts = new CancellationTokenSource();
        Task.Run(() => ListenLoopAsync(_cts.Token));
    }

    public static async Task SendFilePathAsync(string appName, string filePath)
    {
        string pipeName = $"QuickDecompile_{appName}_Pipe";
        try
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            await client.ConnectAsync(timeout: 2000);
            using var writer = new StreamWriter(client);
            await writer.WriteLineAsync(filePath);
            await writer.FlushAsync();
        }
        catch
        {
        }
    }

    private async Task ListenLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var server = new NamedPipeServerStream(_pipeName, PipeDirection.In, maxNumberOfServerInstances: 1);
                await server.WaitForConnectionAsync(ct);
                using var reader = new StreamReader(server);
                string? line = await reader.ReadLineAsync(ct);
                if (!string.IsNullOrEmpty(line))
                    FileOpenRequested?.Invoke(line);
            }
            catch (OperationCanceledException) { break; }
            catch {  }
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
    }
}
