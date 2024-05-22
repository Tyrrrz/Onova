using oZnova.Updater.Utils.Extensions;

namespace oZnova.Updater;

// This executable applies the update by copying over new files.
// It's required because the updatee cannot update itself while the files are still in use.

public static class Program
{
    public static void Main(string[] args)
    {
        var updateeFilePath = args[0];
        var packageContentDirPath = args[1];
        var restartUpdatee = bool.Parse(args[2]);
        var routedArgs = args[3].FromBase64().GetString();

        using var updater = new Updater(
            updateeFilePath,
            packageContentDirPath,
            restartUpdatee,
            routedArgs
        );
        updater.Run();
    }
}
