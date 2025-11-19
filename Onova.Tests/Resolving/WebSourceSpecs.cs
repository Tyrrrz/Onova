using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Onova.Services;
using Onova.Tests.Utils.Extensions;
using Xunit;

namespace Onova.Tests.Resolving;

public class WebSourceSpecs : IDisposable
{
    private string TempDirPath { get; } =
        Path.Combine(Directory.GetCurrentDirectory(), $"{nameof(WebSourceSpecs)}_{Guid.NewGuid()}");

    public WebSourceSpecs() => Directory.Reset(TempDirPath);

    public void Dispose() => Directory.DeleteIfExists(TempDirPath);

    private WebPackageResolver CreateWebPackageResolver() =>
        new(
            "https://raw.githubusercontent.com/Tyrrrz/OnovaTestRepo/master/TestWebPackageManifest.txt"
        );

    [Fact]
    public async Task I_can_use_a_custom_web_server_as_a_package_source()
    {
        // Arrange
        var resolver = CreateWebPackageResolver();

        var destFilePath = Path.Combine(TempDirPath, "Output.onv");

        // Act
        await resolver.DownloadPackageAsync(Version.Parse("2.0"), destFilePath);

        // Assert
        var content = await File.ReadAllTextAsync(destFilePath);
        content.Should().Be("Hello world");
    }

    [Fact]
    public async Task When_using_a_custom_web_server_as_a_package_source_packages_are_extracted_from_a_manifest()
    {
        // Arrange
        var resolver = CreateWebPackageResolver();

        // Act
        var versions = await resolver.GetPackageVersionsAsync();

        // Assert
        versions
            .Should()
            .BeEquivalentTo(
                new[] { Version.Parse("1.0"), Version.Parse("2.0"), Version.Parse("3.0") }
            );
    }
}
