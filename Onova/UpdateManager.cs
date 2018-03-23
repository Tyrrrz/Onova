using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Onova.Exceptions;
using Onova.Internal;
using Onova.Models;
using Onova.Services;

namespace Onova
{
    /// <summary>
    /// Entry point for handling application updates.
    /// </summary>
    public class UpdateManager : IUpdateManager
    {
        private const string UpdaterResourceName = "Onova.Updater.exe";

        private readonly AssemblyMetadata _updatee;
        private readonly IPackageResolver _resolver;
        private readonly IPackageExtractor _extractor;

        private readonly string _storageDirPath;
        private readonly string _updaterFilePath;

        private bool _updaterLaunched;

        /// <summary>
        /// Initializes an instance of <see cref="UpdateManager"/>.
        /// </summary>
        public UpdateManager(AssemblyMetadata updatee, IPackageResolver resolver, IPackageExtractor extractor)
        {
            _updatee = updatee.GuardNotNull(nameof(updatee));
            _resolver = resolver.GuardNotNull(nameof(resolver));
            _extractor = extractor.GuardNotNull(nameof(extractor));

            _storageDirPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Onova", _updatee.Name);

            _updaterFilePath = Path.Combine(_storageDirPath, $"{_updatee.Name}.Updater.exe");
        }

        /// <summary>
        /// Initializes an instance of <see cref="UpdateManager"/> on the entry assembly.
        /// </summary>
        public UpdateManager(IPackageResolver resolver, IPackageExtractor extractor)
            : this(new AssemblyMetadata(Assembly.GetEntryAssembly()), resolver, extractor)
        {
        }

        private string GetPackageFilePath(Version version) => Path.Combine(_storageDirPath, $"{version}.onv");

        private string GetPackageContentDirPath(Version version) => Path.Combine(_storageDirPath, $"{version}");

        /// <inheritdoc />
        public void Cleanup()
        {
            if (Directory.Exists(_storageDirPath))
                Directory.Delete(_storageDirPath, true);
        }

        /// <inheritdoc />
        [NotNull, ItemNotNull]
        public IReadOnlyList<Version> GetPreparedUpdates()
        {
            // Check if storage directory exists
            if (!Directory.Exists(_storageDirPath))
                return Array.Empty<Version>();

            // Enumerate directories in storage directory
            var versions = new HashSet<Version>();

            foreach (var dirPath in Directory.EnumerateDirectories(_storageDirPath))
            {
                var dirName = Path.GetFileName(dirPath);

                // Try to parse version
                if (!Version.TryParse(dirName, out var version))
                    continue;

                // Make sure the package file no longer exists
                // (if it exists along with unpacked directory, it could mean it wasn't unpacked properly)
                var packageFilePath = GetPackageFilePath(version);
                if (File.Exists(packageFilePath))
                    continue;

                // Add to list
                versions.Add(version);
            }

            return versions.ToArray();
        }

        /// <inheritdoc />
        [NotNull]
        public async Task<CheckForUpdatesResult> CheckForUpdatesAsync()
        {
            // Get versions
            var versions = await _resolver.GetVersionsAsync().ConfigureAwait(false);
            var lastVersion = versions.Max();
            var canUpdate = lastVersion != null && _updatee.Version < lastVersion;

            return new CheckForUpdatesResult(versions, lastVersion, canUpdate);
        }

        /// <inheritdoc />
        public async Task PrepareUpdateAsync(Version version,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            version.GuardNotNull(nameof(version));

            // Set up progress aggregator
            var progressAggregator = progress != null
                ? new ProgressAggregator(progress)
                : null;

            // Get paths
            var packageFilePath = GetPackageFilePath(version);
            var packageContentDirPath = GetPackageContentDirPath(version);

            // Create storage directory
            Directory.CreateDirectory(_storageDirPath);

            // Download package
            await _resolver.DownloadAsync(version, packageFilePath,
                progressAggregator?.Split(0.9), // 0% -> 90%
                cancellationToken).ConfigureAwait(false);

            // Create directory for package contents
            DirectoryEx.Reset(packageContentDirPath);

            // Extract package contents
            await _extractor.ExtractAsync(packageFilePath, packageContentDirPath,
                progressAggregator?.Split(0.1), // 90% -> 100%
                cancellationToken).ConfigureAwait(false);

            // Delete package
            File.Delete(packageFilePath);

            // Extract updater
            await Assembly.GetExecutingAssembly().CopyResourceAsync(UpdaterResourceName, _updaterFilePath)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task LaunchUpdaterAsync(Version version, bool restart = true)
        {
            version.GuardNotNull(nameof(version));

            // Find the package directory
            var packageContentDirPath = GetPackageContentDirPath(version);
            if (!Directory.Exists(packageContentDirPath))
                throw new UpdateNotPreparedException(version);

            // Ensure updater hasn't been launched yet
            if (_updaterLaunched)
                throw new InvalidOperationException("Updater has already been launched.");

            // Get current process ID
            var currentProcessId = ProcessEx.GetCurrentProcessId();

            // Prepare arguments
            var args = $"{currentProcessId} " +
                       $"\"{_updatee.FilePath}\" " +
                       $"\"{packageContentDirPath}\" " +
                       $"{restart}";

            // Decide if updater needs to be elevated
            var isElevated = !DirectoryEx.CheckWriteAccess(_updatee.DirectoryPath);

            // Launch the updater
            ProcessEx.StartCli(_updaterFilePath, args, isElevated);
            _updaterLaunched = true;

            // Wait a bit until it starts so that it can attach to our process ID
            await Task.Delay(333).ConfigureAwait(false);
        }
    }
}