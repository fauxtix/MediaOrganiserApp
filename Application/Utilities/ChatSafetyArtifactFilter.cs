using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MediaOrganizerApp.Application.Utilities;

/// <summary>
/// Detects and removes content-safety classification lines (e.g.
/// "User Safety: safe;", "Response Safety: safe", "Safety Categories: ...")
/// that some guard/free models emit instead of, or before, a real answer.
/// These are model artifacts, not chat answers.
/// </summary>
public static partial class ChatSafetyArtifactFilter
{
    [GeneratedRegex(
        @"^(?:[-*+]\s*)?(?:user\s+safety|response\s+safety|safety\s+categories|safety\s+rating|content\s+safety)\s*:.*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SafetyLineRegex();

    private static bool IsSafetyLine(string rawLine)
    {
        var normalized = rawLine.Trim().Trim('*', '_', '`', '#', '>', '-', ' ').Trim();
        return normalized.Length > 0 && SafetyLineRegex().IsMatch(normalized);
    }

    /// <summary>
    /// True when the text contains only safety-classification lines (nothing else).
    /// </summary>
    public static bool IsSafetyOnly(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var hasLine = false;
        foreach (var raw in text.Split('\n'))
        {
            if (raw.Trim().Length == 0)
                continue;

            if (!IsSafetyLine(raw))
                return false;

            hasLine = true;
        }

        return hasLine;
    }

    /// <summary>
    /// Removes any safety-classification lines from the text, keeping the rest.
    /// </summary>
    public static string StripSafetyLines(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text ?? string.Empty;

        var kept = new List<string>();
        foreach (var raw in text.Split('\n'))
        {
            if (IsSafetyLine(raw))
                continue;

            kept.Add(raw);
        }

        var result = string.Join('\n', kept).Trim();

        // Never let the strip blank a real answer: if removing the lines would erase
        // everything but the text was not a safety-only artifact, keep the original.
        if (result.Length == 0 && !string.IsNullOrWhiteSpace(text) && !IsSafetyOnly(text))
            return text;

        return result;
    }
}
