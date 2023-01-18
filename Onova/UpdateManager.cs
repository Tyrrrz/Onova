using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Onova.Exceptions;
using Onova.Models;
using Onova.Services;
using Onova.Utils;
using Onova.Utils.Extensions;

namespace Onova;

/// <summary>
/// Entry point for handling application updates.
/// </summary>
public class UpdateManager : IUpdateManager
{
    private const string UpdaterResourceName = "Onova.Updater.exe";

    private readonly IPackageResolver _resolver;
    private readonly IPackageExtractor _extractor;

    private readonly string _storageDirPath;
    private readonly string _updaterFilePath;
    private readonly string _lockFilePath;

    private LockFile? _lockFile;
    private bool _isDisposed;

    /// <inheritdoc />
    public AssemblyMetadata Updatee { get; }

    /// <summary>
    /// Initializes an instance of <see cref="UpdateManager" />.
    /// </summary>
    public UpdateManager(AssemblyMetadata updatee, IPackageResolver resolver, IPackageExtractor extractor)
    {
        Updatee = updatee;
        _resolver = resolver;
        _extractor = extractor;

        // Set storage directory path
        _storageDirPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Onova",
            updatee.Name
        );

        // Set updater executable file path
        _updaterFilePath = Path.Combine(_storageDirPath, $"{updatee.Name}.Updater.exe");

