using HarfBuzzSharp;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Formatting;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using RtfPipe.Tokens;
using Syncfusion.Blazor.Spinner;
using System.Globalization;
using Web.Blazor.Services.Common;

namespace Web.Blazor.Pages.Publications
{
    public partial class GoogleBooksApiSearch
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IStringLocalizer<App> localizerApp { get; set; }
        [Inject] public IPublicacaoService PubsService { get; set; }
        [Inject] public IAutorService autoresService { get; set; }
        [Inject] public IGoogleBooksService GoogleBooksService { get; set; }
        [Inject] public Web.Blazor.Services.ApiWrappers.AiBookClientService AiBookClient { get; set; }
        [Inject] public Web.Blazor.Services.Common.InternetConnectivityService Internet { get; set; }
        [Inject] public ILogger<App> _logger { get; set; }
        [Inject] public IUtilitiesService utilitiesService { get; set; }

        // TODO - to test for now, Books should have it's own progress client
        // to avoid conflicts with music metadata retrieval that can happen at the same time.
        // If this works well we can consider refactoring the music metadata
        // retrieval to also use a progress client and unify the UI.
        [Inject] public BooksInfoProgressClient ProgressClient { get; set; }


        [Parameter] public bool  EditMode { get; set; }
        [Parameter] public PubsWithDescriptions ApiRecord { get; set; }
        [Parameter] public EventCallback<GoogleBooksSearchResult> OnGoogleResponse { get; set; }

        protected string authorToSearch = string.Empty;
        protected string titleToSearch = string.Empty;
        protected bool alertUser = false;
        protected string alertMessage = string.Empty;
        protected string noteMessage = string.Empty;
        int authorId = 0;

        private bool showAiProgress;
        private string aiProgressMessage = string.Empty;

        private string? lastAiNotes;
        private double? lastAiConfidence;
        private bool lastAiUsed;

        private SfSpinner? spinner;
        private bool isBusy;

        private string busyStatusMessage = "";

        private CancellationTokenSource? booksInfoCts;

        string uiLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        protected override void OnInitialized()
        {
            alertUser = false;
            alertMessage = string.Empty;
            noteMessage = string.Empty;

            // Keep local fields editable, but start them with the current record values
            // so opening the dialog pre-fills from the parent form (original behavior).
            authorToSearch = ApiRecord?.Autor ?? string.Empty;
            titleToSearch = ApiRecord?.Titulo ?? string.Empty;
        }

        protected override void OnParametersSet()
        {
            // When the dialog is (re)opened we get new parameters; only overwrite the local
            // fields if the user hasn't typed anything yet.
            if (string.IsNullOrWhiteSpace(titleToSearch))
                titleToSearch = ApiRecord?.Titulo ?? string.Empty;
            if (string.IsNullOrWhiteSpace(authorToSearch))
                authorToSearch = ApiRecord?.Autor ?? string.Empty;
        }

        protected async Task GetBookInfo(string bookTitle, string author)
        {

            await ProgressClient.StartAsync(NavigationManager.BaseUri.TrimEnd('/'));
            ProgressClient.ProgressReceived -= OnProgressReceived;
            ProgressClient.ProgressReceived += OnProgressReceived;

            if (string.IsNullOrEmpty(bookTitle) && string.IsNullOrEmpty(author))
            {
                alertUser = true;
                alertMessage = "Preencha critérios de pesquisa, p.f.";
                return;
            }

            if (isBusy)
                return;

            isBusy = true;
            busyStatusMessage = "";
            booksInfoCts?.Cancel();
            booksInfoCts?.Dispose();
            booksInfoCts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(90));

            if (spinner is not null)
                await spinner.ShowAsync();

