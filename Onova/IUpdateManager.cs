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
        /// Gets a list of all prepared updates.
        /// </summary>
        IReadOnlyList<Version> GetPreparedUpdates();

        /// <summary>
        /// Prepares an update to specified version.
        /// </summary>
        Task PrepareUpdateAsync(Version version,
            IProgress<double> progress = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Launches an external executable that will apply an update to given version, once this application exits.
        /// </summary>
        void LaunchUpdater(Version version, bool restart = true);
    }
}