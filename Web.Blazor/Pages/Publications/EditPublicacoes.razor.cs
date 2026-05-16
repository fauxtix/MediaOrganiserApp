using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.ImageEditor;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;
using System.Globalization;
using Web.Blazor.Services.ApiWrappers;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace Web.Blazor.Pages.Publications
{
    public partial class EditPublicacoes
    {
        [Parameter] public PubsWithDescriptions SelectedRecord { get; set; }
        [Parameter] public bool blnEdit { get; set; } // true = for Edit Mode, false = Insert
        [Parameter] public int CurrentCategory { get; set; }

        [Inject] TranslateClientService TranslateClient { get; set; }

        [Inject] public IPublicacaoService PubsService { get; set; }
        [Inject] public IAutorService autoresService { get; set; }
        [Inject] public ICategoriasService categoriasService { get; set; }
        [Inject] public IFormatosService formatosService { get; set; }
        [Inject] public ILocaisArquivoService locaisArquivoService { get; set; }
        [Inject] public IEditorasService editorasService { get; set; }
        [Inject] public IFormService formService { get; set; }
        [Inject] public IUtilitiesService utilitiesService { get; set; }
        [Inject] public IFileService fileService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }

        [Inject] public IGoogleBooksService GoogleBooksService { get; set; }

        [Inject] public IConfiguration config { get; set; }
        [Inject] public IWebHostEnvironment env { get; set; }
        [Inject] public ILogger<App> _logger { get; set; }
        [Inject] public IStringLocalizer<App> localizerApp { get; set; }
        [Inject] public SfDialogService DialogService { get; set; }

        public string CoverImage { get; set; }
        public int authorKey;

        SfImageEditor ImageEditor;
        private List<ImageEditorToolbarItemModel> customToolbarItem = new List<ImageEditorToolbarItemModel>() { };

        public string Resumo { get; set; }

        private string Extensions;
        private string downloadsFolder;

        private IEnumerable<AutorDTO> authorList;
        private AutorDTO newAuthor = new();
        private IEnumerable<FormatoPublicacaoVM> formatList;
        private IEnumerable<LocalArquivoVM> storageList;
        private IEnumerable<PublisherVM> editorList;
        private IEnumerable<CategoryVM> categoryList;

        private SfCheckBox<bool> chkLido;

        private bool PdfVisibility = false;
        private bool AuthorVisibility = false;
        private bool SearchApiVisibility = false;
        protected DialogEffect Effects = DialogEffect.FadeZoom;

        public int? DefaultValue = 100;
        public string UploadedFile { get; set; }
        private bool AppConfigured = true;

        IReadOnlyList<IBrowserFile> selectedFiles;
        protected IBrowserFile arquivo;
        bool Exibir { get; set; } = false;
        private bool FoundValidExt = false;
        string[] validExtensions = new string[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", "tiff" };

        private SfUploader objUploader;
        private UploadFiles uploaderFile; // syncfusion
        private string uploadedFileName { get; set; } = "";

        bool ShowLookup = false;

        protected bool alertUser = false;
        protected string alertMessage = string.Empty;
        protected string authorToSearch = string.Empty;
        protected bool dataReceivedFromGoogle = false;

        protected string? aiNotes;
        protected double? aiConfidence;

        protected int pubId;

        protected ApplicationSettingsDto AppSettings;

        private bool TranslateVisible { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            alertUser = false;
            alertMessage = string.Empty;
            authorToSearch = string.Empty;
            aiNotes = null;
            aiConfidence = null;

            var appSettingsTask = AppSettingsService.GetSettingsAsync();
            var authorsTask = autoresService.GetAllAsync();
            var formatsTask = formatosService.GetAllAsync();
            var locaisTask = locaisArquivoService.GetAllAsync();
            var publishersTask = editorasService.GetAllAsync();
            var categoriesTask = categoriasService.GetAllAsync(CategoryType.Publicacoes);

            await Task.WhenAll(appSettingsTask, authorsTask, formatsTask, locaisTask, publishersTask, categoriesTask);

            AppSettings = await appSettingsTask;

            var authors = await authorsTask;
            authorList = authors is not null ? authors.OrderBy(p => p.Descricao).ToList() : null;

            var formats = await formatsTask;
            formatList = formats is not null ? formats.OrderBy(p => p.Descricao).ToList() : null;

            var locais = await locaisTask;
            storageList = locais is not null ? locais.OrderBy(p => p.Descricao).ToList() : null;

            var publishers = await publishersTask;
            editorList = publishers is not null ? publishers.OrderBy(p => p.Descricao).ToList() : null;

            var categories = await categoriesTask;
            categoryList = categories is not null ? categories.OrderBy(p => p.Descricao).ToList() : null;

            if (authorList is null || formatList is null || storageList is null || editorList is null || categoryList is null)
                AppConfigured = false;

            Extensions = blnEdit ? ".png, .gif, .jpg, .jpeg" : ".pdf";
            downloadsFolder = config.GetValue<string>("AppSettings:PastaDownloadsPdf");

        }
        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrEmpty(SelectedRecord.ImagemCapa))
            {
                pubId = SelectedRecord.Id;
                var isABook = string.IsNullOrEmpty(SelectedRecord.DigitalPath);
                var isWordDocument = !isABook && Path.GetExtension(SelectedRecord.DigitalPath.ToLower()) == ".docx";
                var tempCoverImage = isWordDocument ? "/images/microsoft-word-logo.jpg" : SelectedRecord.ImagemCapa;

                var isSourceFileDocx = isWordDocument;
                if (isSourceFileDocx)
                {
                    CoverImage = "/images/word.png";
                }
                else
                {
                    var imageFilePath = Path.Combine(env.WebRootPath, tempCoverImage);
                    if (File.Exists(imageFilePath))
                    {
                        var fileInfo = new System.IO.FileInfo(imageFilePath);
                        if (File.Exists(imageFilePath) && fileInfo.Length < 1024)
                        {
                            CoverImage = "/images/no-image-available.png";
                        }
                        else
                        {
                            CoverImage = tempCoverImage;
                        }
                    }
                    else
                    {
                        if (SelectedRecord.ImagemCapa.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        {
                            // Normalize to HTTPS to avoid mixed-content blocking
                            var url = SelectedRecord.ImagemCapa;

                            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                            {
                                url = string.Concat("https://", url.AsSpan("http://".Length));
                            }

                            CoverImage = url;
                        }
                        else
                        {
                            CoverImage = "/images/No-image-available.png";
                        }

                        SelectedRecord.ImagemCapa = CoverImage;
                    }
                }
            }
            else
                CoverImage = "images/No-image-available.png";

            StateHasChanged();
        }


        private void OnBtnPdfClicked(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            PdfVisibility = true;
        }

        public void onLidoChange(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
        {
            SelectedRecord.Lido = chkLido.Checked;
        }

        public void Selected(SelectedEventArgs args)
        {
            uploadedFileName = args.FilesData[0].Name;
        }

        async Task HandleFileSelected(InputFileChangeEventArgs e)
        {
            Exibir = true;
            try
            {
                selectedFiles = e.GetMultipleFiles();

                arquivo = selectedFiles.FirstOrDefault();


                if (validExtensions.Contains(Path.GetExtension(arquivo.Name).ToLower()))
                {
                    FoundValidExt = true;
                }

                if (!FoundValidExt)
                {
                    //Mensagem = $"Ficheiro não tem extensão válida. Verifique, por favor";
                    //clsName = "alert alert-warning";
                    //UploadOk = false;

                }
                else if (arquivo != null)
                {
                    var filePath = Path.Combine(Path.GetDirectoryName(SelectedRecord.ImagemCapa), arquivo.Name);

                    using (Stream stream = arquivo.OpenReadStream())
                    {
                        using (FileStream fs = System.IO.File.Create(filePath))
                        {
                            await stream.CopyToAsync(fs);
                            SelectedRecord.ImagemCapa = filePath;
                            await objUploader.ClearAllAsync();
                            CoverImage = fileService.DecodeImage(filePath);
                        }
                    }
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public async Task OnChange(UploadChangeEventArgs args)
        {
            string previousImagePath = string.Empty;
            string coverImageToReplace = string.Empty;
            string previousImage = string.Empty;
            uploaderFile = args.Files[0];
            var bookCoversDirectory = AppSettings.PastaImagensCapasLivros;
            string[] subFolders = bookCoversDirectory.Split("\\");
            string BookImagesDirectory = Path.Combine(env.WebRootPath, "Files", subFolders[1], subFolders[2]);
            string path = string.Empty;

            if (!Directory.Exists(BookImagesDirectory))
                Directory.CreateDirectory(BookImagesDirectory);

            if (!blnEdit )
            {
                uploadedFileName = uploaderFile.File.Name;
                path = Path.Combine(BookImagesDirectory, uploadedFileName);
                if (!File.Exists(path))
                {
                    using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None))
                    {
                        await uploaderFile.File.OpenReadStream().CopyToAsync(fs);
                    }
                }

                CoverImage = $"/Files/{subFolders[1]}/{subFolders[2]}/{uploadedFileName}";
                var dirImagePos = BookImagesDirectory.IndexOf("Files");
                SelectedRecord.ImagemCapa = Path.Combine(BookImagesDirectory.Substring(dirImagePos), uploadedFileName);
                await objUploader.ClearAllAsync();
                await InvokeAsync(StateHasChanged);

                return;
            }


            var directoryName = "";
            if (SelectedRecord.Id > 0) // edit mode
            {
                coverImageToReplace = SelectedRecord.ImagemCapa;
                if (!string.IsNullOrWhiteSpace(coverImageToReplace))
                {
                    previousImage = coverImageToReplace.Substring(coverImageToReplace.LastIndexOf("\\") + 1);
                }
            }

            try
            {
                if (previousImage.ToLower() == uploadedFileName.ToLower())
                {
                    await objUploader.ClearAllAsync();
                    return;
                }

                if (string.IsNullOrEmpty(SelectedRecord.DigitalPath)) // not a digital file (pdf)
                {
                    // assume we have an image downloaded from Google Books, we need to save it locally and update the path accordingly
                    if (SelectedRecord.ImagemCapa.ToLower().StartsWith("http") ||
                        SelectedRecord.ImagemCapa.ToLower().Contains("photo") ||
                        SelectedRecord.ImagemCapa.ToLower().Contains("image") ||
                        SelectedRecord.ImagemCapa.ToLower().StartsWith("file") // local image file, but need to be replaced
                        || SelectedRecord.ImagemCapa.ToLower().Contains("available")) // image path contains "available" (no image available)
                    {


                        path = Path.Combine(BookImagesDirectory, uploadedFileName);
                        if (!File.Exists(path))
                        {
                            using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None))
                            {
                                await uploaderFile.File.OpenReadStream().CopyToAsync(fs);
                            }
                        }

                        CoverImage = $"/Files/{subFolders[1]}/{subFolders[2]}/{uploadedFileName}";
                        var dirImagePos = BookImagesDirectory.IndexOf("Files");
                        SelectedRecord.ImagemCapa = Path.Combine(BookImagesDirectory.Substring(dirImagePos), uploadedFileName);
                        await objUploader.ClearAllAsync();
                        await InvokeAsync(StateHasChanged);

                        return;
                    }
                }

                var folderName = SelectedRecord.DigitalPath.Substring(0, SelectedRecord.DigitalPath.LastIndexOf("\\"));
                var imagePath = Path.Combine(env.WebRootPath, folderName);
                var imageFile = Path.Combine(imagePath, "Capas", uploaderFile.File.Name);

                if (File.Exists(imageFile))
                {
                    CoverImage = fileService.DecodeImage(imageFile);
                    SelectedRecord.ImagemCapa = CoverImage;
                    await objUploader.ClearAllAsync();

                    StateHasChanged();
                    return;
                }

                if (!string.IsNullOrWhiteSpace(SelectedRecord.ImagemCapa) ||
                    !SelectedRecord.ImagemCapa.ToLower().Contains("images"))
                {
                    directoryName = Path.Combine(env.WebRootPath, Path.GetDirectoryName(SelectedRecord.ImagemCapa));
                    if (!SelectedRecord.ImagemCapa.ToLower().Contains("available"))
                    {
                        path = Path.Combine(directoryName, uploadedFileName);
                    }
                    else
                    {
                        path = imageFile;
                    }
                    previousImagePath = Path.Combine(directoryName, previousImage);
                }
                else
                {
                    BookImagesDirectory = Path.Combine(env.WebRootPath, "images", "BookHardCovers");

                    if (!Directory.Exists(BookImagesDirectory))
                        Directory.CreateDirectory(BookImagesDirectory);

                    path = Path.Combine(BookImagesDirectory, uploadedFileName);

                }
                if (!File.Exists(path))
                {

                    using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None))
                    {
                        await uploaderFile.File.OpenReadStream().CopyToAsync(fs);
                    }

                    //using (Stream stream = uploaderFile.File.OpenReadStream())
                    //{
                    //    using (FileStream fs = System.IO.File.Create(path))
                    //    {
                    //        await stream.CopyToAsync(fs);
                    //    }
                    //}

                    // Apaga imagem anterior
                    var deletePreviousImage = config.GetValue<string>("AppSettings:RemoveImagemAposAlteracao");
                    if (previousImage != uploadedFileName && deletePreviousImage == "true")
                    {
                        File.Delete(previousImagePath);
                    }
                }

                if (path.ToLower().Contains("book")) // hard cover publication (book)
                {
                    CoverImage = $"/images/bookhardcovers/{uploadedFileName}";
                    SelectedRecord.ImagemCapa = CoverImage;
                }
                else // pdf
                {
                    CoverImage = fileService.DecodeImage(path);
                    CoverImage = CoverImage.Replace(@"\\", @"\");
                    SelectedRecord.ImagemCapa = CoverImage;

                }

                SelectedRecord.ImagemCapa = CoverImage; // path;
                await objUploader.ClearAllAsync();

                StateHasChanged();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

        }



        protected async Task HandleApiResponse(GoogleBooksSearchResult resp)
        {
            if (resp?.Record is not null)
            {
                string yesMsg = localizerApp["Sim"];
                string noMsg = localizerApp["Nao"];
                string confirmMsg = localizerApp["ConfirmarTransferencia"];

                var dialogOptions = new DialogOptions()
                {
                    AllowDragging = true,
                    CloseOnEscape = false,
                    Width = "400px",
                    Height = "180px",
                    ShowCloseIcon = false,
                    PrimaryButtonOptions = new DialogButtonOptions() { Content = yesMsg, IconCss = "fa fa-check" },
                    CancelButtonOptions = new DialogButtonOptions() { Content = noMsg, IconCss = "fa fa-times" }
                };


                //bool isConfirm = await DialogService.ConfirmAsync(confirmMsg, resp.Titulo, dialogOptions);
                //if (isConfirm)
                //{
                //    SearchApiVisibility = false;

                //    SelectedRecord = resp;
                //    CoverImage = SelectedRecord.ImagemCapa;
                //    SearchApiVisibility = false;
                //}
                //else
                //{
                //    SearchApiVisibility = false;
                //}

                SearchApiVisibility = false;
                dataReceivedFromGoogle = true;

                aiNotes = resp.AiNotes;
                aiConfidence = resp.AiConfidence;

                SelectedRecord = resp.Record;
                CoverImage = SelectedRecord.ImagemCapa;

                StateHasChanged();

            }
        }

        private async Task CreateAuthor(string sNomeAutor)
        {
            AutorDTO autor = new AutorDTO()
            {
                Descricao = sNomeAutor,
                Sobre = "",
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

        private void GetSelectedAuthor(AutorDTO author)
        {
            ShowLookup = false;
            if (author is not null)
            {
                SelectedRecord.IdAutor = author.Id;
                StateHasChanged();
            }
        }

        private void OnChangeAuthor(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, AutorDTO> args)
        {
            SelectedRecord.IdAutor = args.Value;
        }
        private void OnChangeFormat(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, FormatoPublicacaoVM> args)
        {
            SelectedRecord.IdFormato = args.Value;

        }
        private void OnChangeCategory(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, CategoryVM> args)
        {
            SelectedRecord.IdCategoria = args.Value;
        }

        private void OnChangePublisher(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, PublisherVM> args)
        {
            SelectedRecord.IdEditora = args.Value;
        }
        private void OnChangeStoragePlace(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, LocalArquivoVM> args)
        {
            SelectedRecord.IdLocalArquivo = args.Value;
        }

        public void FecharPdf()
        {
            PdfVisibility = false;
        }

        private string ConvertToProperCase(string input)
        {
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(input.ToLower());
        }

        private Dictionary<string, object> htmlAttribute = new Dictionary<string, object>()
        {
            {"rows", "3" }
        };

        public object[] Tools = new object[]
        {
            "Bold", "Italic",
            "FontName", "FontSize",
            "Formats", "Alignments",
            "Outdent", "Indent", "|", "CreateTable",
            "Undo", "Redo"
        };

        private Task OpenTranslateDialog()
        {
            // Ensure InitialText is read by the TranslateText component (it reads InitialText on parameters set)
            // Set default languages if you want different defaults
            TranslateVisible = true;
            return Task.CompletedTask;
        }

        private async Task HandleTranslateRequested(TranslateRequestDto dto)
        {

            if (dto is null || string.IsNullOrWhiteSpace(dto.Text))
                return;

                SelectedRecord.Resumo = dto.Text;
                TranslateVisible = false;
                StateHasChanged();
                await Task.CompletedTask;
        }
    }
}