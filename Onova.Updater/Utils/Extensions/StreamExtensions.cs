using System.IO;

namespace Onova.Updater.Utils.Extensions;

internal static class StreamExtensions
{
    // TODO: replace by a polyfill
    public static void CopyTo(this Stream source, Stream destination)
    {
        var buffer = new byte[16 * 1024];
        int read;

        while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            destination.Write(buffer, 0, read);
    }
}