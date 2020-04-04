using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Onova.Internal
{
    internal static class HttpClientEx
    {
        private static HttpClient? _singleton;

        public static HttpClient GetSingleton()
        {
            // Return cached singleton if already initialized
            if (_singleton != null)
                return _singleton;

            // Configure handler
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            handler.UseCookies = false;

            // Configure client
            var client = new HttpClient(handler, true);
            client.DefaultRequestHeaders.Add("User-Agent", "Onova (github.com/Tyrrrz/Onova)");

            return _singleton = client;
        }

        public static async Task<string> GetStringAsync(this HttpClient client, string requestUri,
            CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseContentRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<JsonDocument> ReadAsJsonAsync(this HttpContent content,
            CancellationToken cancellationToken = default)
        {
            using var stream = await content.ReadAsStreamAsync();
            return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        }

        public static async Task<JsonDocument> GetJsonAsync(this HttpClient client, string requestUri,
            CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsJsonAsync(cancellationToken);
        }

        public static async Task CopyToStreamAsync(this HttpContent content, Stream destination,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            using var source = await content.ReadAsStreamAsync();

            var length = content.Headers.ContentLength;

            var buffer = new byte[81920];
            var totalBytesCopied = 0L;
            int bytesCopied;
            do
            {
                // Copy
                bytesCopied = await source.CopyBufferedToAsync(destination, buffer, cancellationToken);

                // Report progress
                totalBytesCopied += bytesCopied;

                if (length != null)
                    progress?.Report(1.0 * totalBytesCopied / length.Value);
            } while (bytesCopied > 0);
        }

        public static async Task GetStreamAndCopyToAsync(this HttpClient client, string requestUri, Stream destination,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            await response.Content.CopyToStreamAsync(destination, progress, cancellationToken);
        }
    }
}