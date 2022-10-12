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

public class AggregateSourceSpecs : IDisposable
{
    private string TempDirPath { get; } = Path.Combine(
        Directory.GetCurrentDirectory(), $"{nameof(AggregateSourceSpecs)}_{Guid.NewGuid()}"
    );

    public AggregateSourceSpecs() => DirectoryEx.Reset(TempDirPath);

    public void Dispose() => DirectoryEx.DeleteIfExists(TempDirPath);

    private LocalPackageResolver CreateLocalPackageResolver(IReadOnlyDictionary<Version, byte[]> packages)
    {
        foreach (var (version, data) in packages)
        {
            var packageFilePath = Path.Combine(TempDirPath, $"{version}.onv");
            File.WriteAllBytes(packageFilePath, data);
        }

        return new LocalPackageResolver(TempDirPath, "*.onv");
    }

    private AggregatePackageResolver CreateAggregatePackageResolver(IReadOnlyDictionary<Version, byte[]> packages)
    {
        var packages1 = packages.Take(packages.Count / 2).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var packages2 = packages.Skip(packages.Count / 2).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var repository1DirPath = Path.Combine(TempDirPath, "1");
        var repository2DirPath = Path.Combine(TempDirPath, "2");

        Directory.CreateDirectory(repository1DirPath);
        Directory.CreateDirectory(repository2DirPath);

        var resolver1 = CreateLocalPackageResolver(packages1);
        var resolver2 = CreateLocalPackageResolver(packages2);

        return new AggregatePackageResolver(resolver1, resolver2);
    }

    private AggregatePackageResolver CreateAggregatePackageResolver(IReadOnlyList<Version> versions) =>
        CreateAggregatePackageResolver(versions.ToDictionary(v => v, _ => new byte[] {1, 2, 3, 4, 5}));

    [Fact]
    public async Task I_can_use_multiple_different_package_sources_at_once()
    {
        // Arrange
        var availablePackages = new Dictionary<Version, byte[]>
        {
            [Version.Parse("1.0")] = new byte[] {1, 2, 3},
            [Version.Parse("2.0")] = new byte[] {4, 5, 6},
            [Version.Parse("3.0")] = new byte[] {7, 8, 9}
        };

        var resolver = CreateAggregatePackageResolver(availablePackages);

        var (version, expectedData) = availablePackages.Last();
        var destFilePath = Path.Combine(TempDirPath, "Output.onv");

        // Act
        await resolver.DownloadPackageAsync(version, destFilePath);

        // Assert
        var data = await File.ReadAllBytesAsync(destFilePath);
        data.Should().BeEquivalentTo(expectedData);
    }

    [Fact]
    public async Task When_using_multiple_different_package_sources_at_once_packages_are_aggregated_from_all_sources()
    {
        // Arrange
        var availableVersions = new[]
        {
            Version.Parse("1.0"),
            Version.Parse("2.0"),
            Version.Parse("3.0")
        };

        var resolver = CreateAggregatePackageResolver(availableVersions);

        // Act
        var versions = await resolver.GetPackageVersionsAsync();

        // Assert
        versions.Should().BeEquivalentTo(availableVersions);
    }
}