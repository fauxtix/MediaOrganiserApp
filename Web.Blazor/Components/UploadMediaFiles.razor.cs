using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Syncfusion.Blazor.Spinner;
using System.Text;
using Web.Blazor.Services.ApiWrappers;

namespace Web.Blazor.Components
{
    public partial class UploadMediaFiles
    {
        [Inject]
        public IStringLocalizer<App> localizerApp { get; set; }
        [Inject] public IUserService UserService { get; set; }
        [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public IMediaServiceWrapper MediaService { get; set; }

        [Parameter]
        public EventCallback<UploadMediaResult> OnEndUpload { get; set; }

        protected SfSpinner SpinnerObj;

        private MediaVM MediaModel = new();
        private string errorMessage;
        MarkupString alertMessage;
        string AlertClass = "alert alert-info";
        int ProgressPercentage = 0;
        IBrowserFile selectedFile;
        long maxFileSize = 1024L * 1024L * 1024L * 2L;
        string[] allowedExtensions = { ".mp3", ".mp4", ".avi", ".wmv", ".flv" };
        bool IsUploadDisabled = true;
        private Guid inputFileId = Guid.NewGuid();

        private string extension = "";
        private string mediaNotes = "";
        private string sUrl = "";
        private int mediaType = 2;  // default = v\u00eddeo
        private int genero = 23; // sem g\u00e9nero
        private string cover = "/images/music-icon2.png";
        private string clsName { get; set; }
        protected bool UploadOk { get; set; }
        private string resultMessage = string.Empty;
        public UploadMediaResult uploadResult { get; set; } = new();

        protected override void OnInitialized()
        {
            errorMessage = localizerApp["TituloNenhumFicheiroSelecionado"];
            alertMessage = new MarkupString($"<strong>{@errorMessage}</strong>");
        }
        private async void OnInputFileChange(InputFileChangeEventArgs e)
        {
            uploadResult = new();
            selectedFile = e.GetMultipleFiles()[0];
            ProgressPercentage = 0;
            IsUploadDisabled = true;

            var path = Path.Combine(env.WebRootPath, "Files", "Uploads", "Media", selectedFile.Name);

            if (selectedFile.Size > maxFileSize)
            {
                uploadResult.Message = string.Format(localizerApp["FileSizeExceeded"], maxFileSize / (1024 * 1024));
                logger.LogWarning(uploadResult.Message);
                uploadResult.Flag = false;

                await OnEndUpload.InvokeAsync(uploadResult);
            }

            else if (!allowedExtensions.Contains(Path.GetExtension(selectedFile.Name).ToLowerInvariant()))
            {
                uploadResult.Message = string.Format(localizerApp["InvalidFileType"], string.Join(", ", allowedExtensions));
                logger.LogWarning(uploadResult.Message);

                uploadResult.Flag = false;

                await OnEndUpload.InvokeAsync(uploadResult);
            }
            else if (System.IO.File.Exists(path))
            {
                uploadResult.Message = $"{localizerApp["TituloFicheiroJaExiste"]}.";
                logger.LogWarning(uploadResult.Message);

                uploadResult.Flag = false;

                await OnEndUpload.InvokeAsync(uploadResult);
            }
            else
            {
                //SetAlert("alert alert-info", "oi oi-info", $"<strong>{selectedFile.Name}</strong> ({selectedFile.Size} bytes) {localizerApp["TituloFicheiroNaoExisteLocalEspecificado"]}.");
                IsUploadDisabled = false;
            }

        }

        private async void OnSubmit()
        {
            string songName = string.Empty;

            if (selectedFile != null)
            {
                var userDetails = await UserService.GetAuthenticatedUserDetails(AuthenticationStateProvider);

                await Task.Delay(100);
                await SpinnerObj?.ShowAsync();
                IsUploadDisabled = true;
                Stream stream = selectedFile.OpenReadStream(maxFileSize);
                var path = Path.Combine(env.WebRootPath, "Files", "Uploads", "Media", selectedFile.Name);

                extension = Path.GetExtension(selectedFile.Name);
                if (extension.ToLower() == ".mp3")
                {
                    songName = GetSongName(path);
                }

                // Set buffer size to 512 KB.
                int bufferSize = 512 * 1024;
                byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(bufferSize);
                int bytesRead;
                long totalBytesRead = 0;
                long fileSize = selectedFile.Size;

                // Use a timer to update the UI every few hundred milliseconds.
                using var timer = new Timer(_ => InvokeAsync(() => StateHasChanged()));
                timer.Change(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));

                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (FileStream fs = System.IO.File.Create(path))
                {
                    try
                    {
                        while ((bytesRead = await stream.ReadAsync(buffer)) != 0)
                        {
                            totalBytesRead += bytesRead;
                            ProgressPercentage = (int)(100 * totalBytesRead / fileSize);
                            await fs.WriteAsync(buffer, 0, bytesRead);
                        }
                    }
                    finally
                    {
                        System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
                    }

                    // Stop the timer and update the UI with the final progress.
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                    ProgressPercentage = 100;
                }

                inputFileId = Guid.NewGuid();

                this.StateHasChanged();

                TagLib.File tagFile = TagLib.File.Create(path);

                if (extension.ToLower() == ".mp3")
                {

                    string mediaGenre = tagFile.Tag.Genres.Length > 0 ? tagFile.Tag.FirstGenre : localizerApp["DefaultGenre"];

                    int categoryId = await categoriasSVC.GetIdCategoria(mediaGenre);
                    if (categoryId == 0)  // alterado API, ver explica��o no c�digo
                    {

                        CategoryVM mediaCategory = new()
                        {
                            Descricao = mediaGenre,
                            TipoCategoria = 2
                        };

                        await categoriasSVC.CreateCategory(mediaCategory);

                        categoryId = await categoriasSVC.GetIdCategoria(mediaGenre);
                    }

                    string albumName = !string.IsNullOrEmpty(tagFile.Tag.Album) ? tagFile.Tag.Album : "";
                    string duration = tagFile.Properties.Duration.ToString(@"hh\:mm\:ss");

                    string fileName = selectedFile.Name;
                    FileInfo fi = new FileInfo(path);
                    long tamanho = fi.Length;

                    StringBuilder sb = new StringBuilder();

                    bool hasPerformer = !string.IsNullOrEmpty(tagFile.Tag.FirstPerformer);
                    var artist = hasPerformer ? tagFile.Tag.FirstPerformer : localizerApp["VerifyAuthor"];
                    if (string.IsNullOrEmpty(albumName))
                    {
                        albumName = await MediaService.GetMp3Album(artist, songName);
                    }

                    if (hasPerformer && !string.IsNullOrEmpty(albumName))
                    {

                        sb.AppendLine(localizerApp["AlbumNameLabel", albumName]);
                        var albumInfo = await MediaService.GetAlbumInfo(artist, albumName);
                        if (!string.IsNullOrEmpty(albumInfo.Notas))
                        {
                            sb.AppendLine();
                            sb.AppendLine(localizerApp["TracksLabel"]);
                            sb.AppendLine();
                            sb.AppendLine(albumInfo.Notas);
                        }

                        if (!string.IsNullOrEmpty(albumInfo.CoverFile))
                            cover = albumInfo.CoverFile;
                    }


                    MediaModel = new()
                    {
                        FileName = !string.IsNullOrEmpty(tagFile.Tag.Title) ? tagFile.Tag.Title : selectedFile.Name,
                        FilePath = path,
                        FileSource = selectedFile.Name,
                        IdGenero = categoryId,
                        IdFormato_Media = 1, // Download da internet
                        FileUrl = sUrl,
                        TipoMedia = 2, // audio
                        Notas = !string.IsNullOrEmpty(sb.ToString()) ? sb.ToString() : "",
                        AnoEdicao = tagFile.Tag.Year > 0 ? tagFile.Tag.Year.ToString() : "",
                        Visualizado = true,
                        Autor = !string.IsNullOrEmpty(tagFile.Tag.FirstPerformer) ? tagFile.Tag.FirstPerformer : localizerApp["VerifyAuthor"],
                        Rating = 0,
                        GuardaEmdisco = 1,
                        Tempo = duration,
                        CoverFile = cover,
                        Tamanho = tamanho.ToString(),
                        DataMov = DateTime.UtcNow,
                        CreatedBy = userDetails.UserId
                    };
                }
                else // not mp3
                {

                    string mediaGenre = tagFile.Tag.FirstGenre is not null ? tagFile.Tag.FirstGenre : localizerApp["DefaultGenreOutro"];
                    string sMp4Picture = "";
                    int mediaFormatId = utilsSvc.GetPkValueFromTable("Formato_Media", "Id", "Descricao", "Armazenamento m\u00e9dia");
                    int mediaCategoryId = utilsSvc.GetPkValueFromTable("Categoria", "Id", "Descricao", mediaGenre);
                    string albumName = !string.IsNullOrEmpty(tagFile.Tag.Album) ? " (" + tagFile.Tag.Album + ")" : "";
                    string duration = tagFile.Properties.Duration.ToString(@"hh\:mm\:ss");
                    var mp4Pictures = tagFile.Tag.Pictures;
                    if (mp4Pictures.Length > 0)
                    {
                        sMp4Picture = Path.Combine("images", "AudioCovers", mp4Pictures[0].Filename);
                    }
                    else
                    {
                        sMp4Picture = cover;
                    }

                    string fileName = selectedFile.Name;
                    FileInfo fi = new FileInfo(path);


                    mediaType = 2;  // TODO implementar em form � parte -- default, para j� = �udio (2)
                    MediaModel = new()
                    {
                        FileName = Path.GetFileNameWithoutExtension(selectedFile.Name),
                        FilePath = path,
                        FileSource = selectedFile.Name,
                        IdGenero = mediaCategoryId,
                        IdFormato_Media = mediaFormatId,
                        FileUrl = sUrl,
                        TipoMedia = (byte)mediaType,
                        AnoEdicao = selectedFile.LastModified.UtcDateTime.Year.ToString(),
                        Notas = localizerApp["LocalFileDefault"],
                        Visualizado = false,
                        Autor = !string.IsNullOrEmpty(tagFile.Tag.FirstPerformer) ? tagFile.Tag.FirstPerformer : "Fausto Lu\u00eds",
                        Rating = 0,
                        GuardaEmdisco = 1,
                        Tempo = duration,
                        Tamanho = fileSize.ToString(),
                        DataMov = selectedFile.LastModified.UtcDateTime,
                        CoverFile = sMp4Picture,
                        CreatedBy = userDetails.UserId
                    };
                }

                try
                {
                    var mediaCreated = await mediaSvc.CreateMediaFile(MediaModel);
                    if (mediaCreated)
                    {
                        resultMessage = string.Format(localizerApp["UploadSuccessMessage"], selectedFile.Name);
                        logger.LogInformation($"Criado ficheiro m�dia ('{selectedFile.Name}') a partir de armazenamento local.");

                        clsName = "alert alert-success";
                        UploadOk = true;
                        await SpinnerObj?.HideAsync();

                    }
                    else
                    {
                        if (System.IO.File.Exists(path))
                            System.IO.File.Delete(path);

                        resultMessage = string.Format(localizerApp["UploadErrorMessage"], selectedFile.Name);
                        logger.LogWarning(resultMessage);


                        clsName = "alert alert-danger";
                        UploadOk = false;
                        await SpinnerObj?.HideAsync();

                    }
                }
                catch (Exception ex)
                {
                    resultMessage = string.Format(localizerApp["UploadExceptionMessage"], selectedFile.Name, ex.Message);
                    logger.LogError(resultMessage);
                    clsName = "alert alert-danger";

                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);

                    UploadOk = false;
                }

                uploadResult.Message = resultMessage;
                uploadResult.Flag = UploadOk;
                uploadResult.ClassName = clsName;

                // wait a bit before closing dialog...
                await Task.Delay(1000);

                await OnEndUpload.InvokeAsync(uploadResult);
                await SpinnerObj?.HideAsync();

            }
        }

        string GetSongName(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);

            int lastDashIndex = fileName.LastIndexOf('-');

            if (lastDashIndex == -1)
                return fileName; // fallback, caso n�o exista '-'

            return fileName.Substring(lastDashIndex + 1).Trim();
        }
        private void SetAlert(string alertClass, string iconClass, string message)
        {
            AlertClass = alertClass;
            alertMessage = new MarkupString($"<span class='{iconClass}' aria-hidden='true'></span> {message}");
        }
    }
}