namespace QuickDecompile.Core;

public interface IDecompilerBackend
{
    string Name { get; }

    bool CanHandle(FileFormat format);

    Task<DecompileResult> DecompileAsync(string filePath, string? entry, CancellationToken ct = default);

    Task<IReadOnlyList<ContainerEntry>> ListEntriesAsync(string filePath, CancellationToken ct = default);
}
