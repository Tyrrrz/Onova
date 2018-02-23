using System;

namespace Onova.Exceptions
{
    /// <summary>
    /// Thrown when a package of specific version was not prepared.
    /// </summary>
    public class PackageNotPreparedException : Exception
    {
        /// <summary>
        /// Package version.
        /// </summary>
        public Version Version { get; }

        /// <inheritdoc />
        public override string Message { get; }

        /// <summary>
        /// Initializes an instance of <see cref="PackageNotPreparedException"/>.
        /// </summary>
        public PackageNotPreparedException(Version version)
        {
            Version = version;
            Message = $"Package with version [{version}] has not been prepared.";
        }
    }
}