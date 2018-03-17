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
        private static string PackagesDirPath => Path.Combine(DummyDirPath, "Packages");

        private static readonly Cli DummyCli = new Cli(DummyFilePath);

        public static void Delete()
        {
            if (Directory.Exists(DummyDirPath))
                Directory.Delete(DummyDirPath, true);
        }

        private static void SetAssemblyVersion(string filePath, Version version)
        {
            var definition = AssemblyDefinition.ReadAssembly(filePath);
            definition.Name.Version = version;
            definition.Write(filePath);
        }

        private static void CreateBase(Version version)
        {
            // Create dummy directory
            Directory.CreateDirectory(DummyDirPath);

            // Copy files
            foreach (var filePath in Directory.EnumerateFiles(TestDirPath))
            {
                var fileExt = Path.GetExtension(filePath);
                var fileName = Path.GetFileName(filePath);

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
            Directory.CreateDirectory(PackagesDirPath);

            // Temporarily copy the dummy
            var dummyTempFilePath = Path.Combine(DummyDirPath, $"{DummyFileName}.{version}");
            File.Copy(DummyFilePath, dummyTempFilePath);

            // Change dummy version
            SetAssemblyVersion(dummyTempFilePath, version);

            // Create package
            using (var zip = ZipFile.Open(Path.Combine(PackagesDirPath, $"{version}.onv"), ZipArchiveMode.Create))
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
            var output = await DummyCli.ExecuteAsync("version");
            output.ThrowIfError();

            return Version.Parse(output.StandardOutput);
        }

        public static async Task CheckPerformUpdateAsync()
        {
            var output = await DummyCli.ExecuteAsync("update");
            output.ThrowIfError();
        }
    }
}