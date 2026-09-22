using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Spinner;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using Web.Blazor.Services.Common;
using Web.Blazor.Services.ApiWrappers;
using Microsoft.Extensions.Configuration;

namespace Web.Blazor.Components.Authors
{
    public partial class AuthorBooks
    {
        [Parameter]
        public string? AuthorName { get; set; }

        [Parameter, EditorRequired]
        public int PageSize { get; set; } = 4;

        [Parameter]
        public bool DebugForceGoogle429 { get; set; } = false;

        [Inject] public BooksInfoProgressClient ProgressClient { get; set; } // Inject the client
        [Inject] public IConfiguration Configuration { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        private string aiProgressMessage = "";
        private bool showAiProgress = false;

        private CancellationTokenSource? _cts;
        protected SfSpinner SpinnerObj;

        private bool isImageViewerVisible;
        private GoogleBookInfo? SelectedBook = new();
        private List<GoogleBookInfo>? books;
        private bool booksLoading;
        private bool aiFallbackUsed;
        private bool googleRateLimited;
        private int currentPage = 1;

        private string? aiProviderMessage;
        private string? _usedAiModel;
        private readonly System.Diagnostics.Stopwatch _loaderStopwatch = new();

        private string SpinnerLabel { get; set; } = "";
        private bool SpinnerVisible { get; set; }

        private string MessageLabel { get; set; } = "";
        private bool MessageVisible { get; set; }

        private readonly SemaphoreSlim _spinnerLock = new(1, 1);
        private bool showAISearchDialog { get; set; } = false;

        // Ollama confirmation dialog (shown when the OpenRouter/cloud step produced no books)
        private bool showOllamaConfirmDialog { get; set; } = false;
        private string? _aiFlowAuthor;
        private string? _aiFlowLanguage;
        private string? _aiFlowConnectionId;
        private string? _cloudFailureReason;

        // results served from the static Google cache (informational only - the cache is never
        // invalidated from the UI; this flag exists so the UI can explain why results reappear).
        private bool booksFromCache;

        // results were fetched live from Google Books just now and written to the static cache
        // for the next visit (informational only; AI/Wikidata results are never cached).
        private bool booksJustCached;

        // results were fetched live from Wikidata (never cached).
        private bool booksFromWikidata;

        // per-primary-provider outcomes, shown when neither produced results (before the AI prompt)
        private string? _googleOutcome;
        private string? _wikidataOutcome;

        // load id: a superseded load (triggered by re-renders) must never write state nor the
        // static Google cache - that orphan write was the source of "Google & AI failed, yet
        // cached thumbnails appear on the next visit".
        private int _loadGeneration;

        // holds last provider result until assigned to books
        private List<GoogleBookInfo>? resultList = null;

        private string uiLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        // cache filtered books to avoid repeated deferred LINQ enumeration during render.
        // AI-sourced items may have no cover image (OpenRouter/Ollama), so the thumbnail
        // requirement only applies to Google/Wikidata results; AI results are shown in a table.
        // Items with low confidence (likely hallucinated) are filtered out.
        private List<GoogleBookInfo>? _filteredBooks;
        private IEnumerable<GoogleBookInfo> CachedFilteredBooks =>
            (_filteredBooks ??= (books ?? Enumerable.Empty<GoogleBookInfo>())
                .Where(b => !string.IsNullOrWhiteSpace(b.Title))
                .Where(b => !aiFallbackUsed || (b.Confidence ?? 0) >= 0.5)
                .Where(b => aiFallbackUsed || !string.IsNullOrWhiteSpace(b.Thumbnail))
                .ToList());

        private int TotalPages
        {
            get
            {
                var count = CachedFilteredBooks.Count();
                if (count == 0) return 1;
                return (int)Math.Ceiling(count / (double)PageSize);
            }
        }

        private IEnumerable<GoogleBookInfo> PagedBooks =>
            CachedFilteredBooks.Skip((currentPage - 1) * PageSize).Take(PageSize);

        protected override async Task OnInitializedAsync()
        {
            ProgressClient.ProgressEventReceived -= OnProgressEventReceived;
            ProgressClient.ProgressEventReceived += OnProgressEventReceived;
        }

        protected override async Task OnParametersSetAsync()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            currentPage = 1;
            await LoadBooksAsync(_cts.Token);
        }

