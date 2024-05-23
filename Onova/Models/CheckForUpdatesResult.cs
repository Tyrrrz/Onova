using System;
using System.Collections.Generic;

namespace Onova.Models;

/// <summary>
/// Result of checking for updates.
/// </summary>
public class CheckForUpdatesResult(
    IReadOnlyList<Version> versions,
    Version? lastVersion,
    bool canUpdate
)
{
    /// <summary>
    /// All available package versions.
    /// </summary>
    public IReadOnlyList<Version> Versions { get; } = versions;

    /// <summary>
    /// Last available package version.
    /// Null if there are no available packages.
    /// </summary>
    public Version? LastVersion { get; } = lastVersion;

    /// <summary>
    /// Whether there is a package with higher version than the current version.
    /// </summary>
    public bool CanUpdate { get; } = canUpdate;
}
