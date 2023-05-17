using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Onova.Utils.Extensions;

internal static class StreamExtensions
{
    public static async Task<int> CopyBufferedToAsync(
        this Stream source,
        Stream destination,
        byte[] buffer,
        CancellationToken cancellationToken = default)
    {
        var bytesCopied = await source.ReadAsync(buffer, cancellationToken);
        await destination.WriteAsync(buffer, 0, bytesCopied, cancellationToken);

        return bytesCopied;
    }

    public static async Task CopyToAsync(
        this Stream source,
        Stream destination,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using var buffer = PooledBuffer.ForStream();

        var totalBytesCopied = 0L;
        while (true)
        {
            var bytesCopied = await source.CopyBufferedToAsync(destination, buffer.Array, cancellationToken);
            if (bytesCopied <= 0)
                break;

            totalBytesCopied += bytesCopied;

            progress?.Report(1.0 * totalBytesCopied / source.Length);
        }
    }
}