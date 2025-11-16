using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;

namespace Onova.Utils.Extensions;

internal static class ReflectionExtensions
{
    extension(Assembly assembly)
    {
        public async Task ExtractManifestResourceAsync(string resourceName, string destFilePath)
        {
            var input =
                assembly.GetManifestResourceStream(resourceName)
                ?? throw new MissingManifestResourceException(
                    $"Could not find resource '{resourceName}'."
                );

            using var output = File.Create(destFilePath);
            await input.CopyToAsync(output);
        }
    }
}
