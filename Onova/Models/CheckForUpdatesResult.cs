using System;
using JetBrains.Annotations;
using Onova.Internal;

namespace Onova.Models
{
    /// <summary>
    /// Result of checking for updates.
    /// </summary>
    public class CheckForUpdatesResult
    {
        /// <summary>
        /// Last available package version.
        /// </summary>
        [NotNull]
        public Version LastVersion { get; }

        /// <summary>
        /// Whether an update is available.
        /// </summary>
        public bool CanUpdate { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="CheckForUpdatesResult"/>.
        /// </summary>
        public CheckForUpdatesResult(Version lastVersion, bool canUpdate)
        {
            LastVersion = lastVersion.GuardNotNull(nameof(lastVersion));
            CanUpdate = canUpdate;
        }
    }
}