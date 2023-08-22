using System.IO;
using System.IO.Compression;

namespace Onova.Tests.Utils.Extensions;

internal static class ZipExtensions
{
    public static void WriteAllBytes(this ZipArchiveEntry entry, byte[] content)
    {
        using var stream = entry.Open();
        stream.Write(content, 0, content.Length);
    }

    public static string ReadAllText(this ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
