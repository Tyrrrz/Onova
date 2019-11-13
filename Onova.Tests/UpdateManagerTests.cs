using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Onova.Models;
using Onova.Services;
using Onova.Tests.Internal;

namespace Onova.Tests
{
    [TestFixture]
    public class UpdateManagerTests
    {
        private const string UpdateeName = "Onova.Tests";

        private static string TempDirPath => Path.Combine(TestContext.CurrentContext.TestDirectory, "Temp");

        private static string StorageDirPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Onova", UpdateeName);

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

            // Delete storage directory
            if (Directory.Exists(StorageDirPath))
                Directory.Delete(StorageDirPath, true);
        }

        private static IEnumerable<TestCaseData> GetTestCases_CheckForUpdatesAsync()
        {
            yield return new TestCaseData(
                Version.Parse("1.0"),
                new[]
                {
                    Version.Parse("1.0"),
                    Version.Parse("2.0"),
                    Version.Parse("3.0")
                },
                Version.Parse("3.0"),
                true
            ).SetName("Higher version available");

            yield return new TestCaseData(
                Version.Parse("3.0"),
                new[]
                {
                    Version.Parse("1.0"),
                    Version.Parse("2.0"),
                    Version.Parse("3.0")
                },
                Version.Parse("3.0"),
                false
            ).SetName("Already highest version");

            yield return new TestCaseData(
                Version.Parse("1.0"),
                new Version[0],
                null,
                false
            ).SetName("No available versions");
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_CheckForUpdatesAsync))]
        public async Task CheckForUpdatesAsync_Test(Version updateeVersion, IReadOnlyList<Version> availableVersions,
            Version expectedLastVersion, bool expectedCanUpdate)
        {
            // Arrange
            var updatee = new AssemblyMetadata(UpdateeName, updateeVersion, "");

            var resolverMock = new Mock<IPackageResolver>();
            resolverMock.Setup(m => m.GetPackageVersionsAsync()).ReturnsAsync(availableVersions);

            var extractorMock = new Mock<IPackageExtractor>();

            using var manager = new UpdateManager(updatee, resolverMock.Object, extractorMock.Object);

            // Act
            var result = await manager.CheckForUpdatesAsync();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Versions, Is.EquivalentTo(availableVersions), "Available versions");
                Assert.That(result.LastVersion, Is.EqualTo(expectedLastVersion), "Last version");
                Assert.That(result.CanUpdate, Is.EqualTo(expectedCanUpdate), "Can update");
            });
        }

        [Test]
        public async Task GetPreparedUpdates_Test()
        {
            // Arrange
            var versions = new[]
            {
                Version.Parse("1.0"),
                Version.Parse("2.0"),
                Version.Parse("3.0")
            };

            foreach (var version in versions)
            {
                var packageFilePath = Path.Combine(TempDirPath, $"{version}.onv");

                using var zip = ZipFile.Open(packageFilePath, ZipArchiveMode.Create);
                zip.CreateEntry("File.bin").WriteAllText("Hello world");
            }

            var updateeVersion = versions.Min();
            var updatee = new AssemblyMetadata(UpdateeName, updateeVersion, "");
            var resolver = new LocalPackageResolver(TempDirPath, "*.onv");
            var extractor = new ZipPackageExtractor();

            using var manager = new UpdateManager(updatee, resolver, extractor);

            foreach (var version in versions)
                await manager.PrepareUpdateAsync(version);

            // Act
            var preparedUpdates = manager.GetPreparedUpdates();

            // Assert
            Assert.That(preparedUpdates, Is.EquivalentTo(versions));
        }

        [Test]
        public async Task IsUpdatePrepared_Test()
        {
            // Arrange
            var versions = new[]
            {
                Version.Parse("1.0"),
                Version.Parse("2.0"),
                Version.Parse("3.0")
            };

            foreach (var version in versions)
            {
                var packageFilePath = Path.Combine(TempDirPath, $"{version}.onv");

                using var zip = ZipFile.Open(packageFilePath, ZipArchiveMode.Create);
                zip.CreateEntry("File.bin").WriteAllText("Hello world");
            }

            var updateeVersion = versions.Min();
            var updatee = new AssemblyMetadata(UpdateeName, updateeVersion, "");
            var resolver = new LocalPackageResolver(TempDirPath, "*.onv");
            var extractor = new ZipPackageExtractor();

            using var manager = new UpdateManager(updatee, resolver, extractor);

            foreach (var version in versions)
                await manager.PrepareUpdateAsync(version);

            // Act & Assert
            foreach (var version in versions)
                Assert.That(manager.IsUpdatePrepared(version));
        }
    }
}