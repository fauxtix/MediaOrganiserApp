using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;
using Web.Blazor.Services.DocumentHelpers;
using Web.Blazor.Utilities;

namespace Web.Blazor.Pages.BaseComponents
{
    public class AutoresBase : ComponentBase
    {
        [Inject] public IPdfGeneratorService PdfGeneratorService { get; set; }
        [Inject] public IAutorService AutorService { get; set; }
        [Inject] public IStringLocalizer<App> localizerApp { get; set; }
        [Inject] public IWebHostEnvironment env { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        protected SfGrid<AutorDTO> gridObj;

        protected SfToast ToastObj;

        public DialogEffect Effects = DialogEffect.Zoom;

        protected SfSpinner SpinnerObj;

        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";

        protected string target { get; set; } = "#target";
        protected string spinnerTarget { get; set; } = "#spinnerContainer";

        protected IEnumerable<AutorDTO> autores;
        protected string[] Initial = (new string[] { "AuthorName" });

        protected AutorDTO SelectedAuthor = new AutorDTO();
        protected bool blnEdit = true;
        public string OperationResult { get; set; }

        // Dialogs visibility flags
        public bool ErrorVisibility { get; set; } = false;
        public bool ReadPdfVisibility { get; set; } = false;
        public bool EditDialogVisibility { get; set; } = false;
        public bool DeleteRecordConfirmVisibility { get; set; } = false;
        public bool DeleteAuthorsConfirmVisibility { get; set; } = false;

        public int authorID { get; set; }
        public string DigitalPath { get; set; } = "";
        public string TituloPdf { get; set; } = "";
        public string authorImage { get; set; }
        public int pubsCatalogadas { get; set; }
        protected List<string> ListValidationMessages = new();

        protected bool IsTableEmpty = false;

        protected int reportOption;


        protected override async Task OnInitializedAsync()
        {
            autores = await GetAllAuthors();
            if (!autores.Any())
            {
                IsTableEmpty = true;
            }
        }

        public async Task OnCommandClicked(CommandClickEventArgs<AutorDTO> args)
        {
            var data = args.RowData;
            authorID = data.Id;

            SelectedAuthor = await AutorService.GetAuthorsById(authorID);
            authorImage = SelectedAuthor.ImagemAutor;

            if (!string.IsNullOrEmpty(authorImage) && File.Exists(authorImage))
            {
                byte[] imageByteData = File.ReadAllBytes(authorImage);
                string imageBase64Data = Convert.ToBase64String(imageByteData);
                string imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);
                if (imageDataURL != null)
                {
                    authorImage = imageDataURL; //$"Images/Autores/{authorImage.Substring(posImg + 1)}";
                }
            }
            else
            {
                authorImage = "Images/NoPhoto.png";
            }

            if (args.CommandColumn.Type == CommandButtonType.Edit)
            {
                this.EditDialogVisibility = true;
                StateHasChanged();
            }

            if (args.CommandColumn.Type == CommandButtonType.Delete)
            {
                DeleteRecordConfirmVisibility = true;
                StateHasChanged();
            }

            if (args.CommandColumn.ButtonOption.Content == "Ler PDF")
            {
                this.ReadPdfVisibility = true;
                StateHasChanged();
            }
        }

        protected async Task OnToolbarClick(Syncfusion.Blazor.Navigations.ClickEventArgs args)
        {

            if (args.Item == null)
                return;

            if (args.Item.Id == "Clear")
            {
                await gridObj.SearchAsync("");
            }
            else if (args.Item.Id == "ASP")
            {
                await FilterAuthorsWithoutPubs();
            }
            else if (args.Item.Id == "ARASP")
            {
                NotifyDeleteAuthors();
            }
            else if (args.Item.Id == "ALPDF")
            {


                await SpinnerObj.ShowAsync();
                var pdfStream = PdfGeneratorService.CreateAuthorsPdf(autores.ToList());
                DigitalPath = $"data:application/pdf;base64,{Convert.ToBase64String(pdfStream.ToArray())}";
                await SpinnerObj.HideAsync();
                ReadPdfVisibility = true;
                await InvokeAsync(StateHasChanged);
            }
            else if (args.Item.Id == "TR")
            {
                await GetAll();
            }
        }

        //protected void OnFileCreated(Syncfusion.Blazor.FileManager.UploadListCreateArgs args)
        //{
        //    Syncfusion.Blazor.FileManager.FileInfo fi = args.FileInfo;
        //}

