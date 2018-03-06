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
    /// Base class for HTTP-based package resolvers.
    /// </summary>
    public abstract class HttpPackageResolver : IPackageResolver
    {
        private readonly IHttpService _httpService;

        /// <summary>
        /// Initializes an instance of <see cref="HttpPackageResolver"/>.
        /// </summary>
        /// <param name="httpService"></param>
        protected HttpPackageResolver(IHttpService httpService)
        {
            _httpService = httpService.GuardNotNull(nameof(httpService));
        }

        /// <summary>
        /// Gets package version to package URL conformity map.
        /// </summary>
        protected abstract Task<IReadOnlyDictionary<Version, string>> GetMapAsync();

        /// <inheritdoc />
        public async Task<IReadOnlyList<Version>> GetPackageVersionsAsync()
        {
            var versions = await GetMapAsync().ConfigureAwait(false);
            return versions.Keys.ToArray();
        }

        private async Task<Stream> GetPackageStreamAsync(Version version)
        {
            version.GuardNotNull(nameof(version));

            var map = await GetMapAsync().ConfigureAwait(false);

            // Try to get package URL
            var url = map.GetOrDefault(version);
            if (url == null)
                throw new PackageNotFoundException(version);

            // Get stream
            return await _httpService.GetStreamAsync(url).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DownloadPackageAsync(Version version, string destFilePath,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            version.GuardNotNull(nameof(version));
            destFilePath.GuardNotNull(nameof(destFilePath));

            using (var input = await GetPackageStreamAsync(version).ConfigureAwait(false))
            using (var output = File.Create(destFilePath))
                await input.CopyToAsync(output, progress, cancellationToken);
        }
    }
}