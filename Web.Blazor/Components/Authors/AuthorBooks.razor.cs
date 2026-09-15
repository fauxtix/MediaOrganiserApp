using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Spinner;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using Web.Blazor.Services.Common;
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
        private readonly System.Diagnostics.Stopwatch _loaderStopwatch = new();

        private string SpinnerLabel { get; set; } = "";
        private bool SpinnerVisible { get; set; }

        private string MessageLabel { get; set; } = "";
        private bool MessageVisible { get; set; }

        private readonly SemaphoreSlim _spinnerLock = new(1, 1);
        private bool showAISearchDialog { get; set; } = false;

        // holds last provider result until assigned to books
        private List<GoogleBookInfo>? resultList = null;

        private string uiLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        // cache filtered books to avoid repeated deferred LINQ enumeration during render
        private List<GoogleBookInfo>? _filteredBooks;
        private IEnumerable<GoogleBookInfo> CachedFilteredBooks =>
            (_filteredBooks ??= (books ?? Enumerable.Empty<GoogleBookInfo>())
                .Where(b => !string.IsNullOrWhiteSpace(b.Title) && !string.IsNullOrWhiteSpace(b.Thumbnail))
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

        private static readonly ConcurrentDictionary<string, (DateTime fetchedAt, List<GoogleBookInfo> items, string source)> _booksCache = new();
        private static readonly TimeSpan GoogleCacheTtl = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan AiCacheTtl = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan WikidataCacheTtl = TimeSpan.FromMinutes(30);

        // Allow external components to clear the client-side cache for an author (e.g. Retry button)
        public static void ClearCacheForAuthor(string? author)
        {
            if (string.IsNullOrWhiteSpace(author)) return;
            _booksCache.TryRemove(author, out _);
        }

        private static TimeSpan GetTtlForSource(string? source)
        {
            if (string.Equals(source, "AI", StringComparison.OrdinalIgnoreCase)) return AiCacheTtl;
            if (string.Equals(source, "Wikidata", StringComparison.OrdinalIgnoreCase)) return WikidataCacheTtl;
            return GoogleCacheTtl;
        }

        private bool TryUseCacheOrClearBadCache(string author)
        {
            if (!_booksCache.TryGetValue(author, out var cached))
                return false;

            var ttl = GetTtlForSource(cached.source);
            if (DateTime.UtcNow - cached.fetchedAt >= ttl)
            {
                _booksCache.TryRemove(author, out _);
                return false;
            }

            if (cached.items is null || cached.items.Count == 0)
            {
                _booksCache.TryRemove(author, out _);
                return false;
            }

            books = cached.items;
            _filteredBooks = null;
            aiFallbackUsed = string.Equals(cached.source, "AI", StringComparison.OrdinalIgnoreCase);

            MessageLabel = L["BooksCachedMessage"];
            _ = UpdateMessageLabelAsync(MessageLabel);
            return true;
        }

        private void SetBooksAndCache(List<GoogleBookInfo> list, string source)
        {
            books = list ?? new List<GoogleBookInfo>();
            _filteredBooks = null;

            _booksCache[AuthorName!] = (DateTime.UtcNow, books, source);
        }

        private async Task<List<GoogleBookInfo>?> TryFetchFromGoogleAsync(CancellationToken ct)
        {
            try
            {
                await UpdateSpinnerLabelAsync(L["LoadingMsg"]);

                var result = await GoogleBooksService.GetBooksByAuthorAsync(AuthorName!);

                if (result == null)
                {
                    MessageLabel = L["AuthorSearchErrorGoogle", AuthorName];
                    await UpdateMessageLabelAsync(MessageLabel);
                    return null;
                }

                if (result.Count == 0)
                {
                    MessageLabel = L["AuthorSearchNoGoogleResults", AuthorName];
                    await UpdateMessageLabelAsync(MessageLabel);
                    return null;
                }

                return result;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                googleRateLimited = true;
                MessageLabel = L["AuthorSearchGoogleRateLimited", AuthorName];
                await UpdateMessageLabelAsync(MessageLabel);
                return null;
            }
            catch (Exception)
            {
                MessageLabel = L["AuthorSearchErrorGoogle", AuthorName];
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
                    MessageLabel = L["AuthorSearchWikidataNoResults", AuthorName];
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
                MessageLabel = L["AuthorSearchErrorWikidata", AuthorName];
                await UpdateMessageLabelAsync(MessageLabel);
                return null;
            }
            catch (Exception)
            {
                MessageLabel = L["AuthorSearchErrorWikidata", AuthorName];
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

            // reset per-load UI flags
            booksLoading = true;
            SpinnerVisible = false;
            googleRateLimited = false;
            aiFallbackUsed = false;
            showAISearchDialog = false;
            MessageVisible = false;
            MessageLabel = "";
            aiProviderMessage = null;

            try
            {

                // 1) Cache
                if (TryUseCacheOrClearBadCache(AuthorName!))
                    return;

                // Time the whole Google/Wikidata/OpenRouter/Ollama attempt
                _loaderStopwatch.Restart();

                // 2) Google
                var google = await TryFetchFromGoogleAsync(ct);
                if (google is { Count: > 0 })
                {
                    SetBooksAndCache(google, source: "Google");
                    return;
                }

                // 3) Wikidata
                var wiki = await TryFetchFromWikidataAsync(ct);
                if (wiki is { Count: > 0 })
                {
                    SetBooksAndCache(wiki, source: "Wikidata");
                    return;
                }

                // 4) Ask confirmation for AI (always opt-in)
                MessageLabel = googleRateLimited
                    ? L["AuthorSearchGoogleRateLimitedCombined", AuthorName]
                    : L["AuthorSearchNoResultsGoogleWikidata", AuthorName];

                await UpdateMessageLabelAsync(MessageLabel);
                showAISearchDialog = true;
            }
            finally
            {
                booksLoading = false;
                SpinnerVisible = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        async Task OnConfirmYes()
        {
            showAISearchDialog = false;

            await UpdateSpinnerLabelAsync(L["LoadingMsg"]);
            await SearchBooksUsingAI(AuthorName!, uiLanguage);

            books = resultList ?? new List<GoogleBookInfo>();
            _filteredBooks = null;
            aiFallbackUsed = true;

            _booksCache[AuthorName!] = (DateTime.UtcNow, books, "AI");

            SpinnerVisible = false;

            _loaderStopwatch.Stop();
            if (books.Count == 0)
            {
                MessageLabel = L["AuthorSearchNoBooksChain", AuthorName, _loaderStopwatch.Elapsed.TotalSeconds];
                await UpdateMessageLabelAsync(MessageLabel);
            }

            await InvokeAsync(StateHasChanged);
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
                MessageLabel = L["AuthorSearchSearchingCloud", AuthorName];
                await UpdateMessageLabelAsync(MessageLabel);

                // Start ProgressClient before AI search
                var apiBase = Configuration["BaseUrl"]?.TrimEnd('/') ?? NavigationManager.BaseUri.TrimEnd('/');
                if (apiBase.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
                    apiBase = apiBase.Substring(0, apiBase.Length - 4);

                await ProgressClient.StartAsync(apiBase);
                var connectionId = ProgressClient.ConnectionId;

                var cloudResult = await AiBookClient.GetAuthorBooksCloudAsync(authorName, uiLanguage, connectionId);
                if (cloudResult != null && cloudResult.Count > 0)
                {
                    resultList = cloudResult.ToList();
                    aiProviderMessage = L["AuthorSearchProviderOpenRouter"];
                }
                else
                {
                    var aiResult = await AiBookClient.GetAuthorBooksAsync(authorName, uiLanguage, connectionId);
                    if (aiResult != null && aiResult.Count > 0)
                    {
                        resultList = aiResult.ToList();
                        aiProviderMessage = L["AuthorSearchProviderOllama"];
                    }
                    else
                    {
                        resultList = new List<GoogleBookInfo>();
                        aiProviderMessage = null;
                        MessageLabel = L["AuthorSearchNoBooksFromAi", AuthorName];
                        await UpdateMessageLabelAsync(MessageLabel);
                    }
                }
            }
            catch (Exception)
            {
                resultList = new List<GoogleBookInfo>();
                aiProviderMessage = null;
                MessageLabel = _loaderStopwatch.IsRunning
                    ? L["AuthorSearchFailedAfter", AuthorName, _loaderStopwatch.Elapsed.TotalSeconds]
                    : L["AuthorSearchFailed", AuthorName];
                await UpdateMessageLabelAsync(MessageLabel);
            }
            finally
            {
                ProgressClient.ProgressEventReceived -= OnProgressEventReceived;
                showAiProgress = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        private void ShowImageViewer(GoogleBookInfo book)
        {
            SelectedBook = book;
            isImageViewerVisible = true;
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
            aiProgressMessage = FormatProgressMessage(update);
            showAiProgress = true;
            InvokeAsync(StateHasChanged);
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
