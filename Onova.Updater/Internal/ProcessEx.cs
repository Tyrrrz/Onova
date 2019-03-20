using System.Diagnostics;
using System.Text;

namespace Onova.Updater.Internal
{
    internal static class ProcessEx
    {
        public static string GetFilePath(this Process process)
        {
            var buffer = new StringBuilder(1024);
            var charsWritten = (uint) buffer.Capacity;

            return NativeMethods.QueryFullProcessImageName(process.Handle, 0, buffer, ref charsWritten)
                ? buffer.ToString()
                : null;
        }
    }
}