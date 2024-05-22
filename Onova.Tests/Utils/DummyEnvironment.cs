using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Mono.Cecil;

namespace oZnova.Tests.Utils;

internal class DummyEnvironment : IDisposable
{
    private static readonly Assembly DummyAssembly = typeof(Dummy.Program).Assembly;
    private static readonly string DummyAssemblyFileName = Path.GetFileName(DummyAssembly.Location);
    private static readonly string DummyAssemblyDirPath = Path.GetDirectoryName(
        DummyAssembly.Location
    )!;

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
        using (
            var zip = ZipFile.Open(
                Path.Combine(DummyPackagesDirPath, $"{version}.onv"),
                ZipArchiveMode.Create
            )
        )
            zip.CreateEntryFromFile(dummyTempFilePath, DummyAssemblyFileName);

        // Delete temp file
        File.Delete(dummyTempFilePath);
    }

    private void Cleanup()
    {
        // Sometimes this fails for some reason, even when dummy has already exited.
        // Use a retry policy to circumvent that.
        for (var retriesRemaining = 5; ; retriesRemaining--)
        {
            try
            {
                DirectoryEx.DeleteIfExists(_rootDirPath);
                break;
            }
            catch (UnauthorizedAccessException) when (retriesRemaining > 0)
            {
                Thread.Sleep(1000);
            }
        }
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

    public string GetLastUpdaterLogs()
    {
        var filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "oZnova",
            DummyAssembly.GetName().Name!,
            "Log.txt"
        );

        return File.Exists(filePath) ? File.ReadAllText(filePath) : "";
    }

    public async Task<string> RunDummyAsync(params string[] arguments)
    {
        var result = await Cli.Wrap("dotnet")
            .WithArguments(a => a.Add(DummyFilePath).Add(arguments))
            .ExecuteBufferedAsync();

        return result.StandardOutput;
    }

    public async Task WaitUntilUpdaterExitsAsync(CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "oZnova",
            DummyAssembly.GetName().Name!,
            "oZnova.lock"
        );

        // Try deleting the lock file
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                File.Delete(filePath);
                return;
            }
            catch (FileNotFoundException)
            {
                return;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                await Task.Delay(100, cancellationToken);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    public void Dispose() => Cleanup();
}
