### v2.2 (12-Sep-2018)

- Added support for relative package URLs in manifest for `WebPackageResolver`.
- Added configurable API base address in `GithubPackageResolver` which can be useful for on-premise hosted instances.
- Fixed exception messages not appearing in Visual Studio.

### v2.1 (31-Mar-2018)

- Improved logging in `Onova.Updater`.
- Fixed some issues with progress not being reported properly or at all.
- Removed `IHttpService`, `HttpService` in favor of using unwrapped `HttpClient`.
- Added default file name pattern to `LocalPackageResolver` which matches all files.
- `LocalPackageResolver` no longer throws an exception if the repository directory doesn't exist.
- `LaunchUpdater` is now a synchronous method.
- Added `IsUpdatePrepared` method that can be used to check if an upate to certain version has already been prepared.

### v2.0 (18-Mar-2018)

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