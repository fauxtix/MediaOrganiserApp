using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using System.Text;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services
{
    public sealed class OllamaBooksService : IAiBooksService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaBooksService> _logger;
        private string _baseUrl;
        private string _model;

        private readonly IConfiguration _config;
        private readonly IPromptTemplateProvider _prompts;
        private readonly IAppSettingsRepository _appSettingsRepository;


        public OllamaBooksService(HttpClient httpClient, ILogger<OllamaBooksService> logger, IConfiguration config, IPromptTemplateProvider prompts, IAppSettingsRepository appSettingsRepository)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config;
            _prompts = prompts;
            _appSettingsRepository = appSettingsRepository;
        }

        public async Task<List<GoogleBookInfo>> GetBooksByAuthorAsync(string author, string language, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(author)) return new List<GoogleBookInfo>();

            try
            {
                // internal default/cap for results when AI generates a list
                var defaultMaxResults = 20;
                var template = await _prompts.GetTemplateAsync("author-books.prompt.txt", cancellationToken);
                var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
                {
                    ["author"] = author,
                    ["max_results"] = defaultMaxResults.ToString(),
                    ["language"] = string.IsNullOrWhiteSpace(language) ? "pt" : language
                });

                var (modelBaseUrl, modelName) = await GetAiSettings();


                var payload = new { model = modelName, prompt, stream = false };

                using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(modelBaseUrl), "/api/generate"))
                {
                    Content = JsonContent.Create(payload)
                };

                using var httpResponse = await _httpClient.SendAsync(request, cancellationToken);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Ollama returned non-success status {Status} when requesting books for {Author}", httpResponse.StatusCode, author);
                    return new List<GoogleBookInfo>();
                }

                var bytes = await httpResponse.Content.ReadAsByteArrayAsync(cancellationToken);
                var raw = Encoding.UTF8.GetString(bytes);
                // log raw response at debug level to aid diagnosis
                _logger.LogDebug("Ollama raw response for {Author}: {Raw}", author, raw);
                var text = TryExtractOllamaResponseText(raw);

                if (string.IsNullOrWhiteSpace(text))
                    return new List<GoogleBookInfo>();

                try
                {
                    using var doc = JsonDocument.Parse(text);

                    // Normalize root into a sequence of elements. If the top-level object looks like
                    // metadata (no book fields) but the raw text contains a JSON array, prefer the array.
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
                        // If object already contains an array property, use it.
                        if (TryAddFromObject(root)) { }
                        else
                        {
                            // Heuristic: if this object does not contain book-like fields but the raw text
                            // contains an array, extract that array and parse it instead.
                            bool hasBookField = root.TryGetProperty("Title", out _) || root.TryGetProperty("title", out _) || root.TryGetProperty("Id", out _) || root.TryGetProperty("id", out _);
                            if (!hasBookField)
                            {
                                var start = text.IndexOf('[');
                                var end = text.LastIndexOf(']');
                                if (start >= 0 && end > start)
                                {
                                    var candidate = text.Substring(start, end - start + 1);
                                    try
                                    {
                                        using var doc2 = JsonDocument.Parse(candidate);
                                        if (doc2.RootElement.ValueKind == JsonValueKind.Array)
                                        {
                                            foreach (var el in doc2.RootElement.EnumerateArray()) elements.Add(el);
                                        }
                                        else
                                        {
                                            // fallback to treating original object as single element
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
                        _logger.LogWarning("Ollama books response had unexpected JSON root for {Author}. Raw: {Raw}", author, text);
                        return new List<GoogleBookInfo>();
                    }

                    var result = new List<GoogleBookInfo>();
                    foreach (var el in elements)
                    {
                        try
                        {
                            // helper to read string properties with alternative names
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
                                            // strip percent
                                            s = s.Trim().TrimEnd('%');
                                            if (double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d2))
                                            {
                                                // if value > 1 and looks like percent, normalize
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

                            // Filter out obvious placeholder/example items
                            var tLower = (g.Title ?? string.Empty).ToLowerInvariant();
                            if (tLower.Contains("example") || tLower.Contains("example book"))
                            {
                                _logger.LogInformation("Skipping placeholder book returned by AI for {Author}: {Title}", author, g.Title);
                                continue;
                            }

                            // If item is marked synthesized, require a minimum confidence to include
                            if (g is { } && g.Synthesized is true)
                            {
                                var conf = g.Confidence ?? 0.0;
                                if (conf < 0.45)
                                {
                                    _logger.LogInformation("Skipping low-confidence synthesized book for {Author}: {Title} (conf={Conf})", author, g.Title, conf);
                                    continue;
                                }
                            }

                            // Skip extremely low confidence items
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

                    return result;
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse Ollama books response for {Author}. Raw: {Raw}", author, text);
                    return new List<GoogleBookInfo>();
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("OllamaBooksService request canceled for author {Author}", author);
                return new List<GoogleBookInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying Ollama for books by {Author}", author);
                return new List<GoogleBookInfo>();
            }
        }

        private static string? TryExtractOllamaResponseText(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            // Normalize and try several extraction strategies to handle markdown/code fences,
            // multiple JSON fragments, or simple raw arrays/objects.
            var cleaned = raw ?? string.Empty;
            cleaned = cleaned.Trim();

            // If wrapped in triple backtick fences (``` or ```json), extract inner content
            const string fence = "```";
            try
            {
                var firstFence = cleaned.IndexOf(fence, StringComparison.Ordinal);
                var lastFence = cleaned.LastIndexOf(fence, StringComparison.Ordinal);
                if (firstFence >= 0 && lastFence > firstFence)
                {
                    cleaned = cleaned.Substring(firstFence + fence.Length, lastFence - (firstFence + fence.Length)).Trim();
                }
            }
            catch { /* ignore and continue with original raw */ }

            // Trim any leading single/back quotes that sometimes appear
            while (!string.IsNullOrEmpty(cleaned) && (cleaned[0] == '`' || cleaned[0] == '\''))
                cleaned = cleaned.Substring(1).TrimStart();

            // Quick attempt: if cleaned starts with [ or { try parsing directly
            try
            {
                using var docDirect = JsonDocument.Parse(cleaned);
                return docDirect.RootElement.ToString();
            }
            catch { /* fall through to other heuristics */ }

            // If text contains multiple JSON fragments, try to extract the first JSON array block
            try
            {
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
                    catch { /* not valid array, continue */ }
                }
            }
            catch { }

            // Fall back to NDJSON / merged lines approach: collect JSON parts or plain lines
            var sb = new StringBuilder();
            foreach (var line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var ln = line.Trim();
                if (string.IsNullOrEmpty(ln)) continue;
                // strip surrounding single backticks
                if ((ln.StartsWith("`") && ln.EndsWith("`")) || (ln.StartsWith("'") && ln.EndsWith("'")))
                    ln = ln.Substring(1, ln.Length - 2).Trim();

                sb.AppendLine(ln);
            }

            var merged = sb.ToString().Trim();

            // If merged contains an array block, extract and return it
            try
            {
                var start = merged.IndexOf('[');
                var end = merged.LastIndexOf(']');
                if (start >= 0 && end > start)
                {
                    var candidate = merged.Substring(start, end - start + 1);
                    using var doc = JsonDocument.Parse(candidate);
                    return doc.RootElement.ToString();
                }
            }
            catch { }

            return string.IsNullOrWhiteSpace(merged) ? null : merged;
        }

        private async Task<(string modelBaseUrl, string modelName)> GetAiSettings()
        {
            var settings = await _appSettingsRepository.GetSettingsAsync();
            return (settings.ModelBaseUrl, settings.AiModelName);
        }

    }
}
