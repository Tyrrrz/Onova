using System.Text.RegularExpressions;

namespace Onova.Internal;

internal static class WildcardPattern
{
    public static bool IsMatch(string input, string pattern)
    {
        pattern = Regex.Escape(pattern);
        pattern = pattern.Replace("\\*", ".*?").Replace("\\?", ".");
        pattern = "^" + pattern + "$";

        return Regex.IsMatch(input, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    }
}