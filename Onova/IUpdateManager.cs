using System;
using System.Threading;
using System.Threading.Tasks;
using Onova.Models;

namespace Onova
{
    /// <summary>
    /// Interface for <see cref="UpdateManager"/>.
    /// </summary>
    public interface IUpdateManager
    {
        /// <summary>
        /// Deletes all prepared packages and temporary files.
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Checks for updates.
        /// </summary>
        Task<CheckForUpdatesResult> CheckForUpdatesAsync();

        /// <summary>
        /// Prepares a package of given version.
        /// </summary>
        Task PreparePackageAsync(Version version,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Enqueues an update to prepared package of given version, which will execute when the process exits.
        /// </summary>
        Task ApplyPackageAsync(Version version, bool restart = true);
    }
}