using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PowerKit.Extensions;

namespace Onova.Utils.Extensions;

internal static class HttpClientExtensions
{
    extension(HttpContent content)
    {
        public async Task<JsonElement> ReadAsJsonAsync(
            CancellationToken cancellationToken = default
        )
        {
            using var stream = await content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream, default, cancellationToken);

            return document.RootElement.Clone();
        }

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

    extension(HttpClient client)
    {
        public async Task<JsonElement> GetJsonAsync(
            string requestUri,
            CancellationToken cancellationToken = default
        )
        {
            using var response = await client.GetAsync(
                requestUri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsJsonAsync(cancellationToken);
        }
    }
}
