using MediaOrganizerApp.Application.DTOs;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Spinner;
using System.Collections.Concurrent;
using System.Globalization;

namespace Web.Blazor.Components.Authors
{
    public partial class AuthorBooks
    {
        [Parameter]
        public string? AuthorName { get; set; }

        [Parameter, EditorRequired]
        public int PageSize { get; set; } = 4;

        private CancellationTokenSource? _cts;
        protected SfSpinner SpinnerObj;

        private bool isImageViewerVisible;
        private GoogleBookInfo? SelectedBook = new();
        private List<GoogleBookInfo>? books;
        private bool booksLoading;
        private bool aiFallbackUsed;
        private bool googleRateLimited;
        private int currentPage = 1;
        private string SpinnerLabel { get; set; }
        private bool SpinnerVisible { get; set; }
        private string MessageLabel { get; set; }
        private bool MessageVisible { get; set; }
        private readonly SemaphoreSlim _spinnerLock = new(1, 1);
        private bool showAISearchDialog = false;
        private List<GoogleBookInfo>? resultList = null;

        string uiLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        private IEnumerable<GoogleBookInfo> FilteredBooks => (books ?? Enumerable.Empty<GoogleBookInfo>()).Where(b =>
            !string.IsNullOrWhiteSpace(b.Title) && !string.IsNullOrWhiteSpace(b.Thumbnail));

        // cache filtered books to avoid repeated deferred LINQ enumeration during render
        private List<GoogleBookInfo>? _filteredBooks;
        private IEnumerable<GoogleBookInfo> CachedFilteredBooks =>
            (_filteredBooks ??= (books ?? Enumerable.Empty<GoogleBookInfo>())
                .Where(b => !string.IsNullOrWhiteSpace(b.Title) && !string.IsNullOrWhiteSpace(b.Thumbnail)).ToList());

        private int TotalPages
        {
            get
            {
                var count = CachedFilteredBooks.Count();
                if (count == 0) return 1;
                return (int)Math.Ceiling(count / (double)PageSize);
            }
        }

        private IEnumerable<GoogleBookInfo> PagedBooks => CachedFilteredBooks.Skip((currentPage - 1) * PageSize).Take(PageSize);

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

        // Allow external components to clear the client-side cache for an author (e.g. Retry button)
        public static void ClearCacheForAuthor(string? author)
        {
            if (string.IsNullOrWhiteSpace(author)) return;
            _booksCache.TryRemove(author, out _);
        }

        private async Task LoadBooksAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested(); books = null;
            _filteredBooks = null;
            if (string.IsNullOrWhiteSpace(AuthorName)) return;

            // Try cache first
            if (_booksCache.TryGetValue(AuthorName!, out var cached))
            {
                var ttl = string.Equals(cached.source, "AI", StringComparison.OrdinalIgnoreCase) ? AiCacheTtl : GoogleCacheTtl;
                if (DateTime.UtcNow - cached.fetchedAt < ttl)
                {
                    // If cached items exist and contain data, use them. If the cached list is empty,
                    // remove the cache entry so we will retry the external lookup (which includes AI fallback).
                    if (cached.items is not null && cached.items.Count > 0)
                    {
                        books = cached.items;
                        if (books.FirstOrDefault()?.Id?.ToLower() != "ratelimited")
                        {
                            if (books.FirstOrDefault()?.Id?.ToLower() != "error")
                            {
                                aiFallbackUsed = string.Equals(cached.source, "AI", StringComparison.OrdinalIgnoreCase);
                                MessageLabel = "Books cached result";
                                UpdateMessageLabelAsync(MessageLabel);
                                return;
                            }
                        }
                    }

                    // cached but empty -> remove and continue to retry (including AI fallback)
                    _booksCache.TryRemove(AuthorName!, out _);
                }
                else
                {
                    // remove stale entry
                    _booksCache.TryRemove(AuthorName!, out _);
                }
            }

            booksLoading = true;
            googleRateLimited = false;

