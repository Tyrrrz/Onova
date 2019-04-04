using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Onova.Models;
using Onova.Services;
using Onova.Tests.Internal;

namespace Onova.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private const string LocalUpdateeName = "Onova.Tests.IntegrationTests";

        private static string TestDirPath => TestContext.CurrentContext.TestDirectory;
        private static string TempDirPath => Path.Combine(TestDirPath, "Temp");

        private static string StorageDirPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Onova", LocalUpdateeName);

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

            // Delete storage
            if (Directory.Exists(StorageDirPath))
                Directory.Delete(StorageDirPath, true);
        }

        [Test]
        public async Task UpdateManager_IsUpdatePrepared_Test()
        {
            // Arrange
            var versions = new[]
            {
                Version.Parse("1.0"),
                Version.Parse("2.0"),
                Version.Parse("3.0")
            };

            // Package file
            foreach (var version in versions)
            {
                var packageFilePath = Path.Combine(TempDirPath, $"{version}.onv");
                using (var zip = ZipFile.Open(packageFilePath, ZipArchiveMode.Create))
                    zip.CreateEntry("Test.txt").WriteAllText("Hello world");
            }

            // Update manager
            var updateeVersion = Version.Parse("1.0");
            var updatee = new AssemblyMetadata(LocalUpdateeName, updateeVersion, "");
            var resolver = new LocalPackageResolver(TempDirPath, "*.onv");
            var extractor = new ZipPackageExtractor();

            using (var manager = new UpdateManager(updatee, resolver, extractor))
            {
                // Act
                foreach (var version in versions)
                    await manager.PrepareUpdateAsync(version);

                // Assert
                foreach (var version in versions)
                    Assert.That(manager.IsUpdatePrepared(version));
            }
        }

        [Test]
        public async Task LocalPackageResolver_GetPackageVersionsAsync_Test()
        {
            // Arrange
            var expectedVersions = new[]
            {
                Version.Parse("1.0"),
                Version.Parse("2.0"),
                Version.Parse("3.0")
            };

            foreach (var expectedVersion in expectedVersions)
                File.WriteAllText(Path.Combine(TempDirPath, $"{expectedVersion}.onv"), "");

            // Act
            var resolver = new LocalPackageResolver(TempDirPath, "*.onv");
            var versions = await resolver.GetPackageVersionsAsync();

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
            File.WriteAllText(Path.Combine(TempDirPath, $"{version}.onv"), expectedContent);

            // Act
            var resolver = new LocalPackageResolver(TempDirPath, "*.onv");
            var destFilePath = Path.Combine(TempDirPath, "Output.onv");
            await resolver.DownloadPackageAsync(version, destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath));
            Assert.That(File.ReadAllText(destFilePath), Is.EqualTo(expectedContent));
        }

        [Test]
        public async Task GithubPackageResolver_GetPackageVersionsAsync_Test()
        {
            // Arrange
            var expectedVersions = new[]
            {
                Version.Parse("1.0"),
                Version.Parse("2.0"),
                Version.Parse("3.0")
            };

            // Act
            var resolver = new GithubPackageResolver("Tyrrrz", "OnovaTestRepo", "*.onv");
            var versions = await resolver.GetPackageVersionsAsync();

            // Assert
            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task GithubPackageResolver_DownloadPackageAsync_Test()
        {
            // Arrange
            var version = Version.Parse("2.0");
            var expectedContent = "Hello world";

            // Act
            var resolver = new GithubPackageResolver("Tyrrrz", "OnovaTestRepo", "*.onv");
            var destFilePath = Path.Combine(TempDirPath, "Output.onv");
            await resolver.DownloadPackageAsync(version, destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath));
            Assert.That(File.ReadAllText(destFilePath), Is.EqualTo(expectedContent));
        }

        [Test]
        public async Task WebPackageResolver_GetPackageVersionsAsync_Test()
        {
            // Arrange
            var expectedVersions = new[]
            {
                Version.Parse("1.0"),
                Version.Parse("2.0"),
                Version.Parse("3.0")
            };

            // Act
            var url = "https://raw.githubusercontent.com/Tyrrrz/OnovaTestRepo/master/TestWebPackageManifest.txt";
            var resolver = new WebPackageResolver(url);
            var versions = await resolver.GetPackageVersionsAsync();

            // Assert
            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task WebPackageResolver_DownloadPackageAsync_Test()
        {
            // Arrange
            var version = Version.Parse("2.0");
            var expectedContent = "Hello world";

            // Act
            var url = "https://raw.githubusercontent.com/Tyrrrz/OnovaTestRepo/master/TestWebPackageManifest.txt";
            var resolver = new WebPackageResolver(url);
            var destFilePath = Path.Combine(TempDirPath, "Output.onv");
            await resolver.DownloadPackageAsync(version, destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath));
            Assert.That(File.ReadAllText(destFilePath), Is.EqualTo(expectedContent));
        }

        [Test]
        public async Task NugetPackageResolver_GetPackageVersionsAsync_Test()
        {
            // Arrange
            var expectedVersions = new[]
            {
                Version.Parse("1.0.0"),
                Version.Parse("2.0.0"),
                Version.Parse("3.0.0")
            };

            // Act
            var url = "https://www.myget.org/F/tyrrrz-test/api/v3/index.json";
            var resolver = new NugetPackageResolver(url, "OnovaTest");
            var versions = await resolver.GetPackageVersionsAsync();

            // Assert
            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task NugetPackageResolver_DownloadPackageAsync_Test()
        {
            // Arrange
            var version = Version.Parse("2.0.0");
            var expectedContent = "Hello world";

            // Act
            var url = "https://www.myget.org/F/tyrrrz-test/api/v3/index.json";
            var resolver = new NugetPackageResolver(url, "OnovaTest");
            var destFilePath = Path.Combine(TempDirPath, "Output.onv");
            await resolver.DownloadPackageAsync(version, destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath));

            using (var zip = ZipFile.OpenRead(destFilePath))
            {
                var content = zip.GetEntry("Files/Content.txt").ReadAllText();
                Assert.That(content, Is.EqualTo(expectedContent));
            }
        }

        [Test]
        public async Task AggregatePackageResolver_GetPackageVersionsAsync_Test()
        {
            // Arrange
            var expectedVersions = new[]
            {
                Version.Parse("1.0"),
                Version.Parse("2.0"),
                Version.Parse("3.0")
            };

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
                new LocalPackageResolver(repository1DirPath, "*.onv"),
                new LocalPackageResolver(repository2DirPath, "*.onv"));
            var versions = await resolver.GetPackageVersionsAsync();

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

            var repository1DirPath = Path.Combine(TempDirPath, "1");
            Directory.CreateDirectory(repository1DirPath);

            var repository2DirPath = Path.Combine(TempDirPath, "2");
            Directory.CreateDirectory(repository2DirPath);
            File.WriteAllText(Path.Combine(repository2DirPath, $"{version}.onv"), expectedContent);

            // Act
            var resolver = new AggregatePackageResolver(
                new LocalPackageResolver(repository1DirPath, "*.onv"),
                new LocalPackageResolver(repository2DirPath, "*.onv"));
            var destFilePath = Path.Combine(TempDirPath, "Output.onv");
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
            var entryPaths = new[]
            {
                "a.txt",
                "1/b.txt",
                "1/2/c.txt"
            };

            var packageFilePath = Path.Combine(TempDirPath, "Package.zip");
            using (var zip = ZipFile.Open(packageFilePath, ZipArchiveMode.Create))
            {
                foreach (var entryPath in entryPaths)
                    zip.CreateEntry(entryPath).WriteAllText(expectedContent);
            }

            // Act
            var extractor = new ZipPackageExtractor();
            var destDirPath = Path.Combine(TempDirPath, "Output");
            await extractor.ExtractPackageAsync(packageFilePath, destDirPath);

            // Assert
            foreach (var entryPath in entryPaths)
            {
                var destEntryPath = Path.Combine(destDirPath, entryPath);
                Assert.That(File.Exists(destEntryPath));
                Assert.That(File.ReadAllText(destEntryPath), Is.EqualTo(expectedContent));
            }
        }

        [Test]
        public async Task NugetPackageExtractor_ExtractPackageAsync_Test()
        {
            // Arrange
            var expectedContent = "Hello world";
            var rootDirPath = "Files";
            var relativeEntryPaths = new[]
            {
                "a.txt",
                "1/b.txt",
                "1/2/c.txt"
            };

            var packageFilePath = Path.Combine(TempDirPath, "Package.nupkg");
            using (var zip = ZipFile.Open(packageFilePath, ZipArchiveMode.Create))
            {
                foreach (var entryPath in relativeEntryPaths)
                    zip.CreateEntry($"{rootDirPath}/{entryPath}").WriteAllText(expectedContent);
            }

            // Act
            var extractor = new NugetPackageExtractor(rootDirPath);
            var destDirPath = Path.Combine(TempDirPath, "Output");
            await extractor.ExtractPackageAsync(packageFilePath, destDirPath);

            // Assert
            foreach (var entryPath in relativeEntryPaths)
            {
                var destEntryPath = Path.Combine(destDirPath, entryPath);
                Assert.That(File.Exists(destEntryPath));
                Assert.That(File.ReadAllText(destEntryPath), Is.EqualTo(expectedContent));
            }
        }
    }
}