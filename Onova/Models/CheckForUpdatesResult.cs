using System;
using System.Collections.Generic;
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
        /// All available package versions.
        /// </summary>
        [NotNull, ItemNotNull]
        public IReadOnlyList<Version> AllPackageVersions { get; }

        /// <summary>
        /// Last available package version.
        /// Null if there are no available package versions.
        /// </summary>
        [CanBeNull]
        public Version LastPackageVersion { get; }

        /// <summary>
        /// Whether there is a package with higher version than the current version.
        /// </summary>
        public bool CanUpdate { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="CheckForUpdatesResult"/>.
        /// </summary>
        public CheckForUpdatesResult(IReadOnlyList<Version> allPackageVersions, Version lastPackageVersion,
            bool canUpdate)
        {
            AllPackageVersions = allPackageVersions.GuardNotNull(nameof(allPackageVersions));
            LastPackageVersion = lastPackageVersion;
            CanUpdate = canUpdate;
        }
    }
}