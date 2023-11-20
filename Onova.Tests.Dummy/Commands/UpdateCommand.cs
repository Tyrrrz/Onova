using System;
using System.IO;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Onova.Services;

namespace Onova.Tests.Dummy.Commands;

[Command("update")]
public class UpdateCommand : ICommand
{
    [CommandOption("packages")]
    public string PackagesDirPath { get; init; } =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Packages");

    [CommandOption("restart")]
    public bool Restart { get; init; }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var updateManager = new UpdateManager(
            new LocalPackageResolver(PackagesDirPath, "*.onv"),
            new ZipPackageExtractor()
        );

        var progressHandler = new Progress<double>(
            p => console.Output.WriteLine($"Progress: {p:P0}")
        );

        await updateManager.CheckPerformUpdateAsync(Restart, progressHandler);
    }
}
