using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Onova.Services;
using Onova.Tests.Utils;
using Xunit;

namespace Onova.Tests.Resolving;

public class LocalSourceSpecs : IDisposable
{
    private string TempDirPath { get; } =
        Path.Combine(
            Directory.GetCurrentDirectory(),
            $"{nameof(LocalSourceSpecs)}_{Guid.NewGuid()}"
        );

    public LocalSourceSpecs() => DirectoryEx.Reset(TempDirPath);

    public void Dispose() => DirectoryEx.DeleteIfExists(TempDirPath);

    private LocalPackageResolver CreateLocalPackageResolver(
        IReadOnlyDictionary<Version, byte[]> packages
    )
    {
        foreach (var (version, data) in packages)
        {
            var packageFilePath = Path.Combine(TempDirPath, $"{version}.onv");
            File.WriteAllBytes(packageFilePath, data);
        }

        return new LocalPackageResolver(TempDirPath, "*.onv");
    }

    private LocalPackageResolver CreateLocalPackageResolver(IReadOnlyList<Version> versions) =>
        CreateLocalPackageResolver(
            versions.ToDictionary(v => v, _ => new byte[] { 1, 2, 3, 4, 5 })
        );

    [Fact]
    public async Task I_can_use_a_local_directory_as_a_package_source()
    {
        // Arrange
        var availablePackages = new Dictionary<Version, byte[]>
        {
            [Version.Parse("1.0")] = [1, 2, 3],
            [Version.Parse("2.0")] = [4, 5, 6],
            [Version.Parse("3.0")] = [7, 8, 9],
        };

        var resolver = CreateLocalPackageResolver(availablePackages);

        var (version, expectedData) = availablePackages.Last();
        var destFilePath = Path.Combine(TempDirPath, "Output.onv");

        // Act
        await resolver.DownloadPackageAsync(version, destFilePath);

        // Assert
        var data = await File.ReadAllBytesAsync(destFilePath);
        data.Should().BeEquivalentTo(expectedData);
    }

    [Fact]
    public async Task When_using_a_local_directory_as_a_package_source_packages_are_mapped_from_files()
    {
        // Arrange
        var availableVersions = new[]
        {
            Version.Parse("1.0"),
            Version.Parse("2.0"),
            Version.Parse("3.0"),
        };

        var resolver = CreateLocalPackageResolver(availableVersions);

        // Act
        var versions = await resolver.GetPackageVersionsAsync();

        // Assert
        versions.Should().BeEquivalentTo(availableVersions);
    }
}
