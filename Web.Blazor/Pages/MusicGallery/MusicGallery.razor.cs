using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.Blazor.Services.ApiWrappers;

namespace Web.Blazor.Pages.MusicGallery
{
    public partial class MusicGallery
    {
        [Inject] public IMediaServiceWrapper mediaService { get; set; }
        [Inject] public ICategoriasService categoriasService { get; set; }
        [Inject] public IStringLocalizer<App> localizerApp { get; set; }
        [Inject] IWebHostEnvironment hostingEnvironment { get; set; }

        protected int CatId { get; set; }
        protected IEnumerable<VideoCategoryLocations> MusicFiles;

        protected SfSpinner SpinnerObj;

        public MediaVM SelectedMusic { get; set; } = new();

        private IEnumerable<CategoryVM> categoryList;

        protected bool ViewDialogVisibility { get; set; } = false;
        protected bool ErrorDialogVisibility { get; set; } = false;
        protected string ErrorMessage { get; set; } = "";

        public string MediaFilePath { get; set; }

        public DialogEffect Effects = DialogEffect.FlipYLeft;

        protected override async Task OnInitializedAsync()
        {
            await GetCategoriesAsync();
            if (categoryList is not null)
            {
                CatId = categoryList.Select(p => p.Id).FirstOrDefault();
                await GetMusicsAsync();
            }
        }

        private async Task GetMusicsAsync()
        {
            try
            {
                await Task.Delay(100);
                await SpinnerObj.ShowAsync();
                //            MusicFiles = await mediaService.GetMusicMedia(CatId);
                MusicFiles = (await mediaService.GetYoutubeMusicMedia(CatId)).ToList() ?? Enumerable.Empty<VideoCategoryLocations>();
                await SpinnerObj.HideAsync();

                if (MusicFiles is null)
                {
                    ErrorDialogVisibility = true;
                    ErrorMessage = "Sem Músicas para mostrar. Verifique, p.f.";
                }
            }
            catch (Exception ex)
            {
                // No results
                MusicFiles = null;
                ErrorDialogVisibility = true;
                ErrorMessage = $"Sem Videos para mostrar. Verifique, p.f. ({ex.Message})";
            }
        }
        private async Task GetCategoriesAsync()
        {
            categoryList = await categoriasService.GetCategoriesWithMediaEntries(); // 20/08/21
        }

        private async Task OnChangeCategory(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, CategoryVM> args)
        {
            CatId = args.Value;
            await GetMusicsAsync();
            StateHasChanged();
        }
    }
}