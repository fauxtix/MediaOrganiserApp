namespace MediaOrganizerApp.Application.DTOs.AI.CloudChat;

/// <summary>Global admin-managed settings surfaced to the test chat UI.</summary>
public class CloudChatSettingsDto
{
    public bool ApiKeyConfigured { get; set; }
    public string DefaultModel { get; set; } = "openrouter/free";
    public bool EnableReasoning { get; set; }
    public bool AllowPaidModels { get; set; }
    public int? FreeDailyCap { get; set; }
    public bool AuthorBooksUseCloud { get; set; } = true;
}

/// <summary>Update payload for the global cloud chat settings (admin only).</summary>
public class CloudChatSettingsUpdateDto
{
    public string? ApiKey { get; set; }
    public string? DefaultModel { get; set; }
    public bool? EnableReasoning { get; set; }
    public bool? AllowPaidModels { get; set; }
    public int? FreeDailyCap { get; set; }
    public bool? AuthorBooksUseCloud { get; set; }
}