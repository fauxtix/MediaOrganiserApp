using System.Globalization;

namespace Web.Blazor.Services.CloudChat;

public static class ContextWindowResolver
{
    public const int OpenRouterDefaultContextLength = 32_768;

    // Gemini never used on this app (OpenRouter only); kept for parity with the reference.
    private const int GeminiDefaultContextLength = 2_000_000;

    public static int GetContextLength(string provider, string model, IReadOnlyDictionary<string, int>? catalog)
    {
        if (string.Equals(provider, "Gemini", StringComparison.OrdinalIgnoreCase))
            return GeminiDefaultContextLength;

        if (!string.IsNullOrWhiteSpace(model) && catalog is not null
            && catalog.TryGetValue(model, out var known) && known > 0)
        {
            return known;
        }

        return OpenRouterDefaultContextLength;
    }

    public static bool TryGetKnownContextLength(
        string provider,
        string model,
        IReadOnlyDictionary<string, int>? catalog,
        out int contextLength)
    {
        contextLength = 0;

        if (string.Equals(provider, "Gemini", StringComparison.OrdinalIgnoreCase))
        {
            contextLength = GeminiDefaultContextLength;
            return true;
        }

        if (!string.IsNullOrWhiteSpace(model) && catalog is not null
            && catalog.TryGetValue(model, out var known) && known > 0)
        {
            contextLength = known;
            return true;
        }

        return false;
    }

    public static int GetDefaultLimit() => OpenRouterDefaultContextLength;

    public static string GetTargetLanguageDefault() => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
}