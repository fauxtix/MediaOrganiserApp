using System.Text;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services;

/// <summary>
/// Shared helpers for extracting text from Ollama /api/generate responses.
/// </summary>
internal static class OllamaResponseHelper
{
    /// <summary>
    /// Extracts the concatenated "response" text from either a single Ollama JSON object
    /// or from an NDJSON stream of partial response objects.
    /// </summary>
    internal static string? TryExtractResponseText(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("response", out var resp))
                return resp.GetString();

            return json;
        }
        catch
        {
            // NDJSON fallback: concatenate "response" fragments from each line.
            var lines = json.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    if (doc.RootElement.TryGetProperty("response", out var resp))
                        sb.Append(resp.GetString());
                }
                catch
                {
                    // ignore non-JSON lines
                }
            }

            var merged = sb.ToString();
            return string.IsNullOrWhiteSpace(merged) ? null : merged;
        }
    }

    /// <summary>
    /// Extracts the first complete JSON object from a string that may contain surrounding text.
    /// </summary>
    internal static string? TryExtractFirstJsonObject(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var start = text.IndexOf('{');
        if (start < 0)
            return null;

        var depth = 0;
        var inString = false;
        var escape = false;

        for (var i = start; i < text.Length; i++)
        {
            var ch = text[i];

            if (inString)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escape = true;
                    continue;
                }

                if (ch == '\"')
                    inString = false;

                continue;
            }

            if (ch == '\"')
            {
                inString = true;
                continue;
            }

            if (ch == '{')
                depth++;
            else if (ch == '}')
            {
                depth--;
                if (depth == 0)
                    return text.Substring(start, i - start + 1);
            }
        }

        return null;
    }
}
