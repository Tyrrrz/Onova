using System;

namespace Onova.Exceptions;

/// <summary>
/// Thrown when launching the updater to install an update that was not prepared.
/// </summary>
public class UpdateNotPreparedException(Version version)
    : Exception(
        $"Update to version '{version}' is not prepared. Please prepare an update before applying it."
    )
{
    /// <summary>
    /// Package version.
    /// </summary>
    public Version Version { get; } = version;
}
