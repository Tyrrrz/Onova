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

        public static void StartCli(string filePath, string args, bool elevated)
        {
            // Start info
            var startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            // Elevated verb
            if (elevated)
            {
                startInfo.Verb = "runas";
                startInfo.UseShellExecute = true;
            }

            // Create process
            var process = new Process {StartInfo = startInfo};

            // Start process
            using (process)
                process.Start();
        }
    }
}