using System;
using System.Diagnostics;
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

        public static int Main(string[] args)
        {
            /*
            //var command = args.Length > 0 ? args[0] : null;
            var command = "update";

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

                Console.Read();
            }
            */

            Console.WriteLine("TEST1");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "VHDPlus.dll",
                    WorkingDirectory = @"C:\Users\HendrikMennen\Source\Repos\VHDP\VHDPlus\bin\Debug\netcoreapp2.1",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = true
                }

            };

            process.Start();

            Console.ReadLine();

            return 0;

        }
    }
}