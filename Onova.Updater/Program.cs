using System.Threading.Tasks;
using Onova.Updater.Internal;

namespace Onova.Updater
{
    // This executable applies the update by copying over new files.
    // It's required because updatee cannot update itself while the files are still in use.

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var updateeFilePath = args[0];
            var packageContentDirPath = args[1];
            var restartUpdatee = bool.Parse(args[2]);
            var routedArgs = args[3].FromBase64().GetString();
            // Base64 encoded semicolon delimited list of paths to executables to wait for
            // before installing the application
            var additionalExecutables = args[4].FromBase64().GetString().Split(';');

            using var updater = new Updater(updateeFilePath, packageContentDirPath, restartUpdatee, routedArgs, additionalExecutables);
            await updater.Run();
        }
    }
}