using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace Web.Blazor.Pages.BaseComponents
{
    public class CategoriasMediaBase : ComponentBase
    {
        [Inject] public ICategoriasService categoriasService { get; set; }
        [Inject] public IStringLocalizer<App> localizerApp { get; set; }
        [Inject] public ILogger<App> logger { get; set; }


        protected SfGrid<CategoryVM> gridObj;

        protected SfToast ToastObj;

        public DialogEffect Effects = DialogEffect.Zoom;

        protected SfSpinner SpinnerObj;

        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";
        protected int ToastTimeOut = 1500;

        protected string target { get; set; } = "#target";

        protected IEnumerable<CategoryVM> categorias { get; set; }

        protected CategoryVM SelectedCategory = new CategoryVM();
        protected bool blnEdit = true;
        public string OperationResult { get; set; }

        // Dialogs visibility flags
        public bool ErrorVisibility { get; set; } = false;
        public bool ReadPdfVisibility { get; set; } = false;
        public bool EditDialogVisibility { get; set; } = false;
        public bool ValidationErrorsVisibility { get; set; } = false;
        public bool DeleteRecordConfirmVisibility { get; set; } = false;
        public bool SpinnerVisibility { get; set; } = false;

        public int categoryID { get; set; }
        public int mediaCatalogadas { get; set; }
        public string DigitalPath { get; set; } = "";
        protected string sErrorMsgs = "";

        public string DialogClass { get; set; } = "e-fixed";

        protected override async Task OnInitializedAsync()
        {
            categorias = await GetAllCategories(CategoryType.Media);
        }

        public async Task OnCommandClicked(CommandClickEventArgs<CategoryVM> args)
        {
            var data = args.RowData;
            categoryID = data.Id;

            SelectedCategory = await categoriasService.GetCategoriesById(categoryID);

            if (args.CommandColumn.Type == CommandButtonType.Edit)
            {
                this.EditDialogVisibility = true;
                blnEdit = true;
                StateHasChanged();
            }

            if (args.CommandColumn.Type == CommandButtonType.Delete)
            {
                this.DeleteRecordConfirmVisibility = true;
                StateHasChanged();
            }
        }
        public async void OnToolbarClick(Syncfusion.Blazor.Navigations.ClickEventArgs args)
        {
            if (args.Item.Id == "Clear")
            {
                await gridObj.SearchAsync("");
            }
        }
        public async Task<IEnumerable<CategoryVM>> GetAllCategories(CategoryType categoryType)
        {
            var categoriesList = await categoriasService.GetAllAsync(categoryType);
            return categoriesList;
        }

        public void Voltar()
        {
            EditDialogVisibility = false;
            StateHasChanged();
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
                    ToastTitle = localizerApp["editionMsg"] + " " + localizerApp["frmCategorias"] + " (Media)";
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["SuccessUpdate"];
                }
                else
                {
                    ToastTitle = localizerApp["creationMsg"] + " " + localizerApp["frmCategorias"] + " (Media)";

                    CategoryVM newCategory = new();
                    newCategory.Descricao = SelectedCategory.Descricao;
                    newCategory.TipoCategoria = 2;

                    var insertOk = await categoriasService.CreateCategory(newCategory);
                    if (insertOk == false)
                    {
                        ToastCssClass = "e-toast-warning";
                        ToastContent = localizerApp["FalhaCriacaoRegisto"];
                        ToastTimeOut = 5000;
                        logger.LogWarning($"{ToastContent} (Registo duplicado?)");
                    }
                    else
                    {
                        ToastCssClass = "e-toast-success";
                        ToastContent = localizerApp["SuccessInsert"];
                        logger.LogInformation(ToastContent);
                        SelectedCategory = new CategoryVM();
                    }
                }

                EditDialogVisibility = false;
                StateHasChanged();

                await Task.Delay(200);
                await ToastObj.ShowAsync(); ;

                await SpinnerObj.ShowAsync();

                categorias = await GetAllCategories(CategoryType.Media);
                await gridObj.Refresh();

                await SpinnerObj.HideAsync();
            }
            else
            {
                this.ErrorVisibility = true;
            }
        }

        public void NotifyAddingRecord()
        {
            SelectedCategory = new CategoryVM();
            SelectedCategory.TipoCategoria = 2; // Media
            blnEdit = false;
            EditDialogVisibility = true;
            StateHasChanged();
        }

        public async Task ConfirmDeleteYes()
        {
            bool categoryHasMediaRecords = await categoriasService.CategoriaComRegistos(categoryID);  // to allow debug
            if (!categoryHasMediaRecords) // No media catalogued, safe to delete
            {
                await categoriasService.DeleteCategory(categoryID);
                this.DeleteRecordConfirmVisibility = false;

                categorias = await GetAllCategories(CategoryType.Media);
                await gridObj.Refresh();

                ToastTitle = localizerApp["deletionMsg"] + " " + localizerApp["frmCaztegorias"] + " (Media)";
                ToastCssClass = "e-toast-success";
                ToastContent = localizerApp["SuccessDelete"];
                StateHasChanged();
                await Task.Delay(100);
                await ToastObj.ShowAsync();
            }
            else
            {
                //Show alert message to the user; close deletion dialog and show the error messages
                //mediaCatalogadas = (await GetMediaCatalogadas(categoryID)).Count();
                var iMediaCatalogadas = GetMediaCatalogadas(categoryID);
                mediaCatalogadas = iMediaCatalogadas.Count();
                this.DeleteRecordConfirmVisibility = false;
                this.ValidationErrorsVisibility = true;
            }
        }

        public IEnumerable<MediaVM> GetMediaCatalogadas(int CatId)
        {
            var output = categoriasService.MediaCatalogados(CatId);
            return output;
        }

        public void ConfirmDeleteNo()
        {
            this.DeleteRecordConfirmVisibility = false;
        }

        public void closeAlertBox()
        {
            this.ValidationErrorsVisibility = false;
            this.DeleteRecordConfirmVisibility = false;
        }

        public void closeErrorBox()
        {
            this.ErrorVisibility = false;
            this.EditDialogVisibility = true;
        }

    }
}