        private void NextPage()
        {
            if (booksLoading) return;
            if (TotalPages <= 1) return;
            if (currentPage < TotalPages) currentPage++;
        }

        private void PrevPage()
        {
            if (booksLoading) return;
            if (TotalPages <= 1) return;
            if (currentPage > 1) currentPage--;
        }

        private void SetBooks(List<GoogleBookInfo> list)
        {
            books = list ?? new List<GoogleBookInfo>();
            _filteredBooks = null;
        }

        // Google-only cache: results from the Google Books API are more reliable than AI
        // output, and caching them reduces anonymous-IP quota usage (429 risk). Wikidata and
        // AI results are deliberately NOT cached.
        private static readonly ConcurrentDictionary<string, (DateTime fetchedAt, List<GoogleBookInfo> items)> _googleBooksCache = new();
        private static readonly TimeSpan GoogleCacheTtl = TimeSpan.FromMinutes(15);

        // Allow external components to force a fresh Google query for an author (e.g. Retry button)
        public static void ClearCacheForAuthor(string? author)
        {
            if (string.IsNullOrWhiteSpace(author)) return;
            _googleBooksCache.TryRemove(author, out _);
        }

        private bool TryUseGoogleCache(string author)
        {
            if (!_googleBooksCache.TryGetValue(author, out var cached))
                return false;

            if (DateTime.UtcNow - cached.fetchedAt >= GoogleCacheTtl)
            {
                _googleBooksCache.TryRemove(author, out _);
                return false;
            }

            if (cached.items is null || cached.items.Count == 0)
            {
                _googleBooksCache.TryRemove(author, out _);
                return false;
            }

            books = cached.items;
            _filteredBooks = null;
            aiFallbackUsed = false;
            booksFromCache = true;
            return true;
        }

        private async Task<List<GoogleBookInfo>?> TryFetchFromGoogleAsync(CancellationToken ct)
        {
            try
            {
                await UpdateSpinnerLabelAsync(L["LoadingMsg"]);

                var result = await GoogleBooksService.GetBooksByAuthorAsync(AuthorName!);

                if (result == null)
                {
                    _googleOutcome = L["AuthorSearchErrorGoogle", AuthorName];
                    MessageLabel = _googleOutcome;
                    await UpdateMessageLabelAsync(MessageLabel);
                    return null;
                }

                if (result.Count == 0)
                {
                    _googleOutcome = L["AuthorSearchNoGoogleResults", AuthorName];
                    MessageLabel = _googleOutcome;
                    await UpdateMessageLabelAsync(MessageLabel);
                    return null;
                }

                // A superseded/anonymous load must not immortalise its late success in the cache.
                if (ct.IsCancellationRequested)
                    return null;

                _googleBooksCache[AuthorName!] = (DateTime.UtcNow, result.ToList());
                booksJustCached = true;

                return result;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                googleRateLimited = true;
                _googleOutcome = L["AuthorSearchGoogleRateLimited", AuthorName];
                MessageLabel = _googleOutcome;
                await UpdateMessageLabelAsync(MessageLabel);
                return null;
            }
            catch (GoogleBooksKeyNotConfiguredException)
            {
                _googleOutcome = L["AuthorSearchGoogleNoKey"];
                MessageLabel = _googleOutcome;
                await UpdateMessageLabelAsync(MessageLabel);
                return null;
            }
            catch (Exception)
            {
                _googleOutcome = L["AuthorSearchErrorGoogle", AuthorName];
                MessageLabel = _googleOutcome;
                await UpdateMessageLabelAsync(MessageLabel);
                return null;
            }
        }
        private async Task<List<GoogleBookInfo>?> TryFetchFromWikidataAsync(CancellationToken ct)
        {
            try
            {
                MessageLabel = L["AuthorSearchSearchingWikidata", AuthorName];
                await UpdateMessageLabelAsync(MessageLabel);

                var wikidataResult = await WikidataClient.GetBooksByAuthorAsync(AuthorName!, uiLanguage);
                if (wikidataResult == null || wikidataResult.Count == 0)
                {
                    _wikidataOutcome = L["AuthorSearchWikidataNoResults", AuthorName];
                    MessageLabel = _wikidataOutcome;
                    await UpdateMessageLabelAsync(MessageLabel);
                    return null;
                }

                var wikiDataResults = new List<GoogleBookInfo>();
                foreach (var b in wikidataResult)
                {
                    wikiDataResults.Add(new GoogleBookInfo
                    {
                        Title = b.Title,
                        PublishedDate = b.PublicationYear.ToString(),
                        Thumbnail = b.CoverImage,
                        Confidence = null
                    });
                }

                return wikiDataResults;
            }
            catch (HttpRequestException)
            {
                _wikidataOutcome = L["AuthorSearchErrorWikidata", AuthorName];
                MessageLabel = _wikidataOutcome;
                await UpdateMessageLabelAsync(MessageLabel);
                return null;
            }
            catch (Exception)
            {
                _wikidataOutcome = L["AuthorSearchErrorWikidata", AuthorName];
                MessageLabel = _wikidataOutcome;
                await UpdateMessageLabelAsync(MessageLabel);
                return null;
            }
        }

