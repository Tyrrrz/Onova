using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using CliWrap;
using Mono.Cecil;
using NUnit.Framework;

namespace Onova.Tests.Internal
{
    internal static class DummyHelper
    {
        private const string DummyFileName = "Onova.Tests.Dummy.exe";

        private static string TestDirPath => TestContext.CurrentContext.TestDirectory;
        private static string DummyDirPath => Path.Combine(TestDirPath, "Dummy");
        private static string DummyFilePath => Path.Combine(DummyDirPath, DummyFileName);
        private static string PackagesDirPath => Path.Combine(DummyDirPath, "Packages");

        private static readonly Cli DummyCli = new Cli(DummyFilePath);

        public static void DeleteDummy()
        {
            if (Directory.Exists(DummyDirPath))
                Directory.Delete(DummyDirPath, true);
        }

        private static void CreateDummy(Version version)
        {
            // Create dummies directory
            Directory.CreateDirectory(DummyDirPath);

            // Copy files
            foreach (var filePath in Directory.EnumerateFiles(TestDirPath))
            {
                var fileExt = Path.GetExtension(filePath);
                var fileName = Path.GetFileName(filePath);

                // Only exe and dll
                if (!fileExt.Equals(".exe", StringComparison.OrdinalIgnoreCase) &&
                    !fileExt.Equals(".dll", StringComparison.OrdinalIgnoreCase))
                    continue;

                File.Copy(filePath, Path.Combine(DummyDirPath, fileName));
            }

            // Change version
            var definition = AssemblyDefinition.ReadAssembly(DummyFilePath);
            definition.Name.Version = version;
            definition.Write(DummyFilePath);
        }

        private static void CreateDummyPackage(Version version)
        {
            // Create packages directory
            Directory.CreateDirectory(PackagesDirPath);

            // Temporarily copy the dummy
            var dummyTempFilePath = Path.Combine(DummyDirPath, $"{DummyFileName}.{version}.exe");
            File.Copy(DummyFilePath, dummyTempFilePath);

            // Change version
            var definition = AssemblyDefinition.ReadAssembly(dummyTempFilePath);
            definition.Name.Version = version;
            definition.Write(dummyTempFilePath);

            // Create package
            using (var outputStream = File.Create(Path.Combine(PackagesDirPath, $"{version}.onv")))
            using (var zip = new ZipArchive(outputStream, ZipArchiveMode.Create))
                zip.CreateEntryFromFile(dummyTempFilePath, DummyFileName);

            // Delete temp file
            File.Delete(dummyTempFilePath);
        }

        public static void SetupDummy(Version baseVersion, params Version[] packageVersions)
        {
            // Delete old dummy
            DeleteDummy();

            // Create base dummy
            CreateDummy(baseVersion);

            // Create packages
            foreach (var version in packageVersions)
                CreateDummyPackage(version);
        }

        private static async Task<string> ExecuteDummyCliAsync(string args)
        {
            var output = await DummyCli.ExecuteAsync(args);

            if (output.HasError)
                Assert.Fail($"Dummy reported an error:{Environment.NewLine}{output.StandardError}");

            return output.StandardOutput;
        }

        public static async Task<Version> GetDummyVersionAsync()
        {
            var stdout = await ExecuteDummyCliAsync("version");

            return Version.Parse(stdout);
        }

        public static async Task UpdateDummyAsync()
        {
            await ExecuteDummyCliAsync("update");
        }
    }
}