using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Onova.Services;
using Onova.Tests.Dummy.Internal;

namespace Onova.Tests.Dummy
{
    // This executable is used as dummy for end-to-end testing.
    // It can print its current version and use Onova to update.

    public static class Program
    {
        private static Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        private static string AssemblyDirPath => AppDomain.CurrentDomain.BaseDirectory;
        private static string LastRunFilePath => Path.Combine(AssemblyDirPath, $"lastrun-{Version}.txt");
        private static string PackagesDirPath => Path.Combine(AssemblyDirPath, "Packages");

        private static readonly IUpdateManager UpdateManager = new UpdateManager(
            new LocalPackageResolver(PackagesDirPath, "*.onv"),
            new ZipPackageExtractor());

        public static async Task Main(string[] args)
        {
            // Dump arguments to file.
            // This is only accurate enough for simple inputs.
            File.WriteAllText(LastRunFilePath, args.JoinToString(" "));

            // Get command name
            var command = args.FirstOrDefault();

            // Print current assembly version
            if (command == "version" || command == null)
            {
                Console.WriteLine(Version);
            }
            // Update to latest version
            else if (command == "update" || command == "update-and-restart")
            {
                var restart = command == "update-and-restart";
                var progressHandler = new Progress<double>(p => Console.WriteLine($"Progress: {p:P0}"));

                await UpdateManager.CheckPerformUpdateAsync(restart, progressHandler);
            }
        }
    }
}