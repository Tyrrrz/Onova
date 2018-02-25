# Onova

[![Build](https://img.shields.io/appveyor/ci/Tyrrrz/Onova/master.svg)](https://ci.appveyor.com/project/Tyrrrz/Onova)
[![Tests](https://img.shields.io/appveyor/tests/Tyrrrz/Onova/master.svg)](https://ci.appveyor.com/project/Tyrrrz/Onova)
[![NuGet](https://img.shields.io/nuget/v/Onova.svg)](https://nuget.org/packages/Onova)
[![NuGet](https://img.shields.io/nuget/dt/Onova.svg)](https://nuget.org/packages/Onova)

Onova is a library that provides a simple but extensible interface to perform auto-updates in your application. It was designed primarily for open source projects that distribute their releases using archive files instead of installers. Acquired updates are applied in place via an external process, so there are no launchers, release files or special directories.

## Download

- Using NuGet: `Install-Package Onova`
- [Continuous integration](https://ci.appveyor.com/project/Tyrrrz/Onova)

## Features

- Minimal configuration
- In-place update via an external executable
- Supported resolvers:
  - `LocalPackageResolver` - file system
  - `GithubPackageResolver` - GitHub releases
- Supported extractors:
  - `ZipPackageExtractor` - zip archives
- Can be extended with custom providers
- Can apply packages of any version, not necessarily latest
- No launchers or additional files needed
- Targets .NET Framework 4.6+

## Usage

##### How package resolving works

Packages and their versions are resolved using an implementation of `IPackageResolver`. Currently there are 2 built-in implementations:

- `LocalPackageResolver` looks for files in the given directory using a predefined search pattern (default is `*.onv`). Package versions are extracted from file names, e.g. file named `MyProject-v2.1.5.onv` corresponds to package version `2.1.5`.
- `GithubPackageResolver` looks for assets with predefined name (default is `Package.onv`) in releases of the given repository. Package versions are extracted from release names, e.g. release named `v1.0` corresponds to package version `1.0`.

Packages whose versions could not be extracted will not be seen by the resolver. Also, if there are multiple packages with the same version, only one of them will be available.

##### How package extraction works

Downloaded packages are extracted using an implementation of `IPackageExtractor`. Currently there is 1 built-in implementation:

- `ZipPackageExtractor` treats packages as zip archives.

##### Basic usage example

```c#
// Set up the manager to look for packages in given directory and treat them as ZIPs
var resolver = new LocalPackageResolver("c:\\test\\packages");
var extractor = new ZipPackageExtractor();
var manager = new UpdateManager(resolver, extractor);

// Check for updates
// If available - download, extract, exit, apply, restart
await manager.PerformUpdateIfAvailableAsync();
```

##### Handling intermediate steps manually

```c#
// Check for updates
var result = await manager.CheckForUpdatesAsync();

if (result.CanUpdate)
{
    Console.WriteLine($"An update is available -- v{result.LastVersion}");

    // Prepare package so that it can be applied later
    await manager.PreparePackageAsync(result.LastVersion);

    Console.WriteLine("An update is prepared. Do you want to restart with the new version? (y/n)");

    if (Console.ReadKey().Key == ConsoleKey.Y)
        // Exit application, apply package and restart
        await manager.ApplyPackageAsync(result.LastVersion);
    else
        // Wait for the application to exit and apply package, without restart
        await manager.EnqueueApplyPackageAsync(result.LastVersion, false);
}
```

##### Configuring for GitHub

```c#
// Set up the manager to look for packages in release assets and treat them as ZIPs
var resolver = new GithubPackageResolver("Tyrrrz", "LightBulb", "OnovaPackage.zip");
var extractor = new ZipPackageExtractor();
var manager = new UpdateManager(resolver, extractor);
```

## Libraries used

- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [NUnit](https://github.com/nunit/nunit)
- [CliWrap](https://github.com/Tyrrrz/CliWrap)
- [Mono.Cecil](https://github.com/jbevain/cecil)