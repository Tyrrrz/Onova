using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Onova.Services
{
    /// <summary>
    /// Provider for resolving packages.
    /// </summary>
    public interface IPackageResolver
    {
        /// <summary>
        /// Gets all available package versions.
        /// </summary>
        Task<IEnumerable<Version>> GetAllVersionsAsync();

        /// <summary>
        /// Gets a stream containing package of given version.
        /// </summary>
        Task<Stream> GetPackageAsync(Version version);
    }
}