        // Set lock file path
        _lockFilePath = Path.Combine(_storageDirPath, "Onova.lock");
    }

    /// <summary>
    /// Initializes an instance of <see cref="UpdateManager" /> on the entry assembly.
    /// </summary>
    public UpdateManager(IPackageResolver resolver, IPackageExtractor extractor)
        : this(AssemblyMetadata.FromEntryAssembly(), resolver, extractor)
    {
    }

    private string GetPackageFilePath(Version version) => Path.Combine(_storageDirPath, $"{version}.onv");

    private string GetPackageContentDirPath(Version version) => Path.Combine(_storageDirPath, $"{version}");

    private void EnsureNotDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    private void EnsureLockFileAcquired()
    {
        // Ensure storage directory exists
        Directory.CreateDirectory(_storageDirPath);

        // Try to acquire lock file if it's not acquired yet
        _lockFile ??= LockFile.TryAcquire(_lockFilePath);

        // If failed to acquire - throw
        if (_lockFile == null)
            throw new LockFileNotAcquiredException();
    }

    private void EnsureUpdaterNotLaunched()
    {
        // Check whether we have write access to updater executable
        // (this is a reasonably accurate check for whether that process is running)
        if (File.Exists(_updaterFilePath) && !FileEx.CheckWriteAccess(_updaterFilePath))
            throw new UpdaterAlreadyLaunchedException();
    }

    private void EnsureUpdatePrepared(Version version)
    {
        if (!IsUpdatePrepared(version))
            throw new UpdateNotPreparedException(version);
    }

    /// <inheritdoc />
    public async Task<CheckForUpdatesResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        // Ensure that the current state is valid for this operation
        EnsureNotDisposed();

        // Get versions
        var versions = await _resolver.GetPackageVersionsAsync(cancellationToken);
        var lastVersion = versions.Max();
        var canUpdate = lastVersion != null && Updatee.Version < lastVersion;

        return new CheckForUpdatesResult(versions, lastVersion, canUpdate);
    }

    /// <inheritdoc />
    public bool IsUpdatePrepared(Version version)
    {
        // Ensure that the current state is valid for this operation
        EnsureNotDisposed();

        // Get package file path and content directory path
        var packageFilePath = GetPackageFilePath(version);
        var packageContentDirPath = GetPackageContentDirPath(version);

        // Package content directory should exist
        // Package file should have been deleted after extraction
        // Updater file should exist
        return !File.Exists(packageFilePath) &&
               Directory.Exists(packageContentDirPath) &&
               File.Exists(_updaterFilePath);
    }

    /// <inheritdoc />
    public IReadOnlyList<Version> GetPreparedUpdates()
    {
        // Ensure that the current state is valid for this operation
        EnsureNotDisposed();

        var result = new List<Version>();

        // Enumerate all immediate directories in storage
        if (Directory.Exists(_storageDirPath))
        {
            foreach (var packageContentDirPath in Directory.EnumerateDirectories(_storageDirPath))
            {
                // Get directory name
                var packageContentDirName = Path.GetFileName(packageContentDirPath);

                // Try to extract version out of the name
                if (string.IsNullOrWhiteSpace(packageContentDirName) ||
                    !Version.TryParse(packageContentDirName, out var version))
                {
                    continue;
                }

                // If this package is prepared - add it to the list
                if (IsUpdatePrepared(version))
                {
                    result.Add(version);
                }
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task PrepareUpdateAsync(Version version,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        // Ensure that the current state is valid for this operation
        EnsureNotDisposed();
        EnsureLockFileAcquired();
        EnsureUpdaterNotLaunched();

        // Set up progress mixer
        var progressMixer = progress != null
            ? new ProgressMixer(progress)
            : null;

        // Get package file path and content directory path
        var packageFilePath = GetPackageFilePath(version);
        var packageContentDirPath = GetPackageContentDirPath(version);

        // Ensure storage directory exists
        Directory.CreateDirectory(_storageDirPath);

        // Download package
        await _resolver.DownloadPackageAsync(version, packageFilePath,
            progressMixer?.Split(0.9), // 0% -> 90%
            cancellationToken
        );

        // Ensure package content directory exists and is empty
        DirectoryEx.Reset(packageContentDirPath);

        // Extract package contents
        await _extractor.ExtractPackageAsync(packageFilePath, packageContentDirPath,
            progressMixer?.Split(0.1), // 90% -> 100%
            cancellationToken
        );

        // Delete package
        File.Delete(packageFilePath);

        // Extract updater
        await Assembly.GetExecutingAssembly().ExtractManifestResourceAsync(UpdaterResourceName, _updaterFilePath);
    }

    /// <inheritdoc />
    public void LaunchUpdater(Version version, bool restart, string restartArguments)
    {
        // Ensure that the current state is valid for this operation
        EnsureNotDisposed();
        EnsureLockFileAcquired();
        EnsureUpdaterNotLaunched();
        EnsureUpdatePrepared(version);

        // Get package content directory path
        var packageContentDirPath = GetPackageContentDirPath(version);

        // Get original command line arguments and encode them to avoid issues with quotes
        var routedArgs = restartArguments.GetBytes().ToBase64();

        // Prepare arguments
        var updaterArgs = $"\"{Updatee.FilePath}\" \"{packageContentDirPath}\" \"{restart}\" \"{routedArgs}\"";

        // Decide if updater needs to be elevated
        var updateeDirPath = Path.GetDirectoryName(Updatee.FilePath);

        var updaterNeedsElevation =
            !string.IsNullOrWhiteSpace(updateeDirPath) &&
            !DirectoryEx.CheckWriteAccess(updateeDirPath);

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        // Create updater process start info
        var updaterStartInfo = new ProcessStartInfo
        {
            FileName = isWindows ? _updaterFilePath : "mono",
            Arguments = isWindows ? updaterArgs : $"\"{_updaterFilePath}\" {updaterArgs}",
            CreateNoWindow = true,
            UseShellExecute = false
        };

        // Configure framework rollover for the updater (allows a .NET 3.5 EXE to run against .NET 4.x CLR)
        // https://gist.github.com/MichalStrehovsky/d6bc5e4d459c23d0cf3bd17af9a1bcf5
        updaterStartInfo.Environment["COMPLUS_OnlyUseLatestCLR"] = "1";

        // If updater needs to be elevated - use shell execute with "runas"
        if (updaterNeedsElevation)
        {
            updaterStartInfo.Verb = "runas";
            updaterStartInfo.UseShellExecute = true;
        }

        // Create and start updater process
        var updaterProcess = new Process {StartInfo = updaterStartInfo};
        using (updaterProcess)
            updaterProcess.Start();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _lockFile?.Dispose();
        }
    }
}