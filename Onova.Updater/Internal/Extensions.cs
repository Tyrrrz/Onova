using System;
using System.Collections.Generic;
using System.Text;

namespace Onova.Updater.Internal
{
    internal static class Extensions
    {
        public static string JoinToString<T>(this IEnumerable<T> source, string separator) =>
            string.Join(separator, source);

        public static string GetString(this byte[] data, Encoding encoding) => encoding.GetString(data);

        public static string GetString(this byte[] data) => data.GetString(Encoding.UTF8);

        public static byte[] FromBase64(this string input) => Convert.FromBase64String(input);
    }
}