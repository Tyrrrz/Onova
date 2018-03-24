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

        /// <inheritdoc />
        public override string Message => $"Update to version [{Version}] needs to be prepared first.";

        /// <summary>
        /// Initializes an instance of <see cref="UpdateNotPreparedException"/>.
        /// </summary>
        public UpdateNotPreparedException(Version version)
        {
            Version = version.GuardNotNull(nameof(version));
        }
    }
}