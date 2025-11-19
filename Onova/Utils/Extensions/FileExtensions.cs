using System;
using System.IO;

namespace Onova.Utils.Extensions;

internal static class FileExtensions
{
    extension(File)
    {
        public static bool CheckWriteAccess(string filePath)
        {
            try
            {
                File.Open(filePath, FileMode.Open, FileAccess.Write).Dispose();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
