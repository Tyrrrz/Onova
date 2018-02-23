using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Onova.Exceptions;
using Onova.Internal;

namespace Onova.Services
{
    /// <summary>
    /// Resolves packages from a local repository.
    /// Packages in the repository are expected to be named "1.0.0.0.onv" where "1.0.0.0" is the package version.
    /// </summary>
    public class LocalPackageResolver : IPackageResolver
    {
        private readonly string _repositoryDirPath;

        /// <summary>
        /// Initializes an instance of <see cref="LocalPackageResolver"/> on the given repository directory.
        /// </summary>
        public LocalPackageResolver(string repositoryDirPath)
        {
            _repositoryDirPath = repositoryDirPath;
        }

        private IReadOnlyDictionary<Version, string> GetPackageFiles()
        {
            var map = new Dictionary<Version, string>();

            foreach (var filePath in Directory.EnumerateFiles(_repositoryDirPath, "*.onv"))
            {
                var nameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

                // Must have parsable version as a name
                if (!Version.TryParse(nameWithoutExt, out var version))
                    continue;

                // Add to dictionary
                map[version] = filePath;
            }

            return map;
        }

        /// <inheritdoc />
        public Task<IEnumerable<Version>> GetAllVersionsAsync()
        {
            var versions = GetPackageFiles().Keys;
            return Task.FromResult(versions);
        }

        /// <inheritdoc />
        public Task<Stream> GetPackageAsync(Version version)
        {
            version.GuardNotNull(nameof(version));

            // Try to get package file path
            var filePath = GetPackageFiles().GetOrDefault(version);
            if (filePath != null)
                return Task.FromResult((Stream) File.OpenRead(filePath));

            throw new PackageNotFoundException(version);
        }
    }
}