using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using NUnit.Framework;
using Onova.Services;
using Onova.Tests.Internal;

namespace Onova.Tests
{
    [TestFixture]
    public class ZipPackageExtractorTests
    {
        private static string TempDirPath => Path.Combine(TestContext.CurrentContext.TestDirectory, "Temp");

        private static void CreateZipArchive(string filePath, IReadOnlyDictionary<string, byte[]> entries)
        {
            using (var zip = ZipFile.Open(filePath, ZipArchiveMode.Create))
            {
                foreach (var entry in entries)
                    zip.CreateEntry(entry.Key).WriteAllBytes(entry.Value);
            }
        }

        [SetUp]
        public void Setup()
        {
            // Ensure temp directory exists and is empty
            DirectoryEx.Reset(TempDirPath);
        }

        [TearDown]
        public void Cleanup()
        {
            // Delete temp directory
            if (Directory.Exists(TempDirPath))
                Directory.Delete(TempDirPath, true);
        }

        [Test]
        public async Task ExtractPackageAsync_Test()
        {
            // Arrange
            var entries = new Dictionary<string, byte[]>
            {
                {"File1.bin", new byte[] {1, 2, 3}},
                {"File2.bin", new byte[] {4, 5, 6}},
                {"SubDir1/", new byte[0]},
                {"SubDir1/File3.bin", new byte[] {7, 8, 9}},
                {"SubDir1/SubDir2/", new byte[0]},
                {"SubDir1/SubDir2/File4.bin", new byte[] {10, 11, 12}}
            };

            var packageFilePath = Path.Combine(TempDirPath, "Package.zip");
            var destDirPath = Path.Combine(TempDirPath, "Output");

            CreateZipArchive(packageFilePath, entries);

            var extractor = new ZipPackageExtractor();

            // Act
            await extractor.ExtractPackageAsync(packageFilePath, destDirPath);

            // Assert
            foreach (var entry in entries)
            {
                var destEntryPath = Path.Combine(destDirPath, entry.Key);

                if (entry.Key.EndsWith("/"))
                {
                    Assert.That(Directory.Exists(destEntryPath), "Directory exists");
                }
                else
                {
                    Assert.That(File.Exists(destEntryPath), "File exists");
                    Assert.That(File.ReadAllBytes(destEntryPath), Is.EqualTo(entry.Value), "File content");
                }
            }
        }
    }
}