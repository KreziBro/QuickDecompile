using System.Text;
using QuickDecompile.Core;

namespace QuickDecompile.Backends;

public sealed class PlainTextBackend : IDecompilerBackend
{
    private const int MaxTextBytes = 10 * 1024 * 1024;

    public string Name => "PlainText";

    public bool CanHandle(FileFormat format) =>
        format == FileFormat.PlainText || format == FileFormat.Unknown;

    public async Task<DecompileResult> DecompileAsync(string filePath, string? entry, CancellationToken ct = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                sw.Stop();
                return DecompileResult.Fail("File not found.", Name, sw.Elapsed);
            }

            await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] buf = new byte[Math.Min(MaxTextBytes, fileInfo.Length)];
            int read = await fs.ReadAsync(buf.AsMemory(0, buf.Length), ct);

            var sb = new StringBuilder();
            sb.AppendLine($"
            sb.AppendLine($"
            sb.AppendLine($"
            sb.AppendLine();

            bool isText = !buf.Take(Math.Min(512, read)).Any(b => b == 0);

            if (isText)
            {
                sb.Append(Encoding.UTF8.GetString(buf, 0, read));
                if (fileInfo.Length > MaxTextBytes)
                    sb.AppendLine($"\n\n... (truncated, file is {fileInfo.Length:N0} bytes)");
            }
            else
            {
                sb.AppendLine("
                sb.AppendLine(HexDump(buf, Math.Min(256, read)));
            }

            sw.Stop();
            return DecompileResult.Ok(sb.ToString(), "text", Name, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return DecompileResult.Fail(ex.Message, Name, sw.Elapsed);
        }
    }

    public Task<IReadOnlyList<ContainerEntry>> ListEntriesAsync(string filePath, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ContainerEntry>>([]);

    private static string HexDump(byte[] data, int length)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < length; i += 16)
        {
            sb.Append($"  {i:X8}  ");
            for (int j = i; j < Math.Min(i + 16, length); j++)
                sb.Append($"{data[j]:X2} ");
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
