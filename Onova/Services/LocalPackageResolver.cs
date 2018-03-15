using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Onova.Exceptions;
using Onova.Internal;

namespace Onova.Services
{
    /// <summary>
    /// Resolves packages from a local repository.
    /// Package file names should contain package versions (e.g. "Package-v1.0.0.0.onv").
    /// </summary>
    public class LocalPackageResolver : IPackageResolver
    {
        private readonly string _repositoryDirPath;
        private readonly string _fileNamePattern;

        /// <summary>
        /// Initializes an instance of <see cref="LocalPackageResolver"/> on the given repository directory.
        /// </summary>
        public LocalPackageResolver(string repositoryDirPath, string fileNamePattern)
        {
            _repositoryDirPath = repositoryDirPath.GuardNotNull(nameof(repositoryDirPath));
            _fileNamePattern = fileNamePattern.GuardNotNull(nameof(fileNamePattern));
        }

        private IReadOnlyDictionary<Version, string> GetPackageVersionFilePathMap()
        {
            var map = new Dictionary<Version, string>();

            foreach (var filePath in Directory.EnumerateFiles(_repositoryDirPath, _fileNamePattern))
            {
                // Get name without extension
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

                // Try to parse version
                var versionText = Regex.Match(fileNameWithoutExt, "(\\d+\\.\\d+(?:\\.\\d+)?(?:\\.\\d+)?)").Groups[1]
                    .Value;
                if (!Version.TryParse(versionText, out var version))
                    continue;

                // Add to dictionary
                map[version] = filePath;
            }

            return map;
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<Version>> GetVersionsAsync()
        {
            var versions = GetPackageVersionFilePathMap().Keys.ToArray();
            return Task.FromResult((IReadOnlyList<Version>) versions);
        }

        /// <inheritdoc />
        public async Task DownloadAsync(Version version, string destFilePath,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            version.GuardNotNull(nameof(version));
            destFilePath.GuardNotNull(nameof(destFilePath));

            // Get map
            var map = GetPackageVersionFilePathMap();

            // Try to get package file path
            var sourceFilePath = map.GetOrDefault(version);
            if (sourceFilePath == null)
                throw new PackageNotFoundException(version);

            // Copy file
            using (var input = File.OpenRead(sourceFilePath))
            using (var output = File.Create(destFilePath))
                await input.CopyToAsync(output, progress, cancellationToken).ConfigureAwait(false);
        }
    }
}