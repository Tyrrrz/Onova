using System.IO;
using System.IO.Compression;

namespace Onova.Tests.Internal
{
    internal static class Extensions
    {
        public static void WriteAllText(this ZipArchiveEntry entry, string contents)
        {
            using (var output = entry.Open())
            using (var writer = new StreamWriter(output))
                writer.Write(contents);
        }

        public static string ReadAllText(this ZipArchiveEntry entry)
        {
            using (var output = entry.Open())
            using (var reader = new StreamReader(output))
                return reader.ReadToEnd();
        }
    }
}