using System;

namespace Onova.Exceptions
{
    /// <summary>
    /// Thrown when launching the updater after it has already been launched.
    /// </summary>
    public class UpdaterAlreadyLaunchedException : Exception
    {
        /// <inheritdoc />
        public override string Message => "Updater has already been launched.";
    }
}