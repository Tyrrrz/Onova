using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using CliWrap;
using Mono.Cecil;
using NUnit.Framework;

namespace Onova.Tests.Internal
{
    internal static class DummyEnvironment
    {
        private const string DummyFileName = "Onova.Tests.Dummy.exe";

        private static string TestDirPath => TestContext.CurrentContext.TestDirectory;
        private static string DummyDirPath => Path.Combine(TestDirPath, "Dummy");
        private static string DummyFilePath => Path.Combine(DummyDirPath, DummyFileName);
        private static string DummyPackagesDirPath => Path.Combine(DummyDirPath, "Packages");

        public static void Delete()
        {
            // Delete directory
            if (Directory.Exists(DummyDirPath))
                Directory.Delete(DummyDirPath, true);
        }

        private static void SetAssemblyVersion(string filePath, Version version)
        {
            using (var assemblyStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite))
            using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyStream))
            {
                // Change assembly version
                assemblyDefinition.Name.Version = version;
                assemblyDefinition.Write(assemblyStream);
            }
        }

        private static void CreateBase(Version version)
        {
            // Create dummy directory
            Directory.CreateDirectory(DummyDirPath);

            // Copy files
            foreach (var filePath in Directory.EnumerateFiles(TestDirPath))
            {
                var fileName = Path.GetFileName(filePath);
                var fileExt = Path.GetExtension(filePath);

                // Only exe and dll
                if (!string.Equals(fileExt, ".exe", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(fileExt, ".dll", StringComparison.OrdinalIgnoreCase))
                    continue;

                File.Copy(filePath, Path.Combine(DummyDirPath, fileName));
            }

            // Change base dummy version
            SetAssemblyVersion(DummyFilePath, version);
        }

        private static void CreatePackage(Version version)
        {
            // Create package directory
            Directory.CreateDirectory(DummyPackagesDirPath);

            // Temporarily copy the dummy
            var dummyTempFilePath = Path.Combine(DummyDirPath, $"{DummyFileName}.{version}");
            File.Copy(DummyFilePath, dummyTempFilePath);

            // Change dummy version
            SetAssemblyVersion(dummyTempFilePath, version);

            // Create package
            using (var zip = ZipFile.Open(Path.Combine(DummyPackagesDirPath, $"{version}.onv"), ZipArchiveMode.Create))
                zip.CreateEntryFromFile(dummyTempFilePath, DummyFileName);

            // Delete temp file
            File.Delete(dummyTempFilePath);
        }

        public static void Setup(Version baseVersion, params Version[] packageVersions)
        {
            // Delete
            Delete();

            // Create base
            CreateBase(baseVersion);

            // Create packages
            foreach (var packageVersion in packageVersions)
                CreatePackage(packageVersion);
        }

        public static async Task<Version> GetCurrentVersionAsync()
        {
            var result = await new Cli(DummyFilePath).SetArguments("version").ExecuteAsync();

            return Version.Parse(result.StandardOutput);
        }

        public static async Task CheckPerformUpdateAsync()
        {
            await new Cli(DummyFilePath).SetArguments("update").ExecuteAsync();
        }
    }
}