using System;
using System.Collections.Generic;

namespace Onova.Models;

/// <summary>
/// Result of checking for updates.
/// </summary>
public class CheckForUpdatesResult
{
    /// <summary>
    /// All available package versions.
    /// </summary>
    public IReadOnlyList<Version> Versions { get; }

    /// <summary>
    /// Last available package version.
    /// Null if there are no available packages.
    /// </summary>
    public Version? LastVersion { get; }

    /// <summary>
    /// Whether there is a package with higher version than the current version.
    /// </summary>
    public bool CanUpdate { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CheckForUpdatesResult" />.
    /// </summary>
    public CheckForUpdatesResult(IReadOnlyList<Version> versions, Version? lastVersion, bool canUpdate)
    {
        Versions = versions;
        LastVersion = lastVersion;
        CanUpdate = canUpdate;
    }
}