            try
            {

                try
                {
                    SpinnerLabel = L["LoadingMsg"];
                    await UpdateSpinnerLabelAsync(SpinnerLabel);

                    var result = await GoogleBooksService.GetBooksByAuthorAsync(AuthorName!);
                    if (result != null)
                    {
                        if (result.FirstOrDefault()?.Id == "RateLimited")
                        {
                            googleRateLimited = true;
                            MessageLabel = $"Google Books rate-limited for {AuthorName}; falling back to AI.";
                            UpdateMessageLabelAsync(MessageLabel);
                            Logger.LogWarning("Google Books API rate limited for author {Author}", AuthorName);
                        }
                        else if (result.FirstOrDefault()?.Id == "Error")
                        {
                            MessageLabel = $"Error fetching Google Books for {AuthorName}";
                            UpdateMessageLabelAsync(MessageLabel);
                            Logger.LogWarning("Error fetching Google Books for author {Author}", AuthorName);
                        }
                    }

                    if (result == null || result.Count == 0 || result.FirstOrDefault()?.Id.ToLower().Contains("ratelimited") == true || result.FirstOrDefault()?.Id.ToLower().Contains("error") == true)
                    {
                        // No Google results -> attempt WikiData fallback
                        MessageLabel = $"No Google results for {AuthorName}; attempting WikiData fallback.";
                        UpdateMessageLabelAsync(MessageLabel);
                        Logger.LogInformation("No Google results for {Author}; attempting WikiData fallback.", AuthorName);

                        var wikidataResult = await WikidataClient.GetBooksByAuthorAsync(AuthorName!, uiLanguage);
                        if (wikidataResult != null && wikidataResult.Count > 0)
                        {
                            List<GoogleBookInfo> wikiDataResults = new();
                            foreach (var b in wikidataResult)
                            {
                                wikiDataResults.Add(new GoogleBookInfo
                                {
                                    Title = b.Title,
                                    PublishedDate = b.PublicationYear.ToString(), // year only, but better than nothing
                                    Thumbnail = b.CoverImage,
                                    Confidence = null // Wikidata results are not AI-generated, so confidence is not applicable
                                });
                            }

                            resultList = wikiDataResults;
                            return;
                        }
                        else
                        {
                            MessageLabel = $"WikiData returned no results for {AuthorName}; falling back to AI.";
                            UpdateMessageLabelAsync(SpinnerLabel);
                            Logger.LogWarning("WikiData returned no results for {Author}; falling back to AI.", AuthorName);
                        }

                        showAISearchDialog = true;
                        // var aiResult = await AiBookClient.GetAuthorBooksAsync(AuthorName!, uiLanguage);
                        // if (aiResult != null && aiResult.Count > 0)
                        // {
                        //     resultList = aiResult.ToList();
                        //     aiFallbackUsed = true;
                        // }
                        // else
                        // {
                        //     resultList = new List<GoogleBookInfo>();
                        // }
                    }
                    else
                    {
                        resultList = result.ToList();
                    }
                }
                catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    // Google rate-limited -> try AI
                    MessageLabel = $"Google Books rate-limited for {AuthorName}; falling back to AI.";
                    UpdateMessageLabelAsync(MessageLabel);
                    Logger.LogWarning(httpEx, "Google Books rate-limited for {Author}; falling back to AI.", AuthorName);
                    showAISearchDialog = true;

                    googleRateLimited = true;

                    // var aiResult = await AiBookClient.GetAuthorBooksAsync(AuthorName!, uiLanguage);
                    // if (aiResult != null && aiResult.Count > 0)
                    // {
                    //     resultList = aiResult.ToList();
                    //     aiFallbackUsed = true;
                    // }
                    // else
                    // {
                    //     resultList = new List<GoogleBookInfo>();
                    // }
                }
                catch (Exception ex)
                {
                    // Other errors -> try AI fallback as well
                    MessageLabel = $"Error fetching Google Books for {AuthorName}; attempting AI fallback.";
                    UpdateMessageLabelAsync(MessageLabel);
                    Logger.LogWarning(ex, "Error fetching Google Books for author {Author}; attempting AI fallback", AuthorName);
                    showAISearchDialog = true;

                }

                books = resultList ?? new List<GoogleBookInfo>();
                // clear cached filtered list so it will be recomputed
                _filteredBooks = null;

                // store in cache (even empty) to avoid hammering API for a short period
                var source = aiFallbackUsed ? "AI" : "Google";
                _booksCache[AuthorName!] = (DateTime.UtcNow, books, source);
            }
            finally
            {
                booksLoading = false;
                SpinnerVisible = false;
                StateHasChanged();
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

        private async Task RunWithSpinnerAsync(string label, Func<Task> work)
        {
            await _spinnerLock.WaitAsync();
            try
            {
                SpinnerLabel = label ?? "";
                await UpdateSpinnerLabelAsync(SpinnerLabel);

                if (work is not null) await work();
            }
            finally
            {
                SpinnerVisible = false;
                SpinnerLabel = "";
                await UpdateSpinnerLabelAsync(SpinnerLabel);
                _spinnerLock.Release();
            }
        }
        // update the label while spinner is visible (forces immediate render)
        private Task UpdateSpinnerLabelAsync(string label)
        {
            SpinnerLabel = label ?? "";
            if (!SpinnerVisible)
                SpinnerVisible = true;

            return InvokeAsync(StateHasChanged);
        }
        // update the label while spinner is visible (forces immediate render)
        private void UpdateMessageLabelAsync(string label)
        {
            MessageLabel = label ?? "";
            if (!MessageVisible)
                MessageVisible = true;
            StateHasChanged();
        }

        async Task OnConfirmYes()
        {
            showAISearchDialog = false;
            await SearchBooksUsingAI(AuthorName, uiLanguage);
        }

        void OnConfirmNo() => showAISearchDialog = false;

        async Task SearchBooksUsingAI(string authorName, string uiLanguage)
        {
            var aiResult = await AiBookClient.GetAuthorBooksAsync(AuthorName!, uiLanguage);
            if (aiResult != null && aiResult.Count > 0)
            {
                aiFallbackUsed = true;
                resultList = aiResult.ToList();
            }
            else
            {
                resultList = new List<GoogleBookInfo>();
            }
        }
    }
}