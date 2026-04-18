using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PowerKit.Extensions;

namespace Onova.Utils.Extensions;

internal static class HttpClientExtensions
{
    extension(HttpContent content)
    {
        public async Task CopyToStreamAsync(
            Stream destination,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default
        )
        {
            var length = content.Headers.ContentLength ?? -1;
            using var source = await content.ReadAsStreamAsync();

            await source.CopyToAsync(destination, length, progress, cancellationToken);
        }
    }
}
