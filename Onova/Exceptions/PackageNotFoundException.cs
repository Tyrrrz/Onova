using System;
using Onova.Internal;

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

        /// <inheritdoc />
        public override string Message => $"Package version [{Version}] was not found.";

        /// <summary>
        /// Initializes an instance of <see cref="PackageNotFoundException"/>.
        /// </summary>
        public PackageNotFoundException(Version version)
        {
            Version = version.GuardNotNull(nameof(version));
        }
    }
}