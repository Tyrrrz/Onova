using System.IO;
using System.IO.Compression;

namespace Onova.Tests.Utils.Extensions;

internal static class ZipExtensions
{
    extension(ZipArchiveEntry entry)
    {
        public void WriteAllBytes(byte[] content)
        {
            using var stream = entry.Open();
            stream.Write(content, 0, content.Length);
        }

        public string ReadAllText()
        {
            using var stream = entry.Open();
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
