using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.Reporting;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Reporting.NETCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MediaOrganizerApp.WebApi.Helpers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        //private readonly IReportLists _reportLists;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger _logger;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IPublicacoesRepository _pubsRepository;
        private readonly IAutoresRepository _authorsRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly ICategoriasRepository _categoriesRepository;
        private readonly IFormatosRepository _formatsRepository;
        private readonly IEditorasRepository _pusblishersRepository;
        private readonly ILocalArquivoRepository _storagePlacesRepository;
        private readonly IFormsHelper _formsHelper;

        public ReportController(IWebHostEnvironment webHostEnvironment,
            ILogger<ReportController> logger,
            IPublicacoesRepository pubsRepository,
            IAutoresRepository authorsRepository,
            IMediaRepository mediaRepository,
            ICategoriasRepository categoriesRepository,
            IFormatosRepository formatsRepository,
            ILocalArquivoRepository storagePlacesRepository,
            IEditorasRepository pusblishersRepository,
            IFormsHelper formsHelper,
            Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _pubsRepository = pubsRepository;
            _authorsRepository = authorsRepository;
            _mediaRepository = mediaRepository;
            _categoriesRepository = categoriesRepository;
            _formatsRepository = formatsRepository;
            _storagePlacesRepository = storagePlacesRepository;
            _pusblishersRepository = pusblishersRepository;
            _formsHelper = formsHelper;
            _configuration = configuration;
        }

        [HttpGet("Publications")]
        public async Task<string> PublicationsReport()
        {
            string rdlcFile = "rptPublicacoesGenerico";
            var location = GetControllerActionNames();

            try
            {
                var publicacoes = await _pubsRepository.Listar(Agrupamento.Todos);
                List<PublicationReportModel> ListaPublicacoes = new();
                foreach (var item in publicacoes)
                {
                    var autor = _formsHelper.GetDescricao(item.IdAutor, "Autor");
                    var editora = _formsHelper.GetDescricao(item.IdEditora, "Editora");
                    var localArquivo = _formsHelper.GetDescricao(item.IdLocalArquivo, "LocalArquivo");
                    var categoria = _formsHelper.GetDescricaoCategoria(item.IdCategoria, 1);

                    ListaPublicacoes.Add(new PublicationReportModel
                    {
                        Title = item.Titulo,
                        Author = autor,
                        Category = categoria,
                        Publisher = editora,
                        Storage = localArquivo,
                        YearPublished = item.AnoEdicao
                    }
                    );
                }


                if (ListaPublicacoes is null)
                    return "";

                var parameters = new[]
                {
                    new ReportParameter("pTitulo", "Listagem de Publicações"),
                    new ReportParameter("pHeader1", "Categoria"),
                    new ReportParameter("pHeader2", "Título"),
                    new ReportParameter("pHeader3", "Editora"),
                    new ReportParameter("pHeader4", "Autor"),
                    new ReportParameter("pSubTitulo", "Todos")
                };

                var result = ExecuteReport(rdlcFile, ListaPublicacoes, parameters);

                var outputFile = rdlcFile.Replace("rpt", "");
                var storedPdfFilename = StoreResultPdf(result, outputFile, "Publications");
                return storedPdfFilename;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (PublicationsReport): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}").ToString();
            }
        }

        [HttpGet("Authors/{opcao:int}")]
        public async Task<string> AuthorsReport(int opcao = 0)
        {
            var location = GetControllerActionNames();

            string reportName;
            IEnumerable<AutorDTO> authorsList;
            IEnumerable<AuthorReportModel> reportList;
            switch (opcao)
            {
                case 0:
                    reportName = "rptAutores_Publicacoes";
                    authorsList = await _authorsRepository.GetAutores_Extended_Async();
                    reportList = Transform(authorsList, opcao);
                    break;
                case 1:
                    reportName = "rptAutoresSemPublicacoes";
                    authorsList = await _authorsRepository.AutoresSemLivros();
                    reportList = Transform(authorsList, opcao);
                    break;
                default:
                    reportName = "rptAutoresGenerico";
                    IEnumerable<PublicacoesVM> autores = _authorsRepository.PublicacoesCatalogadas(0);
                    List<AuthorReportModel> ListaAutores = new List<AuthorReportModel>();
                    foreach (var item in autores)
                    {
                        ListaAutores.Add(new AuthorReportModel
                        {
                            AuthorName = item.Descricao,
                            TituloPub = item.Titulo,
                            EditoraPub = item.Descricao,
                            TituloListagem = "Publicações dos Autores",
                            Header1 = "Nome",
                            Header2 = "Titulo catalogado"
                        });
                    }
                    reportList = ListaAutores;
                    break;
            }

            try
            {
                if (reportList is null)
                    return "";

                var result = ExecuteReport(reportName, reportList);

                var outputFile = reportName.Replace("rpt", "");

                var storedPdfFilename = StoreResultPdf(result, outputFile, "Authors");
                return storedPdfFilename;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (AuthorsReport): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}").ToString();
            }
        }


        [HttpGet("Publishers")]
        public async Task<string> PublishersReport()
        {
            var location = GetControllerActionNames();

            string rdlcFile = "rptEditorasGenerico";
            try
            {
                var editoras = await _pusblishersRepository.Listar_Editora(""); // Todos
                List<PublisherReportModel> ListaEditoras = new();
                foreach (var item in editoras)
                {
                    ListaEditoras.Add(new PublisherReportModel
                    {
                        Id = item.Id,
                        Descricao = item.Descricao,
                        Morada = item.Morada,
                        Telefone = item.Telefone,
                        EMail = item.EMail,
                        Site = item.Site,
                        TituloListagem = "Listagem de Editoras",
                        Header1 = "Editora",
                        Header2 = "eMail",
                        Header3 = "Contacto"
                    });
                }

                if (ListaEditoras is null)
                    return "";

                var result = ExecuteReport(rdlcFile, ListaEditoras);

                var outputFile = rdlcFile.Replace("rpt", "");

                var storedPdfFilename = StoreResultPdf(result, outputFile, "Publishers");
                return storedPdfFilename;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (PublishersReport): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}").ToString();
            }
        }

        [HttpGet("Media/{ReportOption}")]
        public async Task<string> MediaFilesReport(AppDefinitions.Query_TipoMedia reportOption)
        {
            var location = GetControllerActionNames();
            string prnAgrupamento = reportOption == AppDefinitions.Query_TipoMedia.Others ? "LP, CD, DVD"
                : reportOption == AppDefinitions.Query_TipoMedia.Audio ? "Audio"
                : reportOption == AppDefinitions.Query_TipoMedia.Video ? "Video (Tutoriais, etc.)"
                : "Todos os registos";

            var parameters = new[]
            {
                new ReportParameter("pAgrupamento", prnAgrupamento),
            };

            string rdlcFile = reportOption == AppDefinitions.Query_TipoMedia.Others ? "rptMediaFormato" : "rptMediaGenero";
            try
            {
                var mediaFiles = reportOption == AppDefinitions.Query_TipoMedia.Todos ? await _mediaRepository.GetAllMediaFromDB_Report()
                    : reportOption == AppDefinitions.Query_TipoMedia.Audio ? await _mediaRepository.GetFilesByMediaType(AppDefinitions.Query_TipoMedia.Audio)
                    : reportOption == AppDefinitions.Query_TipoMedia.Video ? await _mediaRepository.GetFilesByMediaType(AppDefinitions.Query_TipoMedia.Video)
                    : await _mediaRepository.GetFilesByMediaType(AppDefinitions.Query_TipoMedia.Others);

                List<MediaReportModel> ListaMediaFiles = new();
                foreach (var item in mediaFiles)
                {
                    ListaMediaFiles.Add(new MediaReportModel
                    {
                        IdMedia = item.Id,
                        Titulo = item.FileName,
                        Autor = item.Autor,
                        TipoMedia = item.TipoMedia,
                        Formato_Media = item.FormatoMedia,
                        Genero = item.Genero
                    });
                }


                if (ListaMediaFiles is null)
                    return "";

                var result = ExecuteReport(rdlcFile, ListaMediaFiles, parameters);

                var outputFile = rdlcFile.Replace("rpt", "");

                var storedPdfFilename = StoreResultPdf(result, outputFile, "Media");
                return storedPdfFilename;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (MediaFilesReport): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}").ToString();
            }
        }


        [HttpGet("Categories")]
        public async Task<string> CategoriesReport(bool CalledFromPubs = true)
        {
            var location = GetControllerActionNames();

            string rdlcFile = "rptCategoriasGenerico";
            try
            {
                var categorias = await _categoriesRepository.GetCategorias_ExtendedAsync(CalledFromPubs ? 1 : 2); // Todos
                List<CategoryReportModel> ListaCategorias = new List<CategoryReportModel>();
                foreach (var item in categorias)
                {
                    ListaCategorias.Add(new CategoryReportModel
                    {
                        Id = item.Id,
                        Descricao = item.Descricao,
                        TipoCategoria = item.TipoCategoria,
                        DescricaoTipoCategoria = item.DescricaoTipoCategoria,
                        TituloListagem = "Listagem de Categorias",
                        Header1 = "Código",
                        Header2 = "Descrição"
                    });
                }

                if (ListaCategorias is null)
                    return "";

                var result = ExecuteReport(rdlcFile, ListaCategorias);

                var outputFile = rdlcFile.Replace("rpt", "");

                var storedPdfFilename = StoreResultPdf(result, outputFile, "Categories");
                return storedPdfFilename;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (CategoriesReport): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}").ToString();
            }
        }

        [HttpGet("Formats")]
        public async Task<string> FormatsReport()
        {
            var location = GetControllerActionNames();

            string rdlcFile = "rptFormatosGenerico";
            try
            {
                var formatos = await _formatsRepository.Listar_Async();
                List<LookupTableReportModel> ListaFormatos = new();
                foreach (var item in formatos)
                {
                    ListaFormatos.Add(new LookupTableReportModel
                    {
                        Id = item.Id,
                        Descricao = item.Descricao,
                        TituloListagem = "Listagem de Formatos",
                        Header1 = "Id",
                        Header2 = "Descrição"
                    });
                }

                if (ListaFormatos is null)
                    return "";

                var result = ExecuteReport(rdlcFile, ListaFormatos);

                var outputFile = rdlcFile.Replace("rpt", "");

                var storedPdfFilename = StoreResultPdf(result, outputFile, "Formats");
                return storedPdfFilename;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (FormatsReport): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}").ToString();
            }
        }


        [HttpGet("StoragePlaces")]
        public async Task<string> StoragePlacesReport()
        {
            var location = GetControllerActionNames();

            string rdlcFile = "rptLocaisArquivoGenerico";
            try
            {
                var locaisArquivo = await _storagePlacesRepository.Listar_LocalArquivo(""); // Todos
                List<LookupTableReportModel> ListaLocaisArquivo = new List<LookupTableReportModel>();
                foreach (var item in locaisArquivo)
                {
                    ListaLocaisArquivo.Add(new LookupTableReportModel
                    {
                        Id = item.Id,
                        Descricao = item.Descricao,
                        TituloListagem = "Listagem de Locais de Arquivo",
                        Header1 = "Id",
                        Header2 = "Descrição"
                    });
                }

                if (ListaLocaisArquivo is null)
                    return "";

                var result = ExecuteReport(rdlcFile, ListaLocaisArquivo);

                var outputFile = rdlcFile.Replace("rpt", "");

                var storedPdfFilename = StoreResultPdf(result, outputFile, "StoragePlaces");
                return storedPdfFilename;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (StoragePlacesReport): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}").ToString();
            }
        }

        private byte[] ExecuteReport(string rdlcFile, object reportList, ReportParameter[] parameters = null)
        {
            // If an external renderer service is configured, forward the data to it
            // First try configuration (appsettings), then fallback to environment variable
            var rendererUrl = _configuration["RDLC_RENDERER_URL"]
                              ?? Environment.GetEnvironmentVariable("RDLC_RENDERER_URL");
            if (!string.IsNullOrWhiteSpace(rendererUrl))
            {
                var dsName = ReportHelper.GetReportDs(rdlcFile);

                Dictionary<string, string> paramDict = null;
                if (parameters is not null)
                {
                    paramDict = new Dictionary<string, string>();
                    foreach (var p in parameters)
                    {
                        // StringCollection uses Count rather than Length
                        paramDict[p.Name] = p.Values != null && p.Values.Count > 0 ? p.Values[0] : string.Empty;
                    }
                }

                var payload = new
                {
                    ReportName = rdlcFile,
                    DataSetName = dsName,
                    Data = reportList,
                    Parameters = paramDict
                };

                using var http = new System.Net.Http.HttpClient { BaseAddress = new Uri(rendererUrl) };
                var resp = http.PostAsJsonAsync("api/render", payload).GetAwaiter().GetResult();
                if (!resp.IsSuccessStatusCode)
                {
                    var text = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    throw new Exception($"Remote renderer error: {resp.StatusCode} - {text}");
                }
                return resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            }

            // Fallback to local rendering
            using var report = new LocalReport();
            string pdfDirPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports_RDLC");
            string rdlcFilePath = $"{pdfDirPath}\\{rdlcFile}.rdlc";
            string dsNameLocal = ReportHelper.GetReportDs(rdlcFile);
            using (Stream rs = new FileStream(rdlcFilePath, FileMode.Open, FileAccess.Read))
                report.LoadReportDefinition(rs);

            if (reportList is not null)
            {
                report.DataSources.Add(new ReportDataSource(dsNameLocal, reportList));
            }

            if (parameters is not null)
                report.SetParameters(parameters);

            var pdf = report.Render("PDF");

            return pdf;
        }


        private string StoreResultPdf(byte[] bytes, string reportName, string folderName)
        {
            var reportFilename = reportName.Replace("rpt", "");

            var webroot = _webHostEnvironment.WebRootPath ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
            var physicalFolder = Path.Combine(webroot, "Reports_pdf", folderName);
            if (!Directory.Exists(physicalFolder))
                Directory.CreateDirectory(physicalFolder);

            var physicalFile = Path.Combine(physicalFolder, $"{reportFilename}.pdf");
            System.IO.File.WriteAllBytes(physicalFile, bytes);

            _logger.LogInformation($"Criado ficheiro {physicalFile}");

            // return public relative url
            return $"/Reports_pdf/{folderName}/{reportFilename}.pdf";
        }

        private void CreatePdfOnFolder(string filename, FileStreamResult fileResult)
        {
            try
            {
                var filenameFolder = Path.GetDirectoryName(filename);
                if (!Directory.Exists(filenameFolder))
                    Directory.CreateDirectory(filenameFolder);

                using (var fileStream = System.IO.File.Create(filename))
                {
                    var fileStreamResult = fileResult;
                    fileStreamResult.FileStream.Seek(0, SeekOrigin.Begin);
                    fileStreamResult.FileStream.CopyTo(fileStream);
                    fileStreamResult.FileStream.Seek(0, SeekOrigin.Begin); //reset position to beginning. If there's any chance the FileResult will be used by a future method, this will ensure it gets left in a usable state - Suggestion by Steven Liekens
                }
                _logger.LogInformation($"Criado ficheiro {filename}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private IEnumerable<AuthorReportModel> Transform(IEnumerable<AutorDTO> autores, int opcao)
        {
            var ListaAutores = new List<AuthorReportModel>();
            foreach (var obj in autores)
            {
                ListaAutores.Add(new AuthorReportModel
                {
                    AuthorName = obj.Descricao,
                    About = obj.Sobre,
                    AuthorImage = obj.ImagemAutor,
                    EditoraPub = obj.NomeEditora,
                    TituloPub = obj.Titulo,
                    TituloListagem = opcao == 0 ? "Autores com publicações catalogadas" : "Autores sem publicações catalogadas",
                    Header1 = "Nome",
                    Header2 = "Titulo"
                });
            }
            return ListaAutores;

        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, $"Algo de errado ocorreu. Contacte o Administrador ({message})");
        }
    }
}