        public void OnPdfButtonClicked(CommandClickEventArgs<PublicacoesVM> args)
        {
            var data = args.RowData;
            if (args.CommandColumn.Type == CommandButtonType.None)
            {
                DigitalPath = Path.Combine(env.WebRootPath, data.DigitalPath);
                TituloPdf = data.Titulo;
                ReadPdfVisibility = true;
                StateHasChanged();
            }
        }

        public void FecharPdf()
        {
            TituloPdf = "";
            DigitalPath = "";
            ReadPdfVisibility = false;
            StateHasChanged();
        }
        public async Task FilterAuthorsWithoutPubs()
        {
            var result = await AutorService.AutoresSemLivros();
            if (result.Count() > 0)
            {
                autores = result;
                reportOption = 1;
            }
            else
            {
                ToastTitle = localizerApp["AutoresSemPublicacoes"];
                ToastContent = localizerApp["msgNoResults"];
                ToastCssClass = "e-toast-success";

                await Task.Delay(200);
                await ToastObj.ShowAsync();
            }
        }

        public async Task DeleteAuthorsWithoutPubs()
        {
            IEnumerable<AutorDTO> autoresSemLivros = await AutorService.AutoresSemLivros();
            if (autoresSemLivros.Count() == 0)
            {
                DeleteAuthorsConfirmVisibility = false;
                ToastTitle = localizerApp["msgDeleteAuthorsWithoutPublications"];
                ToastCssClass = "e-toast-info";
                ToastContent = localizerApp["msgNoResults"];

                await Task.Delay(200);
                await this.ToastObj.ShowAsync();
            }
            else
            {
                try
                {
                    foreach (var autor in autoresSemLivros)
                    {
                        var Id = autor.Id;
                        await AutorService.DeleteAuthor(Id);
                    }

                    ToastTitle = localizerApp["msgDeleteAuthorsWithoutPublications"];
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["msgSuccess"];

                    StateHasChanged();

                    await Task.Delay(200);
                    await this.ToastObj.ShowAsync();

                    await SpinnerObj.ShowAsync();

                    autores = await GetAllAuthors();
                    await gridObj.Refresh();

                    await SpinnerObj.HideAsync();
                }
                catch (Exception)
                {
                    ToastTitle = localizerApp["msgDeleteAuthorsWithoutPublications"];
                    ToastCssClass = "e-toast-error";
                    ToastContent = localizerApp["ErroGeral"];

                    StateHasChanged();

                    await Task.Delay(200);
                    await this.ToastObj.ShowAsync();

                    await SpinnerObj.ShowAsync();

                    autores = await GetAllAuthors();
                    await gridObj.Refresh();
                    await SpinnerObj.HideAsync();
                }
            }
        }

        public async Task GetAll()
        {
            autores = await GetAllAuthors();
            reportOption = 0;
            StateHasChanged();
        }

        public async Task<IEnumerable<AutorDTO>> GetAllAuthors()
        {
            var autList = await AutorService.GetAllAsync();
            autList.OrderByDescending(p => p.Id).ToList();
            return autList;
        }

        public IEnumerable<PublicacoesVM> GetPubsCatalogadas(int auId)
        {
            var output = AutorService.PublicacoesCatalogadas(auId);
            return output;
        }

        public void Voltar()
        {
            EditDialogVisibility = false;
            StateHasChanged();
        }

        public void OnLoad()
        {
            //await SpinnerObj.ShowAsync();
        }
        public void DataBound()
        {
            //await SpinnerObj.HideAsync();
        }
        public void OnActionBegin(ActionEventArgs<AutorDTO> args)
        {
            if (args.RequestType.ToString() == "Sorting")
            {
                //await SpinnerObj.ShowAsync();
            }
            if (args.RequestType.ToString() == "Grouping")
            {
                //await SpinnerObj.ShowAsync();
            }
        }

