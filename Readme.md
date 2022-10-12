# Onova

[![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7)](https://vshymanskyy.github.io/StandWithUkraine)
[![Build](https://img.shields.io/github/workflow/status/Tyrrrz/Onova/CI/master)](https://github.com/Tyrrrz/Onova/actions)
[![Coverage](https://img.shields.io/codecov/c/github/Tyrrrz/Onova/master)](https://codecov.io/gh/Tyrrrz/Onova)
[![Version](https://img.shields.io/nuget/v/Onova.svg)](https://nuget.org/packages/Onova)
[![Downloads](https://img.shields.io/nuget/dt/Onova.svg)](https://nuget.org/packages/Onova)
[![Discord](https://img.shields.io/discord/869237470565392384?label=discord)](https://discord.gg/2SUWKFnHSm)
[![Donate](https://img.shields.io/badge/donate-$$$-8a2be2.svg)](https://tyrrrz.me/donate)
[![Fuck Russia](https://img.shields.io/badge/fuck-russia-e4181c.svg?labelColor=000000)](https://twitter.com/tyrrrz/status/1495972128977571848)

> 🟡 **Project status**: maintenance mode<sup>[[?]](https://github.com/Tyrrrz/.github/blob/master/docs/project-status.md)</sup>

**Onova** is a lightweight auto-update framework for desktop applications.
It's primarily designed for performing in-place updates for portable applications distributed via archive files (as opposed to installers or packages).
The library requires minimal configuration, doesn't impose any changes to the CI/CD process, and doesn't affect the life cycle of your application.

✨ See also [Onova.Publisher](https://github.com/dady8889/Onova.Publisher) — community project that provides an integrated installation experience based on **Onova**.

## Terms of use<sup>[[?]](https://github.com/Tyrrrz/.github/blob/master/docs/why-so-political.md)</sup>

By using this project or its source code, for any purpose and in any shape or form, you grant your **implicit agreement** to all the following statements:

- You **condemn Russia and its military aggression against Ukraine**
- You **recognize that Russia is an occupant that unlawfully invaded a sovereign state**
- You **support Ukraine's territorial integrity, including its claims over temporarily occupied territories of Crimea and Donbas**
- You **reject false narratives perpetuated by Russian state propaganda**

To learn more about the war and how you can help, [click here](https://tyrrrz.me). Glory to Ukraine! 🇺🇦

## Install

- 📦 [NuGet](https://nuget.org/packages/Onova): `dotnet add package Onova`

## Features

- Requires minimal configuration
- Supports the following package resolvers:
  - `LocalPackageResolver` - file system
  - `GithubPackageResolver` - GitHub releases
  - `WebPackageResolver` - web version manifest
  - `NugetPackageResolver` - NuGet feed
  - `AggregatePackageResolver` - aggregates multiple resolvers
- Supports the following package extractors:
  - `ZipPackageExtractor` - zip archives
  - `NugetPackageExtractor` - NuGet packages
- Extendable with custom resolvers and extractors
- Can report progress and supports cancellation
- Allows updating to any available version, not necessarily latest
- Overwrites files in-place using an external executable
- Works with multiple running instances of an application
- Automatically prompts for elevated privileges if necessary
- Fully self-contained and doesn't require additional files
- Supports desktop apps built with .NET Core 3.0+
- Targets .NET Framework 4.6.1+ and .NET Standard 2.0+ (Windows only)
- No external dependencies

## Workflow

### Package resolving

Packages and their versions are resolved using an implementation of `IPackageResolver`. Currently there are 5 built-in implementations:

#### `LocalPackageResolver`

This implementation looks for files in the specified directory using a predefined pattern. Package versions are extracted from file names, e.g. file named `MyProject-v2.1.5.zip` corresponds to package version `2.1.5`.

#### `GithubPackageResolver`

This implementation looks for assets in releases of specified GitHub repository using a predefined pattern. Package versions are extracted from release names, e.g. release named `v1.0` corresponds to package version `1.0`.

Since .NET assemblies do not support semantic versions, pre-releases are ignored.

#### `WebPackageResolver`

This implementation requests a version manifest using the specified URL. The server is expected to respond with a plain-text list of package versions and their URLs, separated by space, one line per package. E.g.:
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

This implementation extracts files from zip-archived packages.

#### `NugetPackageExtractor`

This implementation extracts files from NuGet packages, from the specified root directory.

## Usage

### Basic usage example

The following code checks for updates and installs them if they are available, in a single operation.

```c#
// Configure to look for packages in specified directory and treat them as zips
using (var manager = new UpdateManager(
    new LocalPackageResolver("c:\\test\\packages", "*.zip"),
    new ZipPackageExtractor()))
{
    // Check for new version and, if available, perform full update and restart
    await manager.CheckPerformUpdateAsync();
}
```

### Handling intermediate steps manually

To provide users with the most optimal experience, you will probably want to handle intermediate steps manually.

```c#
// Check for updates
var result = await manager.CheckForUpdatesAsync();
if (result.CanUpdate)
{
    // Prepare an update by downloading and extracting the package
    // (supports progress reporting and cancellation)
    await manager.PrepareUpdateAsync(result.LastVersion);

    // Launch an executable that will apply the update
    // (can be instructed to restart the application afterwards)
    manager.LaunchUpdater(result.LastVersion);

    // Terminate the running application so that the updater can overwrite files
    Environment.Exit(0);
}
```

### Handling updates with multiple running instances of the application

To prevent conflicts when running multiple instances of the same application, only one instance of `UpdateManager` (across all processes) is able to prepare updates and launch the updater.

In order to correctly handle cases where multiple instances of the application may try to update at the same time, you need to catch these exceptions:

- `LockFileNotAcquiredException` - thrown by `PrepareUpdateAsync` and `LaunchUpdater` when this instance of `UpdateManager` cannot acquire a lock file. This means that another instance currently owns the lock file and is probably performing an update.
- `UpdaterAlreadyLaunchedException` - thrown by `PrepareUpdateAsync` and `LaunchUpdater` when an updater executable has already been launched, either by this instance of `UpdateManager` or another instance that has released the lock file.

The updater will wait until all instances of the application have exited before applying an update, regardless of which instance launched it.

## Etymology

The name "Onova" is derived from the Ukrainian word for "update" (noun).