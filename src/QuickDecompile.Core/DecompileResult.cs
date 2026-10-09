namespace QuickDecompile.Core;

public sealed class DecompileResult
{
    public bool Success { get; init; }

    public string? SourceCode { get; init; }

    public string Language { get; init; } = "text";

    public string BackendName { get; init; } = string.Empty;

    public TimeSpan Elapsed { get; init; }

    public string? ErrorMessage { get; init; }

    public static DecompileResult Ok(string code, string language, string backend, TimeSpan elapsed) =>
        new() { Success = true, SourceCode = code, Language = language, BackendName = backend, Elapsed = elapsed };

    public static DecompileResult Fail(string message, string backend, TimeSpan elapsed) =>
        new() { Success = false, ErrorMessage = message, SourceCode = $"
}
