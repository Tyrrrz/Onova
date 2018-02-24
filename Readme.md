# Onova

[![Build](https://img.shields.io/appveyor/ci/Tyrrrz/Onova/master.svg)](https://ci.appveyor.com/project/Tyrrrz/Onova)
[![Tests](https://img.shields.io/appveyor/tests/Tyrrrz/Onova/master.svg)](https://ci.appveyor.com/project/Tyrrrz/Onova)
[![NuGet](https://img.shields.io/nuget/v/Onova.svg)](https://nuget.org/packages/Onova)
[![NuGet](https://img.shields.io/nuget/dt/Onova.svg)](https://nuget.org/packages/Onova)

Onova is a library that provides a simple but expandable interface to perform auto-updates in your application.

## Download

- Using NuGet: `Install-Package Onova`
- [Continuous integration](https://ci.appveyor.com/project/Tyrrrz/Onova)

## Features

- In-place update via an external executable
- Can be extended with custom providers
- No launchers or additional files needed
- Targets .NET Framework 4.6+

## Usage

##### Basic example

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
    Console.WriteLine($"An update is available -- version {result.LastVersion}");

    // Prepare package so that it can be applied later
    await manager.PreparePackageAsync(result.LastVersion);

    Console.WriteLine("An update is prepared. Do you want to restart with the new version? (y/n)");

    if (Console.ReadKey().Key == ConsoleKey.Y)
        // Exit application and apply package
        await manager.ApplyPackageAsync(result.LastVersion);
    else
        // Wait for the application to exit and apply package
        await manager.EnqueueApplyPackageAsync(result.LastVersion);
}
```

##### Updating from GitHub

```c#
// Set up the manager to look for packages in release assets and treat them as ZIPs
var resolver = new GithubPackageResolver("Tyrrrz", "LightBulb", "OnovaPackage.zip");
var extractor = new ZipPackageExtractor();
var manager = new UpdateManager(resolver, extractor);
```

## Libraries used

- [NUnit](https://github.com/nunit/nunit)
- [CliWrap](https://github.com/Tyrrrz/CliWrap)
- [Mono.Cecil](https://github.com/jbevain/cecil)