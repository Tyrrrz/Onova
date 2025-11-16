using System;
using System.Text;

namespace Onova.Updater.Utils.Extensions;

internal static class BinaryExtensions
{
    extension(byte[] data)
    {
        public string GetString(Encoding encoding) => encoding.GetString(data);

        public string GetString() => data.GetString(Encoding.UTF8);
    }

    extension(string input)
    {
        public byte[] FromBase64() => Convert.FromBase64String(input);
    }
}
