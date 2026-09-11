using MediaOrganizerApp.Application.DTOs;

namespace MediaOrganizerApp.WebApi.Services;

/// <summary>
/// Provides access to the Ollama model catalog (installed models) and model management
/// operations (pull / delete) backed by the local Ollama server.
/// </summary>
public interface IOllamaModelsService
{
    /// <summary>
    /// Lists the models currently installed on the Ollama server (GET /api/tags).
    /// </summary>
    Task<IEnumerable<AIModelVM>> GetModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pulls (downloads) a model by name on the Ollama server (POST /api/pull).
    /// </summary>
    Task<bool> PullModelAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a model by name on the Ollama server (DELETE /api/delete).
    /// </summary>
    Task<bool> DeleteModelAsync(string name, CancellationToken cancellationToken = default);
}
