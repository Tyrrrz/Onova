using System;
using System.Threading;
using System.Threading.Tasks;

namespace Onova
{
    /// <summary>
    /// Extensions for <see cref="Onova"/>.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Checks for new version and performs an update if available.
        /// </summary>
        public static async Task CheckPerformUpdateAsync(this IUpdateManager manager, bool restart = true,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            // Check
            var result = await manager.CheckForUpdatesAsync();
            if (!result.CanUpdate || result.LastVersion == null)
                return;

            // Prepare
            await manager.PrepareUpdateAsync(result.LastVersion, progress, cancellationToken);

            // Apply
            manager.LaunchUpdater(result.LastVersion, restart);

            // Exit
            Environment.Exit(0);
        }
    }
}