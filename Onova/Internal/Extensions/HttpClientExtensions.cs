using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Onova.Internal.Extensions
{
    internal static class HttpClientExtensions
    {
        public static async Task<string> GetStringAsync(
            this HttpClient client,
            string requestUri,
            CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(
                requestUri,
                HttpCompletionOption.ResponseContentRead,
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<JsonElement> ReadAsJsonAsync(
            this HttpContent content,
            CancellationToken cancellationToken = default)
        {
            using var stream = await content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream, default, cancellationToken);

            return document.RootElement.Clone();
        }

        public static async Task<JsonElement> GetJsonAsync(
            this HttpClient client,
            string requestUri,
            CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(
                requestUri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsJsonAsync(cancellationToken);
        }

        public static async Task CopyToStreamAsync(
            this HttpContent content,
            Stream destination,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var length = content.Headers.ContentLength;
            using var source = await content.ReadAsStreamAsync();

            using var buffer = PooledBuffer.ForStream();

            var totalBytesCopied = 0L;
            int bytesCopied;
            do
            {
                bytesCopied = await source.CopyBufferedToAsync(destination, buffer.Array, cancellationToken);
                totalBytesCopied += bytesCopied;

                if (length != null)
                    progress?.Report(1.0 * totalBytesCopied / length.Value);
            } while (bytesCopied > 0);
        }
    }
}