using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Onova.Models;
using Onova.Services;

namespace Onova.Tests
{
    [TestFixture]
    public class UnitTests
    {
        private const string LocalUpdateeName = "Onova.Tests.UnitTests";

        private static string StorageDirPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Onova", LocalUpdateeName);

        [TearDown]
        public void Cleanup()
        {
            // Delete storage
            if (Directory.Exists(StorageDirPath))
                Directory.Delete(StorageDirPath, true);
        }

        [Test]
        public async Task UpdateManager_CheckForUpdatesAsync_HigherVersionAvailable_Test()
        {
            // Arrange

            // Resolver mock
            var resolverMock = new Mock<IPackageResolver>();
            var availableVersions = new[]
            {
                Version.Parse("1.0"),
                Version.Parse("2.0"),
                Version.Parse("3.0")
            };
            resolverMock.Setup(m => m.GetPackageVersionsAsync()).ReturnsAsync(availableVersions);

            // Extractor mock
            var extractorMock = new Mock<IPackageExtractor>();

            // Updatee mock
            var updateeVersion = availableVersions.Min();
            var updatee = new AssemblyMetadata(LocalUpdateeName, updateeVersion, "");

            // Update manager
            using (var manager = new UpdateManager(updatee, resolverMock.Object, extractorMock.Object))
            {
                // Act
                var result = await manager.CheckForUpdatesAsync();

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Versions, Is.EquivalentTo(availableVersions));
                Assert.That(result.LastVersion, Is.EqualTo(availableVersions.Max()));
                Assert.That(result.CanUpdate);
            }
        }

        [Test]
        public async Task UpdateManager_CheckForUpdatesAsync_AlreadyHighestVersion_Test()
        {
            // Arrange

            // Resolver mock
            var resolverMock = new Mock<IPackageResolver>();
            var availableVersions = new[]
            {
                Version.Parse("1.0"),
                Version.Parse("2.0"),
                Version.Parse("3.0")
            };
            resolverMock.Setup(m => m.GetPackageVersionsAsync()).ReturnsAsync(availableVersions);

            // Extractor mock
            var extractorMock = new Mock<IPackageExtractor>();

            // Updatee mock
            var updateeVersion = availableVersions.Max();
            var updatee = new AssemblyMetadata(LocalUpdateeName, updateeVersion, "");

            // Update manager
            using (var manager = new UpdateManager(updatee, resolverMock.Object, extractorMock.Object))
            {
                // Act
                var result = await manager.CheckForUpdatesAsync();

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Versions, Is.EquivalentTo(availableVersions));
                Assert.That(result.LastVersion, Is.EqualTo(availableVersions.Max()));
                Assert.That(result.CanUpdate, Is.Not.True);
            }
        }

        [Test]
        public async Task UpdateManager_CheckForUpdatesAsync_NoAvailableVersions_Test()
        {
            // Arrange

            // Resolver mock
            var resolverMock = new Mock<IPackageResolver>();
            var availableVersions = Array.Empty<Version>();
            resolverMock.Setup(m => m.GetPackageVersionsAsync()).ReturnsAsync(availableVersions);

            // Extractor mock
            var extractorMock = new Mock<IPackageExtractor>();

            // Updatee mock
            var updateeVersion = Version.Parse("1.0");
            var updatee = new AssemblyMetadata(LocalUpdateeName, updateeVersion, "");

            // Update manager
            using (var manager = new UpdateManager(updatee, resolverMock.Object, extractorMock.Object))
            {
                // Act
                var result = await manager.CheckForUpdatesAsync();

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Versions, Is.EquivalentTo(availableVersions));
                Assert.That(result.LastVersion, Is.Null);
                Assert.That(result.CanUpdate, Is.Not.True);
            }
        }
    }
}