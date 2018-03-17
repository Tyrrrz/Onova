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
        public static bool IsBlank(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static bool IsNotBlank(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        public static string SubstringUntil(this string str, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = str.IndexOf(sub, comparison);
            return index < 0 ? str : str.Substring(0, index);
        }

        public static string SubstringAfter(this string str, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = str.IndexOf(sub, comparison);
            return index < 0 ? string.Empty : str.Substring(index + sub.Length, str.Length - index - sub.Length);
        }

        public static string[] Split(this string input, params string[] separators)
        {
            return input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        public static int AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> sequence)
        {
            return sequence.Count(hashSet.Add);
        }

        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key,
            TValue defaultValue = default(TValue))
        {
            return dic.TryGetValue(key, out var result) ? result : defaultValue;
        }

        public static async Task<int> CopyChunkToAsync(this Stream source, Stream destination,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var buffer = new byte[81920];

            // Read
            var bytesCopied = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);

            // Write
            await destination.WriteAsync(buffer, 0, bytesCopied, cancellationToken).ConfigureAwait(false);

            return bytesCopied;
        }

        public static async Task CopyToAsync(this Stream source, Stream destination,
            IProgress<double> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var totalBytesCopied = 0L;
            int bytesCopied;
            do
            {
                // Copy
                bytesCopied = await source.CopyChunkToAsync(destination, cancellationToken)
                    .ConfigureAwait(false);

                // Report progress
                totalBytesCopied += bytesCopied;
                progress?.Report(1.0 * bytesCopied / totalBytesCopied);
            } while (bytesCopied > 0);
        }

        public static async Task CopyResourceAsync(this Assembly assembly, string resourceName, string destFilePath)
        {
            var input = assembly.GetManifestResourceStream(resourceName);
            if (input == null)
                throw new MissingManifestResourceException($"Could not find resource [{resourceName}].");

            using (var output = File.Create(destFilePath))
                await input.CopyToAsync(output).ConfigureAwait(false);
        }
    }
}