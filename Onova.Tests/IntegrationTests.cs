using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Onova.Services;
using Onova.Tests.Internal;

namespace Onova.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private static string TestDirPath => TestContext.CurrentContext.TestDirectory;
        private static string TempDirPath => Path.Combine(TestDirPath, "Temp");

        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory(TempDirPath);
        }

        [TearDown]
        public void Cleanup()
        {
            if (Directory.Exists(TempDirPath))
                Directory.Delete(TempDirPath, true);
        }

        [Test]
        public async Task LocalPackageResolver_GetVersionsAsync_Test()
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
            var versions = await resolver.GetVersionsAsync();

            // Assert
            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task LocalPackageResolver_DownloadAsync_Test()
        {
            // Arrange
            var version = Version.Parse("2.0");
            var expectedContent = "Hello world";
            File.WriteAllText(Path.Combine(TempDirPath, $"{version}.onv"), expectedContent);

            // Act
            var resolver = new LocalPackageResolver(TempDirPath, "*.onv");
            var destFilePath = Path.Combine(TempDirPath, "Output.onv");
            await resolver.DownloadAsync(version, destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath));
            Assert.That(File.ReadAllText(destFilePath), Is.EqualTo(expectedContent));
        }

        [Test]
        public async Task GithubPackageResolver_GetVersionsAsync_Test()
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
            var versions = await resolver.GetVersionsAsync();

            // Assert
            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task GithubPackageResolver_DownloadAsync_Test()
        {
            // Arrange
            var version = Version.Parse("2.0");
            var expectedContent = "Hello world";

            // Act
            var resolver = new GithubPackageResolver("Tyrrrz", "OnovaTestRepo", "*.onv");
            var destFilePath = Path.Combine(TempDirPath, "Output.onv");
            await resolver.DownloadAsync(version, destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath));
            Assert.That(File.ReadAllText(destFilePath), Is.EqualTo(expectedContent));
        }

        [Test]
        public async Task WebPackageResolver_GetVersionsAsync_Test()
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
            var versions = await resolver.GetVersionsAsync();

            // Assert
            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task WebPackageResolver_DownloadAsync_Test()
        {
            // Arrange
            var version = Version.Parse("2.0");
            var expectedContent = "Hello world";

            // Act
            var url = "https://raw.githubusercontent.com/Tyrrrz/OnovaTestRepo/master/TestWebPackageManifest.txt";
            var resolver = new WebPackageResolver(url);
            var destFilePath = Path.Combine(TempDirPath, "Output.onv");
            await resolver.DownloadAsync(version, destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath));
            Assert.That(File.ReadAllText(destFilePath), Is.EqualTo(expectedContent));
        }

        [Test]
        public async Task NugetPackageResolver_GetVersionsAsync_Test()
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
            var versions = await resolver.GetVersionsAsync();

            // Assert
            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task NugetPackageResolver_DownloadAsync_Test()
        {
            // Arrange
            var version = Version.Parse("2.0.0");
            var expectedContent = "Hello world";

            // Act
            var url = "https://www.myget.org/F/tyrrrz-test/api/v3/index.json";
            var resolver = new NugetPackageResolver(url, "OnovaTest");
            var destFilePath = Path.Combine(TempDirPath, "Output.onv");
            await resolver.DownloadAsync(version, destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath));

            using (var input = File.OpenRead(destFilePath))
            using (var zip = new ZipArchive(input, ZipArchiveMode.Read))
            {
                var content = zip.GetEntry("Files/Content.txt").ReadAllText();
                Assert.That(content, Is.EqualTo(expectedContent));
            }
        }

        [Test]
        public async Task AggregatePackageResolver_GetVersionsAsync_Test()
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
            var versions = await resolver.GetVersionsAsync();

            // Assert
            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task AggregatePackageResolver_DownloadAsync_Test()
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
            await resolver.DownloadAsync(version, destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath));
            Assert.That(File.ReadAllText(destFilePath), Is.EqualTo(expectedContent));
        }

        [Test]
        public async Task ZipPackageExtractor_ExtractPackageAsync_Test()
        {
            // Arrange
            var expectedContent = "Hello world";
            var expectedEntryPaths = new[]
            {
                "a.txt",
                "1/b.txt",
                "1/2/c.txt"
            };

            var packageFilePath = Path.Combine(TempDirPath, "Package.zip");
            using (var output = File.Create(packageFilePath))
            using (var zip = new ZipArchive(output, ZipArchiveMode.Create))
            {
                foreach (var expectedEntryPath in expectedEntryPaths)
                    zip.CreateEntry(expectedEntryPath).WriteAllText(expectedContent);
            }

            // Act
            var extractor = new ZipPackageExtractor();
            var destDirPath = Path.Combine(TempDirPath, "Output");
            await extractor.ExtractAsync(packageFilePath, destDirPath);

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