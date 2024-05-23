using System;

namespace Onova.Exceptions;

/// <summary>
/// Thrown when an attempt to acquire a lock file failed.
/// </summary>
public class LockFileNotAcquiredException()
    : Exception(
        "Could not acquire a lock file. Most likely, another instance of this application currently owns the lock file."
    );
