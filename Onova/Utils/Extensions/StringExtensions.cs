using System;
using System.Text;

namespace Onova.Utils.Extensions;

internal static class StringExtensions
{
    extension(string s)
    {
        public string SubstringUntil(
            string sub,
            StringComparison comparison = StringComparison.Ordinal
        )
        {
            var index = s.IndexOf(sub, comparison);
            return index >= 0 ? s[..index] : s;
        }

        public string SubstringAfter(
            string sub,
            StringComparison comparison = StringComparison.Ordinal
        )
        {
            var index = s.IndexOf(sub, comparison);
            return index >= 0
                ? s[(index + sub.Length)..]
                : string.Empty;
        }

        public byte[] GetBytes(Encoding encoding) => encoding.GetBytes(s);

        public byte[] GetBytes() => s.GetBytes(Encoding.UTF8);
    }

    extension(byte[] data)
    {
        public string ToBase64() => Convert.ToBase64String(data);
    }
}
