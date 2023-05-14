using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using Onova.Updater.Utils;

namespace Onova.Updater;

public class Updater : IDisposable
{
    private readonly string _updateeFilePath;
    private readonly string _packageContentDirPath;
    private readonly bool _restartUpdatee;
    private readonly string _routedArgs;

    private readonly TextWriter _log = File.CreateText(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log.txt")
    );

    private IDisposable? _updateeDirLock;

    public Updater(
        string updateeFilePath,
        string packageContentDirPath,
        bool restartUpdatee,
        string routedArgs)
    {
        _updateeFilePath = updateeFilePath;
        _packageContentDirPath = packageContentDirPath;
        _restartUpdatee = restartUpdatee;
        _routedArgs = routedArgs;
    }

    private void WriteLog(string content)
    {
        var date = DateTimeOffset.Now.ToString("dd-MMM-yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);
        _log.WriteLine($"{date}> {content}");
        _log.Flush();
    }

    private void AcquireLock()
    {
        WriteLog("Locking the updatee directory...");
        for (var retriesRemaining = 10;; retriesRemaining--)
        {
            try
            {
                _updateeDirLock = DirectoryEx.Lock(Path.GetDirectoryName(_updateeFilePath)!);
                break;
            }
            catch (Win32Exception) when (retriesRemaining > 0)
            {
                Thread.Sleep(1000);
            }
        }
    }

    private void ApplyUpdate()
    {
        WriteLog("Copying package contents from storage to the updatee directory...");
        DirectoryEx.Copy(_packageContentDirPath, Path.GetDirectoryName(_updateeFilePath)!);

        try
        {
            WriteLog("Deleting package contents from storage...");
            Directory.Delete(_packageContentDirPath, true);
        }
        catch (Exception ex)
        {
            // Not a critical error
            WriteLog($"Failed to delete package contents from storage: {ex}");
        }
    }

    private void StartUpdatee()
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(_updateeFilePath),
                Arguments = _routedArgs,
                // Don't let the child process inherited the current console window
                UseShellExecute = true
            }
        };

        // If the updatee is an .exe file, start it directly.
        // This covers self-contained .NET Core apps and legacy .NET Framework apps.
        if (string.Equals(Path.GetExtension(_updateeFilePath), ".exe", StringComparison.OrdinalIgnoreCase))
        {
            process.StartInfo.FileName = _updateeFilePath;
        }
        // Otherwise, locate the apphost by looking for the .exe file with same name.
        // This covers framework-dependent .NET Core apps.
        else if (File.Exists(Path.ChangeExtension(_updateeFilePath, ".exe")))
        {
            process.StartInfo.FileName = Path.ChangeExtension(_updateeFilePath, ".exe");
        }
        // As fallback, try to run the updatee through the .NET CLI
        {
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = $"{_updateeFilePath} {_routedArgs}";
        }

        WriteLog($"Restarting updatee [{process.StartInfo.FileName} {process.StartInfo.Arguments}]...");
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
                - UpdateeFilePath = {_updateeFilePath}
                - PackageContentDirPath = {_packageContentDirPath}
                - RestartUpdatee = {_restartUpdatee}
                - RoutedArgs = {_routedArgs}
                """
            );

            AcquireLock();
            ApplyUpdate();

            if (_restartUpdatee)
                StartUpdatee();
        }
        catch (Exception ex)
        {
            WriteLog(ex.ToString());
        }
    }

    public void Dispose()
    {
        _log.Dispose();
        _updateeDirLock?.Dispose();
    }
}