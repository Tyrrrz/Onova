# Onova

[![Build](https://img.shields.io/appveyor/ci/Tyrrrz/Onova/master.svg)](https://ci.appveyor.com/project/Tyrrrz/Onova)
[![Tests](https://img.shields.io/appveyor/tests/Tyrrrz/Onova/master.svg)](https://ci.appveyor.com/project/Tyrrrz/Onova)
[![NuGet](https://img.shields.io/nuget/v/Onova.svg)](https://nuget.org/packages/Onova)
[![NuGet](https://img.shields.io/nuget/dt/Onova.svg)](https://nuget.org/packages/Onova)

Onova is a lightweight auto-update framework for desktop applications. It was primarily designed for updating portable applications that are distributed using archive files, but can be extended for other use cases. Updates downloaded by Onova are installed using an embedded external executable, by overwriting files when the application exits. The library requires minimal configuration, doesn't impose any changes to the CI/CD process, and doesn't affect the application's life cycle.

## Download

- [NuGet](https://nuget.org/packages/Onova): `Install-Package Onova`
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
- Extendable with custom resolvers and extractors
- Progress reporting and cancellation
- Update to any available version, not necessarily latest
- Overwrite files in-place using an external executable
- Automatically prompt for elevated privileges if necessary
- Fully self-contained
- Targets .NET Framework 4.6+ and .NET Standard 2.0 (Windows only)

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

### Updating with multiple running instances

To prevent conflicts when running multiple instances of the same application, only the instance that acquired a special lock file is allowed to perform an update. The lock file is acquired inside `UpdateManager`'s constructor and released when `UpdateManager` is disposed. If the lock file wasn't acquired, no exception will be thrown in the constructor itself but subsequent calls to some of the methods will throw an exception.

```c#
try
{
    await manager.PrepareUpdateAsync(...);
}
catch (LockFileNotAcquiredException)
{
    // Another instance of this application owns the lock file
}
```

## Libraries used

- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Moq](https://github.com/Moq/moq4)
- [NUnit](https://github.com/nunit/nunit)
- [CliWrap](https://github.com/Tyrrrz/CliWrap)
- [Mono.Cecil](https://github.com/jbevain/cecil)