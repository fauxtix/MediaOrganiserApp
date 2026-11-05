using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Domain.Entities;
using System.Text;
using System.IO;
using Web.Blazor.Services.Common;

namespace Web.Blazor.Services.DocumentHelpers
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly IConfiguration _config;
        private readonly IFormService _formService;
        private readonly IAppSettingsService _settingsService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IPublicacaoService _publicacaoService;

        public FileService(ILogger<FileService> logger,
            IConfiguration config, IFormService formService,
            IWebHostEnvironment hostingEnvironment, IAppSettingsService settingsService, IPublicacaoService publicacaoService)
        {
            _logger = logger;
            _config = config;
            _formService = formService;
            _hostingEnvironment = hostingEnvironment;
            _settingsService = settingsService;
            _publicacaoService = publicacaoService;
        }


        /// <summary>
        /// TransfereFicheiroParaRepositorio
        /// </summary>
        /// <param name="PublicacaoACopiar"></param>
        /// <returns>File and Cover Full Path</returns>
        public async Task<(string, string)> TransfereFicheiroParaRepositorio(PublicacaoACopiar fileParameters)
        {
            try
            {
                string sDefaultUploadFolder_Pfd = await GetPdfFolder();
                string pubPath = fileParameters.Publicacao;
                string sGenero = _formService.GetDescricao(fileParameters.CodGenero, "Categoria");

                // verifica tamanho do nome da categoria
                int sGeneroSource_Length = pubPath.LastIndexOf("\\") + "\\".Length - pubPath.LastIndexOf("\\") + "Publicacoes\\".Length - 1;

                // Nome da categoria
                string sGenero_Source = pubPath.Substring(pubPath.IndexOf("Publicacoes\\") + "Publicacoes\\".Length, sGeneroSource_Length);

                // nome do pdf
                string sPublicacao_Source = Path.GetFileName(pubPath);
                // pasta-origem do pdf
                string sPubsFolder_Source = Path.Combine(sDefaultUploadFolder_Pfd, sGenero_Source);

                string sPubsFolder_Destination = Path.Combine(sDefaultUploadFolder_Pfd, sGenero);
                string sPublicacao_Destination = Path.Combine(sPubsFolder_Destination, sPublicacao_Source);

                string sCoverFile_Source = Path.Combine(sPubsFolder_Source, "Capas", Path.GetFileNameWithoutExtension(pubPath) + ".png");
                string sCoversFolder_Source = Path.Combine(sPubsFolder_Source, "Capas");
                string sCoversFolder_Destination = Path.Combine(sPubsFolder_Destination, "Capas");
                string sCoverFile_Destination = Path.Combine(sCoversFolder_Destination, Path.GetFileNameWithoutExtension(pubPath) + ".png");

                if (!Directory.Exists(sPubsFolder_Destination))
                {
                    Directory.CreateDirectory(sPubsFolder_Destination);
                    Directory.CreateDirectory(sCoversFolder_Destination);
                }
                if (!File.Exists(sPublicacao_Destination))
                {
                    FileInfo pubFile = new FileInfo(pubPath);
                    FileInfo coverFile = new(sCoverFile_Source);

                    //if (IsFileLocked(pubFile))
                    //{
                    //    _logger.LogError($"Pdf {pubFile.Name} não foi movido, está em uso por outro processo");
                    //    return ($"Pdf {pubFile.Name} não foi movido, está em uso por outro processo", "error");
                    //}

                    //if (IsFileLocked(coverFile) && File.Exists(sCoverFile_Source))
                    //{
                    //    _logger.LogError($"Imagem {coverFile} não foi movida, está em uso por outro processo");
                    //    return ($"Imagem {coverFile} não foi movida, está em uso por outro processo", "error");
                    //}

                    try
                    {
                        File.Move(pubPath, sPublicacao_Destination);

                        if (File.Exists(sCoverFile_Source)) // has the pdf a cover?
                        {
                            File.Move(sCoverFile_Source, sCoverFile_Destination);
                        }

                        // wait for file system to settle
                        await Task.Delay(500);

                        // delete covers folder if it's truly empty (no files and no subfolders)
                        if (Directory.Exists(sCoversFolder_Source))
                        {
                            try
                            {
                                bool coversEmpty = !System.Linq.Enumerable.Any(Directory.EnumerateFileSystemEntries(sCoversFolder_Source));
                                if (coversEmpty)
                                {
                                    Directory.Delete(sCoversFolder_Source);
                                }
                            }
                            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                            {
                                _logger.LogWarning(ex, $"Could not delete cover folder '{sCoversFolder_Source}'. It may not be empty or is in use.");
                            }
                        }

                        // delete the publication folder if it's truly empty (no files and no subfolders)
                        if (Directory.Exists(sPubsFolder_Source))
                        {
                            try
                            {
                                bool pubFolderEmpty = !System.Linq.Enumerable.Any(Directory.EnumerateFileSystemEntries(sPubsFolder_Source));
                                if (pubFolderEmpty)
                                {
                                    Directory.Delete(sPubsFolder_Source);
                                }
                            }
                            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                            {
                                _logger.LogWarning(ex, $"Could not delete publication folder '{sPubsFolder_Source}'. It may not be empty or is in use.");
                            }
                        }
                    }
                    catch (FileNotFoundException fnfe)
                    {
                        _logger.LogError($"Pdf {pubFile.Name} não foi encontrado. Verifique, por favor");
                        return ($"{fnfe.Message}: The file cannot be found.", "error");
                    }
                    catch (IOException ioe)
                    {
                        _logger.LogError($"An I/O error has occurred ({ioe.Message})");
                        return ($"An I/O error has occurred ({ioe.Message})", "error");
                    }
                    catch (OutOfMemoryException oome)
                    {
                        _logger.LogError($"{oome.Message}: There is insufficient memory to read the file.");
                        return ($"{oome.Message}: There is insufficient memory to read the file.", "error");
                    }
                }

                var settings = await _settingsService.GetSettingsAsync();
                var downloadsFolder = "Files" + settings.PastaPublicacoes;
                var PubDestination_virtual = Path.Combine(downloadsFolder, sGenero, sPublicacao_Source);
                var CoverDestination_virtual = Path.Combine(downloadsFolder, sGenero, "Capas\\", Path.GetFileName(sCoverFile_Source));
                return (PubDestination_virtual, CoverDestination_virtual);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao transferir ficheiro");
                return ("", "");
            }

        }

        public bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    using (var sr = new StreamReader(stream, Encoding.Default))
                    {
                        sr.Close();
                        sr.Dispose();
                        stream.Close();
                        stream.Dispose();
                    }
                }
            }
            catch // (IOException ex)
            {
                //the file is unavailable because it is:

                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
        public string DecodeImage(string coverFile)
        {
            if (string.IsNullOrEmpty(coverFile))
            {
                return "";
            }

            string imgUrl = "";
            var fullP = WebPathHelper.ToPhysicalPath(_hostingEnvironment, coverFile);
            var fileInfo = new FileInfo(fullP);
            if (File.Exists(fullP) && fileInfo.Length < 1024)
            {
                imgUrl = "/images/no-image-available.png";
            }
            else
            {
                string[] aFolders = coverFile.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (aFolders.Length > 2)
                {
                    if (aFolders.Length > 8)
                    {
                        imgUrl = coverFile.Substring(coverFile.ToLower().LastIndexOf("files"));
                    }
                    else
                    {
                        imgUrl = coverFile;
                    }
                }
                else
                    imgUrl = coverFile.Replace('\\', '/');
            }
            return imgUrl;
        }
        private async Task<string> GetPdfFolder()
        {
            var settings = await _settingsService.GetSettingsAsync();
            string defaultUploadFolder_Pdf = Path.Combine(_hostingEnvironment.WebRootPath, "Files" + settings.PastaPublicacoes);
            return defaultUploadFolder_Pdf;
        }
        public async Task<bool> DeletePhysicalPubPdf(PubsWithDescriptions pub)
        {
            var digitalPath = WebPathHelper.ToPhysicalPath(_hostingEnvironment, pub.DigitalPath);
            var isSourceFileDocx = Path.GetExtension(digitalPath.ToLower()) == ".docx";
            var isSourceFileEmailMessage = Path.GetExtension(digitalPath.ToLower()) == ".msg";
            var noPubImage = pub.ImagemCapa.ToLower().Contains("no-image-available.png");
            try
            {
                if (!string.IsNullOrEmpty(digitalPath))
                {
                    if (File.Exists(digitalPath))
                    {
                        if (isSourceFileDocx)
                        {
                            var wordPdfFile = digitalPath.Replace("docx", "pdf");
                            if (File.Exists(wordPdfFile))
                            {
                                File.Delete(wordPdfFile);
                            }
                        }
                        else if (isSourceFileEmailMessage)
                        {
                            var emailMessage = digitalPath.Replace("msg", "pdf");
                            if (File.Exists(emailMessage))
                            {
                                File.Delete(emailMessage);
                            }
                        }
                        else
                            File.Delete(digitalPath);

                    }
                }

                var pubCover = Path.Combine(_hostingEnvironment.WebRootPath, pub.ImagemCapa);

                if (!string.IsNullOrEmpty(pubCover) && !noPubImage)
                {
                    if (File.Exists(pubCover))
                    {
                        System.IO.FileInfo fileInfo = new System.IO.FileInfo(pubCover);

                        if (IsFileLocked(fileInfo))
                        {
                            _logger.LogError($"Error deleting {Path.GetFileName(digitalPath)}. File locked. Please check");
                            return false;
                        }

                        File.Delete(pubCover);
                    }

                    // delete the cover and pub directories if there are no files in it
                    var sourceCoverDir = Path.GetDirectoryName(pubCover);
                    if (Directory.Exists(sourceCoverDir))
                    {
                        var filesInCoverDir = Directory.GetFiles(sourceCoverDir);
                        if (filesInCoverDir.Length == 0)
                        {
                            Directory.Delete(sourceCoverDir);
                        }
                    }

                    var sourcePdfDir = Path.GetDirectoryName(digitalPath);
                    if (Directory.Exists(sourcePdfDir))
                    {
                        var filesInSourceDir = Directory.GetFiles(sourcePdfDir);
                        if (filesInSourceDir.Length == 0)
                        {
                            Directory.Delete(sourcePdfDir);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting {Path.GetFileName(digitalPath)}. {ex.Message} Please check");
                PdfsAndPngNotDeleted fileToStore = new()
                {
                    Filename = digitalPath
                };
                try
                {
                    await _publicacaoService.StorePdfNotDeleted(fileToStore);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed storing not-deleted pdf record");
                }
                return false;
            }
        }
    }
}
