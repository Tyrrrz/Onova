using System;
using Onova.Internal;

namespace Onova.Exceptions
{
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
        /// Initializes an instance of <see cref="UpdateNotPreparedException"/>.
        /// </summary>
        public UpdateNotPreparedException(Version version)
            : base($"Update to version [{version}] needs to be prepared first.")
        {
            Version = version.GuardNotNull(nameof(version));
        }
    }
}