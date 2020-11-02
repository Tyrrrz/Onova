using System;
using System.IO;
using System.Threading.Tasks;

namespace Onova.Updater.Internal
{
    internal static class FileEx
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

        public static async Task<bool> CheckWriteAccessAsync(string filePath)
        {
            while (!CheckWriteAccess(filePath))
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            return true;
        }
    }
}