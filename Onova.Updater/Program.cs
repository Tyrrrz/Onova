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
        private static Mutex _mutex;

        private static string AssemblyDirPath => AppDomain.CurrentDomain.BaseDirectory;
        private static string LogFilePath => Path.Combine(AssemblyDirPath, "Log.txt");
        private static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        private static void Update(int updateeProcessId, string updateeFilePath,
            string packageContentDirPath, bool restartUpdatee)
        {
            // Get updatee directory path from file path
            var updateeDirPath = Path.GetDirectoryName(updateeFilePath);

            // Wait until updatee dies
            _log.WriteLine("Waiting for updatee to exit...");
            ProcessEx.WaitForExit(updateeProcessId);

            // Copy over the extracted package
            _log.WriteLine("Copying package contents...");
            DirectoryEx.Copy(packageContentDirPath, updateeDirPath);

            // Launch the updatee again if requested
            if (restartUpdatee)
            {
                _log.WriteLine("Restarting updatee...");
                Process.Start(updateeFilePath);
            }

            // Delete package directory
            _log.WriteLine("Deleting package contents...");
            Directory.Delete(packageContentDirPath, true);
        }

        public static void Main(string[] args)
        {
            // Write log
            using (_log = File.AppendText(LogFilePath))
            {
                _log.WriteLine($"Onova Updater v{Version} started on {DateTimeOffset.Now}");
                _log.WriteLine($"Executed with args: {string.Join(" ", args)}");

                try
                {
                    // Extract arguments
                    var updateeProcessId = int.Parse(args[0]);
                    var updateeFilePath = args[1];
                    var packageContentDirPath = args[2];
                    var restartUpdatee = bool.Parse(args[3]);

                    // Use mutex to synchronize with other updaters
                    _log.WriteLine("Acquiring mutex...");
                    _mutex = new Mutex(false, $"Onova-{updateeProcessId}", out var isMutexOwner);

                    // Exit if the mutex is owned by other process
                    if (!isMutexOwner)
                    {
                        _log.Write("Mutex is owned by someone else. Exiting.");
                        return;
                    }

                    Update(updateeProcessId, updateeFilePath, packageContentDirPath, restartUpdatee);
                    _log.WriteLine("Update completed successfully");
                }
                catch (Exception ex)
                {
                    _log.WriteLine(ex.ToString());
                }
                finally
                {
                    _mutex?.ReleaseMutex();
                    _log.WriteLine();
                }
            }
        }
    }
}