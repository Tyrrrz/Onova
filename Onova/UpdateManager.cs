using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
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
    public class UpdateManager
    {
        private readonly AssemblyMetadata _updatee;
        private readonly IPackageResolver _resolver;
        private readonly IPackageExtractor _extractor;

        private readonly string _storageDirPath;

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
                "Onova",
                _updatee.Name);
        }

        /// <summary>
        /// Initializes an instance of <see cref="UpdateManager"/> on the entry assembly.
        /// </summary>
        public UpdateManager(IPackageResolver resolver, IPackageExtractor extractor)
            : this(new AssemblyMetadata(Assembly.GetEntryAssembly()), resolver, extractor)
        {
        }

        /// <summary>
        /// Deletes all prepared packages and temporary files.
        /// </summary>
        public void Cleanup()
        {
            if (Directory.Exists(_storageDirPath))
                Directory.Delete(_storageDirPath, true);
        }

        /// <summary>
        /// Checks for updates.
        /// </summary>
        [NotNull]
        public async Task<CheckForUpdatesResult> CheckForUpdatesAsync()
        {
            // Get all available versions
            var versions = await _resolver.GetAllVersionsAsync().ConfigureAwait(false);

            // Find the latest
            var lastVersion = versions.Max() ?? _updatee.Version;
            var canUpdate = _updatee.Version < lastVersion;

            return new CheckForUpdatesResult(lastVersion, canUpdate);
        }

        private async Task CopyPackageToStorageAsync(Version version)
        {
            // Create storage directory
            Directory.CreateDirectory(_storageDirPath);

            // Get path
            var packageFilePath = Path.Combine(_storageDirPath, $"{version}.onv");

            // Copy
            using (var input = await _resolver.GetPackageAsync(version).ConfigureAwait(false))
            using (var output = File.Create(packageFilePath))
                await input.CopyToAsync(output).ConfigureAwait(false);
        }

        private async Task ExtractPackageToStorageAsync(Version version)
        {
            // Get paths
            var packageFilePath = Path.Combine(_storageDirPath, $"{version}.onv");
            var packageContentDirPath = Path.Combine(_storageDirPath, $"{version}");

            // (Re)create directory
            if (Directory.Exists(packageContentDirPath))
                Directory.Delete(packageContentDirPath, true);
            Directory.CreateDirectory(packageContentDirPath);

            // Extract
            await _extractor.ExtractPackageAsync(packageFilePath, packageContentDirPath).ConfigureAwait(false);

            // Delete package
            File.Delete(packageFilePath);
        }

        private async Task CopyUpdaterToStorageAsync()
        {
            // Get the resource containing updater executable
            var input = Assembly.GetExecutingAssembly().GetManifestResourceStream("Onova.Updater.exe");
            if (input == null)
                throw new MissingManifestResourceException("Updater resource is missing.");

            // Get path
            var updaterFilePath = Path.Combine(_storageDirPath, "Onova.exe");

            // Copy
            using (input)
            using (var output = File.Create(updaterFilePath))
                await input.CopyToAsync(output).ConfigureAwait(false);
        }

        /// <summary>
        /// Prepares a package of given version.
        /// </summary>
        public async Task PreparePackageAsync(Version version)
        {
            version.GuardNotNull(nameof(version));

            // Copy package to storage directory
            await CopyPackageToStorageAsync(version).ConfigureAwait(false);

            // Extract package to package directory
            await ExtractPackageToStorageAsync(version).ConfigureAwait(false);

            // Copy the current version of updater to storage directory
            await CopyUpdaterToStorageAsync().ConfigureAwait(false);
        }

        private async Task LaunchUpdaterAsync(string packageContentDirPath, bool restart)
        {
            // Get current process id
            var currentProcessId = ProcessEx.GetCurrentProcessId();
            
            // Check if updater has already been started
            if (Mutex.TryOpenExisting($"Onova-{currentProcessId}", out _))
                throw new InvalidOperationException("Updater has already been launched.");

            // Prepare arguments
            var updaterArgs = $"{currentProcessId} " +
                              $"\"{_updatee.FilePath}\" " +
                              $"\"{packageContentDirPath}\" " +
                              $"{restart}";

            // Launch the updater
            var updaterFilePath = Path.Combine(_storageDirPath, "Onova.exe");
            ProcessEx.StartCli(updaterFilePath, updaterArgs);

            // Wait a bit until it starts so that it can attach to our process id
            await Task.Delay(333).ConfigureAwait(false);
        }

        /// <summary>
        /// Enqueues an update to prepared package of given version, which will execute when the process exits.
        /// </summary>
        public async Task EnqueueApplyPackageAsync(Version version, bool restart = true)
        {
            version.GuardNotNull(nameof(version));

            // Find the package directory
            var packageContentDirPath = Path.Combine(_storageDirPath, $"{version}");
            if (!Directory.Exists(packageContentDirPath))
                throw new PackageNotPreparedException(version);

            // Launch the updater
            await LaunchUpdaterAsync(packageContentDirPath, restart).ConfigureAwait(false);
        }

        /// <summary>
        /// Exits current process and applies a prepared package of given version.
        /// </summary>
        public async Task ApplyPackageAsync(Version version, bool restart = true)
        {
            version.GuardNotNull(nameof(version));

            await EnqueueApplyPackageAsync(version, restart).ConfigureAwait(false);
            Environment.Exit(0);
        }

        /// <summary>
        /// Checks for updates and updates to newest version if available.
        /// </summary>
        public async Task PerformUpdateIfAvailableAsync(bool restart = true)
        {
            // Check for updates
            var checkForUpdatesResult = await CheckForUpdatesAsync().ConfigureAwait(false);
            if (!checkForUpdatesResult.CanUpdate)
                return;

            // Prepare and apply package
            await PreparePackageAsync(checkForUpdatesResult.LastVersion).ConfigureAwait(false);
            await ApplyPackageAsync(checkForUpdatesResult.LastVersion, restart).ConfigureAwait(false);
        }
    }
}