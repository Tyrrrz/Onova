using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using Onova.Updater.Utils;
using Onova.Updater.Utils.Extensions;

namespace Onova.Updater;

public class Updater(
    string updateeFilePath,
    string packageContentDirPath,
    bool restartUpdatee,
    string routedArgs
) : IDisposable
{
    private readonly TextWriter _log = File.CreateText(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log.txt")
    );

    private void WriteLog(string content)
    {
        var date = DateTimeOffset.Now.ToString(
            "dd-MMM-yyyy HH:mm:ss.fff",
            CultureInfo.InvariantCulture
        );

        var entry = $"{date}> {content}";

        Console.WriteLine(entry);

        _log.WriteLine(entry);
        _log.Flush();
    }

    private void WaitForUpdateeExit()
    {
        WriteLog("Waiting for updatee to exit...");

        for (var retriesRemaining = 15; retriesRemaining > 0; retriesRemaining--)
        {
            if (FileEx.CheckWriteAccess(updateeFilePath))
                return;

            Thread.Sleep(1000);
        }

        throw new TimeoutException("Updatee did not exit in time.");
    }

    private void ApplyUpdate()
    {
        WriteLog("Copying package contents from storage to the updatee directory...");
        Directory.Copy(packageContentDirPath, Path.GetDirectoryName(updateeFilePath)!);

        try
        {
            WriteLog("Deleting package contents from storage...");
            Directory.Delete(packageContentDirPath, true);
        }
        catch (Exception ex)
        {
            // Not a critical error
            WriteLog($"Failed to delete package contents from storage: {ex}");
        }
    }

    private void StartUpdatee()
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            WorkingDirectory = Path.GetDirectoryName(updateeFilePath),
            Arguments = routedArgs,
            // Don't let the child process inherit the current console window
            UseShellExecute = true,
        };

        // If the updatee is an .exe file, start it directly.
        // This covers self-contained .NET Core apps and legacy .NET Framework apps.
        if (
            string.Equals(
                Path.GetExtension(updateeFilePath),
                ".exe",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            process.StartInfo.FileName = updateeFilePath;
        }
        // Otherwise, locate the apphost by looking for the .exe file with the same name.
        // This covers framework-dependent .NET Core apps.
        else if (File.Exists(Path.ChangeExtension(updateeFilePath, ".exe")))
        {
            process.StartInfo.FileName = Path.ChangeExtension(updateeFilePath, ".exe");
        }
        // As a fallback, try to run the updatee through the .NET CLI
        else
        {
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = $"{updateeFilePath} {routedArgs}";
        }

        WriteLog(
            $"Restarting updatee [{process.StartInfo.FileName} {process.StartInfo.Arguments}]..."
        );
        process.Start();
        WriteLog($"Restarted with process ID: {process.Id}.");
    }

    public void Run()
    {
        try
        {
            var updaterVersion = Assembly.GetExecutingAssembly().GetName().Version;

            WriteLog(
                $"""
                Onova Updater v{updaterVersion} started with the following arguments:
                - UpdateeFilePath = {updateeFilePath}
                - PackageContentDirPath = {packageContentDirPath}
                - RestartUpdatee = {restartUpdatee}
                - RoutedArgs = {routedArgs}
                """
            );

            WaitForUpdateeExit();
            ApplyUpdate();

            if (restartUpdatee)
                StartUpdatee();

            WriteLog("Update completed successfully.");
        }
        catch (Exception ex)
        {
            WriteLog(ex.ToString());
        }
    }

    public void Dispose()
    {
        _log.Dispose();
    }
}
