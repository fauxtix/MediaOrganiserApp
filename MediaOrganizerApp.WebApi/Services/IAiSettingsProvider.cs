namespace MediaOrganizerApp.WebApi.Services;

/// <summary>
/// Provides AI settings (base URL and model name) with short-lived in-memory caching
/// so the database is not hit on every AI request.
/// </summary>
public interface IAiSettingsProvider
{
    Task<(string ModelBaseUrl, string ModelName)> GetAiSettingsAsync();

    /// <summary>
    /// Invalidates the cached AI settings so the next <see cref="GetAiSettingsAsync"/>
    /// reflects the latest values (e.g. after saving app settings).
    /// </summary>
    void Invalidate();
}
