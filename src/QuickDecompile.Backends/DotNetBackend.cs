using System.Diagnostics;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using QuickDecompile.Core;

namespace QuickDecompile.Backends;

public sealed class DotNetBackend : IDecompilerBackend
{
    public string Name => "ICSharpCode.Decompiler";

    public bool CanHandle(FileFormat format) =>
        format == FileFormat.DotNetAssembly;

    public async Task<DecompileResult> DecompileAsync(string filePath, string? entry, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await Task.Run(() => DecompileInternal(filePath, entry, ct), ct);
            sw.Stop();
            return DecompileResult.Ok(result, "csharp", Name, sw.Elapsed);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            sw.Stop();
            return DecompileResult.Fail(ex.Message, Name, sw.Elapsed);
        }
    }

    public async Task<IReadOnlyList<ContainerEntry>> ListEntriesAsync(string filePath, CancellationToken ct = default)
    {
        return await Task.Run(() => ListInternal(filePath, ct), ct);
    }

    private static string DecompileInternal(string filePath, string? entry, CancellationToken ct)
    {
        var settings = new DecompilerSettings
        {
            ThrowOnAssemblyResolveErrors = false,
        };

        var decompiler = new CSharpDecompiler(filePath, settings);

        if (string.IsNullOrEmpty(entry))
        {
            return decompiler.DecompileWholeModuleAsString();
        }

        var typeName = new FullTypeName(entry);
        return decompiler.DecompileTypeAsString(typeName);
    }

    private static IReadOnlyList<ContainerEntry> ListInternal(string filePath, CancellationToken ct)
    {
        var settings = new DecompilerSettings { ThrowOnAssemblyResolveErrors = false };
        var decompiler = new CSharpDecompiler(filePath, settings);
        var module = decompiler.TypeSystem.MainModule;

        var paths = new List<string>();
        foreach (var type in module.TopLevelTypeDefinitions)
        {
            ct.ThrowIfCancellationRequested();
            if (type.Name.StartsWith('<')) continue;
            string ns = type.Namespace.Replace('.', '/');
            string path = string.IsNullOrEmpty(ns) ? type.Name : $"{ns}/{type.Name}";
            paths.Add(path);
        }

        return EntryTreeBuilder.Build(paths);
    }
}
