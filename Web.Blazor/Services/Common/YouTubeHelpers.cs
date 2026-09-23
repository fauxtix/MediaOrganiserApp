using System;
using System.Linq;

namespace Web.Blazor.Services.Common
{
    public static class YouTubeHelpers
    {
        public static string GetVideoId(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return null;

            if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
                Uri.TryCreate("https://" + fileUrl.Trim(), UriKind.Absolute, out uri);

            if (uri is null)
                return null;

            string host = uri.Host.ToLowerInvariant();

            if (host.EndsWith("youtu.be"))
            {
                string id = uri.Segments.Length > 1 ? uri.Segments[^1].TrimEnd('/') : null;
                return string.IsNullOrWhiteSpace(id) ? null : id;
            }

            if (!host.EndsWith("youtube.com"))
                return null;

            var segments = uri.Segments
                .Where(s => s.Length > 1)
                .Select(s => s.TrimEnd('/'))
                .ToArray();

            if (segments.Length == 0)
                return null;

            string first = segments[0].ToLowerInvariant();

            switch (first)
            {
                case "watch":
                    return GetQueryParam(uri.Query, "v");
                case "shorts":
                case "embed":
                case "live":
                case "v":
                    return segments.Length > 1 ? segments[1] : null;
                default:
                    return null;
            }
        }

        private static string GetQueryParam(string query, string key)
        {
            if (string.IsNullOrEmpty(query))
                return null;

            if (query[0] == '?')
                query = query.Substring(1);

            foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                int idx = pair.IndexOf('=');
                string name = idx >= 0 ? pair.Substring(0, idx) : pair;
                string value = idx >= 0 ? pair.Substring(idx + 1) : null;

                if (string.Equals(name, key, StringComparison.OrdinalIgnoreCase))
                    return Uri.UnescapeDataString(value);
            }

            return null;
        }
    }
}