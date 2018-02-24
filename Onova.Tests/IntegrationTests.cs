using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Onova.Services;

namespace Onova.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private static string TestDirPath => TestContext.CurrentContext.TestDirectory;
        private static string TempDirPath => Path.Combine(TestDirPath, "Temp");

        private static string StorageDirPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Onova",
            Assembly.GetExecutingAssembly().GetName().Name);

        [TearDown]
        public void Cleanup()
        {
            if (Directory.Exists(StorageDirPath))
                Directory.Delete(StorageDirPath, true);
            if (Directory.Exists(TempDirPath))
                Directory.Delete(TempDirPath, true);
        }

        [Test]
        public async Task LocalPackageResolver_GetAllVersionsAsync_Test()
        {
            // Arrange
            var expectedVersions = new[] {Version.Parse("1.0"), Version.Parse("2.0")};
            Directory.CreateDirectory(TempDirPath);
            foreach (var expectedVersion in expectedVersions)
                File.WriteAllText(Path.Combine(TempDirPath, $"{expectedVersion}.onv"), "");

            // Act
            var resolver = new LocalPackageResolver(TempDirPath);
            var versions = await resolver.GetAllVersionsAsync();

            // Assert
            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task LocalPackageResolver_GetPackageAsync_Test()
        {
            // Arrange
            var version = Version.Parse("2.0");
            const string expectedContent = "Hello world";
            Directory.CreateDirectory(TempDirPath);
            File.WriteAllText(Path.Combine(TempDirPath, $"{version}.onv"), expectedContent);

            // Act
            var resolver = new LocalPackageResolver(TempDirPath);
            var stream = await resolver.GetPackageAsync(version);

            // Assert
            Assert.That(stream, Is.Not.Null);

            using (stream)
            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync();
                Assert.That(content, Is.EqualTo(expectedContent));
            }
        }

        [Test]
        public async Task GithubPackageResolver_GetAllVersionsAsync_Test()
        {
            // This uses a stub repository (github.com/Tyrrrz/OnovaTestRepo)

            // Arrange
            var expectedVersions = new[] {Version.Parse("1.0"), Version.Parse("2.0"), Version.Parse("3.0")};

            // Act
            var resolver = new GithubPackageResolver("Tyrrrz", "OnovaTestRepo", "Test.onv");
            var versions = await resolver.GetAllVersionsAsync();

            // Assert
            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task GithubPackageResolver_GetPackageAsync_Test()
        {
            // This uses a stub repository (github.com/Tyrrrz/OnovaTestRepo)

            // Arrange
            const string expectedContent = "Hello world";

            // Act
            var resolver = new GithubPackageResolver("Tyrrrz", "OnovaTestRepo", "Test.onv");
            var stream = await resolver.GetPackageAsync(Version.Parse("2.0"));

            // Assert
            Assert.That(stream, Is.Not.Null);

            using (stream)
            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync();
                Assert.That(content, Is.EqualTo(expectedContent));
            }
        }

        [Test]
        public async Task ZipPackageExtractor_ExtractPackageAsync_Test()
        {
            // Arrange
            const string expectedContent = "Hello world";
            var expectedEntryFilePaths = new[] {"a.txt", "1\\b.txt", "1\\2\\c.txt"};
            Directory.CreateDirectory(TempDirPath);
            var packageFilePath = Path.Combine(TempDirPath, "1.0.0.0.onv");
            using (var output = File.Create(packageFilePath))
            using (var zip = new ZipArchive(output, ZipArchiveMode.Create))
            {
                foreach (var expectedEntryFilePath in expectedEntryFilePaths)
                {
                    using (var stream = zip.CreateEntry(expectedEntryFilePath).Open())
                    using (var writer = new StreamWriter(stream))
                        writer.Write(expectedContent);
                }
            }

            // Act
            var extractor = new ZipPackageExtractor();
            var outputDirPath = Path.Combine(TempDirPath, Guid.NewGuid().ToString());
            await extractor.ExtractPackageAsync(packageFilePath, outputDirPath);

            // Assert
            foreach (var expectedEntryFilePath in expectedEntryFilePaths)
            {
                var outputEntryPath = Path.Combine(outputDirPath, expectedEntryFilePath);
                Assert.That(File.Exists(outputEntryPath));
                Assert.That(File.ReadAllText(outputEntryPath), Is.EqualTo(expectedContent));
            }
        }
    }
}