using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Onova.Exceptions;
using Onova.Internal;

namespace Onova.Services
{
    /// <summary>
    /// Resolves packages from release assets of a GitHub repository.
    /// </summary>
    public class GithubPackageResolver : IPackageResolver
    {
        private readonly IHttpService _httpService;
        private readonly string _repositoryOwner;
        private readonly string _repositoryName;
        private readonly string _assetName;

        /// <summary>
        /// Initializes an instance of <see cref="GithubPackageResolver"/> with a custom HTTP service.
        /// </summary>
        public GithubPackageResolver(IHttpService httpService, string repositoryOwner, string repositoryName,
            string assetName)
        {
            _httpService = httpService.GuardNotNull(nameof(httpService));
            _repositoryOwner = repositoryOwner.GuardNotNull(nameof(repositoryOwner));
            _repositoryName = repositoryName.GuardNotNull(nameof(repositoryName));
            _assetName = assetName.GuardNotNull(nameof(assetName));
        }

        /// <summary>
        /// Initializes an instance of <see cref="GithubPackageResolver"/>.
        /// </summary>
        public GithubPackageResolver(string repositoryOwner, string repositoryName, string assetName)
            : this(HttpService.Instance, repositoryOwner, repositoryName, assetName)
        {
        }

        private async Task<IReadOnlyDictionary<Version, string>> GetPackageAssetUrlMapAsync()
        {
            var map = new Dictionary<Version, string>();

            // Get releases
            var request = $"https://api.github.com/repos/{_repositoryOwner}/{_repositoryName}/releases";
            var response = await _httpService.GetStringAsync(request).ConfigureAwait(false);
            var releasesJson = JToken.Parse(response);

            foreach (var releaseJson in releasesJson)
            {
                var name = releaseJson["name"].Value<string>();

                // Try to parse version from name
                if (!Version.TryParse(name, out var version))
                    continue;
                
                // Find asset
                var assetsJson = releaseJson["assets"];
                foreach (var assetJson in assetsJson)
                {
                    var assetName = assetJson["name"].Value<string>();
                    var assetUrl = assetJson["browser_download_url"].Value<string>();

                    // See if name matches
                    if (!string.Equals(assetName, _assetName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Add to dictionary
                    map[version] = assetUrl;
                }
            }

            return map;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Version>> GetAllVersionsAsync()
        {
            var map = await GetPackageAssetUrlMapAsync().ConfigureAwait(false);
            return map.Keys;
        }

        /// <inheritdoc />
        public async Task<Stream> GetPackageAsync(Version version)
        {
            version.GuardNotNull(nameof(version));

            // Try to get package asset URL
            var map = await GetPackageAssetUrlMapAsync().ConfigureAwait(false);
            var assetUrl = map.GetOrDefault(version);
            if (assetUrl == null)
                throw new PackageNotFoundException(version);

            return await _httpService.GetStreamAsync(assetUrl).ConfigureAwait(false);
        }
    }
}