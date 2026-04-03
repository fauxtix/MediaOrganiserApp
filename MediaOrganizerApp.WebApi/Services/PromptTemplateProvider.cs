using System.Collections.Concurrent;
using System.Text;

namespace MediaOrganizerApp.WebApi.Services;

public interface IPromptTemplateProvider
{
    Task<string> GetTemplateAsync(string templateName, CancellationToken cancellationToken = default);
}

public sealed class PromptTemplateProvider : IPromptTemplateProvider
{
    private readonly ILogger<PromptTemplateProvider> _logger;
    private readonly string _promptsRoot;
    private readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

    public PromptTemplateProvider(IHostEnvironment env, ILogger<PromptTemplateProvider> logger)
    {
        _logger = logger;
        _promptsRoot = Path.Combine(env.ContentRootPath, "Prompts");
    }

    public async Task<string> GetTemplateAsync(string templateName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(templateName))
            throw new ArgumentException("Template name is required.", nameof(templateName));

        if (_cache.TryGetValue(templateName, out var cached) && !string.IsNullOrWhiteSpace(cached))
            return cached;
            // Allow overrides in an external folder (used by the admin UI).
            // Check common override locations before falling back to the embedded Prompts folder.
            var overrideCandidates = new[] {
                Path.Combine(AppContext.BaseDirectory, "Prompts_Ollama", templateName),
                Path.Combine(Environment.CurrentDirectory, "Prompts_Ollama", templateName),
                Path.Combine(_promptsRoot, templateName) // fallback
            };

            string? path = null;
            foreach (var candidate in overrideCandidates)
            {
                if (File.Exists(candidate))
                {
                    path = candidate;
                    break;
                }
            }

            if (path is null)
                throw new FileNotFoundException($"Prompt template not found. Searched: {string.Join(";", overrideCandidates)}");

        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var text = await reader.ReadToEndAsync(cancellationToken);

            _cache[templateName] = text;
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load prompt template {TemplateName} from {PromptsRoot}", templateName, _promptsRoot);
            throw;
        }
    }
}

public static class PromptTemplateRenderer
{
    public static string Render(string template, IReadOnlyDictionary<string, string?> values)
    {
        if (template is null)
            throw new ArgumentNullException(nameof(template));

        if (values is null || values.Count == 0)
            return template;

        var s = template;
        foreach (var kvp in values)
        {
            var token = "{" + kvp.Key + "}";
            s = s.Replace(token, kvp.Value ?? string.Empty, StringComparison.Ordinal);
        }

        return s;
    }
}
