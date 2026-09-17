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

    /// <summary>
    /// Known-good FREE OpenRouter model ids (verified live slugs, not fabricated
    /// ":free" suffixes) used as a TIE-BREAK boost when ranking the live catalog:
    /// a free model that appears here is ordered before a model of equal quality
    /// score. It is NOT the source of truth — new/unknown free models with a higher
    /// live "intelligence" index are automatically preferred. If a slug disappears
    /// from the live catalog it is skipped silently.
    /// </summary>
    public static readonly string[] RecommendedFreeModelIds =
    {
        "z-ai/glm-5.2:free",
        "google/gemma-4-31b-it:free",
        "nvidia/nemotron-3-ultra-550b-a55b:free",
        "nvidia/nemotron-3-super-120b-a12b:free",
        "nvidia/nemotron-3.5-lightning:free",
        "google/gemma-4-26b-a4b-it:free",
        "thinkingmachines/inkling:free"
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

    public static bool IsRecommendedFree(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId)) return false;
        var id = modelId.Trim();
        return RecommendedFreeModelIds.Contains(id, StringComparer.OrdinalIgnoreCase);
    }
}
