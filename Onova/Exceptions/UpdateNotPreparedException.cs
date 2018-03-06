using System;

namespace Onova.Exceptions
{
    /// <summary>
    /// Thrown when an update was not prepared.
    /// </summary>
    public class UpdateNotPreparedException : Exception
    {
        /// <summary>
        /// Package version.
        /// </summary>
        public Version Version { get; }

        /// <inheritdoc />
        public override string Message { get; }

        /// <summary>
        /// Initializes an instance of <see cref="UpdateNotPreparedException"/>.
        /// </summary>
        public UpdateNotPreparedException(Version version)
        {
            Version = version;
            Message = $"Update to version [{version}] not prepared.";
        }
    }
}