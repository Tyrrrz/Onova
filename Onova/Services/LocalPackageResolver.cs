using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly string _searchPattern;

        /// <summary>
        /// Initializes an instance of <see cref="LocalPackageResolver"/> on the given repository directory.
        /// </summary>
        public LocalPackageResolver(string repositoryDirPath, string searchPattern = "*.onv")
        {
            _repositoryDirPath = repositoryDirPath.GuardNotNull(nameof(repositoryDirPath));
            _searchPattern = searchPattern.GuardNotNull(nameof(searchPattern));
        }

        private IReadOnlyDictionary<Version, string> GetPackageFilePathMap()
        {
            var map = new Dictionary<Version, string>();

            foreach (var filePath in Directory.EnumerateFiles(_repositoryDirPath, _searchPattern))
            {
                var nameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                var versionText = Regex.Match(nameWithoutExt, "(\\d+\\.\\d+(?:\\.\\d+)?(?:\\.\\d+)?)").Groups[1].Value;

                // Must have parsable version as a name
                if (!Version.TryParse(versionText, out var version))
                    continue;

                // Add to dictionary
                map[version] = filePath;
            }

            return map;
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<Version>> GetAllVersionsAsync()
        {
            var versions = GetPackageFilePathMap().Keys;
            return Task.FromResult((IReadOnlyList<Version>) versions.ToArray());
        }

        /// <inheritdoc />
        public Task<Stream> GetPackageAsync(Version version)
        {
            version.GuardNotNull(nameof(version));

            // Try to get package file path
            var filePath = GetPackageFilePathMap().GetOrDefault(version);
            if (filePath != null)
                return Task.FromResult((Stream) File.OpenRead(filePath));

            throw new PackageNotFoundException(version);
        }
    }
}