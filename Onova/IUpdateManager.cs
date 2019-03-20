using System;
using System.Threading;
using System.Threading.Tasks;
using Onova.Models;

namespace Onova
{
    /// <summary>
    /// Interface for <see cref="UpdateManager"/>.
    /// </summary>
    public interface IUpdateManager : IDisposable
    {
        /// <summary>
        /// Checks for updates.
        /// </summary>
        Task<CheckForUpdatesResult> CheckForUpdatesAsync();

        /// <summary>
        /// Checks whether an update to given version has been prepared.
        /// </summary>
        bool IsUpdatePrepared(Version version);

        /// <summary>
        /// Prepares an update to given version.
        /// </summary>
        Task PrepareUpdateAsync(Version version,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Launches an external executable that will apply an update to given version, once this application exits.
        /// </summary>
        void LaunchUpdater(Version version, bool restart = true);
    }
}