using System;
using MediaOrganizerApp.Application.Utilities;

namespace MediaOrganizerApp.Application.Configurations;

/// <summary>Global (admin) settings keys for the OpenRouter cloud chat.</summary>
public static class CloudChatSettingsKeys
{
    public const string OpenRouterApiKey = "CloudChat:OpenRouterApiKey";
    public const string DefaultModel = "CloudChat:DefaultModel";
    public const string EnableReasoning = "CloudChat:EnableReasoning";
    public const string AllowPaidModels = "CloudChat:AllowPaidModels";
    public const string FreeDailyCap = "CloudChat:FreeDailyCap";
    public const string AuthorBooksUseCloud = "CloudChat:AuthorBooksUseCloud";
    public const string ExtractionModels = "CloudChat:ExtractionModels";
    public const string DefaultModelValue = "openrouter/free";

    /// <summary>Determines whether a model id runs on OpenRouter's free tier.</summary>
    public static bool IsFreeModel(string id)
        => string.Equals(id, DefaultModelValue, StringComparison.OrdinalIgnoreCase)
        || id.EndsWith(":free", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Ensures a resolved model honors the <c>AllowPaidModels</c> toggle. Free
    /// models (and paid models when the toggle is on) pass through untouched;
    /// a paid recommended model falls back to its <c>:free</c> variant, and any
    /// other paid model falls back to the default free router.
    /// </summary>
    public static string ApplyPaidModelGate(string model, bool allowPaidModels)
    {
        if (allowPaidModels || IsFreeModel(model))
            return model;

        return RecommendedExtractionModels.IsRecommended(model)
            ? model + ":free"
            : DefaultModelValue;
    }
}