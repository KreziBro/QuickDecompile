using System.IO.Compression;

namespace QuickDecompile.Core;

public static class FileFormatDetector
{
    private static readonly byte[] MagicCafeBabe = [0xCA, 0xFE, 0xBA, 0xBE];
    private static readonly byte[] MagicDex      = "dex\n"u8.ToArray();
    private static readonly byte[] MagicZip      = [0x50, 0x4B, 0x03, 0x04];
    private static readonly byte[] MagicMz        = [0x4D, 0x5A];

    private const int PeCliHeaderOffset = 0x168;

    public static async Task<FileFormat> DetectAsync(string filePath, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
            return FileFormat.Unknown;

        byte[] header = new byte[8];
        int read;
        await using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
        {
            read = await fs.ReadAsync(header.AsMemory(0, header.Length), ct);
        }

        if (read < 4)
            return FileFormat.PlainText;

        if (StartsWith(header, MagicCafeBabe))
            return FileFormat.JavaClass;

        if (read >= 4 && StartsWith(header, MagicDex))
            return FileFormat.AndroidDex;

        if (StartsWith(header, MagicZip))
            return await ClassifyZipAsync(filePath, ct);

        if (read >= 2 && StartsWith(header, MagicMz))
        {
            if (await HasCliHeaderAsync(filePath, ct))
                return FileFormat.DotNetAssembly;
        }

        return FileFormat.PlainText;
    }

    private static async Task<FileFormat> ClassifyZipAsync(string filePath, CancellationToken ct)
    {
        try
        {
            using var zip = ZipFile.OpenRead(filePath);

            bool hasDex    = false;
            bool hasClass  = false;
            bool hasAabBase = false;

            foreach (var entry in zip.Entries)
            {
                ct.ThrowIfCancellationRequested();
                var name = entry.FullName;

                if (name.StartsWith("classes", StringComparison.OrdinalIgnoreCase) && name.EndsWith(".dex", StringComparison.OrdinalIgnoreCase))
                    hasDex = true;

                if (name.EndsWith(".class", StringComparison.OrdinalIgnoreCase))
                    hasClass = true;

                if (name.StartsWith("base/", StringComparison.OrdinalIgnoreCase))
                    hasAabBase = true;
            }

            if (hasDex && hasAabBase)
                return FileFormat.AndroidAab;

            if (hasDex)
                return FileFormat.AndroidApk;

            if (hasClass)
                return FileFormat.JavaJar;

            return FileFormat.PlainText;
        }
        catch (InvalidDataException)
        {
            return FileFormat.PlainText;
        }
    }

    private static async Task<bool> HasCliHeaderAsync(string filePath, CancellationToken ct)
    {
        try
        {
            await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            using var br = new BinaryReader(fs);

            if (fs.Length < 0x40) return false;
            fs.Seek(0x3C, SeekOrigin.Begin);
            int peOffset = br.ReadInt32();

            if (peOffset <= 0 || peOffset + 4 >= fs.Length) return false;
            fs.Seek(peOffset, SeekOrigin.Begin);

            if (br.ReadUInt32() != 0x00004550) return false;

            fs.Seek(peOffset + 4 + 20, SeekOrigin.Begin);

            if (fs.Position + 2 >= fs.Length) return false;
            ushort optMagic = br.ReadUInt16();

            long optHeaderStart = peOffset + 4 + 20;
            long ddOffset = optMagic == 0x020B
                ? optHeaderStart + 112
                : optHeaderStart + 96;

            long cliEntryPos = ddOffset + 14 * 8;
            if (cliEntryPos + 8 > fs.Length) return false;

            fs.Seek(cliEntryPos, SeekOrigin.Begin);
            uint cliRva = br.ReadUInt32();

            return cliRva != 0;
        }
        catch
        {
            return false;
        }
    }

    private static bool StartsWith(byte[] data, byte[] prefix)
    {
        if (data.Length < prefix.Length) return false;
        for (int i = 0; i < prefix.Length; i++)
            if (data[i] != prefix[i]) return false;
        return true;
    }
}
