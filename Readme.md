# Onova

[![Build](https://img.shields.io/appveyor/ci/Tyrrrz/Onova/master.svg)](https://ci.appveyor.com/project/Tyrrrz/Onova)
[![Tests](https://img.shields.io/appveyor/tests/Tyrrrz/Onova/master.svg)](https://ci.appveyor.com/project/Tyrrrz/Onova)
[![NuGet](https://img.shields.io/nuget/v/Onova.svg)](https://nuget.org/packages/Onova)
[![NuGet](https://img.shields.io/nuget/dt/Onova.svg)](https://nuget.org/packages/Onova)

Onova is a library that provides a framework for performing auto-updates in applications. It was designed primarily for projects that distribute their releases using archive files instead of installers, but can be configured to support almost any setup. Acquired updates are applied in place using an external executable, so there are no launchers, release files or special directories.

## Download

- Using NuGet: `Install-Package Onova`
- [Continuous integration](https://ci.appveyor.com/project/Tyrrrz/Onova)

## Features

- Minimal required configuration
- Supported resolvers:
  - `LocalPackageResolver` - file system
  - `GithubPackageResolver` - GitHub releases
  - `WebPackageResolver` - web version manifest
  - `NugetPackageResolver` - NuGet feed
  - `AggregatePackageResolver` - aggregates multiple resolvers
- Supported extractors:
  - `ZipPackageExtractor` - zip archives
  - `NugetPackageExtractor` - NuGet packages
- Can be extended with custom resolvers and extractors
- Progress reporting and cancellation
- Can apply updates to any version, not necessarily latest
- In-place update using an external executable
- Automatically prompts for elevated privileges if necessary
- Fully self-contained
- Targets .NET Framework 4.6+

## Workflow

### Package resolving

Packages and their versions are resolved using an implementation of `IPackageResolver`. Currently there are 5 built-in implementations:

#### `LocalPackageResolver` 

This implementation looks for files in the specified directory using a predefined pattern. Package versions are extracted from file names, e.g. file named `MyProject-v2.1.5.zip` corresponds to package version `2.1.5`.

#### `GithubPackageResolver`

This implementation looks for assets in releases of specified GitHub repository using a predefined pattern. Package versions are extracted from release names, e.g. release named `v1.0` corresponds to package version `1.0`.

Since .NET assemblies do not support semantic versions, pre-releases are ignored.

#### `WebPackageResolver`

This implementation requests a version manifest using specified URL. The manifest should contain a list of package versions and their URLs, separated by space, one line per package. E.g.:
```
1.0 https://my.server.com/1.0.zip
2.0 https://my.server.com/2.0.zip
```

#### `NugetPackageResolver`

This implementation resolves packages from the specified NuGet feed.

Since .NET assemblies do not support semantic versions, pre-releases are ignored.

#### `AggregatePackageResolver`

This implementation provides aggregation over multiple other `IPackageResolver` instances. It allows resolving and downloading packages from more than one source.

### Package extraction

Downloaded packages are extracted using an implementation of `IPackageExtractor`. Currently there are 2 built-in implementations:

#### `ZipPackageExtractor`

This implementation treats packages as zip archives.

#### `NugetPackageExtractor`

This implementation treats packages as zip archives with NuGet structure. Files are extracted from the specified root directory.

## Usage

##### Basic usage example

```c#
// Configure to look for packages in specified directory and treat them as zips
var manager = new UpdateManager(
    new LocalPackageResolver("c:\\test\\packages", "*.zip"),
    new ZipPackageExtractor());

// Check for new version and perform full update if available
await manager.CheckPerformUpdateAsync();
```

##### Handling intermediate steps manually

```c#
// Check for updates
var result = await manager.CheckForUpdatesAsync();
if (result.CanUpdate)
{
    // Prepare an update so it can be applied later
    // (supports optional progress reporting and cancellation)
    await manager.PrepareUpdateAsync(result.LastVersion);

    // Launch an executable that will apply the update
    // (can optionally restart application on completion)
    await manager.LaunchUpdaterAsync(result.LastVersion);

    // External updater will wait until the application exits
    Environment.Exit(0);
}
```

## Libraries used

- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Moq](https://github.com/Moq/moq4)
- [NUnit](https://github.com/nunit/nunit)
- [CliWrap](https://github.com/Tyrrrz/CliWrap)
- [Mono.Cecil](https://github.com/jbevain/cecil)