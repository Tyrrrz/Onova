using System;
using System.Linq;

namespace Onova.Utils.Extensions;

internal static class EnvironmentExtensions
{
    extension(Environment)
    {
        public static string GetCommandLineWithoutExecutable()
        {
            // Get the executable name
            var exeName = Environment.GetCommandLineArgs().First();
            var quotedExeName = $"\"{exeName}\"";

            // Remove the quoted executable name from command line and return it
            if (
                Environment.CommandLine.StartsWith(
                    quotedExeName,
                    StringComparison.OrdinalIgnoreCase
                )
            )
                return Environment.CommandLine[quotedExeName.Length..].Trim();

            // Remove the unquoted executable name from command line and return it
            if (Environment.CommandLine.StartsWith(exeName, StringComparison.OrdinalIgnoreCase))
                return Environment.CommandLine[exeName.Length..].Trim();

            // Safe guard, shouldn't reach here
            return Environment.CommandLine;
        }
    }
}
