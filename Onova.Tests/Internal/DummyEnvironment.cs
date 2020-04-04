using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Mono.Cecil;

namespace Onova.Tests.Internal
{
    internal class DummyEnvironment : IDisposable
    {
        private static readonly Assembly DummyAssembly = typeof(Dummy.Program).Assembly;
        private static readonly string DummyAssemblyFileName = Path.GetFileName(DummyAssembly.Location);
        private static readonly string DummyAssemblyDirPath = Path.GetDirectoryName(DummyAssembly.Location)!;

        private readonly string _rootDirPath;

        private string DummyFilePath => Path.Combine(_rootDirPath, DummyAssemblyFileName);
        private string DummyPackagesDirPath => Path.Combine(_rootDirPath, "Packages");

        public DummyEnvironment(string rootDirPath)
        {
            _rootDirPath = rootDirPath;
        }

        private void SetAssemblyVersion(string filePath, Version version)
        {
            using var assemblyStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite);
            using var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyStream);

            assemblyDefinition.Name.Version = version;
            assemblyDefinition.Write(assemblyStream);
        }

        private void CreateBase(Version version)
        {
            // Create dummy directory
            Directory.CreateDirectory(_rootDirPath);

            // Copy files
            foreach (var filePath in Directory.EnumerateFiles(DummyAssemblyDirPath))
            {
                var fileName = Path.GetFileName(filePath);
                File.Copy(filePath, Path.Combine(_rootDirPath, fileName));
            }

            // Change base dummy version
            SetAssemblyVersion(DummyFilePath, version);
        }

        private void CreatePackage(Version version)
        {
            // Create package directory
            Directory.CreateDirectory(DummyPackagesDirPath);

            // Temporarily copy the dummy
            var dummyTempFilePath = Path.Combine(_rootDirPath, $"{DummyAssemblyFileName}.{version}");
            File.Copy(DummyFilePath, dummyTempFilePath);

            // Change dummy version
            SetAssemblyVersion(dummyTempFilePath, version);

            // Create package
            using (var zip = ZipFile.Open(Path.Combine(DummyPackagesDirPath, $"{version}.onv"), ZipArchiveMode.Create))
                zip.CreateEntryFromFile(dummyTempFilePath, DummyAssemblyFileName);

            // Delete temp file
            File.Delete(dummyTempFilePath);
        }

        private void Cleanup()
        {
            if (Directory.Exists(_rootDirPath))
                Directory.Delete(_rootDirPath, true);
        }

        public void Setup(Version baseVersion, IReadOnlyList<Version> availableVersions)
        {
            Cleanup();

            CreateBase(baseVersion);

            foreach (var version in availableVersions)
                CreatePackage(version);
        }

        public string[] GetLastRunArguments(Version version) =>
            File.ReadAllLines(Path.Combine(_rootDirPath, $"lastrun-{version}.txt"));

        public async Task<string> RunDummyAsync(params string[] arguments)
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments(a => a
                    .Add(DummyFilePath)
                    .Add(arguments))
                .ExecuteBufferedAsync();

            return result.StandardOutput;
        }

        public void Dispose() => Cleanup();
    }
}