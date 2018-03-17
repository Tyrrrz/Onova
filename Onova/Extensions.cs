using System;
using System.Threading;
using System.Threading.Tasks;
using Onova.Internal;

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
        public static async Task CheckPerformUpdateAsync(this IUpdateManager updateManager, bool restart = true,
            IProgress<double> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            updateManager.GuardNotNull(nameof(updateManager));

            // Check
            var result = await updateManager.CheckForUpdatesAsync().ConfigureAwait(false);
            if (!result.CanUpdate)
                return;

            // Prepare
            await updateManager.PrepareUpdateAsync(result.LastVersion, progress, cancellationToken)
                .ConfigureAwait(false);

            // Apply
            await updateManager.LaunchUpdaterAsync(result.LastVersion, restart).ConfigureAwait(false);

            // Exit
            Environment.Exit(0);
        }
    }
}