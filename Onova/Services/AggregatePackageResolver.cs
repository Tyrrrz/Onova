using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Onova.Exceptions;
using Onova.Utils.Extensions;

namespace Onova.Services;

/// <summary>
/// Resolves packages using multiple other package resolvers.
/// </summary>
public class AggregatePackageResolver : IPackageResolver
{
    private readonly IReadOnlyList<IPackageResolver> _resolvers;

    /// <summary>
    /// Initializes an instance of <see cref="AggregatePackageResolver" />.
    /// </summary>
    public AggregatePackageResolver(IReadOnlyList<IPackageResolver> resolvers)
    {
        _resolvers = resolvers;
    }

    /// <summary>
    /// Initializes an instance of <see cref="AggregatePackageResolver" />.
    /// </summary>
    public AggregatePackageResolver(params IPackageResolver[] resolvers)
        : this((IReadOnlyList<IPackageResolver>) resolvers)
    {
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Version>> GetPackageVersionsAsync(CancellationToken cancellationToken = default)
    {
        var aggregateVersions = new HashSet<Version>();

        // Get unique package versions provided by all resolvers
        foreach (var resolver in _resolvers)
        {
            var versions = await resolver.GetPackageVersionsAsync(cancellationToken);
            aggregateVersions.AddRange(versions);
        }

        return aggregateVersions.ToArray();
    }

    private async Task<IPackageResolver?> TryGetResolverForPackageAsync(
        Version version,
        CancellationToken cancellationToken)
    {
        // Try to find the first resolver that has this package version
        foreach (var resolver in _resolvers)
        {
            var versions = await resolver.GetPackageVersionsAsync(cancellationToken);
            if (versions.Contains(version))
                return resolver;
        }

        // Return null if none of the resolvers provide this package version
        return null;
    }

    /// <inheritdoc />
    public async Task DownloadPackageAsync(Version version, string destFilePath,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        // Find a resolver that has this package version
        var resolver =
            await TryGetResolverForPackageAsync(version, cancellationToken) ??
            throw new PackageNotFoundException(version);

        // Download package
        await resolver.DownloadPackageAsync(version, destFilePath, progress, cancellationToken);
    }
}