using System.IO;

namespace Onova.Updater.Internal
{
    internal static class PathEx
    {
        public static bool AreEqual(string path1, string path2)
        {
            var fullPath1 = Path.GetFullPath(path1);
            var fullPath2 = Path.GetFullPath(path2);

            return fullPath1 == fullPath2;
        }
    }
}