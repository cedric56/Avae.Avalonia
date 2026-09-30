using System.Text.RegularExpressions;

namespace Avae.Avalonia;

internal static partial class Extensions
{
    public static IEnumerable<string> SplitOnCapitals(this string text)
    {
        var regex = CapitalizedWordRegex();
        foreach (Match match in regex.Matches(text))
        {
            yield return match.Value;
        }
    }

    public static T ToEnum<T>(this string value, T defaultValue = default) where T : struct
    {
        return Enum.TryParse(value, true, out T result) ? result : defaultValue;
    }

    [GeneratedRegex(@"\p{Lu}\p{Ll}*")]
    private static partial Regex CapitalizedWordRegex();
}