            try
            {
                lastAiUsed = false;
                lastAiNotes = null;
                lastAiConfidence = null;

                var hasInternet = await Internet.HasInternetAsync();
                if (!hasInternet)
                {
                    // No internet: skip GoogleBooksApi (requires internet) and try the local AI directly.
                    var ai = await TryGetAiBookAsync(bookTitle, author);
                    if (ai is not null)
                    {
                        ApplyAiBookToRecord(ai, bookTitle, author);

                        noteMessage = "Internet unavailable. Used local AI (Ollama) to generate the summary.";
                        await OnGoogleResponse.InvokeAsync(new Web.Blazor.Pages.Publications.GoogleBooksSearchResult
                        {
                            Record = ApiRecord,
                            AiNotes = lastAiNotes,
                            AiConfidence = lastAiConfidence,
                            UsedAi = true,
                            UsedOfflineFallback = true
                        });
                        return;
                    }

                    alertUser = true;
                    alertMessage = "Internet indisponível. Não foi possível consultar a Google Books API.";
                    return;
                }

                string translatedTitle = "";
                var bookInfo = await GoogleBooksService.GetBookAsync(bookTitle, author);
                if (bookInfo is null)
                {
                    translatedTitle = await autoresService.TranslateWithMyMemoryAsync(bookTitle, "pt", "en");
                    bookInfo = await GoogleBooksService.GetBookAsync(translatedTitle, author);
                    if (bookInfo is null)
                    {
                        var ai = await TryGetAiBookAsync(bookTitle, author);
                        if(ai is not null)
                        {
                            // TODO completar
                        }

                    }
                }

                if (bookInfo.items is null || bookInfo.items.Length == 0)
                {
                    // Situation 1: Google Books has no results. Try AI fallback to at least fill the summary.
                    var ai = await TryGetAiBookAsync(bookTitle, author);
                    if (ai is not null)
                    {
                        ApplyAiBookToRecord(ai, bookTitle, author);

                        await OnGoogleResponse.InvokeAsync(new Web.Blazor.Pages.Publications.GoogleBooksSearchResult
                        {
                            Record = ApiRecord,
                            AiNotes = lastAiNotes,
                            AiConfidence = lastAiConfidence,
                            UsedAi = true
                        });
                        return;
                    }

                    alertUser = true;
                    alertMessage = localizerApp["msgNoResults"];
                    return;
                }
                else
                {
                    float salePrice = 0;
                    var retailPrice = bookInfo.items[0].saleInfo.retailPrice;
                    if (retailPrice is not null)
                    {

                        var itemPrice = bookInfo.items
                            .FirstOrDefault(bi => bi.saleInfo.retailPrice != null && bi.saleInfo.retailPrice.amount > 0).saleInfo.listPrice.amount;

                        if (itemPrice > 0)
                            salePrice = itemPrice;
                    }
                    var booksAuthors = new List<string>();
                    var booksDescriptions = new List<string>();
                    var booksTitles = new List<string>();
                    var booksPublishedDates = new List<string>();
                    var booksImages = new List<string>();
                    var booksPages = new List<string>();
                    var booksItems = bookInfo.items;

                    string textSnippet = "";
                    var AiDescription = "";
                    if (booksItems[0].searchInfo == null) // may have more than one item, search the first one with data in the AI
                                               // (second one may have data that don't correspond do the book we're interest in)
                    {
                        AiDescription = await TryGetAiSummaryAsync(bookTitle, author) ;
                    }

                    foreach (var bookitem in booksItems.Select(v => v.volumeInfo))
                    {
                        if (bookitem is not null)
                        {
                            var bookDesc = bookitem.description is not null ? 
                                bookitem.description : 
                                !string.IsNullOrEmpty(textSnippet) 
                                ? textSnippet : 
                                !string.IsNullOrEmpty(AiDescription) 
                                ? AiDescription : "";
                            booksDescriptions.Add(!string.IsNullOrEmpty(bookDesc) ? bookDesc : "N/A");

                            if (bookitem.authors is not null)
                                booksAuthors.Add(bookitem.authors.FirstOrDefault());


                            if (bookitem.imageLinks is not null)
                            {
                                booksImages.Add(bookitem.imageLinks.thumbnail);
                            }
                            else
                            {
                                booksImages.Add("images/NoPhoto.png");
                            }

                            if (bookitem.title is not null)
                            {
                                booksTitles.Add(bookitem.title);
                            }

                            if (bookitem.pageCount > 0)
                            {
                                booksPages.Add(bookitem.pageCount.ToString());
                            }
                            else
                            {
                                booksPages.Add("0");

                            }

                            if (bookitem.publishedDate is not null)
                            {
                                booksPublishedDates.Add(bookitem.publishedDate);
                            }

                        }
                    }

                    string englishAuthorName = "";

                    // TODO revisit this code later. It was meant to handle the case where 
                    // user searches in Portuguese but Google Books only has English data, 
                    // but it ended up being more complex than expected and is currently disabled 
                    // to avoid breaking the main flow. 
                    // The AI fallback should mitigate most of the cases where this would be needed, so it's not a high priority.

                    // if (!string.IsNullOrEmpty(bookTitle))
                    // {
                    //     var titleMatch = booksTitles?.FirstOrDefault(titleToCheck => titleToCheck.ToLower() == bookTitle.ToLower());
                    //     if (string.IsNullOrEmpty(titleMatch))
                    //     {
                    //         englishAuthorName = await autoresService.TranslateWithMyMemoryAsync(bookTitle, "pt", "en");
                    //         titleMatch = booksTitles?.FirstOrDefault(titleToCheck => titleToCheck.ToLower() == englishAuthorName.ToLower());
                    //         if (string.IsNullOrEmpty(titleMatch))
                    //         {
                    //             alertUser = true;
                    //             StringBuilder sbTitles = new();
                    //             foreach (var item in booksTitles)
                    //             {

                    //                 sbTitles.AppendLine($"'{item}',");
                    //             }
                    //             var titlesFound = sbTitles.ToString().Trim();
                    //             titlesFound = titlesFound.Remove(titlesFound.Length - 1);
                    //             noteMessage = "No data returned. Below are the books found for the author";
                    //             alertMessage = titlesFound;
                    //             return;
                    //         }
                    //     }
                    // }

                    string bookAuthor = "";
                    //bool bookAuthorNameIsInEnglish = !string.IsNullOrEmpty(englishAuthorName); // TODO - investigate, the result = 'x' (??)
                    //if (bookAuthorNameIsInEnglish)
                    //{
                    //    bookAuthor = booksAuthors.FirstOrDefault();
                    //}
                    //else
                    //{
                        bookAuthor = author;
                    //}

                    var bookCategory = bookInfo.items[0].volumeInfo.categories is not null ? bookInfo.items[0].volumeInfo.categories.FirstOrDefault() : "Desconhecida";
                    var bookDescription = booksDescriptions.Where(bd => bd is not null).FirstOrDefault();
                    var bookImage = booksImages.Where(bi => bi is not null).FirstOrDefault();
                    var bookPublishDate = booksPublishedDates.FirstOrDefault();
                    var bookItemPages = booksPages.FirstOrDefault();
                    var bookItemTitle = booksTitles.FirstOrDefault();

                    if (!string.IsNullOrEmpty(bookAuthor))
                    {
                        bookAuthor = bookAuthor.Replace("'", "");
                        var authorExist = await autoresService.AutorExiste(bookAuthor);
                        var authorNameConverted = ConvertToProperCase(bookAuthor);
                        if (!authorExist)
                        {
                            await CreateAuthor(authorNameConverted);
                            authorId = await autoresService.GetIdByDescription(authorNameConverted);
                        }
                        else
                        {
                            authorId = await autoresService.GetIdByDescription(authorNameConverted);
                        }

                        ApiRecord.Autor = authorNameConverted;
                        authorToSearch = authorNameConverted;

                    }
                    int iCategoria = utilitiesService.GetPkValueFromTable("Categoria", "Id", "Descricao", bookCategory);
                    if (iCategoria == 0)
                    {
                        iCategoria = utilitiesService.GetPkValueFromTable("Categoria", "Id", "Descricao", "Desconhecida");
                    }

                    ApiRecord.IdCategoria = iCategoria;

                    if (!string.IsNullOrEmpty(bookDescription))
                    {
                        var translatedAboutText = await autoresService.TranslateWithMyMemoryAsync(bookDescription, "en", "pt");
                        bookDescription = !string.IsNullOrEmpty(translatedAboutText) ? translatedAboutText : bookDescription;
                        ApiRecord.Resumo = bookDescription;
                    }
                    else
                    {
                        // Situation 2: Google Books found the book but lacks description. Try AI for summary only.
                        var aiSummary = await TryGetAiSummaryAsync(bookTitle, author);
                        if (!string.IsNullOrWhiteSpace(aiSummary))
                        {
                            ApiRecord.Resumo = aiSummary;
                        }
                    }

                    if (!string.IsNullOrEmpty(bookPublishDate))
                    {
                        if (bookPublishDate.Length == 4)
                        {
                            bookPublishDate = bookPublishDate + "-01-01";
                        }
                        if (DataFormat.IsValidDate(bookPublishDate))
                        {
                            ApiRecord.DataMovimento = DateTime.Parse(bookPublishDate);
                            ApiRecord.AnoEdicao = ApiRecord.DataMovimento.Year.ToString();
                        }
                    }
                    if (!string.IsNullOrEmpty(bookImage))
                    {
                        ApiRecord.ImagemCapa = bookImage;
                    }
                    else
                    {
                        ApiRecord.ImagemCapa = "images/NoPhoto.png";
                    }

                    if (authorId == 0)
                    {
                        authorId = await autoresService.GetIdByDescription("Desconhecido");
                    }

                    int iFormato = utilitiesService.GetPkValueFromTable("Formato", "Id", "Descricao", "Capa dura");
                    if (iFormato == 0)
                    {
                        iFormato = utilitiesService.GetPkValueFromTable("Formato", "Id", "Descricao", "Desconhecido");
                    }

                    ApiRecord.Titulo = bookTitle;
                    ApiRecord.IdFormato = iFormato;
                    ApiRecord.IdAutor = authorId;
                    ApiRecord.Paginas = int.Parse(bookItemPages) > 0 ? int.Parse(bookItemPages) : 0;
                    ApiRecord.Preco = (decimal)salePrice > 0 ? (decimal)salePrice : 0;

                    await OnGoogleResponse.InvokeAsync(new Web.Blazor.Pages.Publications.GoogleBooksSearchResult
                    {
                        Record = ApiRecord,
                        AiNotes = lastAiUsed ? lastAiNotes : null,
                        AiConfidence = lastAiUsed ? lastAiConfidence : null,
                        UsedAi = lastAiUsed
                    });
                }

            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    alertUser = true;
                    alertMessage = $"Título ('{bookTitle}') não foi encontrado. Confirme, pf.";
                }

            }
            catch (Exception ex)
            {
                alertUser = true;
                alertMessage = $"Erro ao consultar api da Google. Confirme, pf";
                _logger.LogError($"Erro ao consultar api da Google. Confirme, pf. ({ex.Message})");
            }
            finally
            {
                if (spinner is not null)
                    await spinner.HideAsync();
                isBusy = false;
            }
        }

        private async Task<string?> TryGetAiSummaryAsync(string title, string author)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                    return null;

                // Reset per-attempt AI flags so previous successes don't affect message visibility.
                lastAiUsed = false;
                lastAiNotes = null;
                lastAiConfidence = null;


                showAiProgress = true;
                aiProgressMessage = "AI - Searching for a book description...";
                await InvokeAsync(StateHasChanged);

                var resp = await AiBookClient.GetBookSummaryOnlyAsync(title, string.IsNullOrWhiteSpace(author) ? null : author, uiLanguage, ProgressClient.ConnectionId);
                if (resp is null)
                {
                    aiProgressMessage = "AI - No response returned.";
                    await InvokeAsync(StateHasChanged);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(resp.Summary))
                {
                    aiProgressMessage = $"AI - No summary returned. {resp.Notes}";
                    await InvokeAsync(StateHasChanged);
                    return null;
                }

                lastAiNotes = resp.Notes;
                lastAiConfidence = resp.Confidence;
                lastAiUsed = true;

                aiProgressMessage = string.IsNullOrWhiteSpace(resp.Notes)
                    ? "AI - Summary generated."
                    : $"AI - Summary generated. {resp.Notes}";
                await InvokeAsync(StateHasChanged);

                return resp.Summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI - Error generating book description.");

                aiProgressMessage = "AI - Error generating book description. Check logs.";
                showAiProgress = true;
                await InvokeAsync(StateHasChanged);
                return null;
            }
        }

        private async Task<MediaOrganizerApp.Application.DTOs.AiBookSummaryResponseDto?> TryGetAiBookAsync(string title, string author)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                    return null;

                var uiLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

                showAiProgress = true;
                aiProgressMessage = "AI - Searching for book metadata/description...";
                await InvokeAsync(StateHasChanged);

                var resp = await AiBookClient.GetBookSummaryAsync(title, string.IsNullOrWhiteSpace(author) ? null : author, uiLanguage);
                if (resp is null)
                    return null;

                lastAiNotes = resp.Notes;
                lastAiConfidence = resp.Confidence;
                lastAiUsed = true;

                aiProgressMessage = string.IsNullOrWhiteSpace(resp.Notes)
                    ? "AI - Data generated."
                    : $"AI - Data generated. {resp.Notes}";
                showAiProgress = true;
                await InvokeAsync(StateHasChanged);

                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI - Error generating book metadata.");

                aiProgressMessage = "AI - Error generating book metadata. Check logs.";
                showAiProgress = true;
                await InvokeAsync(StateHasChanged);
                return null;
            }
        }

        private void ApplyAiBookToRecord(MediaOrganizerApp.Application.DTOs.AiBookSummaryResponseDto ai, string bookTitle, string author)
        {
            ApiRecord.Titulo = string.IsNullOrWhiteSpace(ai.Title) ? bookTitle : ai.Title;

            if (!string.IsNullOrWhiteSpace(ai.Author))
            {
                ApiRecord.Autor = ai.Author;
            }
            else if (!string.IsNullOrWhiteSpace(author))
            {
                ApiRecord.Autor = author;
            }

            authorToSearch = ApiRecord.Autor;

            if (!string.IsNullOrWhiteSpace(ai.Summary))
                ApiRecord.Resumo = ai.Summary;

            if (!string.IsNullOrWhiteSpace(ai.Category))
            {
                var iCategoria = utilitiesService.GetPkValueFromTable("Categoria", "Id", "Descricao", ai.Category);
                if (iCategoria == 0)
                    iCategoria = utilitiesService.GetPkValueFromTable("Categoria", "Id", "Descricao", "Desconhecida");
                ApiRecord.IdCategoria = iCategoria;
            }

            if (ai.Pages is not null && ai.Pages.Value > 0)
                ApiRecord.Paginas = ai.Pages.Value;

            if (ai.Price is not null && ai.Price.Value > 0)
                ApiRecord.Preco = ai.Price.Value;

            if (!string.IsNullOrWhiteSpace(ai.CoverImageUrl))
                ApiRecord.ImagemCapa = ai.CoverImageUrl;

            if (!string.IsNullOrWhiteSpace(ai.PublishedDate))
            {
                var dateToParse = ai.PublishedDate;
                if (dateToParse.Length == 4)
                    dateToParse += "-01-01";

                if (DataFormat.IsValidDate(dateToParse))
                {
                    ApiRecord.DataMovimento = DateTime.Parse(dateToParse);
                    ApiRecord.AnoEdicao = ApiRecord.DataMovimento.Year.ToString();
                }
            }

            if (ApiRecord.IdFormato == 0)
            {
                var iFormato = utilitiesService.GetPkValueFromTable("Formato", "Id", "Descricao", "Capa dura");
                if (iFormato == 0)
                    iFormato = utilitiesService.GetPkValueFromTable("Formato", "Id", "Descricao", "Desconhecido");
                ApiRecord.IdFormato = iFormato;
            }
        }

        private string ConvertToProperCase(string input)
        {
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(input.ToLower());
        }

        private async Task CreateAuthor(string sNomeAutor)
        {
            AutorDTO autor = new AutorDTO()
            {
                Descricao = sNomeAutor,
                Sobre = localizerApp["msgUploadAuthor_AboutMsg"] + " " + DateTime.Now.ToShortDateString(),
                ImagemAutor = "",
                DataNascimento = null
            };

            try
            {
                await autoresService.CreateAuthor(autor);
            }
            catch (Exception exc)
            {
                throw new ApplicationException(localizerApp["msgUploadAuthorCreation_Error"] +
                    "\r\n\r\n" + exc.StackTrace);
            }
        }

        private void OnProgressReceived(string message)
        {
            busyStatusMessage = message;
            InvokeAsync(StateHasChanged);
        }

        private void CloseDialog()
        {
            ApiRecord.Titulo = "";
            noteMessage = "";
            authorToSearch = "";
            titleToSearch = "";
            alertUser = false;
        }

    }
}