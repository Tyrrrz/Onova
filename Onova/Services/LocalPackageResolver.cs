using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Onova.Exceptions;
using Onova.Utils;
using Onova.Utils.Extensions;

namespace Onova.Services;

/// <summary>
/// Resolves packages from a local repository.
/// Package file names should contain package versions (e.g. "MyProject-v1.8.3.onv").
/// </summary>
public class LocalPackageResolver : IPackageResolver
{
    private readonly string _repositoryDirPath;
    private readonly string _fileNamePattern;

    /// <summary>
    /// Initializes an instance of <see cref="LocalPackageResolver" />.
    /// </summary>
    public LocalPackageResolver(string repositoryDirPath, string fileNamePattern = "*")
    {
        _repositoryDirPath = repositoryDirPath;
        _fileNamePattern = fileNamePattern;
    }

    private IReadOnlyDictionary<Version, string> GetPackageVersionFilePathMap()
    {
        var map = new Dictionary<Version, string>();

        // Check if repository directory exists
        if (!Directory.Exists(_repositoryDirPath))
            return map;

        // Enumerate files in repository directory
        foreach (var filePath in Directory.EnumerateFiles(_repositoryDirPath))
        {
            // See if the name matches
            var fileName = Path.GetFileName(filePath);
            if (!WildcardPattern.IsMatch(fileName, _fileNamePattern))
                continue;

            // Try to parse version
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath) ?? "";
            var versionText = Regex
                .Match(fileNameWithoutExt, "(\\d+\\.\\d+(?:\\.\\d+)?(?:\\.\\d+)?)")
                .Groups[1]
                .Value;
            if (!Version.TryParse(versionText, out var version))
                continue;

            // Add to dictionary
            map[version] = filePath;
        }

        return map;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Version>> GetPackageVersionsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var versions = GetPackageVersionFilePathMap().Keys.ToArray();
        return Task.FromResult((IReadOnlyList<Version>)versions);
    }

    /// <inheritdoc />
    public async Task DownloadPackageAsync(
        Version version,
        string destFilePath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        var map = GetPackageVersionFilePathMap();

        var sourceFilePath = map.GetValueOrDefault(version);
        if (string.IsNullOrWhiteSpace(sourceFilePath))
            throw new PackageNotFoundException(version);

        using var input = File.OpenRead(sourceFilePath);
        using var output = File.Create(destFilePath);

        await input.CopyToAsync(output, progress, cancellationToken);
    }
}
