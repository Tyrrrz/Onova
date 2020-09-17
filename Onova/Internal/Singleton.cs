using System;
using System.Net;
using System.Net.Http;

namespace Onova.Internal
{
    internal static class Singleton
    {
        private static readonly Lazy<HttpClient> LazyHttpClient = new Lazy<HttpClient>(() =>
        {
            var handler = new HttpClientHandler();

            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            handler.UseCookies = false;

            var httpClient = new HttpClient(handler, true);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Onova (github.com/Tyrrrz/Onova)");

            return httpClient;
        });

        public static HttpClient HttpClient => LazyHttpClient.Value;
    }
}