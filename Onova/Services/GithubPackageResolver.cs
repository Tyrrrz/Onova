using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Onova.Exceptions;
using Onova.Internal;

namespace Onova.Services
{
    /// <summary>
    /// Resolves packages from release assets of a GitHub repository.
    /// Release names should contain package versions (e.g. "v1.0.0.0").
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
            string assetName = "Package.onv")
        {
            _httpService = httpService.GuardNotNull(nameof(httpService));
            _repositoryOwner = repositoryOwner.GuardNotNull(nameof(repositoryOwner));
            _repositoryName = repositoryName.GuardNotNull(nameof(repositoryName));
            _assetName = assetName.GuardNotNull(nameof(assetName));
        }

        /// <summary>
        /// Initializes an instance of <see cref="GithubPackageResolver"/>.
        /// </summary>
        public GithubPackageResolver(string repositoryOwner, string repositoryName, string assetName = "Package.onv")
            : this(HttpService.Instance, repositoryOwner, repositoryName, assetName)
        {
        }

        private async Task<IReadOnlyDictionary<Version, string>> GetMapAsync()
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
                var versionText = Regex.Match(name, "(\\d+\\.\\d+(?:\\.\\d+)?(?:\\.\\d+)?)").Groups[1].Value;
                if (!Version.TryParse(versionText, out var version))
                    continue;

                // Skip pre-releases
                var isPrerelease = releaseJson["prerelease"].Value<bool>();
                if (isPrerelease)
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
        public async Task<IReadOnlyList<Version>> GetAllVersionsAsync()
        {
            var map = await GetMapAsync().ConfigureAwait(false);
            return map.Keys.ToArray();
        }

        /// <inheritdoc />
        public async Task<Stream> GetPackageAsync(Version version)
        {
            version.GuardNotNull(nameof(version));

            // Try to get package asset URL
            var map = await GetMapAsync().ConfigureAwait(false);
            var assetUrl = map.GetOrDefault(version);
            if (assetUrl == null)
                throw new PackageNotFoundException(version);

            return await _httpService.GetStreamAsync(assetUrl).ConfigureAwait(false);
        }
    }
}