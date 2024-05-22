using System.Text.RegularExpressions;

namespace oZnova.Utils;

internal static class WildcardPattern
{
    public static bool IsMatch(string input, string pattern)
    {
        var regex = '^' + Regex.Escape(pattern).Replace("\\*", ".*?").Replace("\\?", ".") + '$';

        return Regex.IsMatch(input, regex, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    }
}
