using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Onova.Updater.Internal;

namespace Onova.Updater
{
    // This executable applies the update by copying over new files.
    // It's required because updatee cannot update itself while the files are still in use.

    public static class Program
    {
        private static TextWriter _log;

        private static string AssemblyDirPath => AppDomain.CurrentDomain.BaseDirectory;
        private static string LogFilePath => Path.Combine(AssemblyDirPath, "Log.txt");
        private static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        private static void WriteLog(string value = null)
        {
            if (value.IsNotBlank())
            {
                var date = DateTimeOffset.Now;
                _log.WriteLine($"{date}: {value}");
            }
            else
            {
                _log.WriteLine();
            }
        }

        private static void Update(int updateeProcessId, string updateeFilePath,
            string packageContentDirPath, bool restartUpdatee)
        {
            // Get updatee directory path from file path
            var updateeDirPath = Path.GetDirectoryName(updateeFilePath);

            // Wait until updatee dies
            WriteLog("Waiting for updatee to exit...");
            ProcessEx.WaitForExit(updateeProcessId);

            // Copy over the extracted package
            WriteLog("Copying package contents...");
            DirectoryEx.Copy(packageContentDirPath, updateeDirPath);

            // Launch the updatee again if requested
            if (restartUpdatee)
            {
                WriteLog("Restarting updatee...");
                Process.Start(updateeFilePath);
            }

            // Delete package directory
            WriteLog("Deleting package contents...");
            Directory.Delete(packageContentDirPath, true);
        }

        public static void Main(string[] args)
        {
            // Write log
            using (_log = File.AppendText(LogFilePath))
            {
                // Launch info
                WriteLog($"Onova Updater v{Version} started with args: {string.Join(" ", args)}");

                try
                {
                    // Extract arguments
                    var updateeProcessId = int.Parse(args[0]);
                    var updateeFilePath = args[1];
                    var packageContentDirPath = args[2];
                    var restartUpdatee = bool.Parse(args[3]);

                    // Execute update
                    Update(updateeProcessId, updateeFilePath, packageContentDirPath, restartUpdatee);
                    WriteLog("Update completed successfully.");
                }
                catch (Exception ex)
                {
                    WriteLog(ex.ToString());
                }

                // White space to separate log entries
                WriteLog();
            }
        }
    }
}