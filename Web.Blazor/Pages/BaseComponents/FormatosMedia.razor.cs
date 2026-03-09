using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Microsoft.Extensions.Localization;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Blazor.Pages.BaseComponents
{
    public class FormatosMediaBase : ComponentBase
    {
        [Inject]
        public IFormatos_MediaService formatosMediaService{ get; set; }
        [Inject]
        public IStringLocalizer<App> localizerApp { get; set; }

        protected SfGrid<FormatoMediaVM> gridObj;

        protected SfToast ToastObj;

        public DialogEffect Effects = DialogEffect.Zoom;

        protected SfSpinner SpinnerObj;

        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";

        protected string target { get; set; } = "#target";

        protected IEnumerable<FormatoMediaVM> formatos;

        protected FormatoMediaVM SelectedRecord = new FormatoMediaVM();
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
        protected string sErrorMsgs = "";

        protected override async Task OnInitializedAsync()
        {
            formatos = await GetAllFormats();
        }

        public async Task OnCommandClicked(CommandClickEventArgs<FormatoMediaVM> args)
        {
            var data = args.RowData;
            formatoID = data.Id;

            SelectedRecord = await formatosMediaService.GetFormatosMediaById(formatoID);

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

        public async void OnToolbarClick(Syncfusion.Blazor.Navigations.ClickEventArgs args)
        {
            if (args.Item.Id == "Clear")
            {
                await gridObj.SearchAsync("");
            }
        }
        public void OnPdfButtonClicked(CommandClickEventArgs<MediaVM> args)
        {
            var data = args.RowData;
            if (args.CommandColumn.Type == CommandButtonType.Edit)
            {
                DigitalPath = data.FileUrl;
                this.ReadPdfVisibility = true;
                StateHasChanged();
            }
        }

        public async Task<IEnumerable<FormatoMediaVM>> GetAllFormats()
        {
            var formatsList = await formatosMediaService.GetAllAsync();
            return formatsList.OrderByDescending(p => p.Id).ToList();
        }

        public IEnumerable<MediaVM> GetMediaCatalogadas(int id)
        {
            //var output = (await formatosMediaService.Media_CatalogadosByFormato(CatId)).ToList(); 

            var output = formatosMediaService.Media_CatalogadosByFormato(id);
            //var output = formatosMediaService.Media_CatalogadosByFormato(id).Result;
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
            sErrorMsgs = formatosMediaService.RegistoComErros(SelectedRecord);
            if (string.IsNullOrEmpty(sErrorMsgs))
            {
                if (blnEdit)
                {
                    await formatosMediaService.UpdateMediaFormat(SelectedRecord.Id, SelectedRecord);
                    ToastTitle = localizerApp["editionMsg"] + " " + localizerApp["frmFormatos"] + " (Media";
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["SuccessUpdate"];
                }
                else
                {
                    await formatosMediaService.CreateMediaFormat(SelectedRecord);
                    ToastTitle = localizerApp["creationMsg"] + " " + localizerApp["frmFormatos"] + " (Media";
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["SuccessInsert"];
                    SelectedRecord = new FormatoMediaVM();
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
            SelectedRecord = new FormatoMediaVM();
            blnEdit = false;
            this.EditDialogVisibility = true;
            StateHasChanged();
        }

        public async Task ConfirmDeleteYes()
        {
            bool NoProblem2Delete = await formatosMediaService.DeleteMediaFormat(formatoID);

            if (NoProblem2Delete) // No publications catalogued, safe to delete
            {
                this.DeleteRecordConfirmVisibility = false;

                formatos = await GetAllFormats();
                await gridObj.Refresh();

                ToastTitle = localizerApp["deletionMsg"] + " " + localizerApp["frmFormatos"] + " (Media)";
                ToastCssClass = "e-toast-danger";
                ToastContent = localizerApp["SuccessDelete"];
                StateHasChanged();
                await Task.Delay(100);
                await ToastObj.ShowAsync();
            }
            else
            {
                //Show alert message to the user; close deletion dialog and show the error messages
                pubsCatalogadas = formatosMediaService.Media_CatalogadosByFormato(formatoID).Count();
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
