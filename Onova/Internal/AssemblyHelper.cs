using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;

namespace Onova.Internal
{
    internal static class AssemblyHelper
    {
        public static async Task CopyResourceAsync(string resourceName, string destFilePath)
        {
            var input = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (input == null)
                throw new MissingManifestResourceException($"Could not find resource [{resourceName}].");

            using (var output = File.Create(destFilePath))
                await input.CopyToAsync(output).ConfigureAwait(false);
        }
    }
}