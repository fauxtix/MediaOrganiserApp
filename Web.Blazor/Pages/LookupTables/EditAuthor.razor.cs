using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Interfaces.Services.WikiData;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Spinner;
using System.Net;
using Web.Blazor.Services.ApiWrappers;
using Web.Blazor.Utilities;
using Path = System.IO.Path;
namespace Web.Blazor.Pages.LookupTables
{
    public partial class EditAuthor
    {

        [Parameter] public AutorDTO SelectedAuthor { get; set; }
        [Parameter] public string AuthorImage { get; set; }
        [Parameter] public bool blnEdit { get; set; } = false;

        [Inject] Microsoft.Extensions.Localization.IStringLocalizer<App> L { get; set; }
        [Inject] IAutorService AutoresService { get; set; }
        [Inject] IWikidataClientService WikiDataService { get; set; }
        [Inject] AiClientService AiClientService { get; set; }
        [Inject] TranslateClientService TranslateClient { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] ILogger<App> Logger { get; set; }
        [Inject] IWebHostEnvironment Env { get; set; }


        protected SfUploader sfUploader;
        public string uploadedFile { get; set; } = "";

        public string imagemAutor { get; set; }

        protected string sPronto { get; set; }

        protected string message = "";
        protected bool alertVisibility;

        protected SfSpinner SpinnerObj;

        protected string? aiNotes;
        protected double? aiConfidence;

        public string[] Items = new string[] { "Open", "|", "Delete", "Download", "Rename", "|", "Details", "Custom" };
        protected (string bio, string photo, string birthDate) authorDetails;

        protected bool ShowBooksVisibility;
        protected override void OnInitialized()
        {
            alertVisibility = false;
            showTranslateModal = false;
        }

        protected async Task OnTranslateApplied(TranslateRequestDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Text))
                return;

