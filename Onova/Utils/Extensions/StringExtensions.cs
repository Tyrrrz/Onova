using System;
using System.Text;

namespace Onova.Utils.Extensions;

internal static class StringExtensions
{
    extension(string str)
    {
        public string SubstringUntil(
            string sub,
            StringComparison comparison = StringComparison.Ordinal
        )
        {
            var index = str.IndexOf(sub, comparison);
            return index < 0 ? str : str[..index];
        }

        public string SubstringAfter(
            string sub,
            StringComparison comparison = StringComparison.Ordinal
        )
        {
            var index = str.IndexOf(sub, comparison);
            return index < 0 ? string.Empty : str[(index + sub.Length)..];
        }

        public byte[] GetBytes(Encoding encoding) => encoding.GetBytes(str);

        public byte[] GetBytes() => str.GetBytes(Encoding.UTF8);
    }

    extension(byte[] data)
    {
        public string ToBase64() => Convert.ToBase64String(data);
    }
}
