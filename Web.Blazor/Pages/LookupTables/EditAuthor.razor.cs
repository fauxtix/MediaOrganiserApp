using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Spinner;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Path = System.IO.Path;
namespace Web.Blazor.Pages.LookupTables
{
    public partial class EditAuthor
    {

        [Parameter] public AutorDTO SelectedAuthor { get; set; }
        [Parameter] public string AuthorImage { get; set; }
        [Parameter] public bool blnEdit { get; set; } = false;

        [Inject] Microsoft.Extensions.Localization.IStringLocalizer<App> L { get; set; }
        [Inject] IAutorService AutoresService { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] IConfiguration Configuration { get; set; }
        [Inject] ILogger<App> Logger { get; set; }
        [Inject] IWebHostEnvironment Env { get; set; }


        protected SfUploader sfUploader;
        public string uploadedFile { get; set; } = "";

        public string imagemAutor { get; set; }

        protected string sPronto { get; set; }

        protected string message = "";
        protected bool alertVisibility;

        protected SfSpinner SpinnerObj;

        public string[] Items = new string[] { "Open", "|", "Delete", "Download", "Rename", "|", "Details", "Custom" };
        protected (string bio, string photo) authorDetails;

        protected override void OnInitialized()
        {
            alertVisibility = false;
        }

        private string GetLocalizedString(string resName)
        {
            var output = L[resName];
            return output;
        }

        protected async void OnChangeUpload(UploadChangeEventArgs args)
        {
            message = "";
            alertVisibility = false;

            foreach (var file in args.Files)
            {
                var pastaImagensAutores = Path.Combine(Env.WebRootPath, "images", "Autores"); // Configuration.GetSection("AppSettings:PastaImagensAutores").Value;

                uploadedFile = Path.Combine(pastaImagensAutores, file.FileInfo.Name);
                if (File.Exists(uploadedFile))
                {
                    AuthorImage = $"images/autores/{file.FileInfo.Name}";

                    SelectedAuthor.ImagemAutor = AuthorImage;
                    StateHasChanged();
                }
                else
                {
                    try
                    {
                        if (uploadedFile != SelectedAuthor.ImagemAutor)
                        {
                            using (FileStream filestream = new FileStream(uploadedFile, FileMode.Create, FileAccess.Write))
                            {
                                await file.File.OpenReadStream(long.MaxValue).CopyToAsync(filestream);
                                AuthorImage = $"images/autores/{file.FileInfo.Name}";
                                SelectedAuthor.ImagemAutor = AuthorImage;
                                StateHasChanged();
                            }
                        }
                        else
                        {
                            message = $"O local previsto ({pastaImagensAutores.TrimEnd('\\')}) não é o que está configurado. Experimente mover imagem para esse local e tente de novo.";
                            StateHasChanged();
                            alertVisibility = true;
                        }

                    }
                    catch (System.Exception ex)
                    {
                        Logger.LogError(ex.Message);
                        alertVisibility = true;
                    }
                }
            }
        }

        protected void OnLoadEnd(Syncfusion.Blazor.Inputs.ProgressEventArgs args)
        {
            var x = args.File.Name;
            //blnEdit = false;
            //this.EditDialogVisibility = true;

        }

        protected void OnUpLoadSuccess(Syncfusion.Blazor.Inputs.SuccessEventArgs args)
        {
            var x = args.File.Name;
            //blnEdit = false;
            //this.EditDialogVisibility = true;

        }

        protected async Task GetAuthorDataFromApi()
        {
            if (string.IsNullOrWhiteSpace(SelectedAuthor.Descricao))
            {
                message = "Sem dados para pesquisar";
                alertVisibility = true;
                StateHasChanged();
                return;
            }

            await Task.Delay(20);
            await SpinnerObj?.ShowAsync();

            try
            {
                var authorName = SelectedAuthor.Descricao;
                authorDetails = await AutoresService.GetAuthorDetails(authorName);
                if (string.IsNullOrEmpty(authorDetails.bio))
                {
                    message = "Não foi possível obter detalhes do autor.";
                    alertVisibility = true;
                    StateHasChanged();
                    await Task.Delay(20);
                    await SpinnerObj.HideAsync();
                    return;
                }
                SelectedAuthor.Sobre = authorDetails.bio;
                if (SelectedAuthor.ImagemAutor != authorDetails.photo && SelectedAuthor.ImagemAutor.ToLower().Contains("no"))
                    SelectedAuthor.ImagemAutor = authorDetails.photo;
                await Task.Delay(20);
                await SpinnerObj.HideAsync();

            }
            catch
            {
                await Task.Delay(20);
                await SpinnerObj?.HideAsync();

            }
        }




        private async Task OnChange(Syncfusion.Blazor.Inputs.UploadChangeEventArgs args)
        {
            foreach (var file in args.Files)
            {
                var path = @"path" + file.FileInfo.Name;
                using (FileStream filestream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    await file.File.OpenReadStream(long.MaxValue).CopyToAsync(filestream);
                }
            }
        }

        //bool UploadVisible { get; set; } = false;

        //protected void SelectedFilesChanged(IEnumerable<UploadFileInfo> files)
        //{
        //    UploadVisible = files.ToList().Count > 0;

        //    InvokeAsync(StateHasChanged);
        //}
        protected string GetUploadUrl(string url)
        {
            string sRet = NavigationManager.ToAbsoluteUri(url).AbsoluteUri;
            return sRet; // NavigationManager.ToAbsoluteUri(url).AbsoluteUri;
        }

        public object[] Tools = new object[]{
        "Bold", "Italic",
        "FontName", "FontSize",
        "Formats", "Alignments", "OrderedList", "UnorderedList",
        "Outdent", "Indent", "|", "CreateTable", "Image", "FullScreen",
         "Undo", "Redo"
    };

        private void AddPhoto()
        {

        }

        private async Task GetAuthorInfo()
        {
            try
            {
                var authorInfo = await AutoresService.GetAuthorInfo_Api(SelectedAuthor.Descricao);
                message = "Autor tem informação disponível...";

            }
            catch
            {
                message = "Autor não tem informação disponível";
                StateHasChanged();
            }
        }


        private Dictionary<string, object> htmlAttribute = new Dictionary<string, object>()
{
        {"rows", "5" }
    };
    }
}