using System;

namespace Onova.Exceptions;

/// <summary>
/// Thrown when a package of given version was not found by a resolver.
/// </summary>
public class PackageNotFoundException(Version version)
    : Exception($"Package version '{version}' was not found by the configured package resolver.")
{
    /// <summary>
    /// Package version.
    /// </summary>
    public Version Version { get; } = version;
}
