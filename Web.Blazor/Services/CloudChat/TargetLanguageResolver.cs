using System.Globalization;

namespace Web.Blazor.Services.CloudChat;

public static class TargetLanguageResolver
{
    public const string Token = "{{TARGET_LANGUAGE}}";

    public static string GetTargetLanguage()
    {
        string lang;
        try
        {
            lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        }
        catch
        {
            lang = "pt";
        }

        return lang.StartsWith("en", StringComparison.OrdinalIgnoreCase)
            ? "English (en-US)"
            : "European Portuguese (pt-PT)";
    }
}