using System;
using System.Text;

namespace Onova.Updater.Utils.Extensions;

internal static class BinaryExtensions
{
    public static string GetString(this byte[] data, Encoding encoding) => encoding.GetString(data);

    public static string GetString(this byte[] data) => data.GetString(Encoding.UTF8);

    public static byte[] FromBase64(this string input) => Convert.FromBase64String(input);
}
