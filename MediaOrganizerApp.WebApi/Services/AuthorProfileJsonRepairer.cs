using MediaOrganizerApp.Application.DTOs.AI;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Services
{
    /// <summary>
    /// Helper to robustly parse or attempt to repair JSON produced by LLMs for author profiles.
    /// Tries direct deserialization, lightweight heuristics, and (optionally) a model-driven repair function.
    /// </summary>
    internal static class AuthorProfileJsonRepairer
    {
        public static async Task<AiAuthorProfileResponseDto> ParseOrRepairAsync(
            string raw,
            Func<string, Task<string>>? repairWithModelAsync = null,
            ILogger? logger = null,
            string? language = null)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new AiAuthorProfileResponseDto
                {
                    Bio = null,
                    Confidence = null,
                    Notes = "Empty model response."
                };
            }

            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // 1) Try direct parse
            try
            {
                var parsed = JsonSerializer.Deserialize<AiAuthorProfileResponseDto>(raw, opts);
                if (parsed != null)
                {
                    Normalize(parsed);
                    return parsed;
                }
            }
            catch (Exception ex)
            {
                logger?.LogDebug(ex, "Direct JSON parse failed for LLM response.");
            }

            // 2) Heuristic extraction and cleanup
            try
            {
                var json = ExtractJsonObject(raw);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    // common fixes
                    json = json.Replace("'", "\""); // single -> double quotes
                    json = Regex.Replace(json, @",\s*([}\]])", "$1"); // trailing commas

                    // normalize whitespace and remove control characters that break JSON parsing
                    json = Regex.Replace(json, "[\r\n\t]+", " ");

                    // add quotes around unquoted property names: { bio: "x" } -> { "bio": "x" }
                    json = Regex.Replace(json, @"(?<=[{,]\s?)([A-Za-z0-9_]+)\s*:", "\"$1\":");

                    var parsed = JsonSerializer.Deserialize<AiAuthorProfileResponseDto>(json, opts);
                    if (parsed != null)
                    {
                        Normalize(parsed);
                        logger?.LogDebug("Parsed LLM response after heuristic cleanup.");
                        return parsed;
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogDebug(ex, "Heuristic JSON cleanup failed.");
            }

            // 3) Ask model to repair (if callback provided)
            if (repairWithModelAsync is not null)
            {
                try
                {
                    var repairPrompt = BuildRepairPrompt(raw, language);
                    var repaired = await repairWithModelAsync(repairPrompt);
                    if (!string.IsNullOrWhiteSpace(repaired))
                    {
                        try
                        {
                            var parsed = JsonSerializer.Deserialize<AiAuthorProfileResponseDto>(repaired, opts);
                            if (parsed != null)
                            {
                                Normalize(parsed);
                                logger?.LogInformation("Repaired LLM response using secondary model call.");
                                return parsed;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger?.LogDebug(ex, "Parsing repaired response failed.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "Model repair attempt failed.");
                }
            }

            // 4) Final fallback: return the raw text as Bio with low confidence and notes containing truncated raw output
            logger?.LogWarning("LLM response could not be parsed or repaired; returning raw text as fallback.");
            // Final fallback: do not place raw model output into the Bio shown to users.
            // Preserve a short truncated raw snippet in Notes so diagnostics are available.
            return new AiAuthorProfileResponseDto
            {
                Bio = null,
                Confidence = null,
                Notes = "Could not parse LLM JSON response. Raw output (truncated): " + Truncate(raw, 1000)
            };
        }

        private static string BuildRepairPrompt(string raw, string? language)
        {
            var sb = new StringBuilder();
            sb.AppendLine("The following text is a malformed response from an assistant. Return ONLY valid JSON matching this schema and nothing else:");
            sb.AppendLine("{ \"bio\": string|null, \"confidence\": number|null, \"notes\": string|null }");
            sb.AppendLine();
            sb.AppendLine("If unsure, set uncertain values to null and explain in notes. Do not add any surrounding text, markdown, or explanation.");
            if (!string.IsNullOrWhiteSpace(language))
            {
                sb.AppendLine($"Return the values localized in language: {language}.\n");
            }
            sb.AppendLine();
            sb.AppendLine("Example:");
            sb.AppendLine("{ \"bio\": \"Born ...\", \"confidence\": 0.75, \"notes\": null }");
            sb.AppendLine();
            sb.AppendLine("Malformed text follows:");
            sb.AppendLine(raw);
            return sb.ToString();
        }

        private static string ExtractJsonObject(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            // Fast heuristic: prefer a substring that contains the expected key names (bio/confidence/notes)
            // Search for the earliest '{' and a matching '}' that encloses one of the keys.
            for (int start = s.IndexOf('{'); start >= 0 && start < s.Length; start = s.IndexOf('{', start + 1))
            {
                for (int end = s.LastIndexOf('}'); end > start; end = s.LastIndexOf('}', end - 1))
                {
                    var candidate = s.Substring(start, end - start + 1);
                    if (candidate.IndexOf("\"bio\"", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        candidate.IndexOf("\'bio\'", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        candidate.IndexOf("confidence", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return candidate;
                    }
                }
            }

            // Fallback: return the first brace-delimited region if nothing obvious found
            var first = s.IndexOf('{');
            var last = s.LastIndexOf('}');
            if (first >= 0 && last > first)
                return s.Substring(first, last - first + 1);
            return string.Empty;
        }

        private static void Normalize(AiAuthorProfileResponseDto dto)
        {
            // Ensure confidence is within 0..1 or null
            if (dto.Confidence is not null)
            {
                if (double.IsNaN(dto.Confidence.Value) || double.IsInfinity(dto.Confidence.Value))
                    dto.Confidence = null;
                else if (dto.Confidence < 0 || dto.Confidence > 1)
                    dto.Confidence = Math.Clamp(dto.Confidence.Value, 0.0, 1.0);
            }
        }

        private static string Truncate(string s, int maxLen)
        {
            if (s == null) return string.Empty;
            return s.Length <= maxLen ? s : s.Substring(0, maxLen) + "...";
        }
    }
}
