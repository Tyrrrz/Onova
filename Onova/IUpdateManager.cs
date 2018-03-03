using System;
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
        Task PreparePackageAsync(Version version);

        /// <summary>
        /// Enqueues an update to prepared package of given version, which will execute when the process exits.
        /// </summary>
        Task EnqueueApplyPackageAsync(Version version, bool restart = true);

        /// <summary>
        /// Exits current process and applies a prepared package of given version.
        /// </summary>
        Task ApplyPackageAsync(Version version, bool restart = true);

        /// <summary>
        /// Checks for updates and updates to newest version if available.
        /// </summary>
        Task PerformUpdateIfAvailableAsync(bool restart = true);
    }
}