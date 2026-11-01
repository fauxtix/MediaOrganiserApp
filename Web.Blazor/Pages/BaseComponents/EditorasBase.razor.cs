using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Web.Blazor.Services.Reporting;
using MediaOrganizerApp.Application.Enums;

namespace Web.Blazor.Pages.BaseComponents
{
    public class EditorasBase : ComponentBase
    {
        [Inject]
        public IRDLC_ReportService rdlcReportService { get; set; }

        [Inject]
        public IEditorasService PublisherService { get; set; }
        [Inject]
        public IStringLocalizer<App> localizerApp { get; set; }
        [Inject]
        public IWebHostEnvironment env { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected SfGrid<PublisherVM> gridObj;

        protected SfToast ToastObj;

        public DialogEffect Effects = DialogEffect.Zoom;

        protected SfSpinner SpinnerObj;

        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";

        protected string target { get; set; } = "#target";

        protected IEnumerable<PublisherVM> publishers;

        protected PublisherVM SelectedPublisher = new PublisherVM();
        protected bool blnEdit = true;
        public string OperationResult { get; set; }

        // Dialogs visibility flags
        public bool ErrorVisibility { get; set; } = false;
        public bool ReadPdfVisibility { get; set; } = false;
        public bool EditDialogVisibility { get; set; } = false;
        public bool ValidationErrorsVisibility { get; set; } = false;
        public bool DeleteRecordConfirmVisibility { get; set; } = false;

        public int publisherID { get; set; }
        public int pubsCatalogadas { get; set; }
        public string DigitalPath { get; set; } = "";
        public string PdfTitle { get; set; } = "";
        protected string sErrorMsgs = "";

        protected string msgListaEditoras = "";

        protected override async Task OnInitializedAsync()
        {
            msgListaEditoras = localizerApp["TituloListagem"];
            publishers = await GetAllPublishers();
        }

        public async void OnCommandClicked(CommandClickEventArgs<PublisherVM> args)
        {
            var data = args.RowData;
            publisherID = data.Id;

            SelectedPublisher = await PublisherService.GetAuthorsById(publisherID);

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

            if (args.CommandColumn.ButtonOption.Content == localizerApp["LerPdf"])
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
            else if (args.Item.Id == "ELPDF")
            {
                await SpinnerObj.ShowAsync();
                DigitalPath = await rdlcReportService.GetPdfLocation(AppDefinitions.ReportType.Publishers);
                await SpinnerObj.HideAsync();
                ReadPdfVisibility = true;
                await InvokeAsync(StateHasChanged);
            }
        }
        public void OnPdfButtonClicked(CommandClickEventArgs<PublicacoesVM> args)
        {
            var data = args.RowData;
            if (args.CommandColumn.Type == CommandButtonType.None)
            {
                DigitalPath = Path.Combine(env.WebRootPath, data.DigitalPath);
                PdfTitle = data.Titulo;
                ReadPdfVisibility = true;
                StateHasChanged();
            }
        }

        public async Task<IEnumerable<PublisherVM>> GetAllPublishers()
        {
            var publisherList = await PublisherService.GetAllAsync();
            return publisherList;
        }

        public IEnumerable<PublicacoesVM> GetPubsCatalogadas(int id) // async Task<IEnumerable<PublicacoesVM>> 
        {
            //var output = await PublisherService.PublicacoesCatalogadas(id);

            var output = PublisherService.PublicacoesCatalogadas(id);
            // var output = PublisherService.PublicacoesCatalogadas(id).Result;  // this also work
            return output;
        }

        public void Voltar()
        {
            EditDialogVisibility = false;
            StateHasChanged();
        }

        public void FecharPdf()
        {
            PdfTitle = "";
            DigitalPath = "";
            ReadPdfVisibility = false;
            StateHasChanged();
        }

        public async Task AddOrSaveRecord()
        {
            //Validate entries
            sErrorMsgs = PublisherService.RegistoComErros(SelectedPublisher);
            if (string.IsNullOrEmpty(sErrorMsgs))
            {
                if (blnEdit)
                {
                    await PublisherService.UpdatePublisher(SelectedPublisher.Id, SelectedPublisher);
                    ToastTitle = localizerApp["editionMsg"] + " " + localizerApp["frmEditoras"];
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["SuccessUpdate"];
                }
                else
                {
                    await PublisherService.CreateAuthor(SelectedPublisher);
                    ToastTitle = localizerApp["creationMsg"] + " " + localizerApp["frmEditoras"];
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["SuccessInsert"];
                    SelectedPublisher = new PublisherVM();
                }

                EditDialogVisibility = false;
                StateHasChanged();

                await Task.Delay(200);
                await ToastObj.ShowAsync();

                publishers = await GetAllPublishers();
                await gridObj.Refresh();
            }
            else
            {
                this.ErrorVisibility = true;
            }
        }

        public void NotifyAddingRecord()
        {
            SelectedPublisher = new PublisherVM();
            blnEdit = false;
            this.EditDialogVisibility = true;
            StateHasChanged();
        }

        public async Task ConfirmDeleteYes()
        {
            bool publisherHasPubs = await PublisherService.EditoraComLivros(publisherID);  // to allow debug
            if (!publisherHasPubs) // No publications catalogued, safe to delete
            {
                await PublisherService .DeleteAuthor(publisherID);
                this.DeleteRecordConfirmVisibility = false;

                publishers = await GetAllPublishers();
                await gridObj.Refresh();

                ToastTitle = localizerApp["deletionMsg"] + " " + localizerApp["frmEditoras"];
                ToastCssClass = "e-toast-success";
                ToastContent = localizerApp["SuccessDelete"];
                StateHasChanged();

                await Task.Delay(200);
                await ToastObj.ShowAsync();
            }
            else
            {
                //Show alert message to the user; close deletion dialog and show the error messages
                pubsCatalogadas = PublisherService.PublicacoesCatalogadas(publisherID).Count();
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
