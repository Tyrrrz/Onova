using System.Diagnostics;
using System.Linq;

namespace Onova.Updater.Internal
{
    internal static class ProcessHelper
    {
        public static void WaitForExit(int processId)
        {
            var process = Process.GetProcesses().FirstOrDefault(p => p.Id == processId);
            process?.WaitForExit();
        }
    }
}