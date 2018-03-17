using System;
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
            resolverMock.Setup(m => m.GetVersionsAsync()).ReturnsAsync(availableVersions);

            // Extractor mock
            var extractorMock = new Mock<IPackageExtractor>();

            // Updatee mock
            var version = availableVersions.Min();
            var updatee = new AssemblyMetadata("", version, "", "");

            // Update manager
            var updateManager = new UpdateManager(updatee, resolverMock.Object, extractorMock.Object);

            // Act
            var result = await updateManager.CheckForUpdatesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Versions, Is.EquivalentTo(availableVersions));
            Assert.That(result.LastVersion, Is.EqualTo(availableVersions.Max()));
            Assert.That(result.CanUpdate);
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
            resolverMock.Setup(m => m.GetVersionsAsync()).ReturnsAsync(availableVersions);

            // Extractor mock
            var extractorMock = new Mock<IPackageExtractor>();

            // Updatee mock
            var version = availableVersions.Max();
            var updatee = new AssemblyMetadata("", version, "", "");

            // Update manager
            var updateManager = new UpdateManager(updatee, resolverMock.Object, extractorMock.Object);

            // Act
            var result = await updateManager.CheckForUpdatesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Versions, Is.EquivalentTo(availableVersions));
            Assert.That(result.LastVersion, Is.EqualTo(availableVersions.Max()));
            Assert.That(result.CanUpdate, Is.Not.True);
        }

        [Test]
        public async Task UpdateManager_CheckForUpdatesAsync_NoAvailableVersions_Test()
        {
            // Arrange

            // Resolver mock
            var resolverMock = new Mock<IPackageResolver>();
            var availableVersions = Array.Empty<Version>();
            resolverMock.Setup(m => m.GetVersionsAsync()).ReturnsAsync(availableVersions);

            // Extractor mock
            var extractorMock = new Mock<IPackageExtractor>();

            // Updatee mock
            var version = Version.Parse("1.0");
            var updatee = new AssemblyMetadata("", version, "", "");

            // Update manager
            var updateManager = new UpdateManager(updatee, resolverMock.Object, extractorMock.Object);

            // Act
            var result = await updateManager.CheckForUpdatesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Versions, Is.EquivalentTo(availableVersions));
            Assert.That(result.LastVersion, Is.Null);
            Assert.That(result.CanUpdate, Is.Not.True);
        }
    }
}