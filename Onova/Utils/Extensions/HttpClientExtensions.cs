using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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
            var length = content.Headers.ContentLength;
            using var source = await content.ReadAsStreamAsync();

            using var buffer = PooledBuffer.ForStream();

            var totalBytesCopied = 0L;
            int bytesCopied;
            do
            {
                bytesCopied = await source.CopyBufferedToAsync(
                    destination,
                    buffer.Array,
                    cancellationToken
                );
                totalBytesCopied += bytesCopied;

                if (length != null)
                    progress?.Report(1.0 * totalBytesCopied / length.Value);
            } while (bytesCopied > 0);
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