            // Apply translated text to the author bio and close modal
            SelectedAuthor.Sobre = dto.Text;
            showTranslateModal = false;
            StateHasChanged();
            await Task.CompletedTask;
        }

        protected async Task OnTranslateVisibleChanged(bool visible)
        {
            showTranslateModal = visible;
            await InvokeAsync(StateHasChanged);
        }

        // flag to control translate modal visibility
        protected bool showTranslateModal { get; set; }

        protected async Task TranslateBio()
        {
            // Button is a test hook for the new translate API. Do not change other flows.
            if (string.IsNullOrWhiteSpace(SelectedAuthor.Sobre))
            {
                ShowAlert("Sem texto para traduzir");
                return;
            }

            await SpinnerObj?.ShowAsync();
            try
            {
                var source = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                var target = source == "en" ? "pt" : "en"; // simple default: translate to English unless already English

                var plain = TextSanitizer.StripHtml(SelectedAuthor.Sobre ?? string.Empty);
                var req = new MediaOrganizerApp.Application.DTOs.AI.TranslateRequestDto
                {
                    Text = plain,
                    SourceLanguage = source,
                    TargetLanguage = target
                };

                var resp = await TranslateClient.TranslateAsync(req);
                if (resp is not null && !string.IsNullOrWhiteSpace(resp.TranslatedText))
                {
                    SelectedAuthor.Sobre = resp.TranslatedText;
                    aiNotes = null;
                    aiConfidence = resp.Confidence;
                    ShowAlert("Bio traduzida pelo servi�o de tradu��o.");
                }
                else
                {
                    ShowAlert("Servi�o de tradu��o n�o retornou texto.");
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogError(ex, "Error calling translate service");
                ShowAlert("Erro ao chamar servi�o de tradu��o.");
            }
            finally
            {
                StateHasChanged();
                await Task.Delay(20);
                await SpinnerObj?.HideAsync();
            }
        }

        private string GetLocalizedString(string resName)
        {
            var output = L[resName];
            return output;
        }

        protected async void OnChangeUpload(UploadChangeEventArgs args)
        {
            message = "";
            alertVisibility = false;

            foreach (var file in args.Files)
            {
                var pastaImagensAutores = Path.Combine(Env.WebRootPath, "images", "Autores"); // Configuration.GetSection("AppSettings:PastaImagensAutores").Value;

                uploadedFile = Path.Combine(pastaImagensAutores, file.FileInfo.Name);
                if (File.Exists(uploadedFile))
                {
                    AuthorImage = $"images/autores/{file.FileInfo.Name}";

                    SelectedAuthor.ImagemAutor = AuthorImage;
                    StateHasChanged();
                }
                else
                {
                    try
                    {
                        if (uploadedFile != SelectedAuthor.ImagemAutor)
                        {
                            using (FileStream filestream = new FileStream(uploadedFile, FileMode.Create, FileAccess.Write))
                            {
                                await file.File.OpenReadStream(long.MaxValue).CopyToAsync(filestream);
                                AuthorImage = $"images/autores/{file.FileInfo.Name}";
                                SelectedAuthor.ImagemAutor = AuthorImage;
                                StateHasChanged();
                            }
                        }
                        else
                        {
                            message = $"O local previsto ({pastaImagensAutores.TrimEnd('\\')}) n�o � o que est� configurado. Experimente mover imagem para esse local e tente de novo.";
                            ShowAlert(message);
                        }

                    }
                    catch (System.Exception ex)
                    {
                        Logger.LogError(ex.Message);
                        ShowAlert(ex.Message);
                    }
                }
            }
        }

        protected void OnLoadEnd(Syncfusion.Blazor.Inputs.ProgressEventArgs args)
        {
            var x = args.File.Name;
            //blnEdit = false;
            //this.EditDialogVisibility = true;

        }

        protected void OnUpLoadSuccess(Syncfusion.Blazor.Inputs.SuccessEventArgs args)
        {
            var x = args.File.Name;
            //blnEdit = false;
            //this.EditDialogVisibility = true;

        }

        protected async Task GetAuthorDataFromApi()
        {
            if (string.IsNullOrWhiteSpace(SelectedAuthor.Descricao))
            {
                ShowAlert("Sem dados para pesquisar");
                return;
            }

            aiNotes = null;
            aiConfidence = null;

            await Task.Delay(20);
            await SpinnerObj?.ShowAsync();

            try
            {
                var authorName = SelectedAuthor.Descricao;
                var authorHasImage = SelectedAuthor.ImagemAutor is not null &&
                    (!SelectedAuthor.ImagemAutor.ToLower().Contains("image") ||
                              !SelectedAuthor.ImagemAutor.ToLower().Contains("photo)")
                    );

               //authorDetails = await WikiDataService.GetWikiDataAuthorDetails(authorName);


                authorDetails = await AutoresService.GetAuthorDetails(authorName);


                if (!blnEdit && !string.IsNullOrEmpty(authorDetails.photo))
                {
                    SelectedAuthor.ImagemAutor = authorDetails.photo;
                }
                else if (blnEdit && SelectedAuthor.ImagemAutor != authorDetails.photo && !authorHasImage)
                {
                    SelectedAuthor.ImagemAutor = authorDetails.photo;
                }

                if (SelectedAuthor.DataNascimento is null)
                {
                    if (!string.IsNullOrEmpty(authorDetails.birthDate))
                    {
                        if (authorDetails.birthDate.Length == 5)
                        {
                            var yearPart = int.Parse(authorDetails.birthDate.Substring(0, 4));
                            SelectedAuthor.DataNascimento = new DateTime(yearPart, 1, 1);
                        }
                        else if (authorDetails.birthDate.Length == 4)
                        {
                            var yearPart = int.Parse(authorDetails.birthDate.Substring(0, 4));
                            SelectedAuthor.DataNascimento = new DateTime(yearPart, 1, 1);
                        }
                        else
                        {
                            SelectedAuthor.DataNascimento = DateTime.Parse(authorDetails.birthDate);
                        }
                    }
                }
                if (authorDetails.bio.ToLower().Contains("sem")) // TODO => localize "sem biografia" check
                {
                    // External API first; AI fallback only when external returns empty bio.
                    if (SelectedAuthor.Sobre is not null &&
                        !string.IsNullOrWhiteSpace(SelectedAuthor.Sobre))
                    {
                        ShowAlert("J� existe Bio para o Autor. AI n�o foi consultada.");
                        await Task.Delay(20);
                        await SpinnerObj.HideAsync();
                        return;
                    }

                    var ai = await AiClientService.GetAuthorProfileAsync(authorName, System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, scenario: "author-api");

                    if (!string.IsNullOrWhiteSpace(ai?.Bio))
                    {
                        SelectedAuthor.Sobre = ai.Bio;
                        aiNotes = ai.Notes;
                        aiConfidence = ai.Confidence;

                        ShowAlert("Bio gerada por AI (fallback).");
                        await Task.Delay(20);
                        await SpinnerObj.HideAsync();
                        return;
                    }

                    ShowAlert("N�o foi poss�vel obter detalhes do autor (API e AI).");
                    await Task.Delay(20);
                    await SpinnerObj.HideAsync();
                    return;
                }
                SelectedAuthor.Sobre = TextSanitizer.StripHtml(authorDetails.bio);
                aiNotes = null;
                aiConfidence = null;

                //if (!string.IsNullOrWhiteSpace(authorDetails.birthDate))
                //    SelectedAuthor.DataNascimento = DateTime.Parse(authorDetails.birthDate);
                await Task.Delay(20);
                await SpinnerObj.HideAsync();

            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.GatewayTimeout)
                {
                    Logger.LogWarning(ex, "Time out");
                }

                // Network failure (e.g., offline). Try AI fallback directly.
                try
                {
                    var authorName = SelectedAuthor.Descricao;
                    var ai = await AiClientService.GetAuthorProfileAsync(authorName, System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, scenario: "author-api");

                    if (!string.IsNullOrWhiteSpace(ai?.Bio))
                    {
                        SelectedAuthor.Sobre = ai.Bio;
                        aiNotes = "Internet unavailable or external API unreachable. " + ai.Notes;
                        aiConfidence = ai.Confidence;

                        ShowAlert("Bio gerada por AI (offline/fallback).");
                    }
                    else
                    {
                        ShowAlert("Internet indispon�vel e AI n�o devolveu bio.");
                    }
                }
                finally
                {
                    StateHasChanged();
                    await Task.Delay(20);
                    await SpinnerObj?.HideAsync();
                }
            }
            catch
            {
                await Task.Delay(20);
                await SpinnerObj?.HideAsync();

            }
        }




        private async Task OnChange(Syncfusion.Blazor.Inputs.UploadChangeEventArgs args)
        {
            foreach (var file in args.Files)
            {
                var path = @"path" + file.FileInfo.Name;
                using (FileStream filestream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    await file.File.OpenReadStream(long.MaxValue).CopyToAsync(filestream);
                }
            }
        }

        //bool UploadVisible { get; set; } = false;

        //protected void SelectedFilesChanged(IEnumerable<UploadFileInfo> files)
        //{
        //    UploadVisible = files.ToList().Count > 0;

        //    InvokeAsync(StateHasChanged);
        //}
        protected string GetUploadUrl(string url)
        {
            string sRet = NavigationManager.ToAbsoluteUri(url).AbsoluteUri;
            return sRet; // NavigationManager.ToAbsoluteUri(url).AbsoluteUri;
        }

        public object[] Tools = new object[]{
        "Bold", "Italic",
        "FontName", "FontSize",
        "Formats", "Alignments", "OrderedList", "UnorderedList",
        "Outdent", "Indent", "|", "CreateTable", "Image", "FullScreen",
         "Undo", "Redo"
    };

        private void AddPhoto()
        {

        }

        private async Task GetAuthorInfo()
        {
            try
            {
                var authorInfo = await AutoresService.GetAuthorInfo_Api(SelectedAuthor.Descricao);
                message = "Autor tem informa��o dispon�vel...";

            }
            catch
            {
                message = "Autor n�o tem informa��o dispon�vel";
                StateHasChanged();
            }
        }

        private void ShowAlert(string msg)
        {
            message = msg;
            alertVisibility = true;
            StateHasChanged();
        }

        private Dictionary<string, object> htmlAttribute = new Dictionary<string, object>()
{
        {"rows", "5" }
    };
    }
}