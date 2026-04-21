using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using SkiaSharp;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Exporting;
using Syncfusion.Pdf.Parsing;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using Web.Blazor.Data;
using Web.Blazor.Services.DocumentHelpers;
using Web.Blazor.Services.Reporting;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace Web.Blazor.Pages.BaseComponents
{
    public class PublicacoesBase : ComponentBase
    {
        [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public IRDLC_ReportService rdlcReportService { get; set; }
        [Inject] public IUtilitiesService utilitiesService { get; set; }
        [Inject] public IFileService fileService { get; set; }
        [Inject] public IMostUsedService mostUsedService { get; set; }
        [Inject] public IPublicacaoService publicacoesService { get; set; }
        [Inject] public IAutorService autoresService { get; set; }
        [Inject] public ICategoriasService categoriasService { get; set; }
        [Inject] public IFormatosService formatosService { get; set; }
        [Inject] public ILocaisArquivoService locaisArquivoService { get; set; }
        [Inject] public IEditorasService editorasService { get; set; }
        [Inject] public IUtilitiesService Utils { get; set; }
        [Inject] public IPdfHelper pdfHelper { get; set; }
        [Inject] public IDocxHelper wordHelper { get; set; }
        [Inject] public IOutlookMsgHelper outlookHelper { get; set; }
        [Inject] public IConfiguration config { get; set; }
        [Inject] public IStringLocalizer<App> localizerApp { get; set; }
        [Inject] protected IWebHostEnvironment hostingEnvironment { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] ILogger<PublicacoesBase> Logger { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IUserService UserService { get; set; }

        [Inject] public IJSRuntime JS { get; set; }
        [Inject] public IPdfGeneratorService PdfGeneratorService { get; set; }


        protected SfGrid<PubsWithDescriptions> gridObj;
        protected SfToast ToastObj;
        protected SfUploader sfUploader;
        protected DialogEffect Effects = DialogEffect.FadeZoom;
        protected SfSpinner SpinnerObj;

        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";
        protected string ToastIcon = "e-icon-ThumbsUp";
        protected int ToastTimeOut = 2500;

        string target { get; set; } = "#container";
        protected string spinnerTarget { get; set; } = "#spinnerContainer";

        public Theme CurrentTheme { get; set; }

        protected IEnumerable<PubsWithDescriptions> publicacoes;

        protected IEnumerable<AutorDTO> authorList;
        protected IEnumerable<FormatoPublicacaoVM> formatList;
        protected IEnumerable<LocalArquivoVM> storageList;
        protected IEnumerable<PublisherVM> publisherList;
        protected IEnumerable<PublisherVM> publisherListAF;
        protected IEnumerable<PublisherVM> publisherListGL;
        protected IEnumerable<PublisherVM> publisherListNS;
        protected IEnumerable<PublisherVM> publisherListTZ;
        protected IEnumerable<CategoryVM> categoryList;

        protected PubsWithDescriptions SelectedRecord = new PubsWithDescriptions();

        protected bool categoryChanged = false;
        protected int categoryBeforeEdition;

        protected bool blnEdit = false;
        public string OperationResult { get; set; }
        private string alertMessage = "";


        // Dialogs visibility flags
        public bool ReadPdfVisibility { get; set; } = false;
        public bool ReadWordFileVisibility { get; set; } = false;
        public bool ErrorVisibility { get; set; } = false;
        public bool EditDialogVisibility { get; set; } = false;
        public bool MaintainEditDialogOpen { get; set; } = true;
        public bool ValidationErrorsVisibility { get; set; } = false;
        public bool DeleteRecordConfirmVisibility { get; set; } = false;
        public bool TitleDialogVisibility { get; set; } = false;
        public bool UpdateTablesVisibility { get; set; } = false;

        protected bool ProgressBarVisibility { get; set; }
        public int publicacaoID { get; set; }

        public int pubsCatalogadas { get; set; }
        protected List<string> ValidationMessages = new();

        public string uploadedFile { get; set; } = "";
        public string uploadedCoverFile { get; set; }
        public Image uploadedCoverFile_img { get; set; } = null;

        public Query Query = new Query();

        // Context menu
        protected string WhoCalled;
        protected string description2Update;

        protected string msgAll;
        protected string msgReaded;
        protected string msgBlankPages;
        protected string msgWithoutCover;
        protected string msgDuplicated;

        protected PubsWithDescriptions selectedRowData { get; set; }
        protected int primaryIndex;

        public class Opcoes
        {
            public int Id { get; set; }
            public string MenuDescription { get; set; }
        }

        public List<Opcoes> OpcoesData;
        public int MenuOption { get; set; }
        public List<MenuItem> MenuItems = new List<MenuItem>();

        public string FileToView { get; set; }

        protected string TituloListagemPublicacoes;
        protected string TituloFicheirosPathErrada;

        protected static List<MenuItem> categoriesSubMenuItems { get; set; } = new List<MenuItem>();
        protected static List<MenuItem> categoriesSubMenuItems_1 { get; set; } = new List<MenuItem>();
        protected static List<MenuItem> categoriesSubMenuItems_2 { get; set; } = new List<MenuItem>();
        protected static List<MenuItem> categoriesSubMenuItems_3 { get; set; } = new List<MenuItem>();

        protected static List<MenuItem> publishersSubMenuItems { get; set; } = new List<MenuItem>();
        protected static List<MenuItem> publishersSubMenuItems_1 { get; set; } = new List<MenuItem>();
        protected static List<MenuItem> publishersSubMenuItems_2 { get; set; } = new List<MenuItem>();
        protected static List<MenuItem> publishersSubMenuItems_3 { get; set; } = new List<MenuItem>();
        protected static List<MenuItem> publishersSubMenuItems_4 { get; set; } = new List<MenuItem>();
        protected static List<MenuItem> publishersSubMenuItems_5 { get; set; } = new List<MenuItem>();

        protected static List<MenuItem> formatsSubMenuItems { get; set; } = new List<MenuItem>();
        protected static List<MenuItem> storingPlacesSubMenuItems { get; set; } = new List<MenuItem>();

        protected List<ActionItem> Actions = new();

        // Modelo para os itens do dropdown

        protected ApplicationSettingsDto settings;

        protected bool ShowEditButtonInPdfViewer { get; set; } = true;

        protected bool isMobile;
        private int WindowWidth { get; set; }

        protected override async Task OnInitializedAsync()
        {
            FileToView = "";

            PopulateActions();

            try
            {
                ProgressBarVisibility = true;
                publicacoes = (await GetAllPublicationsWithDescriptions()).ToList() ?? Enumerable.Empty<PubsWithDescriptions>();

                settings = await AppSettingsService.GetSettingsAsync();

                authorList = await autoresService.GetAllAsync();
                formatList = await formatosService.GetAllAsync();
                storageList = await locaisArquivoService.GetAllAsync();
                var publisherList_ = await editorasService.GetAllAsync();

                publisherList = publisherList_?.OrderBy(p => p.Descricao) ?? Enumerable.Empty<PublisherVM>();
                var categoryList_ = await categoriasService.GetAllAsync(CategoryType.Publicacoes);


                categoryList = categoryList_?.OrderBy(c => c.Descricao) ?? Enumerable.Empty<CategoryVM>();

                ProgressBarVisibility = false;

                TituloListagemPublicacoes = localizerApp["TituloListagem"];
                TituloFicheirosPathErrada = localizerApp["TituloCaminhoFicheirosIncorreto"];

                if (authorList is null || formatList is null || storageList is null || publisherList is null || categoryList is null)
                {
                    ValidationMessages = new()
                {
                    "Para continuar, deverá configurar tabelas da aplicação"
                };

                    EditDialogVisibility = false;
                    ReadPdfVisibility = false;
                    ReadWordFileVisibility = false;
                    DeleteRecordConfirmVisibility = false;
                    ValidationErrorsVisibility = false;
                    UpdateTablesVisibility = false;
                    ErrorVisibility = true;

                    ProgressBarVisibility = false;
                }

                SetupMenuItems();


            }
            catch (Exception exc)
            {
                Logger.LogError(exc.Message);
            }
        }


        private void PopulateActions()
        {
            // Lista de ações para o dropdown
            Actions = new List<ActionItem>
            {
                new ActionItem { Text = "Editar", Value = "Edit" },
                new ActionItem { Text = "Excluir", Value = "Delete" },
                new ActionItem { Text = "Ler PDF", Value = "ReadPdf" }
            };
        }

        private void UpdateView()
        {
            isMobile = WindowWidth <= 768; // Lógica para identificar visualização mobile
        }

        protected void OnActionSelected(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, ActionItem> args)
        {
            switch (args.Value)
            {
                case "Edit":
                    Console.WriteLine("Ação: Editar");
                    // Lógica para editar
                    break;
                case "Delete":
                    Console.WriteLine("Ação: Excluir");
                    // Lógica para excluir
                    break;
                case "ReadPdf":
                    Console.WriteLine("Ação: Ler PDF");
                    // Lógica para abrir o PDF
                    break;
            }
        }

        public async void OnLoad()
        {
            await SpinnerObj.ShowAsync();
        }
        public async void DataBound()
        {
            await SpinnerObj.HideAsync();
        }
        public async void OnActionBegin(ActionEventArgs<PubsWithDescriptions> args)
        {
            if (args.RequestType.ToString() == "Sorting")
            {
                await SpinnerObj.ShowAsync();
            }
            if (args.RequestType.ToString() == "Grouping")
            {
                await SpinnerObj.ShowAsync();
            }
        }


        public async Task OnCommandClicked(CommandClickEventArgs<PubsWithDescriptions> args)
        {
            var data = args.RowData;
            publicacaoID = data.Id;
            SelectedRecord = await publicacoesService.GetByIdAsync_Extended(publicacaoID);
            string sCover = SelectedRecord.ImagemCapa;

            primaryIndex = await this.gridObj.GetRowIndexByPrimaryKeyAsync(data.Id);

            if (primaryIndex >= 0)
            {
                await gridObj.SelectRowAsync(primaryIndex);
            }

            categoryBeforeEdition = SelectedRecord.IdCategoria;

            if (!string.IsNullOrEmpty(sCover))
            {
                if (File.Exists(sCover))
                {
                    uploadedCoverFile = fileService.DecodeImage(sCover);
                }
                else
                {
                    uploadedCoverFile = "Images/NoImage2.png";
                }
            }
            else
            {
                uploadedCoverFile = "Images/NoImage2.png";
            }

            if (args.CommandColumn.Type == CommandButtonType.Edit)
            {
                EditPublication();
            }

            if (args.CommandColumn.Type == CommandButtonType.Delete)
            {
                DeleteRecordConfirmVisibility = true;
                StateHasChanged();
            }

            // Read Pdf
            if (args.CommandColumn.Type == CommandButtonType.None)
            {
                if (ValidationMessages is not null)
                    ValidationMessages.Clear();

                var fileLocation = Path.Combine(hostingEnvironment.ContentRootPath, "wwwroot", SelectedRecord.DigitalPath);

                //var fileLocation = Path.Combine(hostingEnvironment.WebRootPath, SelectedRecord.DigitalPath);
                var fileExtension = Path.GetExtension(fileLocation).ToLower();
                FileToView = fileLocation;

                if (!File.Exists(fileLocation))
                {
                    PrepareAndShowValidationErrors();
                }
                else if (fileExtension == ".pdf")
                {
                    ProgressBarVisibility = true;
                    ShowEditButtonInPdfViewer = true;
                    ReadPdfVisibility = true;
                    ProgressBarVisibility = false;
                }
                else if (fileExtension == ".msg")
                {
                    var pdfCreated = fileLocation.Replace("msg", "pdf");
                    var weHaveAPdfAlready = File.Exists(pdfCreated);
                    if (weHaveAPdfAlready)
                    {
                        FileToView = pdfCreated;
                        ReadPdfVisibility = true;
                        return;
                    }
                    else
                    {
                        await Task.Delay(300);
                        ProgressBarVisibility = true;
                        var msgConverted = "";

                        msgConverted = await outlookHelper.ConvertMsgToPdfAsync(fileLocation);
                        if (msgConverted != null)
                        {
                            await Task.Delay(300);
                            ProgressBarVisibility = false;
                            FileToView = msgConverted;
                            ReadPdfVisibility = true;
                        }
                    }

                }
                else // docx
                {
                    try
                    {
                        var pdfCreated = fileLocation.Replace("docx", "pdf");
                        var weHaveAPdfAlready = File.Exists(pdfCreated);
                        if (weHaveAPdfAlready)
                        {
                            FileToView = pdfCreated;
                            ReadPdfVisibility = true;
                            return;
                        }

                        await Task.Delay(300);
                        ProgressBarVisibility = true;
                        var pdfConverted = wordHelper.ConvertToPdf(fileLocation);
                        if (pdfConverted != null)
                        {
                            // search record already created (with docx)

                            await Task.Delay(300);
                            ProgressBarVisibility = false;
                            FileToView = pdfConverted;
                            ReadPdfVisibility = true;
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex.Message);
                    }
                }

                StateHasChanged();
            }
        }

        private void PrepareAndShowValidationErrors()
        {
            if (Path.GetExtension(SelectedRecord.DigitalPath).ToLower() != ".pdf")
            {
                ValidationMessages = new()
                {
                    $"'{SelectedRecord.Titulo}' ({SelectedRecord.Categoria}) {localizerApp["TituloFicheiroNaoPdf"]}."
                };
            }
            else if (!string.IsNullOrEmpty(SelectedRecord.DigitalPath))
            {
                string filePath = Path.GetDirectoryName(SelectedRecord.DigitalPath).ToLower();
                int pos = filePath.IndexOf("uploads");
                if (pos >= 0)
                {
                    string result = filePath.Substring(pos);
                    ValidationMessages = new()
                    {
                        $"{localizerApp["TituloFicheiro"]} {SelectedRecord.DigitalPath} {localizerApp["TituloFicheiroNaoExisteLocalEspecificado"]}",
                        $"Path: {result}",
                        $"{localizerApp["TituloFicheiro"]}: {Path.GetFileName(SelectedRecord.DigitalPath)}"
                    };
                }
                else
                {
                    ValidationMessages.Add(localizerApp["TituloPastaUploadsNãoEncontrada"]);
                }
            }

            EditDialogVisibility = false;
            MaintainEditDialogOpen = false;
            ReadPdfVisibility = false;
            ReadWordFileVisibility = false;
            FileToView = "";
            ProgressBarVisibility = false;
            ErrorVisibility = true;
        }

        public void EditPublication()
        {
            blnEdit = true;
            EditDialogVisibility = true;
            StateHasChanged();
        }

        protected async void OnRecordDoubleClick(RecordDoubleClickEventArgs<PubsWithDescriptions> args)
        {
            var data = args.RowData;
            publicacaoID = data.Id;
            SelectedRecord = await publicacoesService.GetByIdAsync_Extended(publicacaoID);

            EditPublication();
        }

        public async Task DDValueChanched(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, Opcoes> args)
        {
            ProgressBarVisibility = true;
            MenuOption = args.Value;
            await ProcessComboAction();
            ProgressBarVisibility = false;
            StateHasChanged();
        }

        private async Task ProcessComboAction()
        {
            // field 'publicacoes' = datagrid datasource
            switch (MenuOption)
            {
                case 0: // normal edit, no action selected in the combobox
                case 99: // All
                    publicacoes = await GetAllPublicationsWithDescriptions();
                    break;
                case 1: // Readed
                    publicacoes = await publicacoesService.Listar_Lidos(true);
                    break;
                case 2: // Pages = 0
                    publicacoes = await publicacoesService.Listar_SemPaginas();
                    break;
                case 3: // Without cover
                    publicacoes = await publicacoesService.Listar_SemCapa();
                    break;
                case 4: // Duplicates
                    publicacoes = await publicacoesService.Listar_Duplicados();
                    break;
                default:
                    break;
            }

            await gridObj.Refresh();
            StateHasChanged();

        }
        public async Task<IEnumerable<PubsWithDescriptions>> GetAllPublicationsWithDescriptions()
        {
            string userId;
            string role;
            var userDetails = await UserService.GetAuthenticatedUserDetails(AuthenticationStateProvider);
            userId = userDetails.UserId;
            role = userDetails.Role;

            if (userId is null)
            {
                // inform user is not authenticated...
            }

            var output = await publicacoesService.GetAllAsync(userId, role);
            return output;
        }

        public void Voltar()
        {
            EditDialogVisibility = false;
        }

        public void FecharPdf()
        {
            ReadPdfVisibility = false;
            FileToView = "";
            StateHasChanged();
        }
        public void FecharWordViewer()
        {
            ReadWordFileVisibility = false;
            FileToView = "";
            StateHasChanged();
        }
        public async Task AddOrSaveRecord()
        {
            var isABook = string.IsNullOrEmpty(SelectedRecord.DigitalPath);
            var isWordDocument = !isABook && Path.GetExtension(SelectedRecord.DigitalPath.ToLower()) == ".docx";
            var coverImage = isWordDocument ? "/images/microsoft-word.png" : SelectedRecord.ImagemCapa;

            var publicacao = new PubsWithDescriptions()
            {
                Id = SelectedRecord.Id,
                isbn = SelectedRecord.isbn == null ? "" : SelectedRecord.isbn,
                Titulo = SelectedRecord.Titulo,
                TituloOriginal = SelectedRecord.TituloOriginal == null ? "" : SelectedRecord.TituloOriginal,
                Resumo = SelectedRecord.Resumo == null ? "" : SelectedRecord.Resumo,
                IdAutor = SelectedRecord.IdAutor,
                IdCategoria = SelectedRecord.IdCategoria,
                IdEditora = SelectedRecord.IdEditora,
                IdFormato = SelectedRecord.IdFormato,
                IdLocalArquivo = SelectedRecord.IdLocalArquivo,
                Preco = SelectedRecord.Preco,
                Paginas = SelectedRecord.Paginas,
                AnoEdicao = SelectedRecord.AnoEdicao,
                ImagemCapa = SelectedRecord.ImagemCapa == null ? "" : coverImage,
                DigitalPath = SelectedRecord.DigitalPath == null ? "" : SelectedRecord.DigitalPath,
                Lido = SelectedRecord.Lido = SelectedRecord.Lido,
                Rating = SelectedRecord.Rating == 0 ? 1 : SelectedRecord.Rating,
                DataMovimento = SelectedRecord.DataMovimento,
                UserID = SelectedRecord.UserID == 0 ? 1 : SelectedRecord.UserID,
                CreatedBy = SelectedRecord.CreatedBy,
                LastModifiedBy = SelectedRecord.LastModifiedBy,
            };

            //Validate entries
            ValidationMessages = publicacoesService.RegistoComErros(publicacao);


            if (ValidationMessages == null)
            {
                var pub = publicacao.DigitalPath;
                var pubToCopy = isWordDocument ? pub.Replace("docx", "pdf") : pub;
                PublicacaoACopiar pub2Transfer = new()
                {
                    PubID = publicacao.Id,
                    Publicacao = Path.Combine(hostingEnvironment.WebRootPath, pub),
                    CodGenero = publicacao.IdCategoria,
                    opcao = OpcoesRegisto.Gravar
                };

                if (blnEdit)
                {
                    try
                    {
                        string digitalPathBeforeUpdate = publicacao.DigitalPath;
                        string coverPathBeforeUpdate = coverImage;

                        await publicacoesService.UpdatePubAsync(publicacao.Id, publicacao);

                        ToastTitle = localizerApp["editionMsg"] + " " + localizerApp["frmLivros"];
                        ToastCssClass = "e-toast-success";
                        ToastContent = localizerApp["SuccessUpdate"];

                        if (publicacao.IdCategoria != categoryBeforeEdition && categoryBeforeEdition > 0)
                        {
                            var categoryVM = await categoriasService.GetCategoriesById(publicacao.IdCategoria);

                            if (categoryVM.Descricao.ToLower().Contains("livro") == false)
                            {
                                var destinationFiles_FullPath = await fileService.TransfereFicheiroParaRepositorio(pub2Transfer);

                                var sDigitalPath = destinationFiles_FullPath.Item1;
                                var sCoverFile = destinationFiles_FullPath.Item2;
                                if (sCoverFile.ToLower() == "error")
                                {
                                    // restore previous paths and update, if error
                                    publicacao.DigitalPath = digitalPathBeforeUpdate;
                                    publicacao.ImagemCapa = coverPathBeforeUpdate;
                                    publicacao.IdCategoria = categoryBeforeEdition;
                                    await publicacoesService.Atualiza_Publicacao(publicacaoID, publicacao);

                                    ValidationMessages = new List<string>
                                    {
                                        $"{localizerApp["FalhaGravacaoRegisto"] } ({sDigitalPath})"
                                    };
                                    ErrorVisibility = true;
                                    StateHasChanged();
                                }
                                else
                                {
                                    ProgressBarVisibility = true;

                                    publicacao.DigitalPath = sDigitalPath;
                                    var physicalCoverPath = Path.Combine(hostingEnvironment.WebRootPath, sCoverFile);

                                    if (File.Exists(physicalCoverPath))
                                    {
                                        var imgSize_2 = new System.IO.FileInfo(physicalCoverPath).Length;
                                        decimal imgSize = Math.Round(imgSize_2 / (decimal)1024, 2);

                                        if (imgSize >= 2)
                                            publicacao.ImagemCapa = sCoverFile;
                                        else
                                            publicacao.ImagemCapa = Path.Combine("images", "No-image-available.png");
                                    }
                                    else
                                    {

                                        if (isWordDocument)
                                        {
                                            publicacao.ImagemCapa = Path.Combine("images", "microsoft-word-logo.png");
                                        }
                                        else
                                        {
                                            publicacao.ImagemCapa = Path.Combine("images", "No-image-available.png");
                                        }
                                    }

                                    ProgressBarVisibility = false;


                                    await publicacoesService.Atualiza_Publicacao(publicacaoID, publicacao);
                                    ToastTitle += $" ({localizerApp["Changed category"]})";
                                    ToastContent = localizerApp["SuccessUpdate"];
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ValidationMessages = new List<string>
                        {
                            $"{localizerApp["FalhaGravacaoRegisto"] } ({ex.Message})"
                        };
                        Logger.LogError((ex.Message));
                        ErrorVisibility = true;
                        StateHasChanged();
                    }
                }
                else
                {
                    try
                    {
                        await publicacoesService.InsertPubAsync(publicacao);

                        ToastTitle = localizerApp["creationMsg"] + " " + localizerApp["frmLivros"];
                        ToastCssClass = "e-toast-success";
                        ToastContent = localizerApp["SuccessInsert"];
                        SelectedRecord = new PubsWithDescriptions();

                    }
                    catch (Exception ex)
                    {
                        ValidationMessages = new List<string>
                        {
                            $"{localizerApp["FalhaCriacaoRegisto"] } ({ex.Message})"
                        };
                        Logger.LogError((ex.Message));
                        ErrorVisibility = true;
                        StateHasChanged();
                    }
                }
                EditDialogVisibility = false;

                if (!string.IsNullOrEmpty(ToastTitle))
                {
                    EditDialogVisibility = false;
                    StateHasChanged();

                    await Task.Delay(200);
                    await ToastObj.ShowAsync();

                    await ProcessComboAction(); // 12-08-2021 - mostra registos da opção selecionada (Todos, sem páginas, etc)

                    //publicacoes = await GetAllPublicationsWithDescriptions();

                    await gridObj.Refresh();

                }
            }
            else
            {
                ErrorVisibility = true;
                MaintainEditDialogOpen = true;
            }
        }

        public async Task NotifyAddingRecord()
        {
            try
            {
                string categoria = "";
                string editora = "";
                string autor = "";
                string formato = publicacoes.Any() ? await mostUsedService.GetOpcaoMaisUsada("idFormato", "Formato", "Formato.Id", "Descricao") : "";
                string localArquivo = publicacoes.Any() ? await mostUsedService.GetOpcaoMaisUsada("idLocalArquivo", "LocalArquivo", "LocalArquivo.Id", "Descricao") : "";

                SelectedRecord = new PubsWithDescriptions()
                {
                    DataMovimento = DateTime.UtcNow,
                    IdAutor = Utils.GetPkValueFromTable("Autor", "Id", "Descricao", !string.IsNullOrEmpty(autor) ? autor : "Desconhecido"),
                    IdCategoria = Utils.GetPkValueFromTable("Categoria", "Id", "Descricao", !string.IsNullOrEmpty(categoria) ? categoria : "Desconhecida"),
                    IdFormato = Utils.GetPkValueFromTable("Formato", "Id", "Descricao", !string.IsNullOrEmpty(formato) ? formato : "Desconhecida"),
                    IdEditora = Utils.GetPkValueFromTable("Editora", "Id", "Descricao", !string.IsNullOrEmpty(editora) ? editora : "Desconhecida"),
                    IdLocalArquivo = Utils.GetPkValueFromTable("LocalArquivo", "Id", "Descricao", !string.IsNullOrEmpty(localArquivo) ? localArquivo : "Desconhecida"),
                    Rating = 1,
                    UserID = 1,
                    Paginas = 0,
                    Lido = false,
                    AnoEdicao = DateTime.Today.Year.ToString(),
                    isbn = ""
                };

                blnEdit = false;
                EditDialogVisibility = true;

            }
            catch (Exception ex)
            {
                Logger.LogError($"{localizerApp["TituloErroNotificacaoCriacaoRegisto"]}: ", ex.Message);
            }
        }
        public async Task ConfirmDeleteYes()
        {
            try
            {
                var pub = await publicacoesService.Query_ById_Extended(publicacaoID);

                await publicacoesService.DeletePubAsync(publicacaoID);

                var deleted = await fileService.DeletePhysicalPubPdf(pub);
                if (!deleted)
                {
                    // If deletion failed, inform the user (ShowProcessResult already used elsewhere)
                    await ShowProcessResult("e-toast-danger", localizerApp["msgDeleteFileFailed"], "e-icon-ThumbsDown", 4000);
                }

                DeleteRecordConfirmVisibility = false;

                publicacoes = await GetAllPublicationsWithDescriptions();
                await gridObj.Refresh();

                ToastTitle = localizerApp["deletionMsg"] + " " + localizerApp["frmLivros"];
                ToastCssClass = "e-toast-warning";
                ToastContent = localizerApp["SuccessDelete"];
                StateHasChanged();
            }
            catch (Exception ex)
            {
                ValidationMessages = new List<string>
                        {
                            $"{localizerApp["FalhaAnulacaoRegisto"] } ({ex.Message})"
                        };
                ErrorVisibility = true;
                StateHasChanged();
            }
        }

        public void ConfirmDeleteNo()
        {
            DeleteRecordConfirmVisibility = false;
        }

        public async Task ConfirmUpdateYes()
        {
            try
            {
                ProgressBarVisibility = true;
                await AtualizaTabelas_Ctx(description2Update);
                UpdateTablesVisibility = false;
                publicacoes = await GetAllPublicationsWithDescriptions();
                await gridObj.Refresh();
                ProgressBarVisibility = false;

                ToastTitle = localizerApp["updateRecordsMsg"] + " " + localizerApp["frmLivros"];
                ToastCssClass = "e-toast-success";
                ToastContent = localizerApp["SuccessUpdates"] + " (" + description2Update + ")";
                ToastTimeOut = 3000;
                StateHasChanged();
                await Task.Delay(200);
                await ToastObj.ShowAsync();
            }
            catch (Exception ex)
            {
                ValidationMessages = new List<string>
                        {
                            $"{localizerApp["FalhaGravacaoRegisto"] } ({ex.Message})"
                        };
                ErrorVisibility = true;
                StateHasChanged();
            }
        }
        public void ConfirmUpdateNo()
        {
            UpdateTablesVisibility = false;
        }

        public async Task OnToolbarClick(ClickEventArgs args)
        {
            if (args.Item.Id == "Clear")
            {
                await gridObj.SearchAsync("");
            }
            else if (args.Item.Id == "PLPDF")
            {
                ShowEditButtonInPdfViewer = false;
                ProgressBarVisibility = true;

                if (publicacoes != null && publicacoes.Any())
                {
                    var pdfStream = PdfGeneratorService.GeneratePublicacoesPdf(publicacoes.ToList());
                    FileToView = $"data:application/pdf;base64,{Convert.ToBase64String(pdfStream.ToArray())}";
                    //FileToView = Convert.ToBase64String(pdfStream.ToArray());
                    ReadPdfVisibility = true;
                }

                ProgressBarVisibility = false;
                await InvokeAsync(StateHasChanged);
            }
            else if (args.Item.Id == "FNOD")
            {
                var pdfFiles = GetPdfFilesWithoutPublications();
            }
            else if (args.Item.Id == "FWRP")
            {
                NavigationManager.NavigateTo("/pubswithwrongpath");
            }
        }

        public void closeAlertBox()
        {
            ValidationErrorsVisibility = false;
            DeleteRecordConfirmVisibility = false;
        }

        public void closeErrorBox()
        {
            ErrorVisibility = false;
            if (MaintainEditDialogOpen)
                EditDialogVisibility = true;
        }

        protected async Task OnActionCompleteHandler(ActionCompleteEventArgs args)
        {
            if (args.FileData.Count() == 0) return;

            string nomeAutorDocumento = "";
            string tituloDocumento = "";
            string tituloOriginal = "";
            string resumoDocumento = "";
            bool autorExiste = false;
            string extensaoFicheiro = "";
            string anoEdicao = "";
            string titulo = "";

            string pastaDownloads;
            string defaultUploadFolder_Pdf;
            string defaultUploadFolder_Pdf_Covers;
            string pdfFullName;
            int pageCount = 0;
            int totalFilesChoosen;
            Dictionary<Metadados, string> fileMetadata = null;
            string filenameWithoutPeriods;

            bool titleExist;

            try
            {
                foreach (var file in args.FileData)
                {
                    ProgressBarVisibility = true;
                    uploadedFile = file.Name;
                    nomeAutorDocumento = "";
                    tituloDocumento = "";
                    tituloOriginal = "";
                    resumoDocumento = "";
                    autorExiste = false;
                    extensaoFicheiro = "";
                    anoEdicao = "";
                    titulo = "";
                    blnEdit = false;
                    EditDialogVisibility = false;

                    pageCount = 0;
                    totalFilesChoosen = args.FileData.Count;
                    fileMetadata = null;

                    pastaDownloads = Path.Combine(settings.PastaPublicacoes);
                    defaultUploadFolder_Pdf = Path.Combine("Files\\" + pastaDownloads, "Desconhecida");
                    defaultUploadFolder_Pdf_Covers = Path.Combine(defaultUploadFolder_Pdf, "Capas\\");
                    pdfFullName = Path.Combine(hostingEnvironment.WebRootPath, defaultUploadFolder_Pdf, uploadedFile);

                    titleExist = await publicacoesService.TituloExiste_BD(uploadedFile);
                    if (titleExist)
                    {
                        await ShowProcessResult("e-toast-warning",
                            $"{localizerApp["lblTitulo"]} {Path.GetFileName(pdfFullName)} {localizerApp["TituloJaExiste"]}.",
                            "e-icon-ThumbsDown",
                            5000);
                        await sfUploader.ClearAllAsync();
                        await sfUploader.CancelAsync();
                        ProgressBarVisibility = false;
                        return;
                    }

                    Directory.CreateDirectory(defaultUploadFolder_Pdf_Covers);


                    filenameWithoutPeriods = RemovePeriodsFromFilename(pdfFullName);
                    uploadedCoverFile = Path.Combine(defaultUploadFolder_Pdf_Covers, filenameWithoutPeriods);
                    extensaoFicheiro = Path.GetExtension(uploadedFile).ToLower();


                    if (extensaoFicheiro == ".msg")
                    {
                        var outlookSvc = new OutlookService();
                        fileMetadata = outlookSvc.ReadMSGMessage(pdfFullName);
                    }
                    else if (extensaoFicheiro == ".pdf")
                    {
                        fileMetadata = pdfHelper.GetPdfMetadata_Sync(pdfFullName);
                    }
                    else if (extensaoFicheiro == ".docx")
                    {
                        using (FileStream docStream = new FileStream(pdfFullName, FileMode.Open, FileAccess.Read))
                        {
                            fileMetadata = wordHelper.GetDocxMetadata(docStream);
                        }
                    }
                    else
                    {
                        ProgressBarVisibility = false;

                        ToastTitle = localizerApp["msgUpload"];
                        ToastCssClass = "e-toast-danger";
                        ToastContent = localizerApp["msgUploadError"];
                        ToastIcon = "e-icon-ThumbsDown";
                        await Task.Delay(200);
                        await ToastObj.HideAsync();
                        await sfUploader.CancelAsync();
                        return;
                    }


                    foreach (var item in fileMetadata)
                    {
                        if (fileMetadata != null)
                        {
                            nomeAutorDocumento = "Desconhecido";
                            resumoDocumento = "N/D";
                            autorExiste = false;
                            foreach (var setting in fileMetadata)
                            {
                                switch (setting.Key)
                                {
                                    case Metadados.Titulo:
                                        tituloDocumento = setting.Value;
                                        break;

                                    case Metadados.AnoEdicao:
                                        anoEdicao = setting.Value;
                                        if (string.IsNullOrEmpty(anoEdicao))
                                        {
                                            anoEdicao = DateTime.Today.Year.ToString();
                                        }
                                        break;
                                    case Metadados.Paginas:
                                        pageCount = int.Parse(setting.Value);
                                        break;

                                    case Metadados.Autor:
                                        nomeAutorDocumento = setting.Value;
                                        if (!string.IsNullOrEmpty(nomeAutorDocumento))
                                        {
                                            nomeAutorDocumento = nomeAutorDocumento.Replace("'", "");
                                            autorExiste = await autoresService.AutorExiste(nomeAutorDocumento);
                                            if (!autorExiste)
                                            {
                                                await CriaAutor(nomeAutorDocumento);
                                            }
                                        }
                                        else
                                        {
                                            nomeAutorDocumento = "Desconhecido";
                                            await CriaAutor(nomeAutorDocumento);

                                        }
                                        break;
                                    case Metadados.Assunto:

                                        if (tituloDocumento.ToLower().Contains("erro"))
                                        {
                                            ValidationMessages = new()
                                            {
                                                tituloDocumento + " " + setting.Value
                                            };
                                            EditDialogVisibility = false;
                                            MaintainEditDialogOpen = false;
                                            ProgressBarVisibility = false;
                                            StateHasChanged();
                                            return;
                                        }

                                        resumoDocumento = localizerApp["msgUploadSubject"] + " " + setting.Value;
                                        if (string.IsNullOrEmpty(setting.Value))
                                            resumoDocumento = "N/D";

                                        break;
                                    case Metadados.Keywords:
                                        if (resumoDocumento == "N/D")
                                        {
                                            resumoDocumento = localizerApp["msgUploadKeywords"] + " " + setting.Value;
                                            if (string.IsNullOrEmpty(setting.Value))
                                                resumoDocumento = "N/D";
                                        }
                                        else
                                        {
                                            resumoDocumento += Environment.NewLine + localizerApp["msgUploadKeywords"] + " " + setting.Value;
                                        }
                                        break;
                                } // switch
                            } // para cada setting do pdf
                        } // PdfMetadata != null

                    }

                    string sPNGPath = "";
                    var imagePath = defaultUploadFolder_Pdf_Covers;

                    if (extensaoFicheiro.ToLower() == ".pdf")
                    {
                        try
                        {
                            var pdfCoverFilePath = Path.Combine(hostingEnvironment.WebRootPath, defaultUploadFolder_Pdf_Covers);
                            uploadedCoverFile_img = pdfHelper.GetPdfCover_Sync(pdfCoverFilePath, pdfFullName, out sPNGPath);
                        }
                        catch (Exception ex)
                        {
                            ValidationMessages = new List<string>
                            {
                                $"{localizerApp["msgUploadCountingPdfPages"] } ({ex.StackTrace})"
                            };
                            Logger.LogError((ex.Message));
                            ProgressBarVisibility = false;

                            ErrorVisibility = true;
                            StateHasChanged();
                        }
                    }

                    if (uploadedCoverFile_img != null)
                    {
                        await using var imageStream = new MemoryStream();
                        uploadedCoverFile_img.Save(imageStream, ImageFormat.Png);
                        imageStream.Flush();

                        imageStream.Position = 0;

                        await using var outStream = File.OpenWrite(sPNGPath);
                        imageStream.WriteTo(outStream);
                    }

                    if (string.IsNullOrEmpty(tituloDocumento))
                    {
                        titulo = Path.GetFileNameWithoutExtension(filenameWithoutPeriods);
                        if (string.IsNullOrEmpty(titulo))
                            titulo = filenameWithoutPeriods;
                    }
                    else
                    {
                        titulo = tituloDocumento;
                    }

                    string editora = publicacoes.Any() ? await mostUsedService.GetOpcaoMaisUsada("idEditora", "Editora", "Editora.Id", "Descricao") : "";
                    string autor = publicacoes.Any() ? await mostUsedService.GetOpcaoMaisUsada("idAutor", "Autor", "Autor.Id", "Descricao") : "";

                    string formato = publicacoes.Any() ? await mostUsedService.GetOpcaoMaisUsada("idFormato", "Formato", "Formato.Id", "Descricao") : "";
                    if (formato.ToLower().Contains("descon") && extensaoFicheiro.ToLower() == ".pdf")
                    {
                        formato = "Digital - EBook";
                    }

                    string localArquivo = publicacoes.Any() ? await mostUsedService.GetOpcaoMaisUsada("idLocalArquivo", "LocalArquivo", "LocalArquivo.Id", "Descricao") : "";

                    tituloOriginal = Path.GetFileName(pdfFullName);

                    int idCategoria = Utils.GetPkValueFromTable("Categoria", "Id", "Descricao", "Desconhecida");
                    if (idCategoria == 0) idCategoria = 1;

                    int idAutor = Utils.GetPkValueFromTable("Autor", "Id", "Descricao", nomeAutorDocumento);
                    if (idAutor == 0)
                        idAutor = Utils.GetPkValueFromTable("Autor", "Id", "Descricao", "Desconhecido");

                    if (idAutor == 0) idAutor = 1;

                    int idEditora = Utils.GetPkValueFromTable("Editora", "Id", "Descricao", !string.IsNullOrEmpty(editora) ? editora : "Desconhecida");
                    if (idEditora == 0) idEditora = 1;

                    int idFormato = Utils.GetPkValueFromTable("Formato", "Id", "Descricao", !string.IsNullOrEmpty(formato) ? formato : "Desconhecida");
                    if (idFormato == 0) idFormato = 1;

                    int idLocalArquivo = Utils.GetPkValueFromTable("LocalArquivo", "Id", "Descricao", !string.IsNullOrEmpty(localArquivo) ? localArquivo : "Desconhecida");
                    if (idLocalArquivo == 0) idLocalArquivo = 1;

                    try
                    {
                        var pubPath = Path.Combine(defaultUploadFolder_Pdf, Path.GetFileName(filenameWithoutPeriods));

                        if (!string.IsNullOrEmpty(sPNGPath))
                        {
                            var imgSize_2 = new System.IO.FileInfo(sPNGPath).Length;
                            decimal imgSize = Math.Round(imgSize_2 / (decimal)1024, 2);
                            if (!string.IsNullOrEmpty(sPNGPath) && imgSize >= 2)
                                imagePath = Path.Combine(defaultUploadFolder_Pdf_Covers, Path.GetFileName(sPNGPath));
                            else
                                imagePath = Path.Combine("images", "No-image-available.png");
                        }
                        else
                        {
                            if (extensaoFicheiro == ".docx")
                                imagePath = Path.Combine("images", "microsoft-word.png");
                        }

                        // Fill model to insert
                        var publicacao = new PubsWithDescriptions()
                        {
                            isbn = "",
                            Titulo = titulo,
                            TituloOriginal = tituloOriginal,
                            Resumo = resumoDocumento,
                            IdAutor = idAutor,
                            IdCategoria = idCategoria,
                            IdEditora = idEditora,
                            IdFormato = idFormato,
                            IdLocalArquivo = idLocalArquivo,
                            Preco = 0,
                            Paginas = pageCount,
                            AnoEdicao = anoEdicao,
                            DigitalPath = pubPath,
                            ImagemCapa = imagePath,
                            Lido = false,
                            Rating = 1,
                            DataMovimento = DateTime.Now,
                            UserID = 1
                        };

                        // Write to database
                        await publicacoesService.InsertPubAsync(publicacao);
                        ProgressBarVisibility = false;

                        await ShowProcessResult("e-toast-success", localizerApp["msgUploadCreationSuccess"] + " " + alertMessage, "e-icon-ThumbsUp");

                    }
                    catch (Exception exc)
                    {
                        Logger.LogError($"{localizerApp["FalhaCriacaoRegisto"]} : : {exc.Message}");
                        await sfUploader.ClearAllAsync();
                        ToastCssClass = "e-toast-danger";
                        ToastIcon = "e-icon-ThumbsDown";
                        ToastContent = $"Erro ao processar ficheiros ({exc.Message})";

                        ToastTimeOut = 4000;
                        ProgressBarVisibility = false;

                        await Task.Delay(200);
                        await ToastObj.ShowAsync();
                    }

                    publicacaoID = await publicacoesService.GetLastIdLivro();

                    categoryBeforeEdition = publicacaoID;

                    if (!string.IsNullOrEmpty(pdfFullName))
                    {
                        if (!File.Exists(pdfFullName))
                        {
                            ProgressBarVisibility = false;

                            alertMessage = @$"\n{localizerApp["msgUploadStorageFileNotInDisk"]}+\r\n({pdfFullName}). Confirme. p.f.";
                            await ShowProcessResult("e-toast-success", localizerApp["msgUploadCreationSuccess"] + " " + alertMessage, "e-icon-ThumbsUp");
                        }
                    }

                    await sfUploader.ClearAllAsync();
                }

                publicacoes = await GetAllPublicationsWithDescriptions();
                await gridObj.Refresh();
                ProgressBarVisibility = false;

                if (publicacaoID > 0)
                    SelectedRecord = await publicacoesService.Listar_ID_Extended(publicacaoID);

                EditDialogVisibility = false;
                StateHasChanged();

            }
            catch (Exception ex)
            {
                ValidationMessages = new List<string>
                 {
                    $"{localizerApp["FalhaGravacaoRegisto"] } ({ex.Message})"
                 };

                Logger.LogError((ex.Message));
                ProgressBarVisibility = false;

                ErrorVisibility = true;
                StateHasChanged();
            }
        }

        public async void OnRowSelected(RowSelectEventArgs<PubsWithDescriptions> args)
        {
            selectedRowData = args.Data;
            primaryIndex = await this.gridObj.GetRowIndexByPrimaryKeyAsync(selectedRowData.Id);
        }

        // TODO - check to see if this is still needed
        protected async Task OnChangeUpload(UploadChangeEventArgs args)
        {
            string nomeAutorDocumento = "";
            string tituloDocumento = "";
            string tituloOriginal = "";
            string resumoDocumento = "";
            bool autorExiste = false;
            string extensaoFicheiro = "";
            string pastaDownloads = Path.Combine(config.GetValue<string>("AppSettings:PastaDownloadsPdf"), "Desconhecida");
            string defaultUploadFolder_Pdf = Path.Combine(hostingEnvironment.WebRootPath, pastaDownloads);

            string defaultUploadFolder_Pdf_Covers = Path.Combine(defaultUploadFolder_Pdf, "Capas\\");

            string anoEdicao = "";
            string titulo = "";

            blnEdit = false;
            EditDialogVisibility = false;

            int iPageCount = 0;
            int FilesProcessed = 0;
            int totalFilesChoosen = args.Files.Count;

            foreach (var file in args.Files)
            {
                string FName = file.FileInfo.Name;

                bool TitleExist = await publicacoesService.TituloExiste_BD(FName);
                if (TitleExist)
                {
                    if (args.Files.Count > 1)
                        continue;
                    else
                    {
                        await ShowProcessResult("e-toast-warning", $"{localizerApp["lblTitulo"]} {FName} {localizerApp["TituloJaExiste"]}.", "e-icon-ThumbsDown", 5000);
                        await sfUploader.ClearAllAsync();
                        return;
                    }
                }
                else
                {
                    FilesProcessed++;
                }

                Dictionary<Metadados, string> fileMetadata = null;

                var filenameWithoutPeriods = RemovePeriodsFromFilename(file.FileInfo.Name);

                uploadedFile = Path.Combine(defaultUploadFolder_Pdf, filenameWithoutPeriods);
                uploadedCoverFile = Path.Combine(defaultUploadFolder_Pdf_Covers, filenameWithoutPeriods);

                extensaoFicheiro = Path.GetExtension(uploadedFile).ToLower();

                try
                {

                    using (FileStream filestream = new FileStream(uploadedFile, FileMode.Create, FileAccess.Write))
                    {
                        await file.File.OpenReadStream(long.MaxValue).CopyToAsync(filestream);
                    }
                }
                catch (FileNotFoundException fnfe)
                {
                    await ShowProcessResult("e-toast-danger", $"{fnfe.Message}: The file cannot be found.", "e-icon-ThumbsDown");
                }
                catch (IOException ioe)
                {
                    await ShowProcessResult("e-toast-danger", $"{ioe.Message}: An I/O error has occurred.", "e-icon-ThumbsDown");
                }
                catch (OutOfMemoryException oome)
                {
                    await ShowProcessResult("e-toast-danger", $"{oome.Message}: There is insufficient memory to read the file.", "e-icon-ThumbsDown");
                }

                catch (Exception ex)
                {
                    await ShowProcessResult("e-toast-danger", ex.Message, "e-icon-ThumbsDown");
                }

                if (extensaoFicheiro == ".msg")
                {
                    var outlookSvc = new OutlookService();
                    fileMetadata = outlookSvc.ReadMSGMessage(uploadedFile);
                }
                else if (extensaoFicheiro == ".pdf")
                {
                    fileMetadata = pdfHelper.GetPdfMetadata_Sync(uploadedFile);
                }
                else if (extensaoFicheiro == ".docx")
                {
                    using (FileStream docStream = new FileStream(uploadedFile, FileMode.Open, FileAccess.Read))
                    {
                        fileMetadata = wordHelper.GetDocxMetadata(docStream);
                    }
                }
                else
                {
                    ToastTitle = localizerApp["msgUpload"];
                    ToastCssClass = "e-toast-danger";
                    ToastContent = localizerApp["msgUploadError"];
                    ToastIcon = "e-icon-ThumbsDown";
                    await Task.Delay(200);
                    await ToastObj.ShowAsync();
                    await sfUploader.CancelAsync();
                    return;
                }


                foreach (var item in fileMetadata)
                {
                    if (fileMetadata != null)
                    {
                        nomeAutorDocumento = "Desconhecido";
                        resumoDocumento = "N/D";
                        autorExiste = false;
                        foreach (var setting in fileMetadata)
                        {
                            switch (setting.Key)
                            {
                                case Metadados.Titulo:
                                    tituloDocumento = setting.Value;
                                    break;

                                case Metadados.AnoEdicao:
                                    anoEdicao = setting.Value;
                                    if (string.IsNullOrEmpty(anoEdicao))
                                    {
                                        anoEdicao = DateTime.Today.Year.ToString();
                                    }
                                    break;
                                case Metadados.Paginas:
                                    iPageCount = int.Parse(setting.Value);
                                    break;

                                case Metadados.Autor:
                                    nomeAutorDocumento = setting.Value;
                                    if (!string.IsNullOrEmpty(nomeAutorDocumento))
                                    {
                                        nomeAutorDocumento = nomeAutorDocumento.Replace("'", "");
                                        autorExiste = await autoresService.AutorExiste(nomeAutorDocumento);
                                        if (!autorExiste)
                                        {
                                            await CriaAutor(nomeAutorDocumento);
                                        }
                                    }
                                    else
                                    {
                                        nomeAutorDocumento = "Desconhecido";
                                    }
                                    break;
                                case Metadados.Assunto:
                                    if (tituloDocumento.ToLower().Contains("erro"))
                                    {
                                        ValidationMessages = new()
                                        {
                                            tituloDocumento + " " + setting.Value
                                        };
                                        EditDialogVisibility = false;
                                        MaintainEditDialogOpen = false;
                                        StateHasChanged();
                                        return;
                                    }

                                    resumoDocumento = localizerApp["msgUploadSubject"] + " " + setting.Value;
                                    if (string.IsNullOrEmpty(setting.Value))
                                        resumoDocumento = "N/D";

                                    break;
                                case Metadados.Keywords:
                                    if (resumoDocumento == "N/D")
                                    {
                                        resumoDocumento = localizerApp["msgUploadKeywords"] + " " + setting.Value;
                                        if (string.IsNullOrEmpty(setting.Value))
                                            resumoDocumento = "N/D";
                                    }
                                    else
                                    {
                                        resumoDocumento += Environment.NewLine + localizerApp["msgUploadKeywords"] + " " + setting.Value;
                                    }
                                    break;
                            } // switch
                        } // para cada setting do pdf
                    } // PdfMetadata != null

                }

                string sPNGPath = "";

                if (extensaoFicheiro.ToLower() == ".pdf")
                {
                    try
                    {
                        var ImagePath = Path.Combine(pastaDownloads, @"Capas\");

                        uploadedCoverFile_img = pdfHelper.GetPdfCover_Sync(defaultUploadFolder_Pdf_Covers, uploadedFile, out sPNGPath);

                    }
                    catch (Exception exc)
                    {
                        throw new ApplicationException(localizerApp["msgUploadCountingPdfPages"] + "\r\n\r\n" + exc.StackTrace);
                    }
                }

                if (uploadedCoverFile_img != null)
                {


                    using (MemoryStream imageStream = new MemoryStream())
                    {
                        uploadedCoverFile_img.Save(imageStream, ImageFormat.Png);
                        imageStream.Flush();

                        imageStream.Position = 0;
                        using (FileStream outStream = File.OpenWrite(sPNGPath))
                        {
                            await file.File.OpenReadStream(long.MaxValue).CopyToAsync(outStream);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(sPNGPath))
                {
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(sPNGPath);

                    if (fileService.IsFileLocked(fileInfo))
                    {
                        Console.WriteLine("Ficheiro de imagem bloqueado...");
                    }
                }

                if (string.IsNullOrEmpty(tituloDocumento)) // não foi possível extrair através de metadata
                {
                    titulo = Path.GetFileNameWithoutExtension(filenameWithoutPeriods);
                    if (string.IsNullOrEmpty(titulo))
                        titulo = filenameWithoutPeriods;
                }
                else
                {
                    titulo = tituloDocumento;
                }

                string editora = await mostUsedService.GetOpcaoMaisUsada("idEditora", "Editora", "Editora.Id", "Descricao");
                string autor = await mostUsedService.GetOpcaoMaisUsada("idAutor", "Autor", "Autor.Id", "Descricao");
                string formato = await mostUsedService.GetOpcaoMaisUsada("idFormato", "Formato", "Formato.Id", "Descricao");
                string localArquivo = await mostUsedService.GetOpcaoMaisUsada("idLocalArquivo", "LocalArquivo", "LocalArquivo.Id", "Descricao");

                tituloOriginal = Path.GetFileName(file.FileInfo.Name);
                int idCategoria = Utils.GetPkValueFromTable("Categoria", "Id", "Descricao", "Desconhecida");
                int idAutor = Utils.GetPkValueFromTable("Autor", "Id", "Descricao", nomeAutorDocumento);

                if (idAutor == 0)
                    idAutor = Utils.GetPkValueFromTable("Autor", "Id", "Descricao", "Desconhecido");

                int idEditora = Utils.GetPkValueFromTable("Editora", "Id", "Descricao", !string.IsNullOrEmpty(editora) ? editora : "Desconhecida");
                int idFormato = Utils.GetPkValueFromTable("Formato", "Id", "Descricao", !string.IsNullOrEmpty(formato) ? formato : "Desconhecida");
                int idLocalArquivo = Utils.GetPkValueFromTable("LocalArquivo", "Id", "Descricao", !string.IsNullOrEmpty(localArquivo) ? localArquivo : "Desconhecida");


                try
                {
                    var pubPath = Path.Combine(pastaDownloads, filenameWithoutPeriods);
                    string imagePath;
                    if (!string.IsNullOrEmpty(sPNGPath))
                        imagePath = Path.Combine(pastaDownloads, @"Capas\", Path.GetFileName(sPNGPath));
                    else
                        imagePath = Path.Combine("images", "No-image-available.png");

                    // Fill model to insert
                    var publicacao = new PubsWithDescriptions()
                    {
                        isbn = "",
                        Titulo = titulo,
                        TituloOriginal = tituloOriginal,
                        Resumo = resumoDocumento,
                        IdAutor = idAutor,
                        IdCategoria = idCategoria,
                        IdEditora = idEditora,
                        IdFormato = idFormato,
                        IdLocalArquivo = idLocalArquivo,
                        Preco = 0,
                        Paginas = iPageCount,
                        AnoEdicao = anoEdicao,
                        DigitalPath = pubPath,
                        ImagemCapa = imagePath,
                        Lido = false,
                        Rating = 1,
                        DataMovimento = DateTime.Now,
                        UserID = 1
                    };

                    // Write to database
                    await publicacoesService.InsertPubAsync(publicacao);
                    await ShowProcessResult("e-toast-success", localizerApp["msgUploadCreationSuccess"] + " " + alertMessage, "e-icon-ThumbsUp");

                }
                catch (Exception exc)
                {
                    Logger.LogError($"Erro ao inserir registo: {exc.Message}");
                    await sfUploader.ClearAllAsync();
                    ToastCssClass = "e-toast-danger";
                    ToastIcon = "e-icon-ThumbsDown";
                    ToastContent = $"Erro ao processar ficheiros ({exc.Message})";

                    ToastTimeOut = 4000;
                    await Task.Delay(200);
                    await ToastObj.ShowAsync();
                }
                publicacaoID = await publicacoesService.GetLastIdLivro();

                categoryBeforeEdition = publicacaoID;

                if (!string.IsNullOrEmpty(uploadedFile))
                {
                    if (!File.Exists(uploadedFile))
                        alertMessage = @$"\n{localizerApp["msgUploadStorageFileNotInDisk"]}+\r\n({uploadedFile}). Confirme. p.f.";
                }
            }  // foreach

            await sfUploader.ClearAllAsync();

            if (totalFilesChoosen != FilesProcessed)
            {
                await ShowProcessResult("e-toast-warning", "Nem todos os ficheiros foram processados.", "e-icon-ThumbsDown");
            }

            publicacoes = await GetAllPublicationsWithDescriptions();
            await gridObj.Refresh();

            if (publicacaoID > 0)
                SelectedRecord = await publicacoesService.Listar_ID_Extended(publicacaoID);

            EditDialogVisibility = false;
            StateHasChanged();
        }

        string RemovePeriodsFromFilename(string fullPath)
        {
            string dir = Path.GetDirectoryName(fullPath);
            string filename = Path.GetFileNameWithoutExtension(fullPath);
            string sanitized = filename.Replace(".", string.Empty);
            string ext = Path.GetExtension(fullPath);

            return Path.Combine(dir, sanitized + ext);
        }
        protected Image ExtractImages(PdfLoadedDocument loadedDocument)
        {
            Stream[] images = null;
            Stream img2Ret = null;

            for (int i = 0; i < loadedDocument.Pages.Count; i++)
            {
                images = null;

                //Get the page
                PdfLoadedPage page = loadedDocument.Pages[i] as PdfLoadedPage;

                //Extract images
                images = page.ExtractImages();
                if (images.Length > 0)
                {
                    // return the first one found
                    img2Ret = images[0];
                    break;
                }
            }

            return Image.FromStream(img2Ret);
        }

        protected void OnUpLoadSuccess(SuccessEventArgs args)
        {
            //blnEdit = false;
            //EditDialogVisibility = true;

        }

        protected async Task CriaAutor(string nomeAutor)
        {
            AutorDTO autor = new AutorDTO()
            {
                Descricao = nomeAutor,
                Sobre = "",
                ImagemAutor = Path.Combine("images", "NoImage_48.png"),
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

        public async Task MenuItemClickedHandler(ContextMenuClickEventArgs<MenuItem> args)
        {
            string msgReaded = localizerApp["msgReaded"];
            string msgAll = localizerApp["msgAll"];

            if (args.Item.Text == msgReaded)
            {
                var lidos = publicacoes.Where(p => p.Paginas == 0).ToList();
                if (lidos.Count() > 0)
                {
                    publicacoes = lidos;
                    StateHasChanged();
                    await gridObj.Refresh();
                }
            }
            else if (args.Item.Text == msgAll)
            {
                publicacoes = await GetAllPublicationsWithDescriptions();
                StateHasChanged();
                await gridObj.Refresh();
            }
        }

        public void OnContextMenuClick(ContextMenuClickEventArgs<PubsWithDescriptions> args)
        {
            MenuItem item = MenuItems.Find(item => item.Id == args.Item.Id.Split("-")[0]);

            WhoCalled = item.Id;
            description2Update = args.Item.Text;
            UpdateTablesVisibility = true;
            StateHasChanged();
        }

        public void SelectCtxMenu(MenuEventArgs<MenuItem> args)
        {
            var selectedItem = args.Item; // Triggers when item is selected
        }

        public async Task AtualizaTabelas_Ctx(string sDescricaoEscolhida)
        {
            var selectedRecords = await gridObj.GetSelectedRecordsAsync();
            if (selectedRecords.Count() == 0)
            {
                return;
            }

            string sCampoDescricao = "Descricao";
            bool specialTreatment = false;
            string sMsgOpcao = "";
            string sQueryUpdate = "UPDATE Publicacao SET ";
            string sQueryUpdateFinal = "";
            string sQuery_GetDescriptionFromTable = "SELECT Id FROM ";

            switch (WhoCalled.ToLower())
            {
                case "updatecategories":
                    sQueryUpdate += "IdCategoria=";
                    specialTreatment = true;
                    sQuery_GetDescriptionFromTable += "Categoria ";
                    sMsgOpcao = "Géneros";
                    break;
                case "updatepublishers":
                    sQueryUpdate += "IdEditora=";
                    sQuery_GetDescriptionFromTable += "Editora ";
                    sMsgOpcao = "Editoras";
                    break;
                case "updateformats":
                    sQueryUpdate += "IdFormato=";
                    sQuery_GetDescriptionFromTable += "Formato ";
                    sMsgOpcao = "Formatos";
                    break;
                case "updatestorageplaces":
                    sQueryUpdate += "IdLocalArquivo=";
                    sQuery_GetDescriptionFromTable += "LocalArquivo ";
                    sMsgOpcao = "Locais de Arquivo";
                    break;
            }

            sQuery_GetDescriptionFromTable += $" WHERE {sCampoDescricao} = '{sDescricaoEscolhida}'";
            int iFkId = Utils.GetCodeFromTable(sQuery_GetDescriptionFromTable);
            sQueryUpdate += iFkId.ToString();

            foreach (var record in selectedRecords)
            {
                sQueryUpdateFinal = $"{sQueryUpdate} WHERE Id = {record.Id}";
                if (!specialTreatment)
                {
                    if (await publicacoesService.UpdatePubsFromContextMenu(sQueryUpdateFinal))
                    {
                        await ShowProcessResult("e-toast-danger", $"Erro ao atualizar tabelas ({sMsgOpcao})", "e-icon-ThumbsDown", 3000);
                        ErrorVisibility = true;
                        StateHasChanged();
                        return;
                    }
                }
                else // special treatment (categories)
                {
                    try
                    {
                        var pub = await publicacoesService.Query_ById_Extended(record.Id);

                        PublicacaoACopiar pubToCopy = new PublicacaoACopiar()
                        {
                            PubID = pub.Id,
                            Publicacao = pub.DigitalPath,
                            CodGenero = pub.IdCategoria,
                            opcao = OpcoesRegisto.Gravar
                        };

                        var destinationFiles = await fileService.TransfereFicheiroParaRepositorio(pubToCopy);

                        if (!string.IsNullOrEmpty(destinationFiles.Item1))
                        {
                            if (await publicacoesService.UpdatePubsFromContextMenu(sQueryUpdateFinal))
                            {
                                record.DigitalPath = destinationFiles.Item1;
                                record.ImagemCapa = destinationFiles.Item2;

                                await publicacoesService.Atualiza_Publicacao(pub.Id, record);
                            }
                        }
                    }
                    catch (FileNotFoundException fnfe)
                    {
                        await ShowProcessResult("e-toast-danger", $"{fnfe.Message}: The file cannot be found.", "e-icon-ThumbsDown");
                    }
                    catch (IOException ioe)
                    {
                        await ShowProcessResult("e-toast-danger", $"{ioe.Message}: An I/O error has occurred.", "e-icon-ThumbsDown");
                    }
                    catch (OutOfMemoryException oome)
                    {
                        await ShowProcessResult("e-toast-danger", $"{oome.Message}: There is insufficient memory to read the file.", "e-icon-ThumbsDown");
                    }

                    catch (Exception ex)
                    {
                        await ShowProcessResult("e-toast-danger", ex.Message, "e-icon-ThumbsDown", 4000);
                    }
                }
            }

            publicacoes = await GetAllPublicationsWithDescriptions();
            StateHasChanged();
            await gridObj.Refresh();
        }



        private void SalvarImagemComoPngComSkiaSharp(System.Drawing.Image imagem, string caminhoSalvarPng)
        {
            using (var bitmap = new SKBitmap(imagem.Width, imagem.Height))
            {
                using (var canvas = new SKCanvas(bitmap))
                {
                    canvas.Clear(SKColor.Empty);

                    using (var image = SKImage.FromBitmap(bitmap))
                    {
                        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                        {
                            using (var stream = File.OpenWrite(caminhoSalvarPng))
                            {
                                data.SaveTo(stream);
                            }
                        }
                    }
                }
            }
        }
        private async Task ShowProcessResult(string displayClass, string errorMsg, string toastIcon = "e-icon-ThumbsUp", int toastTimeout = 3000)
        {
            ToastCssClass = displayClass;
            ToastContent = errorMsg;
            ToastIcon = toastIcon;
            ToastTimeOut = toastTimeout;
            ToastTitle = localizerApp["creationMsg"] + " " + localizerApp["frmLivros"];
            StateHasChanged();
            await Task.Delay(200);
            await ToastObj?.ShowAsync();
        }

        private List<string> GetPdfFilesWithoutPublications()
        {
            var mediaStorageDirectory = Path.Combine(hostingEnvironment.WebRootPath, "Files", "Uploads", "Publicacoes", "Desconhecida");
            var pdfFiles = System.IO.Directory.EnumerateFiles(mediaStorageDirectory, "*.pdf", SearchOption.AllDirectories);
            List<string> files = new();
            List<string> fileNamesInDatabase = new();
            foreach (var publication in publicacoes)
            {
                fileNamesInDatabase.Add(publication.Titulo);
            }

            foreach (string currentFile in pdfFiles)
            {
                string fileName = currentFile.Substring(mediaStorageDirectory.Length + 1);
                files.Add(fileName);
            }

            return files;
        }

        protected async Task HideToast()
        {
            await ToastObj.HideAsync();
        }

        protected void OnUploadStartHandler(UploadingEventArgs args)
        {
            ProgressBarVisibility = true;
        }
        protected void OnChunkSuccessHandler(SuccessEventArgs args)
        {
            ProgressBarVisibility = false;

        }
        protected void OnChunkFailureHandler(Syncfusion.Blazor.Inputs.FailureEventArgs args)
        {
            Logger.LogError(args.Response.ToString());
            ProgressBarVisibility = false;
        }

        protected void SetupMenuItems()
        {
            OpcoesData = new List<Opcoes> {
                    new Opcoes() { Id= 99, MenuDescription= localizerApp["msgAll"]  },
                    new Opcoes() { Id= 1,  MenuDescription= localizerApp["msgReaded"]  },
                    new Opcoes() { Id= 2,  MenuDescription= localizerApp["msgBlankPages"]  },
                    new Opcoes() { Id= 3,  MenuDescription= localizerApp["msgBlankCover"]  },
                    new Opcoes() { Id= 4,  MenuDescription= localizerApp["msgDuplicated"]  } };


            categoriesSubMenuItems = SubMenuCategoriesWithDbData();
            categoriesSubMenuItems_1 = SubMenuCategoriesWithDbData_1();
            categoriesSubMenuItems_2 = SubMenuCategoriesWithDbData_2();
            categoriesSubMenuItems_3 = SubMenuCategoriesWithDbData_3();

            publishersSubMenuItems = SubMenuPublishersWithDbData();
            publishersSubMenuItems_1 = SubMenuPublishersWithDbData_1();
            publishersSubMenuItems_2 = SubMenuPublishersWithDbData_2();
            publishersSubMenuItems_3 = SubMenuPublishersWithDbData_3();
            publishersSubMenuItems_4 = SubMenuPublishersWithDbData_4();
            publishersSubMenuItems_5 = SubMenuPublishersWithDbData_5();

            formatsSubMenuItems = SubMenuFormatsWithDbData();


#pragma warning disable BL0005

            MenuItems = new List<MenuItem>()
            {
            new MenuItem {
                Text = localizerApp["msgUpdateCategories"] ,
                Id = "updateCategories",
                Items = ctxSubMenuCategories()},
            new MenuItem {
                Text = localizerApp["msgUpdatePublishers"],
                Id = "updatePublishers",
                Items = ctxSubMenuPublishers()},
            new MenuItem {
                Text = localizerApp["msgUpdateFormats"],
                Id = "updateFormats",
                Items = SubMenuFormatsWithDbData()},
            new MenuItem {
                Text = localizerApp["msgUpdateStoragePlaces"],
                Id = "updateStoragePlaces",
                Items = SubMenuStoragePlacesWithDbData()}
            };
        }

        protected List<MenuItem> ctxSubMenuPublishers()
        {
#pragma warning disable BL0005
            var contextSubMenuPublishers = new List<MenuItem>()
            {
            new MenuItem {
                Text = "A-E" ,
                Id = "AE",
                Items = publishersSubMenuItems_1},
            new MenuItem {
                Text = "F-J",
                Id = "FJ",
                Items = publishersSubMenuItems_2},
            new MenuItem {
                Text = "K-O",
                Id = "KO",
                Items = publishersSubMenuItems_3},
            new MenuItem {
                Text = "P-T",
                Id = "PT",
                Items = publishersSubMenuItems_4},
            new MenuItem {
                Text = "U-Z",
                Id = "UZ",
                Items = publishersSubMenuItems_5}
            };
#pragma warning restore BL0005
            return contextSubMenuPublishers;
        }

        protected List<MenuItem> ctxSubMenuCategories()
        {
#pragma warning disable BL0005
            var contextSubMenuCategories = new List<MenuItem>()
            {
            new MenuItem {
                Text = "A-D" ,
                Id = "AD",
                Items = categoriesSubMenuItems_1},
            new MenuItem {
                Text = "E-O" ,
                Id = "EO",
                Items = categoriesSubMenuItems_2},
            new MenuItem {
                Text = "P-Z",
                Id = "PZ",
                Items = categoriesSubMenuItems_3}
            };
#pragma warning restore BL0005
            return contextSubMenuCategories;
        }

        protected List<MenuItem> SubMenuCategoriesWithDbData()
        {
            var subMenuItems = new List<MenuItem>();
            if (categoryList is not null)
            {
#pragma warning disable BL0005
                foreach (var entry in categoryList)
                {
                    subMenuItems.Add(new MenuItem
                    { Text = entry.Descricao, Id = "updateCategories-" + entry.Id.ToString() });
                }
#pragma warning restore BL0005
                return subMenuItems;
            }
            else
            { return null; }
        }

        protected List<MenuItem> SubMenuPublishersWithDbData()
        {
            var subMenuItems = new List<MenuItem>();
            if (publisherList is not null)
            {
#pragma warning disable BL0005
                foreach (var entry in publisherList)
                {
                    subMenuItems.Add(new MenuItem
                    { Text = entry.Descricao, Id = "updatePublishers-" + entry.Id.ToString() });
                }
#pragma warning restore BL0005
                return subMenuItems;
            }
            else { return null; }
        }

        protected List<MenuItem> SubMenuPublishersWithDbData_1()
        {
            List<string> prefixList = new List<string>() { "A", "B", "C", "D", "E" };
            return GetGroupedPublishers(prefixList);
        }
        protected List<MenuItem> SubMenuPublishersWithDbData_2()
        {
            List<string> prefixList = new List<string>() { "F", "G", "H", "I", "J" };
            return GetGroupedPublishers(prefixList);
        }
        protected List<MenuItem> SubMenuPublishersWithDbData_3()
        {
            List<string> prefixList = new List<string>() { "K", "L", "M", "N", "O" };
            return GetGroupedPublishers(prefixList);
        }
        protected List<MenuItem> SubMenuPublishersWithDbData_4()
        {
            List<string> prefixList = new List<string>() { "P", "Q", "R", "S", "T" };
            return GetGroupedPublishers(prefixList);
        }
        protected List<MenuItem> SubMenuPublishersWithDbData_5()
        {
            List<string> prefixList = new List<string>() { "U", "V", "X", "Y", "Z" };
            return GetGroupedPublishers(prefixList);
        }
        protected List<MenuItem> SubMenuCategoriesWithDbData_1()
        {
            List<string> prefixList = new List<string>() { "A", "B", "C", "D" };
            return GetGroupedCategories(prefixList);
        }
        protected List<MenuItem> SubMenuCategoriesWithDbData_2()
        {
            List<string> prefixList = new List<string>() { "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O" };
            return GetGroupedCategories(prefixList);
        }
        protected List<MenuItem> SubMenuCategoriesWithDbData_3()
        {
            List<string> prefixList = new List<string>() { "P", "Q", "R", "S", "T", "U", "V", "X", "Y", "Z" };
            return GetGroupedCategories(prefixList);
        }

        protected List<MenuItem> GetGroupedPublishers(List<string> prefixList)
        {
            if (publisherList is not null)
            {
                var publishersGrouped = publisherList.Where(x => prefixList.Any(p => x.Descricao.StartsWith(p)));
                var subMenuItems = new List<MenuItem>();

#pragma warning disable BL0005
                foreach (var entry in publishersGrouped)
                {
                    subMenuItems.Add(new MenuItem
                    { Text = entry.Descricao, Id = "updatePublishers-" + entry.Id.ToString() });
                }
#pragma warning restore BL0005

                return subMenuItems;
            }
            else { return null; }

        }

        protected List<MenuItem> GetGroupedCategories(List<string> prefixList)
        {
            if (categoryList is not null)
            {
                var categoriesGrouped = categoryList.Where(x => prefixList.Any(p => x.Descricao.StartsWith(p)));
                var subMenuItems = new List<MenuItem>();
#pragma warning disable BL0005
                foreach (var entry in categoriesGrouped)
                {
                    subMenuItems.Add(new MenuItem
                    { Text = entry.Descricao, Id = "updateCategories-" + entry.Id.ToString() });
                }
#pragma warning restore BL0005
                return subMenuItems;
            }
            else { return null; }
        }

        protected List<MenuItem> SubMenuFormatsWithDbData()
        {
            var subMenuItems = new List<MenuItem>();
            if (formatList is not null)
            {
#pragma warning disable BL0005
                foreach (var entry in formatList)
                {
                    subMenuItems.Add(new MenuItem
                    { Text = entry.Descricao, Id = "updateFormats-" + entry.Id.ToString() });
                }
            }
#pragma warning restore BL0005
            return subMenuItems;
        }
        protected List<MenuItem> SubMenuStoragePlacesWithDbData()
        {
            var subMenuItems = new List<MenuItem>();
            if (storageList is not null)
            {
#pragma warning disable BL0005
                foreach (var entry in storageList)
                {
                    subMenuItems.Add(new MenuItem
                    { Text = entry.Descricao, Id = "updateStoragePlaces-" + entry.Id.ToString() });
                }
            }
#pragma warning restore BL0005
            return subMenuItems;
        }

        protected class ActionItem
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        /// <summary>
        /// Dialog to show author read-only details / author books (used by Publicacoes.razor)
        /// </summary>
        public bool AuthorVisibility { get; set; } = false;

        /// <summary>
        /// Selected author details populated when opening the dialog
        /// </summary>
        protected AutorDTO SelectedAuthorDetails { get; set; } = new AutorDTO();

        /// <summary>
        /// Open the author dialog by author id (call from the grid button/template)
        /// </summary>
        public async Task ShowAuthorByIdAsync(int authorId)
        {
            try
            {
                // store selection on SelectedRecord so razor components that use SelectedRecord.IdAutor still work
                if (SelectedRecord == null) SelectedRecord = new PubsWithDescriptions();
                SelectedRecord.IdAutor = authorId;

                // load author details (autoresService is already injected in this class)
                SelectedAuthorDetails = await autoresService.GetAuthorsById(authorId);

                // show dialog
                AuthorVisibility = true;

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error opening author dialog");
                // swallow or set a toast/visibility flag as per existing patterns
            }
        }

        /// <summary>
        /// Convenience wrapper if you have the row object available in the template
        /// </summary>
        public Task ShowAuthorFromRowAsync(PubsWithDescriptions row)
        {
            if (row == null) return Task.CompletedTask;
            return ShowAuthorByIdAsync(row.IdAutor);
        }
    }
}