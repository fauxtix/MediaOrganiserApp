using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;
using Web.Blazor.Services.Reporting;
using MediaOrganizerApp.Application.DTOs.AI;
using Web.Blazor.Services.ApiWrappers;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace Web.Blazor.Pages.BaseComponents
{
    public class CategoriasPublicacoesBase : ComponentBase
    {
        [Inject]
        public IRDLC_ReportService rdlcReportService { get; set; }

        [Inject]
        public ICategoriasService categoriasService { get; set; }
        [Inject]
        public IStringLocalizer<App> localizerApp { get; set; }
        [Inject]
        public IWebHostEnvironment env { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public TranslateClientService TranslateClient { get; set; }

        protected SfGrid<CategoryVM> gridObj;

        protected SfToast ToastObj;

        public DialogEffect Effects = DialogEffect.Zoom;

        protected SfSpinner SpinnerObj;

        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";

        protected string target { get; set; } = "#target";

        protected IEnumerable<CategoryVM> categoriasPublicacoes;

        protected CategoryVM SelectedCategory = new CategoryVM();
        protected bool blnEdit = true;
        public string OperationResult { get; set; }

        // Dialogs visibility flags
        public bool ErrorVisibility { get; set; } = false;
        public bool ReadPdfVisibility { get; set; } = false;
        public bool EditDialogVisibility { get; set; } = false;
        public bool ValidationErrorsVisibility { get; set; } = false;
        public bool DeleteRecordConfirmVisibility { get; set; } = false;
        public bool TranslationDialogVisible { get; set; } = false;

        // flag to control translate modal visibility
        protected bool showTranslateModal { get; set; }

        public int categoryID { get; set; }
        public int pubsCatalogadas { get; set; }
        public string PdfFile { get; set; } = "";
        public string PdfTitle { get; set; } = "";
        protected string sErrorMsgs = "";

        protected string msgListaCategorias = "Listagem";
        public string DialogClass { get; set; } = "e-fixed";

        protected override async Task OnInitializedAsync()
        {
            categoriasPublicacoes = await GetAllCategories(CategoryType.Publicacoes);
        }

        public async Task OnToolbarClick(ClickEventArgs args)
        {
            if (args.Item.Id == "Clear")
            {
                await gridObj.SearchAsync("");
            }
            else if (args.Item.Id == "CLPDF")
            {
                await SpinnerObj.ShowAsync();
                PdfFile = await rdlcReportService.GetPdfLocation(AppDefinitions.ReportType.Categories);
                await SpinnerObj.HideAsync();
                ReadPdfVisibility = true;
                await InvokeAsync(StateHasChanged);
            }
        }

        public async Task OnCommandClicked(CommandClickEventArgs<CategoryVM> args)
        {
            var data = args.RowData;
            categoryID = data.Id;

            SelectedCategory = await categoriasService.GetCategoriesById(categoryID);

            if (args.CommandColumn.Type == CommandButtonType.Edit)
            {
                EditDialogVisibility = true;
                blnEdit = true;
                StateHasChanged();
            }

            if (args.CommandColumn.Type == CommandButtonType.Delete)
            {
                DeleteRecordConfirmVisibility = true;
                StateHasChanged();
            }
        }

        protected void RowSelectHandler(RowSelectEventArgs<PublicacoesVM> args)
        {
            PdfFile = Path.Combine(env.WebRootPath, args.Data.DigitalPath);
            ReadPdfVisibility = true;
            StateHasChanged();
        }
        public void OnPdfButtonClicked(CommandClickEventArgs<PublicacoesVM> args)
        {
            PdfFile = Path.Combine(env.WebRootPath, args.RowData.DigitalPath);
            PdfTitle = args.RowData.Titulo;
            ReadPdfVisibility = true;
            StateHasChanged();
        }

        public void ClosePdfDialog()
        {
            PdfFile = "";
            PdfTitle = "";
            ReadPdfVisibility = false;
            StateHasChanged();
        }
        public async Task<IEnumerable<CategoryVM>> GetAllCategories(CategoryType category)
        {
            var categoresList = await categoriasService.GetAllAsync(category);
            return categoresList;
        }

        public IEnumerable<PublicacoesVM> GetPubsCatalogadas(int catId)
        {
            //var output = await categoriasService.PublicacoesCatalogadas(CatId); // 0 = 'Todas as pubs.
            var output = categoriasService.PublicacoesCatalogadas(catId);

            return output;
        }
        public async Task Voltar()
        {
            ToastTitle = localizerApp["editionMsg"] + " " + localizerApp["frmCategorias"];
            ToastCssClass = "e-toast-warning";
            ToastContent = localizerApp["OperacaoCancelada"];

            EditDialogVisibility = false;
            StateHasChanged();

            await Task.Delay(200);
            await ToastObj.ShowAsync();
        }

        public async Task AddOrSaveRecord()
        {
            //Validate entries
            sErrorMsgs = categoriasService.RegistoComErros(SelectedCategory);
            if (string.IsNullOrEmpty(sErrorMsgs))
            {
                if (blnEdit)
                {
                    await categoriasService.UpdateCategory(categoryID, SelectedCategory);
                    ToastTitle = localizerApp["editionMsg"] + " " + localizerApp["frmCategorias"];
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["SuccessUpdate"];
                }
                else
                {
                    CategoryVM newCategory = new();
                    newCategory.Descricao = SelectedCategory.Descricao;
                    newCategory.TipoCategoria = 1;

                    await categoriasService.CreateCategory(newCategory);
                    ToastTitle = localizerApp["creationMsg"] + " " + localizerApp["frmCategorias"];
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["SuccessInsert"];

                    SelectedCategory = new CategoryVM();
                }

                EditDialogVisibility = false;
                StateHasChanged();

                await Task.Delay(200);
                await ToastObj.ShowAsync();

                await SpinnerObj.ShowAsync();

                categoriasPublicacoes = await GetAllCategories(CategoryType.Publicacoes);
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
            SelectedCategory = new CategoryVM();
            blnEdit = false;
            EditDialogVisibility = true;
            StateHasChanged();
        }

        public async Task ConfirmDeleteYes()
        {
            bool publisherHasPubs = await categoriasService.CategoriaComRegistos(categoryID);  // to allow debug
            if (!publisherHasPubs) // No publications catalogued, safe to delete
            {
                await categoriasService.DeleteCategory(categoryID);
                DeleteRecordConfirmVisibility = false;

                categoriasPublicacoes = await GetAllCategories(CategoryType.Publicacoes);
                await gridObj.Refresh();

                ToastTitle = localizerApp["deletionMsg"] + " " + localizerApp["frmCategorias"];
                ToastCssClass = "e-toast-success";
                ToastContent = localizerApp["SuccessDelete"];
                StateHasChanged();
                await Task.Delay(100);
                await ToastObj.ShowAsync();
            }
            else
            {
                //Show alert message to the user; close deletion dialog and show the error messages
                pubsCatalogadas = categoriasService.PublicacoesCatalogadas(categoryID).Count();
                DeleteRecordConfirmVisibility = false;
                ValidationErrorsVisibility = true;
            }
        }

        public void ConfirmDeleteNo()
        {
            DeleteRecordConfirmVisibility = false;
        }

        public void closeAlertBox()
        {
            ValidationErrorsVisibility = false;
            DeleteRecordConfirmVisibility = false;
        }

        public void closeErrorBox()
        {
            ErrorVisibility = false;
            EditDialogVisibility = true;
        }

        protected async Task OnTranslateApplied(TranslateRequestDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Text))
                return;

            // Apply translated text to the category description and close modal
            SelectedCategory.Descricao = dto.Text;
            showTranslateModal = false;
            StateHasChanged();
            await Task.CompletedTask;
        }

    }
}
