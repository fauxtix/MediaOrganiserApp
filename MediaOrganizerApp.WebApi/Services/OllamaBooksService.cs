using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services;

public sealed class OllamaBooksService : OllamaBaseClient, IAiBooksService
{
    private readonly ILogger<OllamaBooksService> _logger;
    private readonly IProgressPublisher _progressPublisher;

    public OllamaBooksService(
        HttpClient httpClient,
        ILogger<OllamaBooksService> logger,
        IPromptTemplateProvider prompts,
        IProgressPublisher progressPublisher,
        IAiSettingsProvider aiSettingsProvider,
        ISystemPromptService systemPromptService)
        : base(httpClient, aiSettingsProvider, systemPromptService, prompts)
    {
        _logger = logger;
        _progressPublisher = progressPublisher;
    }

    public async Task<List<GoogleBookInfo>> GetBooksByAuthorAsync(
        string author,
        string language,
        string? connectionId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(author)) return new List<GoogleBookInfo>();

        try
        {
            const int defaultMaxResults = 20;
            await PublishAsync(connectionId, correlationId, "preparing_prompt", 5, "Preparing books prompt...", cancellationToken);

            var fullPrompt = await BuildFullPromptAsync("author-books.prompt.txt", new Dictionary<string, string?>
            {
                ["author"] = author,
                ["max_results"] = defaultMaxResults.ToString(),
                ["language"] = string.IsNullOrWhiteSpace(language) ? "pt" : language
            }, cancellationToken);

            await PublishAsync(connectionId, correlationId, "request_sent", 40, "Sending books prompt to AI model...", cancellationToken);
            var text = await ExecuteGenerateAsync(fullPrompt, cancellationToken);
            await PublishAsync(connectionId, correlationId, "parsing", 75, "Parsing books AI response...", cancellationToken);

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Ollama returned no JSON for {Author}.", author);
                throw new InvalidOperationException("AI returned no JSON content");
            }

