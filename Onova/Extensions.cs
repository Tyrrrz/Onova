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
        /// Checks for updates, prepares latest package and exits application to apply it.
        /// Returns early if there are no updates available.
        /// </summary>
        public static async Task CheckPerformUpdateAsync(this IUpdateManager updateManager, bool restart = true,
            IProgress<double> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check
            var result = await updateManager.CheckForUpdatesAsync().ConfigureAwait(false);
            if (!result.CanUpdate)
                return;

            // Prepare
            await updateManager.PreparePackageAsync(result.LastVersion, progress, cancellationToken)
                .ConfigureAwait(false);

            // Apply
            await updateManager.ApplyPackageAsync(result.LastVersion, restart).ConfigureAwait(false);

            // Exit
            Environment.Exit(0);
        }
    }
}