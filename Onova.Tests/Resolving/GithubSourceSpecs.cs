using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Onova.Services;
using Onova.Tests.Utils;
using Xunit;

namespace Onova.Tests.Resolving;

public class GithubSourceSpecs : IDisposable
{
    private string TempDirPath { get; } = Path.Combine(
        Directory.GetCurrentDirectory(),
        $"{nameof(GithubSourceSpecs)}_{Guid.NewGuid()}"
    );

    public GithubSourceSpecs() => DirectoryEx.Reset(TempDirPath);

    public void Dispose() => DirectoryEx.DeleteIfExists(TempDirPath);

    private GithubPackageResolver CreateGithubPackageResolver()
    {
        var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("User-Agent", "Onova Tests (github.com/Tyrrrz/Onova)");

        // Prefer authenticated requests to avoid rate limiting
        var accessToken = Environment.GetEnvironmentVariable("TEST_GITHUB_TOKEN");
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(accessToken);
        }

        return new GithubPackageResolver(httpClient, "Tyrrrz", "OnovaTestRepo", "*.onv");
    }

    [Fact]
    public async Task I_can_use_a_GitHub_repository_as_a_package_source()
    {
        // Arrange
        var resolver = CreateGithubPackageResolver();

        var version = Version.Parse("2.0");
        var destFilePath = Path.Combine(TempDirPath, "Output.onv");

        // Act
        await resolver.DownloadPackageAsync(version, destFilePath);

        // Assert
        var content = await File.ReadAllTextAsync(destFilePath);
        content.Should().Be("Hello world");
    }

    [Fact]
    public async Task When_using_a_GitHub_repository_as_a_package_source_packages_are_mapped_from_releases()
    {
        // Arrange
        var resolver = CreateGithubPackageResolver();

        // Act
        var versions = await resolver.GetPackageVersionsAsync();

        // Assert
        versions.Should().BeEquivalentTo(new[]
        {
            Version.Parse("1.0"),
            Version.Parse("2.0"),
            Version.Parse("3.0")
        });
    }
}