            var normalized = NormalizeBooksJson(text);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                _logger.LogWarning("Books response normalization produced no JSON for {Author}.", author);
                throw new InvalidOperationException("AI returned no parseable JSON content");
            }

            using var doc = JsonDocument.Parse(normalized);

            var elements = new List<JsonElement>();
            var root = doc.RootElement;

            bool TryAddFromObject(JsonElement obj)
            {
                if (obj.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in items.EnumerateArray()) elements.Add(el);
                    return true;
                }
                if (obj.TryGetProperty("books", out var books) && books.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in books.EnumerateArray()) elements.Add(el);
                    return true;
                }
                if (obj.TryGetProperty("response", out var respProp) && respProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in respProp.EnumerateArray()) elements.Add(el);
                    return true;
                }
                return false;
            }

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in root.EnumerateArray()) elements.Add(el);
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                if (!TryAddFromObject(root))
                {
                    var hasBookField = root.TryGetProperty("Title", out _) || root.TryGetProperty("title", out _) || root.TryGetProperty("Id", out _) || root.TryGetProperty("id", out _);
                    if (!hasBookField)
                    {
                        var start = normalized.IndexOf('[');
                        var end = normalized.LastIndexOf(']');
                        if (start >= 0 && end > start)
                        {
                            var candidate = normalized.Substring(start, end - start + 1);
                            try
                            {
                                using var doc2 = JsonDocument.Parse(candidate);
                                if (doc2.RootElement.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var el in doc2.RootElement.EnumerateArray()) elements.Add(el);
                                }
                                else
                                {
                                    elements.Add(root);
                                }
                            }
                            catch
                            {
                                elements.Add(root);
                            }
                        }
                        else
                        {
                            elements.Add(root);
                        }
                    }
                    else
                    {
                        elements.Add(root);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Ollama books response had unexpected JSON root for {Author}. Raw: {Raw}", author, normalized);
                return new List<GoogleBookInfo>();
            }

            var result = new List<GoogleBookInfo>();
            foreach (var el in elements)
            {
                try
                {
                    static string? ReadString(JsonElement e, params string[] names)
                    {
                        foreach (var n in names)
                        {
                            if (e.TryGetProperty(n, out var p))
                            {
                                if (p.ValueKind == JsonValueKind.String) return p.GetString();
                                return p.ToString();
                            }
                        }
                        return null;
                    }

                    static double? ReadDouble(JsonElement e, params string[] names)
                    {
                        foreach (var n in names)
                        {
                            if (e.TryGetProperty(n, out var p))
                            {
                                if (p.ValueKind == JsonValueKind.Number && p.TryGetDouble(out var d)) return d;
                                if (p.ValueKind == JsonValueKind.String)
                                {
                                    var s = p.GetString();
                                    if (string.IsNullOrWhiteSpace(s)) return null;
                                    s = s.Trim().TrimEnd('%');
                                    if (double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d2))
                                    {
                                        if (d2 > 1) return d2 > 1 && d2 <= 100 ? d2 / 100.0 : d2;
                                        return d2;
                                    }
                                }
                            }
                        }
                        return null;
                    }

                    var title = ReadString(el, "Title", "title", "name");
                    if (string.IsNullOrWhiteSpace(title)) continue;
                    var id = ReadString(el, "Id", "id") ?? ReadString(el, "googleId", "google_id");
                    var published = ReadString(el, "PublishedDate", "publishedDate", "date") ?? string.Empty;
                    var thumb = ReadString(el, "Thumbnail", "thumbnail", "image") ?? "images/NoPhoto.png";
                    var confidence = ReadDouble(el, "Confidence", "confidence");

                    if (confidence is not null)
                    {
                        if (confidence < 0) confidence = 0;
                        if (confidence > 1) confidence = confidence > 1 && confidence <= 100 ? confidence / 100.0 : 1;
                    }

                    var g = new GoogleBookInfo
                    {
                        Id = id,
                        Title = title,
                        PublishedDate = published,
                        Thumbnail = thumb,
                        Confidence = confidence
                    };

                    var tLower = (g.Title ?? string.Empty).ToLowerInvariant();
                    if (tLower.Contains("example") || tLower.Contains("example book"))
                    {
                        _logger.LogInformation("Skipping placeholder book returned by AI for {Author}: {Title}", author, g.Title);
                        continue;
                    }

                    if (g is { Synthesized: true })
                    {
                        var conf = g.Confidence ?? 0.0;
                        if (conf < 0.45)
                        {
                            _logger.LogInformation("Skipping low-confidence synthesized book for {Author}: {Title} (conf={Conf})", author, g.Title, conf);
                            continue;
                        }
                    }

                    if (g.Confidence is not null && g.Confidence < 0.10)
                    {
                        _logger.LogInformation("Skipping very low-confidence book for {Author}: {Title} (conf={Conf})", author, g.Title, g.Confidence);
                        continue;
                    }

                    result.Add(g);
                    if (result.Count >= defaultMaxResults) break;
                }
                catch (Exception innerEx)
                {
                    _logger.LogDebug(innerEx, "Error parsing book element for {Author}", author);
                }
            }

            await PublishAsync(connectionId, correlationId, "completed", 100, "Books response processed.", cancellationToken);
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("OllamaBooksService request canceled for author {Author}", author);
            await PublishAsync(connectionId, correlationId, "cancelled", 100, "Books request canceled.", cancellationToken, "warning");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying Ollama for books by {Author}", author);
            await PublishAsync(connectionId, correlationId, "failed", 100, ex.Message, cancellationToken, "error");
            throw;
        }
    }

    private static string? NormalizeBooksJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var cleaned = raw.Trim();

        const string fence = "```";
        var firstFence = cleaned.IndexOf(fence, StringComparison.Ordinal);
        var lastFence = cleaned.LastIndexOf(fence, StringComparison.Ordinal);
        if (firstFence >= 0 && lastFence > firstFence)
            cleaned = cleaned.Substring(firstFence + fence.Length, lastFence - (firstFence + fence.Length)).Trim();

        while (!string.IsNullOrEmpty(cleaned) && (cleaned[0] == '`' || cleaned[0] == '\''))
            cleaned = cleaned[1..].TrimStart();

        try
        {
            using var docDirect = JsonDocument.Parse(cleaned);
            return docDirect.RootElement.ToString();
        }
        catch
        {
            // keep trying
        }

        var extracted = OllamaResponseHelper.TryExtractFirstJsonObject(cleaned);
        if (!string.IsNullOrWhiteSpace(extracted))
            return extracted;

        var start = cleaned.IndexOf('[');
        var end = cleaned.LastIndexOf(']');
        if (start >= 0 && end > start)
        {
            var candidate = cleaned.Substring(start, end - start + 1);
            try
            {
                using var doc = JsonDocument.Parse(candidate);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    return candidate;
            }
            catch
            {
                // ignore
            }
        }

        return cleaned;
    }

    private Task PublishAsync(
        string? connectionId,
        string? correlationId,
        string stage,
        int percent,
        string message,
        CancellationToken cancellationToken,
        string level = "info")
        => _progressPublisher.PublishAsync(
            ProgressChannel.BooksInfo,
            new ProgressUpdateDto
            {
                CorrelationId = correlationId,
                Stage = stage,
                Percent = percent,
                Message = message,
                Level = level,
                Service = nameof(OllamaBooksService),
                Operation = "author-books"
            },
            connectionId,
            cancellationToken);
}
