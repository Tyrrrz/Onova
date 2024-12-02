using System;
using System.Net;
using System.Net.Http;

namespace Onova.Utils;

internal static class Http
{
    private static readonly Lazy<HttpClient> ClientLazy = new(() =>
    {
        var handler = new HttpClientHandler();

        if (handler.SupportsAutomaticDecompression)
            handler.AutomaticDecompression =
                DecompressionMethods.GZip | DecompressionMethods.Deflate;

        handler.UseCookies = false;

        var httpClient = new HttpClient(handler, true);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Onova (github.com/Tyrrrz/Onova)");

        return httpClient;
    });

    public static HttpClient Client => ClientLazy.Value;
}
