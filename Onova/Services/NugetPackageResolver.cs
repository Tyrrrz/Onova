using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Onova.Exceptions;
using Onova.Internal;
using Onova.Internal.Extensions;

namespace Onova.Services
{
    /// <summary>
    /// Resolves packages from a NuGet feed.
    /// </summary>
    public class NugetPackageResolver : IPackageResolver
    {
        private readonly HttpClient _httpClient;
        private readonly string _serviceIndexUrl;
        private readonly string _packageId;

        private string PackageIdNormalized => _packageId.ToLowerInvariant();

        /// <summary>
        /// Initializes an instance of <see cref="NugetPackageResolver"/>.
        /// </summary>
        public NugetPackageResolver(HttpClient httpClient, string serviceIndexUrl, string packageId)
        {
            _httpClient = httpClient;
            _serviceIndexUrl = serviceIndexUrl;
            _packageId = packageId;
        }

        /// <summary>
        /// Initializes an instance of <see cref="NugetPackageResolver"/>.
        /// </summary>
        public NugetPackageResolver(string serviceIndexUrl, string packageId)
            : this(Singleton.HttpClient, serviceIndexUrl, packageId)
        {
        }

        private async Task<string> GetPackageBaseAddressResourceUrlAsync(CancellationToken cancellationToken)
        {
            // Get all available resources
            var responseJson = await _httpClient.GetJsonAsync(_serviceIndexUrl, cancellationToken);
            var resourcesJson = responseJson.GetProperty("resources");

            // Get URL of the required resource
            var expectedResourceType = "PackageBaseAddress/3.0.0";
            foreach (var resourceJson in resourcesJson.EnumerateArray())
            {
                // Check resource type
                var resourceType = resourceJson.GetProperty("@type").GetString();
                if (resourceType == expectedResourceType)
                    return resourceJson.GetProperty("@id").GetString();
            }

            // Resource not found
            throw new InvalidOperationException($"[{expectedResourceType}] resource not found in service index.");
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Version>> GetPackageVersionsAsync(CancellationToken cancellationToken = default)
        {
            // Get package base address resource URL
            var resourceUrl = await GetPackageBaseAddressResourceUrlAsync(cancellationToken);

            // Get versions
            var request = $"{resourceUrl}/{PackageIdNormalized}/index.json";
            var responseJson = await _httpClient.GetJsonAsync(request, cancellationToken);
            var versionsJson = responseJson.GetProperty("versions");
            var versions = new HashSet<Version>();

            foreach (var versionJson in versionsJson.EnumerateArray())
            {
                // Try to parse version
                var versionText = versionJson.GetString();
                if (!Version.TryParse(versionText, out var version))
                    continue;

                // Add to list
                versions.Add(version);
            }

            return versions.ToArray();
        }

        /// <inheritdoc />
        public async Task DownloadPackageAsync(Version version, string destFilePath,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            // Get package base address resource URL
            var resourceUrl = await GetPackageBaseAddressResourceUrlAsync(cancellationToken);

            // Get package URL
            var packageUrl = $"{resourceUrl}/{PackageIdNormalized}/{version}/{PackageIdNormalized}.{version}.nupkg";

            // Get response
            using var response = await _httpClient.GetAsync(packageUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            // If status code is 404 then this version doesn't exist
            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new PackageNotFoundException(version);

            // Ensure success status code otherwise
            response.EnsureSuccessStatusCode();

            // Copy content to file
            using var output = File.Create(destFilePath);
            await response.Content.CopyToStreamAsync(output, progress, cancellationToken);
        }
    }
}