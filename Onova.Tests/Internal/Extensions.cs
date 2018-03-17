using System.IO;
using System.IO.Compression;

namespace Onova.Tests.Internal
{
    internal static class Extensions
    {
        public static ZipArchiveEntry CreateTextEntry(this ZipArchive archive, string entryName, string content)
        {
            var entry = archive.CreateEntry(entryName);

            using (var output = entry.Open())
            using (var writer = new StreamWriter(output))
                writer.Write(content);

            return entry;
        }
    }
}