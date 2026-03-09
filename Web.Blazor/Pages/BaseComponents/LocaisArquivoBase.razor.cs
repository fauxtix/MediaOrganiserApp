using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Hosting;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Web.Blazor.Services.Reporting;
using MediaOrganizerApp.Application.Enums;

namespace Web.Blazor.Pages.BaseComponents
{
    public class LocaisArquivoBase : ComponentBase
    {
        [Inject]
        public IRDLC_ReportService rdlcReportService { get; set; }
        [Inject]
        public ILocaisArquivoService locaisArquivoService { get; set; }
        [Inject]
        public IStringLocalizer<App> localizerApp { get; set; }
        [Inject]
        public IWebHostEnvironment env { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected SfGrid<LocalArquivoVM> gridObj;

        protected SfToast ToastObj;

        public DialogEffect Effects = DialogEffect.Zoom;

        protected SfSpinner SpinnerObj;

        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";

        protected string target { get; set; } = "#target";

        protected IEnumerable<LocalArquivoVM> locaisArquivo;

        protected LocalArquivoVM SelectedStoragePlace = new LocalArquivoVM();
        protected bool blnEdit = true;
        public string OperationResult { get; set; }

        // Dialogs visibility flags
        public bool ErrorVisibility { get; set; } = false;
        public bool ReadPdfVisibility { get; set; } = false;
        public bool EditDialogVisibility { get; set; } = false;
        public bool ValidationErrorsVisibility { get; set; } = false;
        public bool DeleteRecordConfirmVisibility { get; set; } = false;

        public int storageID { get; set; }
        public int pubsCatalogadas { get; set; }
        public string PdfTitle { get; set; } = "";
        public string DigitalPath { get; set; } = "";
        protected string sErrorMsgs = "";

        protected override async Task OnInitializedAsync()
        {
            locaisArquivo = await GetAllStoragePlaces();
        }

        public async Task OnCommandClicked(CommandClickEventArgs<LocalArquivoVM> args)
        {
            var data = args.RowData;
            storageID = data.Id;

            SelectedStoragePlace = await locaisArquivoService.GetStoragePlaceById(storageID);

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

        public async Task OnToolbarClick(Syncfusion.Blazor.Navigations.ClickEventArgs args)
        {
            if (args.Item.Id == "Clear")
            {
                await gridObj.SearchAsync("");
            }
            else if (args.Item.Id == "FLPDF")
            {
                await SpinnerObj.ShowAsync();
                DigitalPath = await rdlcReportService.GetPdfLocation(AppDefinitions.ReportType.StoragePlaces);
                await SpinnerObj.HideAsync();
                ReadPdfVisibility = true;
                await InvokeAsync(StateHasChanged);
            }
        }
        public void OnPdfButtonClicked(CommandClickEventArgs<PublicacoesVM> args)
        {
            var data = args.RowData;
            if (args.CommandColumn.Type == CommandButtonType.Edit)
            {
                PdfTitle = data.Titulo;
                DigitalPath = Path.Combine(env.WebRootPath, data.DigitalPath);
                ReadPdfVisibility = true;
                StateHasChanged();
            }
        }

        public void FecharPdf()
        {
            PdfTitle = "";
            DigitalPath = "";
            ReadPdfVisibility = false;
            StateHasChanged();
        }

        public async Task<IEnumerable<LocalArquivoVM>> GetAllStoragePlaces()
        {
            var storageList = await locaisArquivoService.GetAllAsync();
            storageList.OrderByDescending(p => p.Id).ToList();
            return storageList;
        }

        public IEnumerable<PublicacoesVM> GetPubsCatalogadas(int id)
        {
            // var output = (await locaisArquivoService.PublicacoesCatalogadas(id)).ToList(); 

            var output = locaisArquivoService.PublicacoesCatalogadas(id);
            // var output = PublisherService.PublicacoesCatalogadas(id).Result;  
            return output;
        }

        public void Voltar()
        {
            EditDialogVisibility = false;
            StateHasChanged();
        }

        public async Task AddOrSaveRecord()
        {
            //Validate entries
            sErrorMsgs = locaisArquivoService.RegistoComErros(SelectedStoragePlace);
            if (string.IsNullOrEmpty(sErrorMsgs))
            {
                if (blnEdit)
                {
                    await locaisArquivoService.UpdateStoragePlace(SelectedStoragePlace.Id, SelectedStoragePlace);
                    ToastTitle = localizerApp["editionMsg"] + " " + localizerApp["frmLocaisArquivo"];
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["SuccessUpdate"];
                }
                else
                {
                    await locaisArquivoService.CreateStoragePlace(SelectedStoragePlace);
                    ToastTitle = localizerApp["creationMsg"] + " " + localizerApp["frmLocaisArquivo"];
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["SuccessInsert"];
                    SelectedStoragePlace = new LocalArquivoVM();
                }

                EditDialogVisibility = false;
                StateHasChanged();

                await Task.Delay(200);
                await ToastObj.ShowAsync();

                locaisArquivo = await GetAllStoragePlaces();
               await  gridObj.Refresh();
            }
            else
            {
                this.ErrorVisibility = true;
            }
        }

        public void NotifyAddingRecord()
        {
            SelectedStoragePlace = new LocalArquivoVM();
            blnEdit = false;
            this.EditDialogVisibility = true;
            StateHasChanged();
        }

        public async Task ConfirmDeleteYes()
        {
            bool DeletedOK = await locaisArquivoService.DeleteStoragePlace(storageID);
            if (DeletedOK) // No publications catalogued, safe to delete
            {
                this.DeleteRecordConfirmVisibility = false;
                locaisArquivo = await GetAllStoragePlaces();
                await gridObj.Refresh();

                ToastTitle = localizerApp["deletionMsg"] + " " + localizerApp["frmLocaisArquivo"];
                ToastCssClass = "e-toast-warning";
                ToastContent = localizerApp["SuccessDelete"];
                StateHasChanged();
                await Task.Delay(100);
                await ToastObj.ShowAsync();
            }
            else
            {
                //Show alert message to the user; close deletion dialog and show the error messages
                pubsCatalogadas = locaisArquivoService.PublicacoesCatalogadas(storageID).Count();
                this.DeleteRecordConfirmVisibility = false;
                this.ValidationErrorsVisibility = true;
            }
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
