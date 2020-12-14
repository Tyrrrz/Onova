// ReSharper disable CheckNamespace

// Polyfills to bridge the missing APIs in older versions of the framework/standard.

#if NETSTANDARD2_0 || NET461
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

internal static partial class PolyfillExtensions
{
    public static string[] Split(this string input, params string[] separators) =>
        input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
}

internal static partial class PolyfillExtensions
{
    public static async Task<int> ReadAsync(this Stream stream, byte[] buffer, CancellationToken cancellationToken) =>
        await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
}

namespace System.Collections.Generic
{
    internal static class PolyfillExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key) =>
            dic.TryGetValue(key!, out var result) ? result! : default!;
    }
}
#endif