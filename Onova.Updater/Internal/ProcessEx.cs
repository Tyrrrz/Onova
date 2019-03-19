using System.Diagnostics;
using System.Text;

namespace Onova.Updater.Internal
{
    internal static class ProcessEx
    {
        public static string GetFilePath(this Process process, int bufferSize = 1024)
        {
            var buffer = new StringBuilder(bufferSize);
            var charsWritten = (uint) bufferSize;

            return NativeMethods.QueryFullProcessImageName(process.Handle, 0, buffer, ref charsWritten)
                ? buffer.ToString()
                : null;
        }
    }
}