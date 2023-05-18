# Changelog

## v2.6.8 (18-May-2023)

- Changed the way the updater executable applies the update. Now, instead of waiting for the updatee to exit and obtaining a lock on the entry assembly file, it obtains a lock on the entire directory. This should help prevent issues where multiple processes attempted to modify target files, resulting in a corrupted state.

## v2.6.7 (27-Apr-2023)

- Improved support for older target frameworks via polyfills.

## v2.6.6 (17-Feb-2023)

- Fixed an issue that prevented the updater executable from running with elevated privileges. Again.

## v2.6.5 (17-Feb-2023)

- Fixed an issue that prevented the updater executable from running with elevated privileges.

## v2.6.4 (16-Jan-2023)

- Fixed an issue that prevented from using Onova with single-file application distributions. (Thanks [@0xced](https://github.com/0xced))
- Removed support for .NET Framework 4.6.1 and replaced it with .NET Framework 4.6.2. This was necessary because some of the official packages that Onova relies on have also dropped support for .NET Framework 4.6.1.

## v2.6.3 (08-Dec-2022)

- Fixed an issue where the HTTP response wasn't checked for a successful status code in `GithubPackageResolver` and `WebPackageResolver`.
- Fixed an issue where the updater executable failed to run on outdated versions of Windows 7. The executable now targets net35 with framework rollover that ensures it can run on any build of Windows 7 and above.

## v2.6.2 (17-Sep-2020)

- Fixed an issue in `GithubPackageResolver` which prevented it from working properly with private repositories. (Thanks [@derech1e](https://github.com/derech1e))

## v2.6.1 (29-Jul-2020)

- Fixed an issue in `GithubPackageResolver` where the version was not extracted correctly if release name was not set, but tag name was. (Thanks [@miakh](https://github.com/miakh))
- Changed logging in `Onova.Updater` so that each new entry is flushed to file as soon as possible. This should help avoid empty logs after crashes.

## v2.6 (04-Apr-2020)

- Added an overload for `LaunchUpdater` that allows specifying custom command line arguments to use when restarting the application.
- Fixed an issue where the application was incorrectly restarted with a hidden window in case of a console application.
- Replaced Newtonsoft.Json dependency with System.Text.Json. This means that Onova doesn't have any external dependencies now, when used in a project targeting .NET Core 3.0 or higher.
- Extended support for cancellation to `CheckForUpdatesAsync` and some other methods.
- Supported .NET Framework version was bumped from v4.6 to v4.6.1.

Note, there were some very minor changes in interfaces `IUpdateManager` and `IPackageResolver`. If you were implementing them yourself, you will have to update to the new contract, but the changes should be trivial.

## v2.5.2 (02-Jan-2020)

- Added an overload for `AssemblyMetadata.FromAssembly` that also takes an assembly file path. This can be used to override default assembly file path in cases where the entry assembly is launched via a proxy.
- Added `IUpdateManager.Updatee` property to expose the updatee metadata for convenience. For example, it can be used to show current application version in the UI.
- Added nullable reference type annotations.
- Added source link.

## v2.5.1 (09-Oct-2019)

- Fixed `GetPreparedUpdates` throwing an exception when storage directory hasn't been created yet.

## v2.5 (07-Oct-2019)

- Added `GetPreparedUpdates` method that returns a list of versions for which an update has been prepared. Useful in certain auto-update scenarios, for example when the preparation happens during application lifetime, but the update is applied at startup.
- When restarting updatee, command line arguments are now routed from the application that initiated the update.

## v2.4.5 (18-Aug-2019)

- Fixed an issue where updatee wasn't restarted properly if it's a `.dll` file.

## v2.4.4 (08-Aug-2019)

- Added support for restarting .NET Core 3.0 apps. If the updatee is not an `.exe` file, it will try to find an `.exe` file with the same name. If it's not found, it will try to restart updatee via `dotnet`.

## v2.4.3 (15-Jun-2019)

- Renamed methods on `IPackageExtractor` and `IPackageResolver`.
- Fixed incorrect behavior in `ZipPackageExtractor` and `NugetPackageExtractor` when used with archives that contain subdirectories.
- Fixed an issue in `Onova.Updater` where files in subdirectories were copied to incorrect locations.
- Fixed an issue where `ZipPackageExtractor` threw an exception when the destination path is rooted.
- Improved exception messages.

## v2.4.2 (21-Mar-2019)

- Fixed an issue where launched instances of `Onova.Updater` weren't detected if they were started by a different process.
- Fixed an issue where `Onova.Updater` didn't properly wait until all updatee instances exit if one of those instances launched after the updater did.
- Log file now keeps track of only one session.

## v2.4.1 (20-Mar-2019)

- Lock file is now acquired on first call to `PrepareUpdateAsync` or `LaunchUpdater`.
- Removed `UpdateManager.Cleanup` due to conflicts with lock file.

## v2.4 (20-Mar-2019)

- Added .NET Standard 2.0 target with Windows-only support for Windows applications running on .NET Core.
- Added a lock file to ensure that only one instance of the application is able to download and install updates.
- `Onova.Updater` will now wait until all instances of the application exit, instead of just the one that launched it.

## v2.3 (14-Mar-2019)

- Added caching support to `GithubPackageResolver` using `If-None-Match` header.
- Fixed an issue where an internal stream didn't implement `ReadAsync` correctly.

## v2.2 (12-Sep-2018)

- Added support for relative package URLs in manifest for `WebPackageResolver`.
- Added configurable API base address in `GithubPackageResolver` which can be useful for on-premise hosted instances.
- Fixed exception messages not appearing in Visual Studio.

## v2.1 (31-Mar-2018)

- Improved logging in `Onova.Updater`.
- Fixed some issues with progress not being reported properly or at all.
- Removed `IHttpService`, `HttpService` in favor of using unwrapped `HttpClient`.
- Added default file name pattern to `LocalPackageResolver` which matches all files.
- `LocalPackageResolver` no longer throws an exception if the repository directory doesn't exist.
- `LaunchUpdater` is now a synchronous method.
- Added `IsUpdatePrepared` method that can be used to check if an upate to certain version has already been prepared.

## v2.0 (18-Mar-2018)

- Added `NugetPackageResolver` which resolves packages from a NuGet feed.
- Added `NugetPackageExtractor` which extracts NuGet packages.
- Added `WebPackageResolver` which resolves packages using a manifest provided over HTTP.
- Added `AggregatePackageResolver` which can be used to combine multiple other `IPackageResolver` instances.
- Asset names are now matched using a wildcard pattern in `GithubPackageResolver`, instead of a strictly predefined name.
- `LocalPackageResolver` now uses its own wildcard pattern matching for files, to avoid some inconsistent behavior provided by native Windows methods.
- Renamed most public API members to improve naming.
- Added `IUpdateManager` to aid in testing.
- Updates can no longer be applied more than once during a single runtime. Trying to launch the updater a second time will throw an exception.
- Implemented progress reporting for downloading and extracting packages.
- `CheckForUpdatesResult` now also contains the list of all available package versions, returned by the resolver.
- `CheckForUpdatesResult.LastVersion` is now `null` if the resolver does not provide packages of any version.
- `Onova.Updater` will now prompt for elevated privileges if it doesn't have write permissions to updatee's directory.
- `Onova.Updater` executable is now renamed according to updatee's name to be more recognizable when prompting user for elevated privileges, e.g. `MyProject.Updater.exe`.
- Fixed `Onova.Updater` not copying files that don't have extensions.
- Added logging to `Onova.Updater`. Log file is saved next to the executable.
- `HttpService.GetStreamAsync` extension method will now try to resolve content length from response headers.