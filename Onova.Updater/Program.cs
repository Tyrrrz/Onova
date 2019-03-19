using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        private static void Update(string updateeFilePath, string packageContentDirPath, bool restartUpdatee)
        {

            // Get running updatee instances
            WriteLog("Looking for running updatee instances...");
            var updateeProcessName = Path.GetFileNameWithoutExtension(updateeFilePath);
            var updateeProcesses = Process.GetProcessesByName(updateeProcessName)
                .Where(p => PathEx.AreEqual(p.GetFilePath(), updateeFilePath))
                .ToArray();

            // Wait until all updatee instances exit
            if (updateeProcesses.Any())
            {
                foreach (var updateeProcess in updateeProcesses)
                {
                    WriteLog($"Waiting for pid:{updateeProcess.Id} to exit...");

                    using (updateeProcess)
                        updateeProcess.WaitForExit();
                }
            }
            else
            {
                WriteLog("There are no running updatee instances.");
            }

            // Copy over the extracted package
            WriteLog("Copying package contents from storage to updatee's directory...");
            var updateeDirPath = Path.GetDirectoryName(updateeFilePath);
            DirectoryEx.Copy(packageContentDirPath, updateeDirPath);

            // Launch the updatee again if requested
            if (restartUpdatee)
            {
                WriteLog("Restarting updatee...");
                Process.Start(updateeFilePath);
            }

            // Delete package directory
            WriteLog("Deleting package contents from storage...");
            Directory.Delete(packageContentDirPath, true);
        }

        public static void Main(string[] args)
        {
            // Write log
            using (_log = File.AppendText(LogFilePath))
            {
                // Launch info
                WriteLog($"Onova Updater v{Version} started with args: {args.JoinToString(" ")}");

                try
                {
                    // Extract arguments
                    var updateeFilePath = args[0];
                    var packageContentDirPath = args[1];
                    var restartUpdatee = bool.Parse(args[2]);

                    // Execute update
                    Update(updateeFilePath, packageContentDirPath, restartUpdatee);
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