using System.Text.RegularExpressions;

namespace Web.Blazor.Utilities;

public static partial class TextSanitizer
{
    [GeneratedRegex("<.*?>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    public static string StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        return HtmlTagRegex().Replace(html, string.Empty).Trim();
    }
}
