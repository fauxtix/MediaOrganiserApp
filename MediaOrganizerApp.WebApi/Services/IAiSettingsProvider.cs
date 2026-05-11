namespace MediaOrganizerApp.WebApi.Services;

/// <summary>
/// Provides AI settings (base URL and model name) with short-lived in-memory caching
/// so the database is not hit on every AI request.
/// </summary>
public interface IAiSettingsProvider
{
    Task<(string ModelBaseUrl, string ModelName)> GetAiSettingsAsync();
}
