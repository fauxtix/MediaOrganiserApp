using Microsoft.AspNetCore.Components;
using System.Net;

namespace Web.Blazor.Services.EmailServices
{
    public class EmailTemplateService
    {

        private readonly HttpClient _http;
        private readonly NavigationManager _nav;

        public EmailTemplateService(HttpClient http, NavigationManager nav)
        {
            _http = http;
            _nav = nav;
        }

        // Load template from wwwroot (relative path: "templates/error-report.html")
        public async Task<string> LoadTemplateAsync(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return string.Empty;

            // If caller already passed an absolute URL, use it directly
            if (Uri.IsWellFormedUriString(relativePath, UriKind.Absolute))
            {
                return await _http.GetStringAsync(relativePath);
            }

            // Build absolute URI using the NavigationManager base URI (works in Blazor Server and WASM)
            var baseUri = _nav.BaseUri; // e.g. "https://localhost:5001/"
            var absolute = new Uri(new Uri(baseUri), relativePath).ToString();
            return await _http.GetStringAsync(absolute);
        }

        // Render template by replacing tokens like {{Key}} with HTML-encoded values
        public string Render(string template, IDictionary<string, string> values)
        {
            if (string.IsNullOrEmpty(template)) return template ?? string.Empty;

            foreach (var kv in values)
            {
                var token = "{{" + kv.Key + "}}";

                // Allow raw HTML ONLY for this placeholder
                if (kv.Key == "ExceptionHtml")
                {
                    template = template.Replace(token, kv.Value ?? string.Empty);
                }
                else
                {
                    var encoded = WebUtility.HtmlEncode(kv.Value ?? "");
                    template = template.Replace(token, encoded);
                }
            }

            return template;
        }
        // Convenience method to build exception HTML block (already HTML encoded)
        public static string BuildExceptionHtml(string exception)
        {
            if (string.IsNullOrWhiteSpace(exception))
                return string.Empty;

            var encodedException = WebUtility.HtmlEncode(exception);

            return $@"<h4 style=""margin-top:16px;color:#343a40;margin-bottom:8px;"">Exception</h4>
                    <pre style=""background:#f8f9fa;padding:12px;border-radius:6px;color:#842029;border:1px solid #f5c2c7;white-space:pre-wrap;"">
                {encodedException}</pre>";
        }
    }
}