namespace QuickDecompile.Core;

public sealed class BackendRegistry
{
    private readonly List<IDecompilerBackend> _backends = [];

    public void Register(IDecompilerBackend backend) => _backends.Add(backend);

    public IDecompilerBackend? Resolve(FileFormat format) =>
        _backends.FirstOrDefault(b => b.CanHandle(format));

    public IReadOnlyList<IDecompilerBackend> All => _backends.AsReadOnly();
}
