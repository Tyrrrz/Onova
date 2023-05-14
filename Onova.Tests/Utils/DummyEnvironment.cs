using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Mono.Cecil;
using Polly;

namespace Onova.Tests.Utils;

internal class DummyEnvironment : IDisposable
{
    private static readonly Assembly DummyAssembly = typeof(Dummy.Program).Assembly;
    private static readonly string DummyAssemblyFileName = Path.GetFileName(DummyAssembly.Location);
    private static readonly string DummyAssemblyDirPath = Path.GetDirectoryName(DummyAssembly.Location)!;

    private readonly string _rootDirPath;

    private string DummyFilePath { get; }

    private string DummyPackagesDirPath { get; }

    public DummyEnvironment(string rootDirPath)
    {
        _rootDirPath = rootDirPath;

        DummyFilePath = Path.Combine(_rootDirPath, DummyAssemblyFileName);
        DummyPackagesDirPath = Path.Combine(_rootDirPath, "Packages");
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
        // Sometimes this fails for some reason, even when dummy has already exited.
        // Use a retry policy to circumvent that.
        var policy = Policy.Handle<UnauthorizedAccessException>().WaitAndRetry(5, _ => TimeSpan.FromSeconds(1));

        policy.Execute(() => DirectoryEx.DeleteIfExists(_rootDirPath));
    }

    public void Setup(Version baseVersion, IReadOnlyList<Version> availableVersions)
    {
        Cleanup();

        CreateBase(baseVersion);

        foreach (var version in availableVersions)
            CreatePackage(version);
    }

    public string[] GetLastRunArguments(Version version)
    {
        var filePath = Path.Combine(_rootDirPath, $"lastrun-{version}.txt");
        return File.Exists(filePath) ? File.ReadAllLines(filePath) : Array.Empty<string>();
    }

    public bool IsRunning() => !FileEx.CheckWriteAccess(DummyFilePath);

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