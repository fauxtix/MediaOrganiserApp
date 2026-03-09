using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
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
    public class FormatosBase : ComponentBase
    {
        [Inject]
        public IRDLC_ReportService rdlcReportService { get; set; }
        [Inject]
        public IFormatosService formatosService{ get; set; }
        [Inject]
        public IStringLocalizer<App> localizerApp { get; set; }
        [Inject]
        public IWebHostEnvironment env { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected SfGrid<FormatoPublicacaoVM> gridObj;

        protected SfToast ToastObj;

        public DialogEffect Effects = DialogEffect.Zoom;

        protected SfSpinner SpinnerObj;

        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";

        protected string target { get; set; } = "#target";

        protected IEnumerable<FormatoPublicacaoVM> formatos;

        protected FormatoPublicacaoVM SelectedRecord = new FormatoPublicacaoVM();
        protected bool blnEdit = true;
        public string OperationResult { get; set; }

        // Dialogs visibility flags
        public bool ErrorVisibility { get; set; } = false;
        public bool ReadPdfVisibility { get; set; } = false;
        public bool EditDialogVisibility { get; set; } = false;
        public bool ValidationErrorsVisibility { get; set; } = false;
        public bool DeleteRecordConfirmVisibility { get; set; } = false;

        public int formatoID { get; set; }
        public int pubsCatalogadas { get; set; }
        public string DigitalPath { get; set; } = "";
        public string TituloPdf { get; set; }
        protected string sErrorMsgs = "";

        protected override async Task OnInitializedAsync()
        {
            formatos = await GetAllFormats();
        }

        public async void OnCommandClicked(CommandClickEventArgs<FormatoPublicacaoVM> args)
        {
            var data = args.RowData;
            formatoID = data.Id;

            SelectedRecord = await formatosService.GetFormatosPublicacoesById(formatoID);

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

            if (args.CommandColumn.ButtonOption.Content == "Read PDF")
            {
                this.ReadPdfVisibility = true;
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
                DigitalPath = await rdlcReportService.GetPdfLocation(AppDefinitions.ReportType.Formats);
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
                DigitalPath = Path.Combine(env.WebRootPath, data.DigitalPath);
                TituloPdf = data.Titulo;
                this.ReadPdfVisibility = true;
                StateHasChanged();
            }
        }

        public async Task<IEnumerable<FormatoPublicacaoVM>> GetAllFormats()
        {
            var formatsList = await formatosService.GetAllAsync();
            return formatsList.OrderByDescending(p => p.Id).ToList();
        }

        public IEnumerable<PublicacoesVM> GetPubsCatalogadas(int id)
        {
            // var output = await formatosService.PublicacoesCatalogadas(CatId); 

            var output = formatosService.PublicacoesCatalogadas(id);
            // var output = PublisherService.PublicacoesCatalogadas(id).Result;  
            return output;
        }

        public void Voltar()
        {
            EditDialogVisibility = false;
            StateHasChanged();
        }

        public void FecharPdf()
        {
            TituloPdf = "";
            DigitalPath = "";
            ReadPdfVisibility = false;
            StateHasChanged();
        }

        public async Task AddOrSaveRecord()
        {
            //Validate entries
            sErrorMsgs = formatosService.RegistoComErros(SelectedRecord);
            if (string.IsNullOrEmpty(sErrorMsgs))
            {
                if (blnEdit)
                {
                    await formatosService.UpdatePublicationFormat(SelectedRecord.Id, SelectedRecord);
                    ToastTitle = localizerApp["editionMsg"] + " " + localizerApp["frmFormatos"];
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["SuccessUpdate"];
                }
                else
                {
                    await formatosService.CreatePublicationFormat(SelectedRecord);
                    ToastTitle = localizerApp["creationMsg"] + " " + localizerApp["frmFormatos"];
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["SuccessInsert"];
                    SelectedRecord = new FormatoPublicacaoVM();
                }

                EditDialogVisibility = false;
                StateHasChanged();

                await Task.Delay(200);
                await ToastObj.ShowAsync();

                formatos = await GetAllFormats();
                await gridObj.Refresh();
            }
            else
            {
                this.ErrorVisibility = true;
            }
        }

        public void NotifyAddingRecord()
        {
            SelectedRecord = new FormatoPublicacaoVM();
            blnEdit = false;
            this.EditDialogVisibility = true;
            StateHasChanged();
        }

        public async Task ConfirmDeleteYes()
        {
            bool NoProblem = await formatosService.DeletePublicationFormat(formatoID);

            if (NoProblem) // No publications catalogued, safe to delete
            {
                this.DeleteRecordConfirmVisibility = false;

                formatos = await GetAllFormats();
                await gridObj.Refresh();

                ToastTitle = localizerApp["deletionMsg"] + " " + localizerApp["frmFormatos"];
                ToastCssClass = "e-toast-danger";
                ToastContent = localizerApp["SuccessDelete"];
                StateHasChanged();
                await Task.Delay(100);
                await ToastObj.ShowAsync();
            }
            else
            {
                //Show alert message to the user; close deletion dialog and show the error messages
                pubsCatalogadas = formatosService.PublicacoesCatalogadas(formatoID).Count();
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
        protected void GetBack()
        {
                NavigationManager.NavigateTo("/admin/managelookuptables");
        }
    }
}
