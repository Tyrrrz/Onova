using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Onova.Services
{
    /// <summary>
    /// Provider for resolving packages.
    /// </summary>
    public interface IPackageResolver
    {
        /// <summary>
        /// Gets all available package versions.
        /// </summary>
        Task<IReadOnlyList<Version>> GetPackageVersionsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads given package version.
        /// </summary>
        Task DownloadPackageAsync(Version version, string destFilePath,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    }
}