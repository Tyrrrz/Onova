using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Onova.Updater.Utils;

internal static class PathEx
{
    private static readonly StringComparison PathStringComparison = RuntimeInformation.IsOSPlatform(
        OSPlatform.Windows
    )
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    public static string GetRelativePath(string basePath, string path)
    {
        var basePathSegments = basePath.Split(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        );
        var pathSegments = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        var commonSegmentsCount = 0;
        for (var i = 0; i < basePathSegments.Length && i < pathSegments.Length; i++)
        {
            if (!string.Equals(basePathSegments[i], pathSegments[i], PathStringComparison))
                break;

            commonSegmentsCount++;
        }

        return string.Join(
            Path.DirectorySeparatorChar.ToString(),
            pathSegments.Skip(commonSegmentsCount).ToArray()
        );
    }
}
