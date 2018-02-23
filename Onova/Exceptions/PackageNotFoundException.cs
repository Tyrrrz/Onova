using System;

namespace Onova.Exceptions
{
    /// <summary>
    /// Thrown when a package of specific version was not found by resolver.
    /// </summary>
    public class PackageNotFoundException : Exception
    {
        /// <summary>
        /// Package version.
        /// </summary>
        public Version Version { get; }

        /// <inheritdoc />
        public override string Message { get; }

        /// <summary>
        /// Initializes an instance of <see cref="PackageNotFoundException"/>.
        /// </summary>
        public PackageNotFoundException(Version version)
        {
            Version = version;
            Message = $"Package with version [{version}] not found.";
        }
    }
}