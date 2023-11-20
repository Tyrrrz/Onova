using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CliFx;

namespace Onova.Tests.Dummy;

public static class Program
{
    // Path to the apphost
    public static string FilePath { get; } =
        Path.ChangeExtension(
            Assembly.GetExecutingAssembly().Location,
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "exe" : null
        );

    public static async Task<int> Main(string[] args) =>
        await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync(args);
}
