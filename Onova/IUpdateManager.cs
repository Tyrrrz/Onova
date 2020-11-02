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
        /// Information about the assembly, for which the updates are managed.
        /// </summary>
        AssemblyMetadata Updatee { get; }

        /// <summary>
        /// Checks for updates.
        /// </summary>
        Task<CheckForUpdatesResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default);

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
            IProgress<double>? progress = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Launches an external executable that will apply an update to given version, once this application exits.
        /// The updater can be instructed to also restart the application after it's updated. Additional executables
        /// are relative to the updatee.
        /// </summary>
        void LaunchUpdater(Version version, bool restart, string restartArguments, string[]? additonalExecutables = null);
    }
}