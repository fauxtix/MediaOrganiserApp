using System;
using System.Linq;

namespace MediaOrganizerApp.Application.Utilities;

/// <summary>
/// Curated OpenRouter model prefixes suitable for structured metadata/bibliography
/// extraction (strong JSON adherence + instruction following). Used to highlight
/// recommended models in the admin picker and to resolve the model for the
/// OpenRouter-backed author-books extraction.
/// </summary>
public static class RecommendedExtractionModels
{
    public static readonly string[] Prefixes =
    {
        "openai/gpt-4o",
        "openai/gpt-4o-mini",
        "google/gemini-2.5-flash",
        "google/gemini-2.0-flash",
        "anthropic/claude-3.5-sonnet",
        "anthropic/claude-sonnet-4",
        "meta-llama/llama-3.3-70b-instruct",
        "deepseek/deepseek-chat",
        "qwen/qwen2.5-72b-instruct"
    };

    public static bool IsRecommended(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId)) return false;
        var id = modelId.Trim();

        if (id.Contains(":reasoning", StringComparison.OrdinalIgnoreCase)
            || id.Contains(":thinking", StringComparison.OrdinalIgnoreCase))
            return false;

        return Prefixes.Any(p => id.StartsWith(p, StringComparison.Ordinal));
    }
}
