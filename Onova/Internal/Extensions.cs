using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;

namespace Onova.Internal
{
    internal static class Extensions
    {
        public static bool IsNullOrWhiteSpace(this string s) => string.IsNullOrWhiteSpace(s);

        public static string SubstringUntil(this string s, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.IndexOf(sub, comparison);
            return index < 0 ? s : s.Substring(0, index);
        }

        public static string SubstringAfter(this string s, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.IndexOf(sub, comparison);
            return index < 0 ? string.Empty : s.Substring(index + sub.Length, s.Length - index - sub.Length);
        }

        public static string[] Split(this string input, params string[] separators) =>
            input.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        public static int AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> sequence) => sequence.Count(hashSet.Add);

        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key) =>
            dic.TryGetValue(key, out var result) ? result : default;

        public static async Task<int> CopyChunkToAsync(this Stream source, Stream destination, byte[] buffer,
            CancellationToken cancellationToken = default)
        {
            // Read
            var bytesCopied = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

            // Write
            await destination.WriteAsync(buffer, 0, bytesCopied, cancellationToken);

            return bytesCopied;
        }

        public static async Task CopyToAsync(this Stream source, Stream destination,
            IProgress<double> progress = null, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[81920];
            var totalBytesCopied = 0L;
            int bytesCopied;
            do
            {
                // Copy
                bytesCopied = await source.CopyChunkToAsync(destination, buffer, cancellationToken);

                // Report progress
                totalBytesCopied += bytesCopied;
                progress?.Report(1.0 * totalBytesCopied / source.Length);
            } while (bytesCopied > 0);
        }

        public static async Task ExtractManifestResourceAsync(this Assembly assembly, string resourceName,
            string destFilePath)
        {
            var input = assembly.GetManifestResourceStream(resourceName);
            if (input == null)
                throw new MissingManifestResourceException($"Could not find resource [{resourceName}].");

            using (var output = File.Create(destFilePath))
                await input.CopyToAsync(output);
        }
    }
}