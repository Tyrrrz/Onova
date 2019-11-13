using System;

namespace Onova.Exceptions
{
    /// <summary>
    /// Thrown when a package of given version was not found by a resolver.
    /// </summary>
    public class PackageNotFoundException : Exception
    {
        /// <summary>
        /// Package version.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// Initializes an instance of <see cref="PackageNotFoundException"/>.
        /// </summary>
        public PackageNotFoundException(Version version)
            : base($"Package version [{version}] was not found by the configured package resolver.")
        {
            Version = version;
        }
    }
}