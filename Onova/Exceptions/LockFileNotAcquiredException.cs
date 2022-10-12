using System;

namespace Onova.Exceptions;

/// <summary>
/// Thrown when an attempt to acquire a lock file failed.
/// </summary>
public class LockFileNotAcquiredException : Exception
{
    /// <summary>
    /// Initializes an instance of <see cref="LockFileNotAcquiredException" />.
    /// </summary>
    public LockFileNotAcquiredException()
        : base("Could not acquire a lock file. Most likely, another instance of this application currently owns the lock file.")
    {
    }
}