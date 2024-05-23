using System;

namespace Onova.Exceptions;

/// <summary>
/// Thrown when launching the updater after it has already been launched.
/// </summary>
public class UpdaterAlreadyLaunchedException()
    : Exception(
        "Updater has already been launched, either by this or another instance of the application."
    );
