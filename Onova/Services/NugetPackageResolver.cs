using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Onova.Exceptions;
using Onova.Internal;

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
            _httpClient = httpClient.GuardNotNull(nameof(httpClient));
            _serviceIndexUrl = serviceIndexUrl.GuardNotNull(nameof(serviceIndexUrl));
            _packageId = packageId.GuardNotNull(nameof(packageId));
        }

        /// <summary>
        /// Initializes an instance of <see cref="NugetPackageResolver"/>.
        /// </summary>
        public NugetPackageResolver(string serviceIndexUrl, string packageId)
            : this(HttpClientEx.GetSingleton(), serviceIndexUrl, packageId)
        {
        }

        private async Task<string> GetPackageBaseAddressResourceUrlAsync()
        {
            // Get all available resources
            var response = await _httpClient.GetStringAsync(_serviceIndexUrl);
            var resourcesJson = JToken.Parse(response)["resources"];

            // Get URL of the required resource
            var expectedResourceType = "PackageBaseAddress/3.0.0";
            foreach (var resourceJson in resourcesJson)
            {
                // Check resource type
                var resourceType = resourceJson["@type"].Value<string>();
                if (resourceType == expectedResourceType)
                    return resourceJson["@id"].Value<string>();
            }

            // Resource not found
            throw new InvalidOperationException($"[{expectedResourceType}] resource not found in service index.");
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Version>> GetPackageVersionsAsync()
        {
            // Get package base address resource URL
            var resourceUrl = await GetPackageBaseAddressResourceUrlAsync();

            // Get versions
            var request = $"{resourceUrl}/{PackageIdNormalized}/index.json";
            var response = await _httpClient.GetStringAsync(request);
            var versionsJson = JToken.Parse(response)["versions"];
            var versions = new HashSet<Version>();

            foreach (var versionJson in versionsJson)
            {
                // Try to parse version
                var versionText = versionJson.Value<string>();
                if (!Version.TryParse(versionText, out var version))
                    continue;

                // Add to list
                versions.Add(version);
            }

            return versions.ToArray();
        }

        /// <inheritdoc />
        public async Task DownloadPackageAsync(Version version, string destFilePath,
            IProgress<double> progress = null, CancellationToken cancellationToken = default)
        {
            version.GuardNotNull(nameof(version));
            destFilePath.GuardNotNull(nameof(destFilePath));

            // Get package base address resource URL
            var resourceUrl = await GetPackageBaseAddressResourceUrlAsync();

            // Get package URL
            var packageUrl = $"{resourceUrl}/{PackageIdNormalized}/{version}/{PackageIdNormalized}.{version}.nupkg";

            // Download
            using (var response = await _httpClient
                .GetAsync(packageUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                // If status code is 404 then this version doesn't exist
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new PackageNotFoundException(version);

                // Ensure success status code otherwise
                response.EnsureSuccessStatusCode();

                // Copy content to file
                using (var input = await response.Content.ReadAsFiniteStreamAsync())
                using (var output = File.Create(destFilePath))
                    await input.CopyToAsync(output, progress, cancellationToken);
            }
        }
    }
}