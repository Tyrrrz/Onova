using System;

namespace Onova.Exceptions;

/// <summary>
/// Thrown when launching the updater after it has already been launched.
/// </summary>
public class UpdaterAlreadyLaunchedException : Exception
{
    /// <summary>
    /// Initializes an instance of <see cref="UpdaterAlreadyLaunchedException" />.
    /// </summary>
    public UpdaterAlreadyLaunchedException()
        : base("Updater has already been launched, either by this or another instance of this application.")
    {
    }
}