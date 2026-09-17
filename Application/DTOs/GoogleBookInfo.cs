using System;

namespace MediaOrganizerApp.Application.DTOs
{
    public class GoogleBookInfo
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? PublishedDate { get; set; }
        public string? Publisher { get; set; }
        public string? Thumbnail { get; set; }
        // Confidence score when provided by AI fallback (0.0 - 1.0)
        public double? Confidence { get; set; }
        // Indicates whether the item was synthesized by the AI (true) or derived from authoritative data (false/null)
        public bool? Synthesized { get; set; }
        // Optional source (URL or identifier) indicating where the item was verified (e.g., Google Books URL)
        public string? Source { get; set; }

        /// <summary>
        /// Determines whether a thumbnail string is a real, renderable cover URL.
        /// Rejects blank values, known placeholders ("NoPhoto", "example.com", "placeholder",
        /// ".invalid", localhost) and non-http(s) values to avoid showing broken images or the
        /// placeholder URLs that AI models pick from prompt examples.
        /// </summary>
        public static bool IsUsableCover(string? thumbnail)
        {
            if (string.IsNullOrWhiteSpace(thumbnail)) return false;
            if (!thumbnail.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !thumbnail.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return false;

            var lower = thumbnail.ToLowerInvariant();
            if (lower.Contains("nophoto") || lower.Contains("no-photo") ||
                lower.Contains("no_image") || lower.Contains("no-image")) return false;
            if (lower.Contains("example.com") || lower.Contains("placeholder") ||
                lower.Contains(".invalid") || lower.Contains("localhost")) return false;
            return true;
        }
    }
}