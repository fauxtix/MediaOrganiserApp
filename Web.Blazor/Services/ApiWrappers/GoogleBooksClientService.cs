using System.Diagnostics;
using System.Text;
using System.Text.Json;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Web.Blazor.Services.ApiWrappers
{
    public class GoogleBooksClientService : IGoogleBooksService
    {
        private readonly HttpClient _http;
        private readonly ILogger<GoogleBooksClientService> _logger;
        private readonly IConfiguration _config;

        public GoogleBooksClientService(HttpClient http, ILogger<GoogleBooksClientService> logger, IConfiguration config)
        {
            _http = http;
            _logger = logger;
            _config = config;
        }

        // The Google Books API key must always be taken from the database (dbo.ApplicationSettings).
        // No config-file fallback: the DB is the single source of truth.
        private string BuildAuthSuffix(out string country)
        {
            var (key, dbCountry, present) = ReadGoogleBooksSettingsFromDatabase();

            if (!present)
            {
                _logger.LogWarning(
                    "Google Books API: no API key configured in database (dbo.ApplicationSettings.GoogleBooksApiKey). Configure it in 'Definições → Outras configurações 2'. Skipping Google Books query.");
                throw new GoogleBooksKeyNotConfiguredException(
                    "Google Books API key is not configured in dbo.ApplicationSettings.GoogleBooksApiKey.");
            }

            country = string.IsNullOrWhiteSpace(dbCountry) ? "PT" : dbCountry.Trim();

            var sb = new StringBuilder();
            sb.Append($"&key={Uri.EscapeDataString(key.Trim())}");
            if (!string.IsNullOrWhiteSpace(country))
                sb.Append($"&country={Uri.EscapeDataString(country)}");

            return sb.ToString();
        }

        private (string Key, string Country, bool Present) ReadGoogleBooksSettingsFromDatabase()
        {
            var connString = _config.GetConnectionString("SqlServerCon");
            if (string.IsNullOrWhiteSpace(connString))
            {
                _logger.LogWarning("Google Books API key source: no ConnectionStrings:SqlServerCon configured. No API key available.");
                return (string.Empty, "PT", false);
            }

            try
            {
                using var con = new SqlConnection(connString);
                using var cmd = new SqlCommand(
                    "SELECT TOP 1 GoogleBooksApiKey, GoogleBooksCountry FROM dbo.ApplicationSettings", con);
                con.Open();

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    _logger.LogWarning("Google Books API key source: dbo.ApplicationSettings has no rows. No API key available.");
                    return (string.Empty, "PT", false);
                }

                var apiKey = reader["GoogleBooksApiKey"]?.ToString() ?? string.Empty;
                var country = reader["GoogleBooksCountry"]?.ToString() ?? string.Empty;
                return (apiKey.Trim(), country.Trim(), !string.IsNullOrWhiteSpace(apiKey));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Google Books API key source: failed to read dbo.ApplicationSettings. No API key available.");
                return (string.Empty, "PT", false);
            }
        }

        // Return null on transient failure/cancellation; return list (possibly empty) on success
        public async Task<List<GoogleBookInfo>?> GetBooksByAuthorAsync(string author)
        {
            if (string.IsNullOrWhiteSpace(author)) return new List<GoogleBookInfo>();

            string? urlForLog = null;
            try
            {
                var sw = Stopwatch.StartNew();
                var q = $"inauthor:\"{Uri.EscapeDataString(author)}\"";
                var defaultMax = 20;
                var url = $"https://www.googleapis.com/books/v1/volumes?q={q}&maxResults={defaultMax}&fields=totalItems,items(id,volumeInfo/title,volumeInfo/publishedDate,volumeInfo/imageLinks)";
                var suffix = BuildAuthSuffix(out var country);
                url += suffix;
                urlForLog = url;

                // TEMPORARY (tests): expose the full URL incl. API key in logs. Re-mask with MaskUrl(url) later.
                _logger.LogInformation("Google Books lookup. Author={Author} Url={Url} KeyUsed={KeyUsed} Country={Country}", author, url, true, country);

                using var resp = await _http.GetAsync(url);

                // Surface rate-limit to callers by throwing so callers can detect 429 specifically
                if (resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    var googleError = await TryReadBodyAsync(resp, 4000);
                    _logger.LogWarning(
                        "Google Books API rate limited (429). Author={Author} ReasonInfo={ReasonInfo} Url={Url} DurationMs={DurationMs}",
                        author, DescribeGoogleError(googleError), url, sw.ElapsedMilliseconds);
                    throw new HttpRequestException("Google Books API rate limited", null, resp.StatusCode);
                }

                if (!resp.IsSuccessStatusCode)
                {
                    string? retryAfter = resp.Headers.TryGetValues("Retry-After", out var ra)
                        ? string.Join(",", ra)
                        : null;

                    string? server = resp.Headers.TryGetValues("Server", out var sv)
                        ? string.Join(",", sv)
                        : null;

                    string? via = resp.Headers.TryGetValues("Via", out var viaHdr)
                        ? string.Join(",", viaHdr)
                        : null;

                    string? cfRay = resp.Headers.TryGetValues("CF-RAY", out var cfr)
                        ? string.Join(",", cfr)
                        : null;

                    var bodyPreview = await TryReadBodyAsync(resp, 4000);

                    var failed = resp.StatusCode >= System.Net.HttpStatusCode.InternalServerError
                        ? "server-side"
                        : "client-side";

                    _logger.LogWarning(
                        "Google Books GetBooksByAuthorAsync non-success ({FailedKind}). Status={StatusCode} ReasonInfo={ReasonInfo} Url={Url} RetryAfter={RetryAfter} Server={Server} Via={Via} CF-RAY={CFRay} DurationMs={DurationMs}",
                        failed,
                        resp.StatusCode,
                        DescribeGoogleError(bodyPreview),
                        url,
                        retryAfter ?? "<none>",
                        server ?? "<none>",
                        via ?? "<none>",
                        cfRay ?? "<none>",
                        sw.ElapsedMilliseconds);

                    return null;
                }

                var raw = await resp.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    _logger.LogWarning("Google Books returned an empty body for author {Author}. Url={Url}", author, url);
                    return null;
                }

                GoogleBooksResponse? respObj;
                try
                {
                    respObj = JsonSerializer.Deserialize<GoogleBooksResponse>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to deserialize Google Books response for {Author}. RawPreview={Raw} Url={Url}",
                        author, Truncate(raw, 1000), url);
                    return null;
                }

                var items = respObj?.items;
                if (respObj is null || items is null || items.Length == 0)
                {
                    _logger.LogWarning(
                        "Google Books returned zero items. Author={Author} TotalItems={TotalItems} ItemsArrayNull={ItemsArrayNull} Url={Url} DurationMs={DurationMs}",
                        author, respObj?.totalItems, items is null, url, sw.ElapsedMilliseconds);
                    return new List<GoogleBookInfo>();
                }

                var list = items.Select(i => new GoogleBookInfo
                {
                    Id = i.id,
                    Title = i.volumeInfo?.title,
                    PublishedDate = i.volumeInfo?.publishedDate,
                    Thumbnail = i.volumeInfo?.imageLinks?.thumbnail ?? i.volumeInfo?.imageLinks?.smallThumbnail
                }).ToList();

                _logger.LogInformation(
                    "Google Books returned {Count} items (total {TotalItems}) for author {Author}. Status=200 Url={Url} DurationMs={DurationMs}",
                    list.Count, respObj?.totalItems, author, url, sw.ElapsedMilliseconds);
                return list;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                throw; // re-throw so callers can detect 429 specifically
            }
            catch (GoogleBooksKeyNotConfiguredException)
            {
                throw; // re-throw so callers can warn the user about the missing API key
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GoogleBooks request cancelled for author {Author}. Url={Url}", author, urlForLog ?? "<not-built>");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GoogleBooks lookup failed for {Author}. Url={Url}", author, urlForLog ?? "<not-built>");
                return null;
            }
        }

        // Keep original method for backward compatibility used elsewhere in codebase
        public async Task<GoogleBook> GetBookAsync(string bookTitle, string author)
        {
            string? urlForLog = null;
            try
            {
                var sw = Stopwatch.StartNew();

                // Build the 'q' parameter with the correct Google Books phrase syntax:
                // double quotes ("...") denote exact-phrase matching; single quotes are
                // treated as literal characters and produced zero results for known works.
                var qParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(bookTitle))
                    qParts.Add($"intitle:\"{bookTitle.Trim()}\"");
                if (!string.IsNullOrWhiteSpace(author))
                    qParts.Add($"inauthor:\"{author.Trim()}\"");

                if (qParts.Count == 0)
                {
                    _logger.LogWarning("Google Books GetBookAsync: título e autor vazios. A devolver livro vazio.");
                    return new GoogleBook();
                }

                var query = $"https://www.googleapis.com/books/v1/volumes?q={string.Join("+", qParts.Select(p => Uri.EscapeDataString(p)))}";

                var suffix = BuildAuthSuffix(out var country);
                query += suffix;
                urlForLog = query;

                // TEMPORARY (tests): expose the full URL incl. API key in logs. Re-mask with MaskUrl(query) later.
                _logger.LogInformation("Google Books GetBookAsync. Title={Title} Author={Author} Url={Url} KeyUsed={KeyUsed} Country={Country}", bookTitle, author, query, true, country);

                using var resp = await _http.GetAsync(query);

                // Surface 429 so callers can detect and fallback to AI
                if (resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    var googleError = await TryReadBodyAsync(resp, 4000);
                    _logger.LogWarning(
                        "Google Books GetBookAsync rate limited (429). Title={Title} Author={Author} ReasonInfo={ReasonInfo} Url={Url} DurationMs={DurationMs}",
                        bookTitle, author, DescribeGoogleError(googleError), query, sw.ElapsedMilliseconds);
                    throw new HttpRequestException("Google Books API rate limited", null, resp.StatusCode);
                }

                if (!resp.IsSuccessStatusCode)
                {
                    var bodyPreview = await TryReadBodyAsync(resp, 4000);

                    var failed = resp.StatusCode >= System.Net.HttpStatusCode.InternalServerError
                        ? "server-side"
                        : "client-side";

                    _logger.LogWarning(
                        "Google Books GetBookAsync failed with status {Status} ({FailedKind}). Title={Title} Author={Author} ReasonInfo={ReasonInfo} Url={Url} DurationMs={DurationMs}",
                        resp.StatusCode, failed, bookTitle, author, DescribeGoogleError(bodyPreview), query, sw.ElapsedMilliseconds);
                    return new GoogleBook();
                }

                var rawBook = await resp.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(rawBook))
                {
                    _logger.LogWarning("Google Books GetBookAsync returned empty body. Title={Title} Author={Author} Url={Url} DurationMs={DurationMs}", bookTitle, author, query, sw.ElapsedMilliseconds);
                    return new GoogleBook();
                }

                GoogleBook? book;
                try
                {
                    book = JsonSerializer.Deserialize<GoogleBook>(rawBook, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error deserializing Google Book in GetBookAsync. Title={Title} Author={Author} RawPreview={Raw} Url={Url}", bookTitle, author, Truncate(rawBook, 1000), query);
                    return new GoogleBook();
                }

                _logger.LogInformation("Google Books GetBookAsync succeeded. Title={Title} Author={Author} Status=200 TotalItems={TotalItems} Url={Url} DurationMs={DurationMs}", bookTitle, author, book?.totalItems, query, sw.ElapsedMilliseconds);
                return book ?? new GoogleBook();
            }
            catch (GoogleBooksKeyNotConfiguredException)
            {
                throw; // re-throw so callers can warn the user about the missing API key
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error in GetBookAsync. Title={Title} Author={Author} Url={Url}", bookTitle, author, urlForLog ?? "<not-built>");
                return new GoogleBook();
            }
        }

        private static string MaskUrl(string url)
        {
            var keyIdx = url.IndexOf("key=", StringComparison.OrdinalIgnoreCase);
            if (keyIdx < 0) return url;

            var sb = new StringBuilder(url.Length + 8);
            sb.Append(url, 0, keyIdx + 4);
            sb.Append("***");
            var amp = url.IndexOf('&', keyIdx);
            if (amp >= 0) sb.Append(url, amp, url.Length - amp);
            return sb.ToString();
        }

        private static async Task<string?> TryReadBodyAsync(HttpResponseMessage resp, int maxLen)
        {
            try
            {
                var raw = await resp.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(raw)) return null;
                return raw.Length <= maxLen ? raw : raw.Substring(0, maxLen) + "...";
            }
            catch (Exception ex)
            {
                return $"<failed to read body: {ex.GetType().Name}: {ex.Message}>";
            }
        }

        private static string Truncate(string s, int maxLen)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= maxLen ? s : s.Substring(0, maxLen) + "...";
        }

        // Google error bodies look like: { "error": { "code": 429, "message": "Quota exceeded",
        //   "errors": [ { "message": "...", "reason": "quotaExceeded", "domain": "usageLimits", ... } ] } }
        // Extract the machine-readable reason so we can tell "quota exceeded" from "backend failed".
        private static string DescribeGoogleError(string? body)
        {
            if (string.IsNullOrWhiteSpace(body)) return "<empty body>";

            try
            {
                var parsed = JsonSerializer.Deserialize<GoogleErrorResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var err = parsed?.error;
                if (err is null) return Truncate(body, 2000);

                var reason = err.errors is null || err.errors.Count == 0
                    ? "<none>"
                    : string.Join(" | ", err.errors.Select(e =>
                        $"{e.reason ?? "?"}@{e.domain ?? "?"} '{e.message ?? ""}'"));

                return $"code={err.code} message='{err.message}' reason=[{reason}] body={Truncate(body, 2000)}";
            }
            catch
            {
                return Truncate(body, 2000);
            }
        }

        // Minimal DTOs for deserialization - reuse existing Application DTOs shape
        private class GoogleBooksResponse { public long? totalItems { get; set; } public Item[]? items { get; set; } }
        private class Item { public string? id { get; set; } public Volumeinfo? volumeInfo { get; set; } }
        private class Volumeinfo { public string? title { get; set; } public string? publishedDate { get; set; } public Imagelinks? imageLinks { get; set; } }
        private class Imagelinks { public string? thumbnail { get; set; } public string? smallThumbnail { get; set; } }
        private class GoogleErrorResponse { public GoogleErrorInfo? error { get; set; } }
        private class GoogleErrorInfo { public int? code { get; set; } public string? message { get; set; } public List<GoogleErrorDetail>? errors { get; set; } }
        private class GoogleErrorDetail { public string? message { get; set; } public string? reason { get; set; } public string? domain { get; set; } public string? location { get; set; } }
    }
}