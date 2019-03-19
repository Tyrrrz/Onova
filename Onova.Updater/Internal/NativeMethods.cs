using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Onova.Updater.Internal
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, StringBuilder lpExeName, ref uint lpdwSize);
    }
}