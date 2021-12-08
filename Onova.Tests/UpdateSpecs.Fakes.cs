using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Onova.Services;

namespace Onova.Tests;

public partial class UpdateSpecs
{
    private class FakePackageResolver : IPackageResolver
    {
        private readonly IReadOnlyList<Version> _versions;

        public FakePackageResolver(IReadOnlyList<Version> versions)
        {
            _versions = versions;
        }

        public Task<IReadOnlyList<Version>> GetPackageVersionsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_versions);

        public Task DownloadPackageAsync(Version version, string destFilePath,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            File.WriteAllText(destFilePath, version.ToString());

            return Task.CompletedTask;
        }
    }

    private class FakePackageExtractor : IPackageExtractor
    {
        public Task ExtractPackageAsync(string sourceFilePath, string destDirPath,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            var sourceFileName = Path.GetFileName(sourceFilePath)!;
            File.Copy(sourceFilePath, Path.Combine(destDirPath, sourceFileName));

            return Task.CompletedTask;
        }
    }
}