using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Onova.Exceptions;
using Onova.Internal;

namespace Onova.Services
{
    /// <summary>
    /// Resolves packages using a manifest served by a web server.
    /// Manifest consists of package versions and URLs, separated by space, one line per version.
    /// </summary>
    public class WebPackageResolver : IPackageResolver
    {
        private readonly IHttpService _httpService;
        private readonly string _manifestUrl;

        /// <summary>
        /// Initializes an instance of <see cref="WebPackageResolver"/> with a custom HTTP service.
        /// </summary>
        public WebPackageResolver(IHttpService httpService, string manifestUrl)
        {
            _httpService = httpService.GuardNotNull(nameof(httpService));
            _manifestUrl = manifestUrl.GuardNotNull(nameof(manifestUrl));
        }

        /// <summary>
        /// Initializes an instance of <see cref="WebPackageResolver"/>.
        /// </summary>
        public WebPackageResolver(string manifestUrl)
            : this(HttpService.Instance, manifestUrl)
        {
        }

        private async Task<IReadOnlyDictionary<Version, string>> GetPackageVersionUrlMapAsync()
        {
            var map = new Dictionary<Version, string>();

            // Get manifest
            var response = await _httpService.GetStringAsync(_manifestUrl).ConfigureAwait(false);

            foreach (var line in response.Split("\n"))
            {
                // Get package version and URL
                var versionText = line.SubstringUntil(" ").Trim();
                var url = line.SubstringAfter(" ").Trim();

                // If either is not set - skip
                if (versionText.IsBlank() || url.IsBlank())
                    continue;

                // Try to parse version
                if (!Version.TryParse(versionText, out var version))
                    continue;

                // Add to dictionary
                map[version] = url;
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