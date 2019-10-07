using System.Collections.Generic;

namespace Onova.Tests.Dummy.Internal
{
    internal static class Extensions
    {
        public static string JoinToString<T>(this IEnumerable<T> source, string separator) =>
            string.Join(separator, source);
    }
}