        public async Task AddOrSaveRecord()
        {
            //Validate entries
            ListValidationMessages = AutorService.RegistoComErros(SelectedAuthor);
            if (ListValidationMessages is null)
            {
                if (blnEdit)
                {
                    await AutorService.UpdateAuthor(SelectedAuthor.Id, SelectedAuthor);
                    ToastTitle = localizerApp["EditMsg"] + " " + localizerApp["lblAutor"];
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["SuccessUpdate"];
                   // blnEdit = true;
                }
                else
                {
                    try
                    {
                        if (await AutorService.AutorExiste(SelectedAuthor.Descricao))
                        {
                            ToastTitle = localizerApp["NewMsg"] + " " + localizerApp["lblAutor"];
                            ToastCssClass = "e-toast-warning";
                            ToastContent = localizerApp["TituloAutorJaExiste"];
                            StateHasChanged();
                            await Task.Delay(200);
                            await this.ToastObj.ShowAsync();
                            await SpinnerObj.HideAsync();
                            return;
                        }

                        await Task.Delay(100);
                        await SpinnerObj.ShowAsync();

                        await AutorService.CreateAuthor(SelectedAuthor);
                        ToastTitle = localizerApp["NewMsg"] + " " + localizerApp["lblAutor"];
                        ToastCssClass = "e-toast-success";
                        ToastContent = localizerApp["RegistoCriadoSucesso"];
                        SelectedAuthor = new AutorDTO();

                    }
                    catch (Exception ex)
                    {
                        ToastTitle = localizerApp["NewMsg"] + " " + localizerApp["lblAutor"];
                        ToastCssClass = "e-toast-error";
                        ToastContent = ex.Message;

                        StateHasChanged();

                        await Task.Delay(200);
                        await this.ToastObj.ShowAsync();

                        await SpinnerObj.ShowAsync();

                        autores = await GetAllAuthors();
                        await gridObj.Refresh();
                        await SpinnerObj.HideAsync();

                    }
                }

                EditDialogVisibility = false;
                StateHasChanged();

                await Task.Delay(200);
                await ToastObj.ShowAsync();

                await SpinnerObj.ShowAsync();

                autores = await GetAllAuthors();
                await gridObj.Refresh();
                await SpinnerObj.HideAsync();
            }
            else
            {
                ErrorVisibility = true;
            }
        }

        public void NotifyAddingRecord()
        {
            SelectedAuthor = new AutorDTO();
            blnEdit = false;
            this.EditDialogVisibility = true;
            StateHasChanged();
        }

        public void NotifyDeleteAuthors()
        {
            this.DeleteAuthorsConfirmVisibility = true;
            StateHasChanged();
        }

        public async Task ConfirmDeleteYes()
        {
            bool authorHasPubs = await AutorService.AutorComLivros(authorID);  // to allow debug
            if (!authorHasPubs) // No publications catalogued, safe to delete
            {
                var deletionOk = await AutorService.DeleteAuthor(authorID);
                if (deletionOk)
                {
                    DeleteRecordConfirmVisibility = false;

                    autores = await GetAllAuthors();
                    await gridObj.Refresh();

                    ToastTitle = localizerApp["DeleteMsg"] + " " + localizerApp["lblAutor"];
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["SuccessDelete"];
                    StateHasChanged();
                    await Task.Delay(100);
                    await ToastObj.ShowAsync();
                }
                else
                {
                    DeleteRecordConfirmVisibility = false;
                    ErrorVisibility = true;
                }
            }
            else
            {
                //Show alert message to the user; close deletion dialog and show the error messages
                pubsCatalogadas = AutorService.PublicacoesCatalogadas(authorID).Count();
                DeleteRecordConfirmVisibility = false;
                ErrorVisibility = true;
            }
        }

        public void ConfirmDeleteNo()
        {
            this.DeleteRecordConfirmVisibility = false;
        }

        public async void ConfirmDeleteAuthorsYes()
        {
            await DeleteAuthorsWithoutPubs();
            this.DeleteAuthorsConfirmVisibility = false;
        }

        public void ConfirmDeleteAuthorsNo()
        {
            this.DeleteAuthorsConfirmVisibility = false;
        }

        public void closeAlertBox()
        {
            ErrorVisibility = false;
            DeleteRecordConfirmVisibility = false;
        }

        public void closeErrorBox()
        {
            ErrorVisibility = false;
            EditDialogVisibility = true;
        }

        public string GetDigitalPath(int pubID)
        {
            var pubsCatalogadas = GetPubsCatalogadas(authorID);
            return pubsCatalogadas.FirstOrDefault().DigitalPath;
        }

        protected void GetBack()
        {
            NavigationManager.NavigateTo("/admin/managelookuptables");
        }

        protected string StripHtml(string? html)
        {
            return TextSanitizer.StripHtml(html);
        }
        protected async Task HideToast()
        {
            await ToastObj.HideAsync();
        }
    }
}