        private async Task LoadBooksAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            books = null;
            resultList = null;
            _filteredBooks = null;

            if (string.IsNullOrWhiteSpace(AuthorName))
                return;

            // this load may write state/cache only while it is still the latest load
            var gen = ++_loadGeneration;

            // reset per-load UI flags
            booksLoading = true;
            SpinnerVisible = false;
            googleRateLimited = false;
            aiFallbackUsed = false;
            booksFromCache = false;
            booksJustCached = false;
            booksFromWikidata = false;
            _googleOutcome = null;
            _wikidataOutcome = null;
            showAISearchDialog = false;
            MessageVisible = false;
            MessageLabel = "";
            aiProviderMessage = null;

            try
            {

                if (TryUseGoogleCache(AuthorName!))
                    return;

                _loaderStopwatch.Restart();

                // Google
                var google = await TryFetchFromGoogleAsync(ct);
                if (gen != _loadGeneration || ct.IsCancellationRequested)
                    return; // superseded: this load must not touch state nor the cache

                if (google is { Count: > 0 })
                {
                    SetBooks(google);
                    return;
                }

                // Wikidata
                var wiki = await TryFetchFromWikidataAsync(ct);
                if (gen != _loadGeneration || ct.IsCancellationRequested)
                    return; // superseded: this load must not touch state nor the cache

                if (wiki is { Count: > 0 })
                {
                    booksFromWikidata = true;
                    SetBooks(wiki);
                    return;
                }

                // Ask confirmation for AI (always opt-in)
                MessageLabel = googleRateLimited
                    ? L["AuthorSearchGoogleRateLimitedCombined", AuthorName]
                    : L["AuthorSearchNoResultsGoogleWikidata", AuthorName];

                await UpdateMessageLabelAsync(MessageLabel);
                showAISearchDialog = true;
            }
            finally
            {
                if (gen == _loadGeneration)
                {
                    booksLoading = false;
                    SpinnerVisible = false;
                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        // Explicitly report what each primary provider returned when both failed (shown in the
        // AI confirmation dialog and in the no-results area).
        private string GetAISearchDialogMessage()
        {
            if (googleRateLimited)
                return L["TituloErroTooManyRequests"];

            var google = _googleOutcome ?? L["AuthorSearchNoGoogleResults", AuthorName];
            var wiki = _wikidataOutcome ?? L["AuthorSearchWikidataNoResults", AuthorName];
            return $"{google} | {wiki}";
        }

        async Task OnConfirmYes()
        {
            showAISearchDialog = false;

            await UpdateSpinnerLabelAsync(L["LoadingMsg"]);
            await SearchBooksUsingAI(AuthorName!, uiLanguage);
        }

        void OnConfirmNo()
        {
            showAISearchDialog = false;
            MessageLabel = L["AuthorSearchCancelled", AuthorName];
            _ = UpdateMessageLabelAsync(MessageLabel);
        }

        private async Task SearchBooksUsingAI(string authorName, string uiLanguage)
        {
            try
            {
                _usedAiModel = null;
                MessageLabel = L["AuthorSearchSearchingCloud", authorName];
                await UpdateMessageLabelAsync(MessageLabel);

                // Start ProgressClient before AI search
                var apiBase = Configuration["BaseUrl"]?.TrimEnd('/') ?? NavigationManager.BaseUri.TrimEnd('/');
                if (apiBase.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
                    apiBase = apiBase.Substring(0, apiBase.Length - 4);

                await ProgressClient.StartAsync(apiBase);
                var connectionId = ProgressClient.ConnectionId;

                var cloudOutcome = await AiBookClient.GetAuthorBooksCloudAsync(authorName, uiLanguage, connectionId);
                if (cloudOutcome?.Books is { Count: > 0 })
                {
                    resultList = cloudOutcome.Books.ToList();
                    aiProviderMessage = BuildProviderMessage(true, _usedAiModel);
                    await FinishAiSearchAsync(authorName);
                    return;
                }

                // OpenRouter falhou/não devolveu títulos: explicar o motivo e perguntar
                // ao utilizador se quer continuar com a IA local (Ollama).
                _aiFlowAuthor = authorName;
                _aiFlowLanguage = uiLanguage;
                _aiFlowConnectionId = connectionId;
                _cloudFailureReason = !string.IsNullOrWhiteSpace(cloudOutcome?.FailureReason)
                    ? cloudOutcome!.FailureReason!
                    : L["AuthorSearchCloudUnavailable", authorName];

                showOllamaConfirmDialog = true;
                MessageLabel = L["AuthorSearchCloudFailedReason", _cloudFailureReason];
                await UpdateMessageLabelAsync(MessageLabel);
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception)
            {
                resultList = new List<GoogleBookInfo>();
                aiProviderMessage = null;
                MessageLabel = _loaderStopwatch.IsRunning
                    ? L["AuthorSearchFailedAfter", AuthorName, _loaderStopwatch.Elapsed.TotalSeconds]
                    : L["AuthorSearchFailed", AuthorName];
                await UpdateMessageLabelAsync(MessageLabel);
                await FinishAiSearchAsync(AuthorName!);
            }
        }

        async Task OnOllamaConfirmYes()
        {
            showOllamaConfirmDialog = false;
            await UpdateSpinnerLabelAsync(L["LoadingMsg"]);

            var author = _aiFlowAuthor ?? AuthorName!;
            try
            {
                var aiResult = await AiBookClient.GetAuthorBooksAsync(
                    _aiFlowAuthor!, _aiFlowLanguage!, _aiFlowConnectionId, null, "cloud-fallback");
                if (aiResult != null && aiResult.Count > 0)
                {
                    resultList = aiResult.ToList();
                    aiProviderMessage = BuildProviderMessage(false, _usedAiModel);
                }
                else
                {
                    resultList = new List<GoogleBookInfo>();
                    aiProviderMessage = null;
                    MessageLabel = L["AuthorSearchNoBooksFromAi", author];
                    await UpdateMessageLabelAsync(MessageLabel);
                }
            }
            catch (Exception)
            {
                resultList = new List<GoogleBookInfo>();
                aiProviderMessage = null;
                MessageLabel = _loaderStopwatch.IsRunning
                    ? L["AuthorSearchFailedAfter", author, _loaderStopwatch.Elapsed.TotalSeconds]
                    : L["AuthorSearchFailed", author];
                await UpdateMessageLabelAsync(MessageLabel);
            }
            finally
            {
                await FinishAiSearchAsync(author);
            }
        }

        void OnOllamaConfirmNo()
        {
            showOllamaConfirmDialog = false;
            _ = HandleOllamaDeclinedAsync();
        }

        private async Task HandleOllamaDeclinedAsync()
        {
            var author = _aiFlowAuthor ?? AuthorName!;
            resultList = new List<GoogleBookInfo>();
            aiProviderMessage = null;
            MessageLabel = L["AuthorSearchCloudCancelledNoBooks", author];
            await UpdateMessageLabelAsync(MessageLabel);
            await FinishAiSearchAsync(author);
        }

        private async Task FinishAiSearchAsync(string authorName)
        {
            books = resultList ?? new List<GoogleBookInfo>();
            _filteredBooks = null;
            aiFallbackUsed = true;

            SpinnerVisible = false;
            showOllamaConfirmDialog = false;

            if (books.Count == 0)
            {
                MessageLabel = L["AuthorSearchNoBooksChain", authorName, _loaderStopwatch.Elapsed.TotalSeconds];
                await UpdateMessageLabelAsync(MessageLabel);
            }

            ProgressClient.ProgressEventReceived -= OnProgressEventReceived;
            showAiProgress = false;
            _loaderStopwatch.Stop();

            await InvokeAsync(StateHasChanged);
        }

        private void ShowImageViewer(GoogleBookInfo book)
        {
            SelectedBook = book;
            isImageViewerVisible = true;
        }

        private static bool HasUsableThumbnail(GoogleBookInfo book)
        {
            return GoogleBookInfo.IsUsableCover(book?.Thumbnail);
        }

        private static bool IsGoogleLinked(string? id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            return !id.StartsWith("llm:", StringComparison.OrdinalIgnoreCase);
        }

        private static string ConfidenceCss(double? confidence)
        {
            if (confidence is null) return "low";
            if (confidence >= 0.7) return "good";
            if (confidence >= 0.45) return "mid";
            return "low";
        }

        private static string ConfidenceText(double? confidence)
        {
            if (confidence is null) return "\u2013";
            var percent = Math.Clamp((int)Math.Round(confidence.Value * 100), 0, 100);
            return $"{percent}%";
        }

        private void CloseImageViewer()
        {
            isImageViewerVisible = false;
        }

        // Spinner label update that actually paints
        private async Task UpdateSpinnerLabelAsync(string label)
        {
            SpinnerLabel = label ?? "";
            SpinnerVisible = true;

            await InvokeAsync(StateHasChanged);
            await Task.Yield();
        }

        // Message update: use InvokeAsync for consistency and reliability
        private Task UpdateMessageLabelAsync(string label)
        {
            MessageLabel = label ?? "";
            MessageVisible = true;
            return InvokeAsync(StateHasChanged);
        }

        private void OnProgressEventReceived(ProgressUpdateDto update)
        {
            if (update.Stage == "cloud_success")
            {
                var parts = (update.Message ?? string.Empty).Split('|');
                if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
                    _usedAiModel = parts[0];
            }
            else if (update.Stage == "provider" && !string.IsNullOrWhiteSpace(update.Message))
            {
                _usedAiModel = update.Message;
            }

            aiProgressMessage = FormatProgressMessage(update);
            showAiProgress = true;
            InvokeAsync(StateHasChanged);
        }

        private string BuildProviderMessage(bool isOpenRouter, string? model)
        {
            if (string.IsNullOrWhiteSpace(model))
                return isOpenRouter ? L["AuthorSearchProviderOpenRouter"] : L["AuthorSearchProviderOllama"];
            return isOpenRouter
                ? L["AuthorSearchProviderOpenRouterModel", model].Value
                : L["AuthorSearchProviderOllamaModel", model].Value;
        }

        private string? FormatProgressMessage(ProgressUpdateDto update)
        {
            switch (update.Stage)
            {
                case "cloud_try":
                    return L["AuthorSearchSweepTrying", update.Message];
                case "cloud_failed":
                    return L["AuthorSearchSweepFailed", update.Message];
                case "cloud_empty":
                    return L["AuthorSearchSweepEmpty", update.Message];
                case "cloud_success":
                    var parts = (update.Message ?? string.Empty).Split('|');
                    var model = parts.Length > 0 ? parts[0] : string.Empty;
                    var count = parts.Length > 1 && int.TryParse(parts[1], out var n) ? n : 0;
                    return L["AuthorSearchSweepSuccess", model, count];
                case "cloud_fallback":
                    return L["AuthorSearchSweepFallback"];
                default:
                    return update.Message;
            }
        }
    }
}
