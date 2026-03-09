using MediaOrganizerApp.Application.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Reporting.NETCore;
using System;
using System.IO;
using System.Threading.Tasks;
using Web.Blazor.Reporting.Helpers;
using Web.Blazor.Reporting.Helpers.Reporting;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Web.Blazor.Controllers.Reporting
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IReportLists _reportLists;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger _logger;

        public ReportController(IReportService reportService, IReportLists reportLists,
            IWebHostEnvironment webHostEnvironment, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _reportLists = reportLists;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [HttpGet("Publications")]
        public async Task<string> PublicationsReport()
        {
            string rdlcFile = "rptPublicacoesGenerico";
            var location = GetControllerActionNames();

            try
            {
                var reportList = await _reportLists.GetPublicationsList();
                if (reportList is null)
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

                var result = ExecuteReport(rdlcFile, reportList, parameters);
                
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
        public async Task<string> AuthorsReport(int opcao)
        {
            var location = GetControllerActionNames();
            string reportName = opcao switch
            {
                0 => "rptAutores_Publicacoes",
                1 => "rptAutoresSemPublicacoes",
                _ => "rptAutoresGenerico",
            };
            try
            {
                var reportList = await _reportLists.GetAuthorsList(opcao);
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
                var reportList = await _reportLists.GetPublishersList();
                if (reportList is null)
                    return "";

                var result = ExecuteReport(rdlcFile, reportList);

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
                var reportList = await _reportLists.GetMediaFilesList(reportOption);
                if (reportList is null)
                    return "";

                var result = ExecuteReport(rdlcFile, reportList, parameters);

                var outputFile = rdlcFile.Replace("rpt", "");

                var storedPdfFilename = StoreResultPdf(result, outputFile, "Media");
                return storedPdfFilename;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (PublicationsReport): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}").ToString();
            }
        }


        [HttpGet("Categories")]
        public async Task<string> CategoriesReport()
        {
            var location = GetControllerActionNames();

            string rdlcFile = "rptCategoriasGenerico";
            try
            {
                var reportList = await _reportLists.GetCategoriesList();
                if (reportList is null)
                    return "";

                var result = ExecuteReport(rdlcFile, reportList);

                var outputFile = rdlcFile.Replace("rpt", "");

                var storedPdfFilename = StoreResultPdf(result, outputFile, "Categories");
                return storedPdfFilename;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (PublicationsReport): {e.Message}");
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
                var reportList = await _reportLists.GetStoragePlacesList();
                if (reportList is null)
                    return "";

                var result = ExecuteReport(rdlcFile, reportList);

                var outputFile = rdlcFile.Replace("rpt", "");

                var storedPdfFilename = StoreResultPdf(result, outputFile, "StoragePlaces");
                return storedPdfFilename;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (PublicationsReport): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}").ToString();
            }
        }

        private byte[] ExecuteReport(string rdlcFile, object reportList, ReportParameter[] parameters = null)
        {
            using var report = new LocalReport();
            string pdfDirPath = Path.Combine(_webHostEnvironment.WebRootPath, "Reports_RDLC");
            string rdlcFilePath = $"{pdfDirPath}\\{rdlcFile}.rdlc";
            string dsName = ReportHelper.GetReportDs(rdlcFile);
            using (Stream rs = new FileStream(rdlcFilePath, FileMode.Open, FileAccess.Read))
                report.LoadReportDefinition(rs);

            if (reportList is not null)
            {
                report.DataSources.Add(new ReportDataSource(dsName, reportList));
            }

            if (parameters is not null)
                report.SetParameters(parameters);

            var pdf = report.Render("PDF");

            return pdf;
        }


        private string StoreResultPdf(byte[] bytes, string reportName, string folderName)
        {
            var reportFilename = reportName.Replace("rpt", "");

            Stream stream = new MemoryStream(bytes);
            stream.Position = 0;
            var pdfFile = $@"{_webHostEnvironment.WebRootPath}\Reports_pdf\{folderName}\{reportFilename}.pdf";
            var downloadPdfFile = $"{reportFilename}.pdf";
            var streamResultFile = File(stream, "application/pdf", downloadPdfFile);

            CreatePdfOnFolder(pdfFile, streamResultFile); // store pdfs for further reading

            return pdfFile;

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
