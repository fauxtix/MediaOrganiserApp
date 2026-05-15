using MediaOrganizerApp.Application.DTOs;
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
            ProgressClient.ProgressReceived -= OnProgressReceived;
            ProgressClient.ProgressReceived += OnProgressReceived;
        }

        protected override async Task OnParametersSetAsync()
        {

            Logger.LogWarning("AuthorBooks params: AuthorName={AuthorName}, DebugForceGoogle429={DebugForceGoogle429}",
                AuthorName, DebugForceGoogle429);
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

            MessageLabel = "Books cached result";
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

                if (DebugForceGoogle429)
                    throw new HttpRequestException("Debug forced 429", null, HttpStatusCode.TooManyRequests);

                var result = await GoogleBooksService.GetBooksByAuthorAsync(AuthorName!);

                if (result == null)
                {
                    MessageLabel = $"Error fetching Google Books for {AuthorName}; trying Wikidata.";
                    await UpdateMessageLabelAsync(MessageLabel);
                    return null;
                }

                if (result.Count == 0)
                {
                    MessageLabel = $"No Google results for {AuthorName}; trying Wikidata.";
                    await UpdateMessageLabelAsync(MessageLabel);
                    return null;
                }

                return result;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                googleRateLimited = true;
                MessageLabel = $"Google Books rate-limited for {AuthorName}; trying Wikidata.";
                await UpdateMessageLabelAsync(MessageLabel);
                return null;
            }
            catch (Exception)
            {
                MessageLabel = $"Error fetching Google Books for {AuthorName}; trying Wikidata.";
                await UpdateMessageLabelAsync(MessageLabel);
                return null;
            }
        }
        private async Task<List<GoogleBookInfo>?> TryFetchFromWikidataAsync(CancellationToken ct)
        {
            try
            {
                MessageLabel = $"Searching Wikidata for {AuthorName}...";
                await UpdateMessageLabelAsync(MessageLabel);

                var wikidataResult = await WikidataClient.GetBooksByAuthorAsync(AuthorName!, uiLanguage);
                if (wikidataResult == null || wikidataResult.Count == 0)
                {
                    MessageLabel = $"Wikidata returned no results for {AuthorName}.";
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
                MessageLabel = $"Error fetching Wikidata results for {AuthorName}.";
                await UpdateMessageLabelAsync(MessageLabel);
                return null;
            }
            catch (Exception)
            {
                MessageLabel = $"Error fetching Wikidata results for {AuthorName}.";
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

            try
            {

                // 1) Cache
                if (TryUseCacheOrClearBadCache(AuthorName!))
                    return;

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
                    ? $"Google is rate-limited and Wikidata has no results for {AuthorName}"
                    : $"No results for {AuthorName} via Google/Wikidata.";

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
            await InvokeAsync(StateHasChanged);
        }

        void OnConfirmNo()
        {
            showAISearchDialog = false;
            MessageLabel = $"AI search cancelled for {AuthorName}.";
            _ = UpdateMessageLabelAsync(MessageLabel);
        }

        private async Task SearchBooksUsingAI(string authorName, string uiLanguage)
        {
            try
            {
                MessageLabel = $"Searching using AI for {AuthorName}...";
                await UpdateMessageLabelAsync(MessageLabel);

                // Start ProgressClient before AI search
                var apiBase = Configuration["BaseUrl"]?.TrimEnd('/') ?? NavigationManager.BaseUri.TrimEnd('/');
                if (apiBase.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
                    apiBase = apiBase.Substring(0, apiBase.Length - 4);

                await ProgressClient.StartAsync(apiBase);
                var connectionId = ProgressClient.ConnectionId;

                var aiResult = await AiBookClient.GetAuthorBooksAsync(authorName, uiLanguage, connectionId);
                if (aiResult != null && aiResult.Count > 0)
                {
                    resultList = aiResult.ToList();
                }
                else
                {
                    resultList = new List<GoogleBookInfo>();
                    MessageLabel = $"AI search returned no Books for {AuthorName}.";
                    await UpdateMessageLabelAsync(MessageLabel);
                }
            }
            catch (Exception)
            {
                resultList = new List<GoogleBookInfo>();
                MessageLabel = $"AI search failed for {AuthorName}.";
                await UpdateMessageLabelAsync(MessageLabel);
            }
            finally
            {
                ProgressClient.ProgressReceived -= OnProgressReceived;
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

        private void OnProgressReceived(string message)
        {
            aiProgressMessage = message;
            showAiProgress = true;
            InvokeAsync(StateHasChanged);
        }
    }
}
