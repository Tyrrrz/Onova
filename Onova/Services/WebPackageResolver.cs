using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Onova.Internal;

namespace Onova.Services
{
    /// <summary>
    /// Resolves packages using a manifest served by a web server.
    /// Manifest files consists of package versions and URLs, separated by space, one line per version.
    /// </summary>
    public class WebPackageResolver : HttpPackageResolver
    {
        private readonly IHttpService _httpService;
        private readonly string _manifestUrl;

        /// <summary>
        /// Initializes an instance of <see cref="WebPackageResolver"/> with a custom HTTP service.
        /// </summary>
        public WebPackageResolver(IHttpService httpService, string manifestUrl)
            : base(httpService)
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

        /// <inheritdoc />
        protected override async Task<IReadOnlyDictionary<Version, string>> GetMapAsync()
        {
            var map = new Dictionary<Version, string>();

            // Get manifest
            var response = await _httpService.GetStringAsync(_manifestUrl).ConfigureAwait(false);

            foreach (var line in response.Split("\n"))
            {
                // Get package ID and URL
                var id = line.SubstringUntil(" ").Trim();
                var url = line.SubstringAfter(" ").Trim();

                // If either is not set - skip
                if (id.IsBlank() || url.IsBlank())
                    continue;

                // Try to parse version
                var versionText = Regex.Match(id, "(\\d+\\.\\d+(?:\\.\\d+)?(?:\\.\\d+)?)").Groups[1].Value;
                if (!Version.TryParse(versionText, out var version))
                    continue;

                // Add to dictionary
                map[version] = url;
            }

            return map;
        }
    }
}