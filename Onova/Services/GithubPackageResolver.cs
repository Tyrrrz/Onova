using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Onova.Exceptions;
using Onova.Internal;
using Onova.Internal.Extensions;

namespace Onova.Services;

/// <summary>
/// Resolves packages from release assets of a GitHub repository.
/// Release names should contain package versions (e.g. "v1.8.3").
/// </summary>
public class GithubPackageResolver : IPackageResolver
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseAddress;
    private readonly string _repositoryOwner;
    private readonly string _repositoryName;
    private readonly string _assetNamePattern;

    private EntityTagHeaderValue? _cachedPackageVersionUrlMapETag;
    private IReadOnlyDictionary<Version, string>? _cachedPackageVersionUrlMap;

    /// <summary>
    /// Initializes an instance of <see cref="GithubPackageResolver"/>.
    /// </summary>
    public GithubPackageResolver(
        HttpClient httpClient,
        string apiBaseAddress,
        string repositoryOwner,
        string repositoryName,
        string assetNamePattern)
    {
        _httpClient = httpClient;
        _apiBaseAddress = apiBaseAddress;
        _repositoryOwner = repositoryOwner;
        _repositoryName = repositoryName;
        _assetNamePattern = assetNamePattern;
    }

    /// <summary>
    /// Initializes an instance of <see cref="GithubPackageResolver"/>.
    /// </summary>
    public GithubPackageResolver(
        HttpClient httpClient,
        string repositoryOwner,
        string repositoryName,
        string assetNamePattern)
        : this(httpClient, "https://api.github.com", repositoryOwner, repositoryName, assetNamePattern)
    {
    }

    /// <summary>
    /// Initializes an instance of <see cref="GithubPackageResolver"/>.
    /// </summary>
    public GithubPackageResolver(
        string apiBaseAddress,
        string repositoryOwner,
        string repositoryName,
        string assetNamePattern)
        : this(Http.Client, apiBaseAddress, repositoryOwner, repositoryName, assetNamePattern)
    {
    }

    /// <summary>
    /// Initializes an instance of <see cref="GithubPackageResolver"/>.
    /// </summary>
    public GithubPackageResolver(
        string repositoryOwner,
        string repositoryName,
        string assetNamePattern)
        : this(Http.Client, repositoryOwner, repositoryName, assetNamePattern)
    {
    }

    private IReadOnlyDictionary<Version, string> ParsePackageVersionUrlMap(JsonElement releasesJson)
    {
        var map = new Dictionary<Version, string>();

        foreach (var releaseJson in releasesJson.EnumerateArray())
        {
            // Get release name
            var releaseTitle = releaseJson.GetProperty("name").GetString();

            // In case property name is null, empty or whitespace then in web version of GitHub it is replaced by a property "tag_name"
            if (string.IsNullOrWhiteSpace(releaseTitle))
            {
                releaseTitle = releaseJson.GetProperty("tag_name").GetString();
            }

            // Try to parse version
            var versionText = Regex.Match(releaseTitle, "(\\d+\\.\\d+(?:\\.\\d+)?(?:\\.\\d+)?)").Groups[1].Value;
            if (!Version.TryParse(versionText, out var version))
                continue;

            // Skip pre-releases
            var isPreRelease = releaseJson.GetProperty("prerelease").GetBoolean();
            if (isPreRelease)
                continue;

            // Find asset
            var assetsJson = releaseJson.GetProperty("assets");
            foreach (var assetJson in assetsJson.EnumerateArray())
            {
                var assetName = assetJson.GetProperty("name").GetString();
                var assetUrl = assetJson.GetProperty("url").GetString();

                // See if name matches
                if (!WildcardPattern.IsMatch(assetName, _assetNamePattern))
                    continue;

                // Add to dictionary
                map[version] = assetUrl;
            }
        }

        return map;
    }

    private async Task<IReadOnlyDictionary<Version, string>> GetPackageVersionUrlMapAsync(CancellationToken cancellationToken)
    {
        // Get releases
        var url = $"{_apiBaseAddress}/repos/{_repositoryOwner}/{_repositoryName}/releases";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Set If-None-Match header if ETag is available
        if (_cachedPackageVersionUrlMapETag != null && _cachedPackageVersionUrlMap != null)
            request.Headers.IfNoneMatch.Add(_cachedPackageVersionUrlMapETag);

        // Get response
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        // If not modified - return cached
        if (response.StatusCode == HttpStatusCode.NotModified)
            return _cachedPackageVersionUrlMap!;

        // Ensure success status code otherwise
        response.EnsureSuccessStatusCode();

        // Parse response
        var responseJson = await response.Content.ReadAsJsonAsync(cancellationToken);
        var map = ParsePackageVersionUrlMap(responseJson);

        // Cache result
        _cachedPackageVersionUrlMapETag = response.Headers.ETag;
        _cachedPackageVersionUrlMap = map;

        // Return result
        return map;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Version>> GetPackageVersionsAsync(CancellationToken cancellationToken = default)
    {
        var versions = await GetPackageVersionUrlMapAsync(cancellationToken);
        return versions.Keys.ToArray();
    }

    /// <inheritdoc />
    public async Task DownloadPackageAsync(Version version, string destFilePath,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        // Get map
        var map = await GetPackageVersionUrlMapAsync(cancellationToken);

        // Try to get package URL
        var packageUrl = map.GetValueOrDefault(version);
        if (string.IsNullOrWhiteSpace(packageUrl))
            throw new PackageNotFoundException(version);

        // Download
        using var request = new HttpRequestMessage(HttpMethod.Get, packageUrl);
        request.Headers.Add("Accept", "application/octet-stream"); // required

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var output = File.Create(destFilePath);
        await response.Content.CopyToStreamAsync(output, progress, cancellationToken);
    }
}