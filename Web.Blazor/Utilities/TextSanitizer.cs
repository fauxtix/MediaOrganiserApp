using System.Text.RegularExpressions;
using System.Net;

namespace Web.Blazor.Utilities;

public static partial class TextSanitizer
{
    [GeneratedRegex("<.*?>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    public static string StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // Decode HTML entities first (e.g., &nbsp;), then strip tags.
        var decoded = WebUtility.HtmlDecode(html);
        return HtmlTagRegex().Replace(decoded, string.Empty)
            .Replace('\u00A0', ' ')
            .Trim();
    }
}
