using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory(TempDirPath);
        }

        [TearDown]
        public void Cleanup()
        {
            if (Directory.Exists(StorageDirPath))
                Directory.Delete(StorageDirPath, true);
            if (Directory.Exists(TempDirPath))
                Directory.Delete(TempDirPath, true);
        }

        [Test]
        public async Task LocalPackageResolver_GetAllPackageVersionsAsync_Test()
        {
            // Arrange
            var expectedVersions = new[] {Version.Parse("1.0"), Version.Parse("2.0")};

            foreach (var expectedVersion in expectedVersions)
                File.WriteAllText(Path.Combine(TempDirPath, $"{expectedVersion}.onv"), "");

            // Act
            var resolver = new LocalPackageResolver(TempDirPath);
            var versions = await resolver.GetAllPackageVersionsAsync();

            // Assert
            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task LocalPackageResolver_DownloadPackageAsync_Test()
        {
            // Arrange
            var version = Version.Parse("2.0");
            var expectedContent = "Hello world";
            var destFilePath = Path.Combine(TempDirPath, $"{Guid.NewGuid()}");

            File.WriteAllText(Path.Combine(TempDirPath, $"{version}.onv"), expectedContent);

            // Act
            var resolver = new LocalPackageResolver(TempDirPath);
            await resolver.DownloadPackageAsync(version, destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath));
            Assert.That(File.ReadAllText(destFilePath), Is.EqualTo(expectedContent));
        }

        [Test]
        public async Task GithubPackageResolver_GetAllPackageVersionsAsync_Test()
        {
            // This uses a stub repository (github.com/Tyrrrz/OnovaTestRepo)

            // Arrange
            var expectedVersions = new[] {Version.Parse("1.0"), Version.Parse("2.0"), Version.Parse("3.0")};

            // Act
            var resolver = new GithubPackageResolver("Tyrrrz", "OnovaTestRepo", "Test.onv");
            var versions = await resolver.GetAllPackageVersionsAsync();

            // Assert
            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task GithubPackageResolver_DownloadPackageAsync_Test()
        {
            // This uses a stub repository (github.com/Tyrrrz/OnovaTestRepo)

            // Arrange
            var version = Version.Parse("2.0");
            var expectedContent = "Hello world";
            var destFilePath = Path.Combine(TempDirPath, $"{Guid.NewGuid()}");

            // Act
            var resolver = new GithubPackageResolver("Tyrrrz", "OnovaTestRepo", "Test.onv");
            await resolver.DownloadPackageAsync(version, destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath));
            Assert.That(File.ReadAllText(destFilePath), Is.EqualTo(expectedContent));
        }

        [Test]
        public async Task WebPackageResolver_GetAllPackageVersionsAsync_Test()
        {
            // This uses a stub manifest from stub repository (github.com/Tyrrrz/OnovaTestRepo)

            // Arrange
            var expectedVersions = new[] {Version.Parse("1.0"), Version.Parse("2.0"), Version.Parse("3.0")};

            // Act
            var url = "https://raw.githubusercontent.com/Tyrrrz/OnovaTestRepo/master/TestWebPackageManifest.txt";
            var resolver = new WebPackageResolver(url);
            var versions = await resolver.GetAllPackageVersionsAsync();

            // Assert
            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task WebPackageResolver_DownloadPackageAsync_Test()
        {
            // This uses a stub manifest from stub repository (github.com/Tyrrrz/OnovaTestRepo)

            // Arrange
            var version = Version.Parse("2.0");
            var expectedContent = "Hello world";
            var destFilePath = Path.Combine(TempDirPath, $"{Guid.NewGuid()}");

            // Act
            var url = "https://raw.githubusercontent.com/Tyrrrz/OnovaTestRepo/master/TestWebPackageManifest.txt";
            var resolver = new WebPackageResolver(url);
            await resolver.DownloadPackageAsync(version, destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath));
            Assert.That(File.ReadAllText(destFilePath), Is.EqualTo(expectedContent));
        }

        [Test]
        public async Task AggregatePackageResolver_GetAllPackageVersionsAsync_Test()
        {
            // Arrange
            var expectedVersions = new[] {Version.Parse("1.0"), Version.Parse("2.0"), Version.Parse("3.0")};

            var repository1DirPath = Path.Combine(TempDirPath, "1");
            Directory.CreateDirectory(repository1DirPath);
            foreach (var expectedVersion in expectedVersions.Take(expectedVersions.Length / 2))
                File.WriteAllText(Path.Combine(repository1DirPath, $"{expectedVersion}.onv"), "");

            var repository2DirPath = Path.Combine(TempDirPath, "2");
            Directory.CreateDirectory(repository2DirPath);
            foreach (var expectedVersion in expectedVersions.Skip(expectedVersions.Length / 2))
                File.WriteAllText(Path.Combine(repository2DirPath, $"{expectedVersion}.onv"), "");

            // Act
            var resolver = new AggregatePackageResolver(
                new LocalPackageResolver(repository1DirPath),
                new LocalPackageResolver(repository2DirPath));
            var versions = await resolver.GetAllPackageVersionsAsync();

            // Assert
            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task AggregatePackageResolver_DownloadPackageAsync_Test()
        {
            // Arrange
            var version = Version.Parse("2.0");
            var expectedContent = "Hello world";
            var destFilePath = Path.Combine(TempDirPath, $"{Guid.NewGuid()}");

            var repository1DirPath = Path.Combine(TempDirPath, "1");
            Directory.CreateDirectory(repository1DirPath);

            var repository2DirPath = Path.Combine(TempDirPath, "2");
            Directory.CreateDirectory(repository2DirPath);
            File.WriteAllText(Path.Combine(repository2DirPath, $"{version}.onv"), expectedContent);

            // Act
            var resolver = new AggregatePackageResolver(
                new LocalPackageResolver(repository1DirPath),
                new LocalPackageResolver(repository2DirPath));
            await resolver.DownloadPackageAsync(version, destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath));
            Assert.That(File.ReadAllText(destFilePath), Is.EqualTo(expectedContent));
        }

        [Test]
        public async Task ZipPackageExtractor_ExtractPackageAsync_Test()
        {
            // Arrange
            var expectedContent = "Hello world";
            var expectedEntryPaths = new[] {"a.txt", "1\\b.txt", "1\\2\\c.txt"};
            var packageFilePath = Path.Combine(TempDirPath, "1.0.0.0.onv");
            var destDirPath = Path.Combine(TempDirPath, Guid.NewGuid().ToString());

            using (var output = File.Create(packageFilePath))
            using (var zip = new ZipArchive(output, ZipArchiveMode.Create))
            {
                foreach (var expectedEntryPath in expectedEntryPaths)
                {
                    using (var stream = zip.CreateEntry(expectedEntryPath).Open())
                    using (var writer = new StreamWriter(stream))
                        await writer.WriteAsync(expectedContent);
                }
            }

            // Act
            var extractor = new ZipPackageExtractor();
            await extractor.ExtractPackageAsync(packageFilePath, destDirPath);

            // Assert
            foreach (var expectedEntryPath in expectedEntryPaths)
            {
                var destEntryPath = Path.Combine(destDirPath, expectedEntryPath);
                Assert.That(File.Exists(destEntryPath));
                Assert.That(File.ReadAllText(destEntryPath), Is.EqualTo(expectedContent));
            }
        }
    }
}