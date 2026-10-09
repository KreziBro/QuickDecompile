using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using QuickDecompile.Core;

namespace QuickDecompile.Backends;

public sealed class JavaBackend : IDecompilerBackend
{
    public string? DecompilerJarPath { get; set; }

    public string DecompilerDisplayName { get; set; } = "Vineflower";

    public string Name => $"Java/{DecompilerDisplayName}";

    public bool CanHandle(FileFormat format) =>
        format is FileFormat.JavaClass or FileFormat.JavaJar;

    public async Task<DecompileResult> DecompileAsync(string filePath, string? entry, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            string? javaExe = FindJava();
            if (javaExe == null)
            {
                sw.Stop();
                return DecompileResult.Fail(
                    "Java runtime not found. Please install JDK/JRE and set JAVA_HOME or add 'java' to PATH.",
                    Name, sw.Elapsed);
            }

            if (string.IsNullOrEmpty(DecompilerJarPath) || !File.Exists(DecompilerJarPath))
            {
                sw.Stop();
                return DecompileResult.Fail(
                    "Java decompiler JAR not found. Place Vineflower.jar (or CFR.jar) next to the application.",
                    Name, sw.Elapsed);
            }

            string result = await DecompileWithJarAsync(javaExe, filePath, entry, ct);
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
        if (filePath.EndsWith(".class", StringComparison.OrdinalIgnoreCase))
            return [];

        return await Task.Run(() => ListJarEntries(filePath, ct), ct);
    }

    private async Task<string> DecompileWithJarAsync(string javaExe, string filePath, string? entry, CancellationToken ct)
    {
        string tmpDir = Path.Combine(Path.GetTempPath(), $"qd_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tmpDir);

        try
        {
            string inputPath = filePath;

            if (entry != null && (filePath.EndsWith(".jar", StringComparison.OrdinalIgnoreCase) ||
                                   filePath.EndsWith(".war", StringComparison.OrdinalIgnoreCase)))
            {
                inputPath = await ExtractClassFromJarAsync(filePath, entry, tmpDir, ct);
            }

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName  = javaExe,
                Arguments = $"-jar \"{DecompilerJarPath}\" \"{inputPath}\" \"{tmpDir}\"",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true,
            };

            process.Start();

            var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
            var stderrTask = process.StandardError.ReadToEndAsync(ct);

            await process.WaitForExitAsync(ct);
            string stdout = await stdoutTask;
            string stderr = await stderrTask;

            if (process.ExitCode != 0 && string.IsNullOrWhiteSpace(stdout))
                throw new Exception($"Decompiler exited with code {process.ExitCode}: {stderr}");

            var javaFiles = Directory.GetFiles(tmpDir, "*.java", SearchOption.AllDirectories);
            if (javaFiles.Length == 0)
                return string.IsNullOrWhiteSpace(stdout) ? $"

            var sb = new StringBuilder();
            sb.AppendLine($"
            foreach (var jf in javaFiles.OrderBy(x => x))
            {
                sb.AppendLine(await File.ReadAllTextAsync(jf, ct));
            }
            return sb.ToString();
        }
        finally
        {
            try { Directory.Delete(tmpDir, recursive: true); } catch {  }
        }
    }

    private static async Task<string> ExtractClassFromJarAsync(string jarPath, string entryName, string outDir, CancellationToken ct)
    {
        using var zip = ZipFile.OpenRead(jarPath);
        var zipEntry = zip.GetEntry(entryName) ??
                       zip.Entries.FirstOrDefault(e => e.FullName.Equals(entryName, StringComparison.OrdinalIgnoreCase));

        if (zipEntry == null)
            throw new FileNotFoundException($"Entry '{entryName}' not found in {jarPath}");

        string destPath = Path.Combine(outDir, Path.GetFileName(entryName));
        await using var dest = File.Create(destPath);
        await using var src  = zipEntry.Open();
        await src.CopyToAsync(dest, ct);
        return destPath;
    }

    private static IReadOnlyList<ContainerEntry> ListJarEntries(string filePath, CancellationToken ct)
    {
        try
        {
            using var zip = ZipFile.OpenRead(filePath);
            var classPaths = zip.Entries
                .Where(e => e.FullName.EndsWith(".class", StringComparison.OrdinalIgnoreCase)
                         && !e.FullName.Contains('$'))
                .Select(e => e.FullName)
                .ToList();

            return EntryTreeBuilder.Build(classPaths);
        }
        catch (InvalidDataException)
        {
            return [];
        }
    }

    internal static string? FindJava()
    {
        string? javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome))
        {
            string candidate = Path.Combine(javaHome, "bin",
                OperatingSystem.IsWindows() ? "java.exe" : "java");
            if (File.Exists(candidate)) return candidate;
        }

        string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
        string javaExe = OperatingSystem.IsWindows() ? "java.exe" : "java";
        foreach (var dir in pathEnv.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(dir)) continue;
            string full = Path.Combine(dir.Trim(), javaExe);
            if (File.Exists(full)) return full;
        }

        return null;
    }
}
