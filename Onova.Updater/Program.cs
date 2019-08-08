using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
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

        private static void WriteLog(string value)
        {
            var date = DateTimeOffset.Now;
            _log.WriteLine($"{date:dd-MMM-yyyy HH:mm:ss.fff}> {value}");
        }

        private static void Update(string updateeFilePath, string packageContentDirPath, bool restartUpdatee)
        {
            var updateeDirPath = Path.GetDirectoryName(updateeFilePath);

            // Wait until updatee is writable to ensure all running instances have exited
            WriteLog("Waiting for all running updatee instances to exit...");
            while (!FileEx.CheckWriteAccess(updateeFilePath))
                Thread.Sleep(100);

            // Copy over the package contents
            WriteLog("Copying package contents from storage to updatee's directory...");
            DirectoryEx.Copy(packageContentDirPath, updateeDirPath);

            // Restart updatee if requested
            if (restartUpdatee)
            {
                WriteLog("Restarting updatee...");

                var startInfo = new ProcessStartInfo
                {
                    WorkingDirectory = updateeDirPath
                };

                // If updatee is an .exe file - start it directly
                if (string.Equals(Path.GetExtension(updateeFilePath), ".exe", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.FileName = updateeFilePath;
                }
                // If not - figure out what to do with it
                else
                {
                    // If there's an .exe file with same name - start it instead
                    // Security vulnerability?
                    if (File.Exists(Path.ChangeExtension(updateeFilePath, ".exe")))
                    {
                        startInfo.FileName = Path.ChangeExtension(updateeFilePath, ".exe");
                    }
                    // Otherwise - start the updatee using dotnet SDK
                    else
                    {
                        startInfo.FileName = "dotnet";
                        startInfo.Arguments = updateeFilePath;
                    }
                }

                using (var restartedUpdateeProcess = Process.Start(updateeFilePath))
                    WriteLog($"Restarted as pid:{restartedUpdateeProcess?.Id}.");
            }

            // Delete package content directory
            WriteLog("Deleting package contents from storage...");
            Directory.Delete(packageContentDirPath, true);
        }

        public static void Main(string[] args)
        {
            // Write log
            using (_log = File.CreateText(LogFilePath))
            {
                // Launch info
                WriteLog($"Onova Updater v{Version} started with args: [{args.JoinToString(", ")}].");

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
            }
        }
    }
}