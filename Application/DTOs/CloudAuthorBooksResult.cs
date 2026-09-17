using System.Collections.Generic;

namespace MediaOrganizerApp.Application.DTOs
{
    /// <summary>
    /// Outcome of the OpenRouter (cloud) author-books step. Carries the books
    /// when the cloud path succeeded and, when it produced nothing, a failure
    /// reason and per-model outcomes so the UI can explain and let the user
    /// decide whether to continue to the local AI provider.
    /// </summary>
    public sealed class CloudAuthorBooksResult
    {
        public List<GoogleBookInfo>? Books { get; set; }

        /// <summary>Model that resolved the request (first tried or the one that succeeded).</summary>
        public string? ResolvedModel { get; set; }

        /// <summary>User-facing explanation when the cloud step produced no books.</summary>
        public string? FailureReason { get; set; }

        /// <summary>Per-model outcome codes ("ok|N", "vazio", "no content", "429 Too Many Requests", ...).</summary>
        public Dictionary<string, string>? Outcomes { get; set; }

        public int AttemptedModelCount { get; set; }

        public bool WasCancelled { get; set; }
    }
}