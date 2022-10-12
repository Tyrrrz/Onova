using System;

namespace Onova.Exceptions;

/// <summary>
/// Thrown when launching the updater to install an update that was not prepared.
/// </summary>
public class UpdateNotPreparedException : Exception
{
    /// <summary>
    /// Package version.
    /// </summary>
    public Version Version { get; }

    /// <summary>
    /// Initializes an instance of <see cref="UpdateNotPreparedException" />.
    /// </summary>
    public UpdateNotPreparedException(Version version)
        : base($"Update to version '{version}' is not prepared. Please prepare an update before applying it.")
    {
        Version = version;
    }
}