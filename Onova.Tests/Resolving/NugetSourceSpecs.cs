using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using FluentAssertions;
using Onova.Services;
using Onova.Tests.Internal;
using Xunit;

namespace Onova.Tests.Resolving
{
    public class NugetSourceSpecs : IDisposable
    {
        private string TempDirPath { get; } = Path.Combine(Directory.GetCurrentDirectory(), $"{nameof(NugetSourceSpecs)}_{Guid.NewGuid()}");

        public NugetSourceSpecs() => DirectoryEx.Reset(TempDirPath);

        public void Dispose() => DirectoryEx.DeleteIfExists(TempDirPath);

        // https://myget.org/feed/tyrrrz-test/package/nuget/OnovaTest
        private static NugetPackageResolver CreateNugetPackageResolver() =>
            new NugetPackageResolver("https://myget.org/F/tyrrrz-test/api/v3/index.json", "OnovaTest");

        [Fact]
        public async Task I_can_use_a_NuGet_repository_as_a_package_source()
        {
            // Arrange
            var resolver = CreateNugetPackageResolver();

            var version = Version.Parse("2.0.0");
            var destFilePath = Path.Combine(TempDirPath, "Output.onv");

            // Act
            await resolver.DownloadPackageAsync(version, destFilePath);

            using var zip = ZipFile.OpenRead(destFilePath);
            var content = zip.GetEntry("Files/Content.txt").ReadAllText();

            // Assert
            content.Should().Be("Hello world");
        }

        [Fact]
        public async Task When_using_a_NuGet_repository_as_a_package_source_packages_are_mapped_directly_from_NuGet_packages()
        {
            // Arrange
            var resolver = CreateNugetPackageResolver();

            // Act
            var versions = await resolver.GetPackageVersionsAsync();

            // Assert
            versions.Should().BeEquivalentTo(
                Version.Parse("1.0.0"),
                Version.Parse("2.0.0"),
                Version.Parse("3.0.0"));
        }
    }
}