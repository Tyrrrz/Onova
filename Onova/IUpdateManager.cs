using System;
using System.Collections.Generic;
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
        /// Deletes all prepared updates and temporary files.
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Gets the list of prepared updates.
        /// </summary>
        IReadOnlyList<Version> GetPreparedUpdates();

        /// <summary>
        /// Checks for updates.
        /// </summary>
        Task<CheckForUpdatesResult> CheckForUpdatesAsync();

        /// <summary>
        /// Prepares an update to given version.
        /// </summary>
        Task PrepareUpdateAsync(Version version,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Launches an external executable that will apply an update to given version, once this application exits.
        /// </summary>
        Task LaunchUpdaterAsync(Version version, bool restart = true);
    }
}