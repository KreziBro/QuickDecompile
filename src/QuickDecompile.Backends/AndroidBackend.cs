using System.Diagnostics;
using System.IO.Compression;
using QuickDecompile.Core;

namespace QuickDecompile.Backends;

public sealed class AndroidBackend : IDecompilerBackend
{
    public string? JadxPath { get; set; }

    public string Name => "jadx";

    public bool CanHandle(FileFormat format) =>
        format is FileFormat.AndroidApk or FileFormat.AndroidAab or FileFormat.AndroidDex;

    public async Task<DecompileResult> DecompileAsync(string filePath, string? entry, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            string? jadx = ResolveJadx();
            if (jadx == null)
            {
                sw.Stop();
                return DecompileResult.Fail(
                    "jadx not found. Download jadx from https://github.com/skylot/jadx/releases and add it to PATH.",
                    Name, sw.Elapsed);
            }

            string result = await RunJadxAsync(jadx, filePath, entry, ct);
            sw.Stop();
            return DecompileResult.Ok(result, "java", Name, sw.Elapsed);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            sw.Stop();
            return DecompileResult.Fail(ex.Message, Name, sw.Elapsed);
        }
    }

    public async Task<IReadOnlyList<ContainerEntry>> ListEntriesAsync(string filePath, CancellationToken ct = default)
    {
        return await Task.Run(() => ListApkEntries(filePath, ct), ct);
    }

    private async Task<string> RunJadxAsync(string jadx, string filePath, string? entry, CancellationToken ct)
    {
        string tmpDir = Path.Combine(Path.GetTempPath(), $"qd_jadx_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tmpDir);

        try
        {
            string args = $"-d \"{tmpDir}\" \"{filePath}\"";
            if (!string.IsNullOrEmpty(entry))
            {
                string cls = entry.Replace('/', '.').RemoveSuffix(".class");
                args = $"--single-class {cls} -d \"{tmpDir}\" \"{filePath}\"";
            }

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName               = jadx,
                Arguments              = args,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true,
            };

            process.Start();
            var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
            var stderrTask = process.StandardError.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);
            string stderr = await stderrTask;
            await stdoutTask;

            var javaFiles = Directory.GetFiles(tmpDir, "*.java", SearchOption.AllDirectories)
                                     .OrderBy(x => x)
                                     .ToArray();

            if (javaFiles.Length == 0)
                return $"

            return await File.ReadAllTextAsync(javaFiles[0], ct);
        }
        finally
        {
            try { Directory.Delete(tmpDir, recursive: true); } catch {  }
        }
    }

    private static IReadOnlyList<ContainerEntry> ListApkEntries(string filePath, CancellationToken ct)
    {
        try
        {
            using var zip = ZipFile.OpenRead(filePath);
            var dexEntries = zip.Entries
                .Where(e => e.FullName.StartsWith("classes", StringComparison.OrdinalIgnoreCase)
                         && e.FullName.EndsWith(".dex", StringComparison.OrdinalIgnoreCase))
                .Select(e => e.FullName)
                .ToList();

            return dexEntries.Select(d => new ContainerEntry
            {
                EntryPath   = d,
                DisplayName = d,
                IsContainer = true,
            }).ToList();
        }
        catch
        {
            return [];
        }
    }

    private string? ResolveJadx()
    {
        if (!string.IsNullOrEmpty(JadxPath) && File.Exists(JadxPath))
            return JadxPath;

        string exe = OperatingSystem.IsWindows() ? "jadx.bat" : "jadx";
        string path = Environment.GetEnvironmentVariable("PATH") ?? "";
        foreach (var dir in path.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(dir)) continue;
            string full = Path.Combine(dir.Trim(), exe);
            if (File.Exists(full)) return full;
        }

        string appDir = AppContext.BaseDirectory;
        foreach (var candidate in Directory.GetFiles(appDir, "jadx*.jar"))
            return candidate;

        return null;
    }
}

file static class StringExtensions
{
    public static string RemoveSuffix(this string s, string suffix) =>
        s.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) ? s[..^suffix.Length] : s;
}
