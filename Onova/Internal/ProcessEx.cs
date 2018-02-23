using System.Diagnostics;

namespace Onova.Internal
{
    internal static class ProcessEx
    {
        public static int GetCurrentProcessId()
        {
            using (var process = Process.GetCurrentProcess())
                return process.Id;
        }

        public static void StartCli(string filePath, string args)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            var process = new Process {StartInfo = startInfo};

            using (process)
                process.Start();
        }
    }
}