using System.Collections.Concurrent;

namespace QuickDecompile.Core;

public sealed class DecompileCache
{
    private readonly ConcurrentDictionary<CacheKey, DecompileResult> _store = new();

    public bool TryGet(string filePath, string? entry, out DecompileResult? result)
    {
        var key = MakeKey(filePath, entry);
        return _store.TryGetValue(key, out result);
    }

    public void Set(string filePath, string? entry, DecompileResult result)
    {
        var key = MakeKey(filePath, entry);
        _store[key] = result;
    }

    public void Invalidate(string filePath)
    {
        foreach (var key in _store.Keys.Where(k => k.FilePath == filePath))
            _store.TryRemove(key, out _);
    }

    private static CacheKey MakeKey(string filePath, string? entry)
    {
        DateTime mtime = File.Exists(filePath) ? File.GetLastWriteTimeUtc(filePath) : DateTime.MinValue;
        return new CacheKey(filePath, entry ?? string.Empty, mtime);
    }

    private readonly record struct CacheKey(string FilePath, string Entry, DateTime Mtime);
}
