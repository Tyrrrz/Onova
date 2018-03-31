using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Onova.Services;

namespace Onova.Tests.Dummy
{
    // This executable is used as dummy for end-to-end testing.
    // It can print its current version and use Onova to update.

    public static class Program
    {
        private static string AssemblyDirPath => AppDomain.CurrentDomain.BaseDirectory;
        private static string PackagesDirPath => Path.Combine(AssemblyDirPath, "Packages");
        private static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        private static readonly IUpdateManager UpdateManager = new UpdateManager(
            new LocalPackageResolver(PackagesDirPath, "*.onv"),
            new ZipPackageExtractor());

        public static async Task MainAsync(string[] args)
        {
            var command = args.Length > 0 ? args[0] : null;

            // Print current assembly version
            if (command == "version" || command == null)
            {
                Console.WriteLine(Version);
            }
            // Update to latest version
            else if (command == "update")
            {
                var progressHandler = new Progress<double>(p => Console.WriteLine($"Progress: {p:P0}"));
                await UpdateManager.CheckPerformUpdateAsync(false, progressHandler);
            }
        }

        public static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();
    }
}