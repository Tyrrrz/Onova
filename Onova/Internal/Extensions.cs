using System;
using System.Collections.Generic;

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

        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key,
            TValue defaultValue = default(TValue))
        {
            return dic.TryGetValue(key, out var result) ? result : defaultValue;
        }
    }
}