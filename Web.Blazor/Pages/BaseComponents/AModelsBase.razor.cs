using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;

namespace Web.Blazor.Pages.BaseComponents
{
    public class AModelsBase : ComponentBase
    {
        [Inject]
        public IAiModelsService aiModelsService { get; set; }
        [Inject]
        public IStringLocalizer<App> localizerApp { get; set; }
        [Inject]
        public IWebHostEnvironment env { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected SfGrid<AIModelVM> gridObj;

        protected SfToast ToastObj;

        public DialogEffect Effects = DialogEffect.Zoom;

        protected SfSpinner SpinnerObj;

        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";

        protected string target { get; set; } = "#target";

        protected IEnumerable<AIModelVM> models;

        protected AIModelVM SelectedModel = new AIModelVM();
        protected bool blnEdit = true;
        public string OperationResult { get; set; }

        // Dialogs visibility flags
        public bool ErrorVisibility { get; set; } = false;
        public bool EditDialogVisibility { get; set; } = false;
        public bool ValidationErrorsVisibility { get; set; } = false;
        public bool DeleteRecordConfirmVisibility { get; set; } = false;
        public int modelId { get; set; }
        protected string sErrorMsgs = "";

        public string DialogClass { get; set; } = "e-fixed";

        protected override async Task OnInitializedAsync()
        {
            models = await GetAllModels();
        }

        public async Task<IEnumerable<AIModelVM>> GetAllModels()
        {
            var modelsList = await aiModelsService.GetAllAsync();
            return modelsList;
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
            sErrorMsgs = aiModelsService.RegistoComErros(SelectedModel);
            if (string.IsNullOrEmpty(sErrorMsgs))
            {
                if (blnEdit)
                {
                    await aiModelsService.UpdateAIModel(modelId, SelectedModel);
                    ToastTitle = localizerApp["editionMsg"] + " " + localizerApp["frmCategorias"];
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["SuccessUpdate"];
                }
                else
                {
                    AIModelVM newModel = new();
                    newModel.Descricao = SelectedModel.Descricao;
                    newModel.Id = 0;

                    await aiModelsService.CreateAIModel(newModel);
                    ToastTitle = localizerApp["creationMsg"] + " " + localizerApp["frmCategorias"];
                    ToastCssClass = "e-toast-success";
                    ToastContent = localizerApp["SuccessInsert"];

                    SelectedModel = new AIModelVM();
                }

                EditDialogVisibility = false;
                StateHasChanged();

                await Task.Delay(200);
                await ToastObj.ShowAsync();

                await SpinnerObj.ShowAsync();

                models = await GetAllModels();
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
            SelectedModel = new AIModelVM();
            blnEdit = false;
            EditDialogVisibility = true;
            StateHasChanged();
        }

        public async Task OnCommandClicked(CommandClickEventArgs<AIModelVM> args)
        {
            var data = args.RowData;
            modelId = data.Id;

            SelectedModel = await aiModelsService.GetModelById(modelId);

            if (args.CommandColumn.Type == CommandButtonType.Delete)
            {
                DeleteRecordConfirmVisibility = true;
                StateHasChanged();
            }
        }


        public async Task ConfirmDeleteYes()
        {
                await aiModelsService.DeleteModel(modelId);
                DeleteRecordConfirmVisibility = false;

                models = await GetAllModels();
                await gridObj.Refresh();

                ToastTitle = localizerApp["deletionMsg"] + " " + localizerApp["frmCategorias"];
                ToastCssClass = "e-toast-success";
                ToastContent = localizerApp["SuccessDelete"];
                StateHasChanged();
                await Task.Delay(100);
                await ToastObj.ShowAsync();
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

    }
}
