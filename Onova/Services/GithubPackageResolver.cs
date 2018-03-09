using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Onova.Internal;

namespace Onova.Services
{
    /// <summary>
    /// Resolves packages from release assets of a GitHub repository.
    /// Release names should contain package versions (e.g. "v1.0.0.0").
    /// </summary>
    public class GithubPackageResolver : HttpPackageResolver
    {
        private readonly IHttpService _httpService;
        private readonly string _repositoryOwner;
        private readonly string _repositoryName;
        private readonly string _assetNamePattern;

        /// <summary>
        /// Initializes an instance of <see cref="GithubPackageResolver"/> with a custom HTTP service.
        /// </summary>
        public GithubPackageResolver(IHttpService httpService, string repositoryOwner, string repositoryName,
            string assetNamePattern = "*.onv")
            : base(httpService)
        {
            _httpService = httpService.GuardNotNull(nameof(httpService));
            _repositoryOwner = repositoryOwner.GuardNotNull(nameof(repositoryOwner));
            _repositoryName = repositoryName.GuardNotNull(nameof(repositoryName));
            _assetNamePattern = assetNamePattern.GuardNotNull(nameof(assetNamePattern));
        }

        /// <summary>
        /// Initializes an instance of <see cref="GithubPackageResolver"/>.
        /// </summary>
        public GithubPackageResolver(string repositoryOwner, string repositoryName, string assetNamePattern = "*.onv")
            : this(HttpService.Instance, repositoryOwner, repositoryName, assetNamePattern)
        {
        }

        /// <inheritdoc />
        protected override async Task<IReadOnlyDictionary<Version, string>> GetMapAsync()
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
    }
}