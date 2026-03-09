using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Popups;
using Web.Blazor.Services.ApiWrappers;
using Web.Blazor.Services.Common;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace Web.Blazor.Pages.Media
{
    public partial class EditYouTube
    {
        [Parameter, EditorRequired] public MediaVM SelectedRecord { get; set; }
        [Parameter, EditorRequired] public int CurrentCategory { get; set; }

        [Inject] public ICategoriasService categoriasService { get; set; }
        [Inject] public IMediaServiceWrapper MediaService { get; set; }
        [Inject] public IGetYoutubeVideoMetadata YoutubeService { get; set; }
        [Inject] public IUtilitiesService utilitiesService { get; set; }
        [Inject] public IFormatos_MediaService formatosService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        [Inject] IStringLocalizer<App> L { get; set; }


        public DialogEffect Effects = DialogEffect.Zoom;

        private IEnumerable<FormatoMediaVM> formatList;
        private IEnumerable<CategoryVM> categoryList;

        private SfRadioButton<bool> chkVideo;
        private SfRadioButton<bool> chkAudio;
        protected DateTime transDate;

        private bool showAlertMessage = false;
        private bool DisableSaveButton = true;
        private string alertMessage = "";
        private bool hideUpperDiv = false;
        private bool hideLowerDiv = true;

        protected override async Task OnInitializedAsync()
        {
            formatList = await formatosService.Listar();
            categoryList = await categoriasService.AllAsync(CategoryType.Media); // true = Publicacoes, false = Media
        }

        protected override void OnParametersSet()
        {
            transDate = SelectedRecord.Id == 0 ? DateTime.Now.Date : SelectedRecord.DataMov;
        }

        private void OnChangeFormato(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, FormatoMediaVM> args)
        {
            SelectedRecord.IdFormato_Media = args.Value;

        }
        private void OnChangeGenero(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, CategoryVM> args)
        {
            SelectedRecord.IdGenero = args.Value;
        }

        public void OnMediaTypeChange(ChangeArgs<bool> args)
        {
            SelectedRecord.TipoMedia = 1;
            StateHasChanged();
        }

        public void OnAudioTypeChange(ChangeArgs<bool> args)
        {
            SelectedRecord.TipoMedia = 2;
            StateHasChanged();
        }

        private async Task GetMetadata()
        {
            showAlertMessage = false;
            alertMessage = "";
            if (!string.IsNullOrEmpty(SelectedRecord.FileUrl))
            {
                int indexPos = SelectedRecord.FileUrl.IndexOf("v=") + 2;
                string videoId = SelectedRecord.FileUrl.Substring(indexPos);
                var videoMetadata = await YoutubeService.GetVideoMetadata(videoId);

                if (videoMetadata is not null)
                {
                    if (SelectedRecord.Id == 0) // Create new entry, try to fill the form Fields
                    {
                        //var categoriasMedia = (await categoriasService.GetCategoriesWithMediaEntries()).ToList();
                        var idCategoriaMedia = (byte)utilitiesService.GetPkValueFromTable("Categoria", "Id", "Descricao", "Outra");

                        var formatosMedia = (await formatosService.GetAllAsync()).ToList();
                        var idFormatoMedia = formatosMedia.SingleOrDefault(f => f.Descricao.ToLower().Contains("url")).Id;
                        SelectedRecord.AnoEdicao = videoMetadata.PublicationDate.Value.Year.ToString();
                        SelectedRecord.Autor = videoMetadata.ChannelTitle;
                        SelectedRecord.FileName = videoMetadata.Title;
                        SelectedRecord.Tempo = videoMetadata.Duration;
                        SelectedRecord.IdGenero = idCategoriaMedia > 0 ? idCategoriaMedia : 1;
                        SelectedRecord.IdFormato_Media = idFormatoMedia;
                        SelectedRecord.Notas = videoMetadata.Description;
                        transDate = videoMetadata.PublicationDate.Value;
                        SelectedRecord.CoverFile = videoMetadata.Thumbnail ?? "";

                        hideLowerDiv = false;
                        hideUpperDiv = true;
                        DisableSaveButton = false;
                        StateHasChanged();
                    }
                    else
                    {
                        SelectedRecord.Tempo = videoMetadata.Duration;
                    }
                }
                else
                {
                    showAlertMessage = true;
                    alertMessage = "Api não devolveu qualquer resultado";
                }
            }
            else
            {
            }
        }

        private void OnUrlChanged()
        {
            if (string.IsNullOrEmpty(SelectedRecord.FileUrl))
            {
                showAlertMessage = false;
                alertMessage = "";
                StateHasChanged();
            }
        }
    }
}