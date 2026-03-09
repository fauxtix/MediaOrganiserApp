using MediaOrganizerApp.Application.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Web.Blazor.Reporting.Helpers;
using Web.Blazor.Reporting.Helpers.Reporting;

namespace Web.Blazor.Controllers.Reporting
{
    [ApiController]
    [Route("[controller]")]
    public class Report_Controller : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IReportLists _reportLists;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger _logger;


        public Report_Controller(IReportService reportService, IReportLists reportLists,
            IWebHostEnvironment webHostEnvironment, ILogger<Report_Controller> logger)
        {
            _reportService = reportService;
            _reportLists = reportLists;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [HttpGet("Authors/{opcao:int}")]
        public async Task<ActionResult> AuthorsReport(int opcao, string reportType = ReportService.defaultReportType)
        {
            var location = GetControllerActionNames();

            string reportName = "";
            switch (opcao)
            {
                case 0:
                    reportName = "rptAutores_Publicacoes";
                    break;
                case 1:
                    reportName = "rptAutoresSemPublicacoes";
                    break;
                case 2:
                    reportName = "rptAutoresGenerico";
                    break;
            }

            try
            {
                var reportList = await _reportLists.GetAuthorsList(opcao);
                if (reportList is null)
                    return NotFound();

                var returnString = _reportService.GenerateAuthorsReportAsync(
                    opcao,
                    reportType, reportList);

                //string output = StoreResultPdf(returnString, reportName, "Autores");


                return File(returnString,
                    System.Net.Mime.MediaTypeNames.Application.Octet,
                    $"{reportName}.{ReportService.GetReportTypeExtension(reportType)}");

            }
            catch (AspNetCore.Reporting.ReportException lpe)
            {
                _logger.LogError(lpe, $"Erro no Api (AuthorsReport): {lpe.Message}");
                return InternalError($"{location}: {lpe.Message} - {lpe.InnerException}");
            }
            catch (IndexOutOfRangeException iore)
            {
                _logger.LogError(iore, $"Erro no Api (AuthorsReport): {iore.Message}");
                return InternalError($"{location}: {iore.Message} - {iore.InnerException}");
            }

            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (AuthorsReport): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("Categories")]
        public async Task<ActionResult> CategoriesReport()
        {
            var location = GetControllerActionNames();

            var reportType = "pdf";
            string reportName = "rptCategoriasGenerico";

            var reportList = await _reportLists.GetCategoriesList();
            if (reportList is null)
                return NotFound();

            try
            {
                var returnString = _reportService.GenerateCategoriesReportAsync(
                    reportType, reportList);

                //string output = StoreResultPdf(returnString, reportName, "Categorias");

                return File(returnString,
                    System.Net.Mime.MediaTypeNames.Application.Octet,
                    $"{reportName}.{ReportService.GetReportTypeExtension(reportType)}");

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (CategoriesReport): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("Formats")]
        public async Task<ActionResult> FormatsReport()
        {
            var location = GetControllerActionNames();

            var reportType = "pdf";
            string reportName = "rptFormatosGenerico";

            var reportList = await _reportLists.GetFormatsList();
            if (reportList is null)
                return NotFound();

            try
            {
                var returnString = _reportService.GenerateLookupTablesReportAsync(
                    reportName,
                    reportType, reportList);

                return File(returnString,
                    System.Net.Mime.MediaTypeNames.Application.Octet,
                    $"{reportName}.{ReportService.GetReportTypeExtension(reportType)}");

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (FormatsReport): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("StoragePlaces")]
        public async Task<ActionResult> StoragePlacesReport()
        {
            var location = GetControllerActionNames();

            var reportType = "pdf";
            string reportName = "rptLocaisArquivoGenerico";

            var reportList = await _reportLists.GetStoragePlacesList();
            if (reportList is null)
                return NotFound();

            try
            {
                var returnString = _reportService.GenerateLookupTablesReportAsync(
                    reportName,
                    reportType, reportList);

                return File(returnString,
                    System.Net.Mime.MediaTypeNames.Application.Octet,
                    $"{reportName}.{ReportService.GetReportTypeExtension(reportType)}");

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (FormatsReport): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }



        [HttpGet("Publishers")]
        public async Task<ActionResult> PublishersReport()
        {
            var location = GetControllerActionNames();

            var reportType = "pdf";
            string reportName = "rptEditorasGenerico";

            try
            {
                var reportList = await _reportLists.GetPublishersList();
                if (reportList is null)
                    return NotFound();

                var returnString = _reportService.GeneratePublishersReportAsync(
                    reportType, reportList);

                //string output = StoreResultPdf(returnString, reportName, "Editoras");


                return File(returnString,
                    System.Net.Mime.MediaTypeNames.Application.Octet,
                    $"{reportName}.{ReportService.GetReportTypeExtension(reportType)}");

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (PublishersReport): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("Publications")]
        public async Task<ActionResult> PublicationsReport()
        {
            var location = GetControllerActionNames();

            var reportType = "pdf";
            string reportName = "rptPublicacoesGenerico";

            try
            {
                var reportList = await _reportLists.GetPublicationsList();
                if (reportList is null)
                    return NotFound();

                var returnString = _reportService.GeneratePublicationsReportAsync(
                    reportType, reportList);

                //string output = StoreResultPdf(returnString, reportName, "Publicacoes");

                return File(returnString,
                    System.Net.Mime.MediaTypeNames.Application.Octet,
                    $"{reportName}.{ReportService.GetReportTypeExtension(reportType)}");

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (PublicationsReport): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("Media/{ReportOption}")]
        public async Task<ActionResult> MediaReport(AppDefinitions.Query_TipoMedia reportOption)
        {
            var location = GetControllerActionNames();
            var reportType = "pdf";
            string reportName = reportOption == AppDefinitions.Query_TipoMedia.Others ? "rptMediaFormato" : "rptMediaGenero";

            try
            {
                var reportList = await _reportLists.GetMediaFilesList(reportOption);
                if (reportList is null)
                    return NotFound();

                var returnString = _reportService.GenerateMediaFilesReportAsync(reportOption,
                    reportType, reportList);

                //string output = StoreResultPdf(returnString, reportName, "Media");
                // return output
                return File(returnString,
                    System.Net.Mime.MediaTypeNames.Application.Octet,
                    $"{reportName}.{ReportService.GetReportTypeExtension(reportType)}");

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api (GetAuthors): {e.Message}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }


        /// <summary>
        /// Generic - not used for now (01/11/2021)
        /// </summary>
        /// <param name="reportName"></param>
        /// <param name="reportType"></param>
        /// <returns></returns>
        [HttpGet("{reportName}/{reportType?}")]
        public ActionResult Get(string reportName, string reportType = ReportService.defaultReportType)
        {
            var reportList = _reportLists.GetReportList(reportName);
            if (reportList is null)
                return NotFound();

            var returnString = _reportService.GenerateReportAsync(
                reportName,
                reportType,
                ReportHelper.GetReportDs(reportName), reportList);

            return File(returnString, System.Net.Mime.MediaTypeNames.Application.Octet,
                $"{reportName}.{ReportService.GetReportTypeExtension(reportType)}");
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
            catch (System.Exception ex)
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
