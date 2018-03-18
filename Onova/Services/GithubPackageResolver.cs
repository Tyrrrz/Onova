using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Onova.Exceptions;
using Onova.Internal;

namespace Onova.Services
{
    /// <summary>
    /// Resolves packages from release assets of a GitHub repository.
    /// Release names should contain package versions (e.g. "v1.8.3").
    /// </summary>
    public class GithubPackageResolver : IPackageResolver
    {
        private readonly IHttpService _httpService;
        private readonly string _repositoryOwner;
        private readonly string _repositoryName;
        private readonly string _assetNamePattern;

        /// <summary>
        /// Initializes an instance of <see cref="GithubPackageResolver"/> with a custom HTTP service.
        /// </summary>
        public GithubPackageResolver(IHttpService httpService, string repositoryOwner, string repositoryName,
            string assetNamePattern)
        {
            _httpService = httpService.GuardNotNull(nameof(httpService));
            _repositoryOwner = repositoryOwner.GuardNotNull(nameof(repositoryOwner));
            _repositoryName = repositoryName.GuardNotNull(nameof(repositoryName));
            _assetNamePattern = assetNamePattern.GuardNotNull(nameof(assetNamePattern));
        }

        /// <summary>
        /// Initializes an instance of <see cref="GithubPackageResolver"/>.
        /// </summary>
        public GithubPackageResolver(string repositoryOwner, string repositoryName, string assetNamePattern)
            : this(HttpService.Instance, repositoryOwner, repositoryName, assetNamePattern)
        {
        }

        private async Task<IReadOnlyDictionary<Version, string>> GetPackageVersionUrlMapAsync()
        {
            var map = new Dictionary<Version, string>();

            // Get releases
            var request = $"https://api.github.com/repos/{_repositoryOwner}/{_repositoryName}/releases";
            var response = await _httpService.GetStringAsync(request).ConfigureAwait(false);
            var releasesJson = JToken.Parse(response);

            foreach (var releaseJson in releasesJson)
            {
                // Get release name
                var releaseName = releaseJson["name"].Value<string>();

                // Try to parse version
                var versionText = Regex.Match(releaseName, "(\\d+\\.\\d+(?:\\.\\d+)?(?:\\.\\d+)?)").Groups[1].Value;
                if (!Version.TryParse(versionText, out var version))
                    continue;

                // Skip pre-releases
                var isPreRelease = releaseJson["prerelease"].Value<bool>();
                if (isPreRelease)
                    continue;

                // Find asset
                var assetsJson = releaseJson["assets"];
                foreach (var assetJson in assetsJson)
                {
                    var assetName = assetJson["name"].Value<string>();
                    var assetUrl = assetJson["browser_download_url"].Value<string>();

                    // See if name matches
                    if (!WildcardPattern.IsMatch(assetName, _assetNamePattern))
                        continue;

                    // Add to dictionary
                    map[version] = assetUrl;
                }
            }

            return map;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Version>> GetVersionsAsync()
        {
            var versions = await GetPackageVersionUrlMapAsync().ConfigureAwait(false);
            return versions.Keys.ToArray();
        }

        /// <inheritdoc />
        public async Task DownloadAsync(Version version, string destFilePath,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            version.GuardNotNull(nameof(version));
            destFilePath.GuardNotNull(nameof(destFilePath));

            // Get map
            var map = await GetPackageVersionUrlMapAsync().ConfigureAwait(false);

            // Try to get package URL
            var packageUrl = map.GetOrDefault(version);
            if (packageUrl == null)
                throw new PackageNotFoundException(version);

            // Download
            using (var input = await _httpService.GetStreamAsync(packageUrl).ConfigureAwait(false))
            using (var output = File.Create(destFilePath))
                await input.CopyToAsync(output, progress, cancellationToken).ConfigureAwait(false);
        }
    }
}