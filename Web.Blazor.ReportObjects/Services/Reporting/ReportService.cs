using AspNetCore.Reporting;
using MediaOrganizerApp.Application.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Web.Blazor.Reporting.Helpers.Reporting
{
    public enum ReportTypeParams
    {
        word,
        excel,
        pdf
    }

    public enum ReportTypeExtensions
    {
        docx,
        xlsx,
        pdf
    }

    public class ReportService : IReportService
    {
        public const string defaultReportType = "pdf";
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ReportService> _logger;


        public ReportService(IWebHostEnvironment env, ILogger<ReportService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public static string GetReportTypeExtension(string reportType)
        {
            if (reportType.Equals(nameof(RenderType.Word), StringComparison.InvariantCultureIgnoreCase))
            {
                return nameof(ReportTypeExtensions.docx);
            }
            else if (reportType.Equals(nameof(RenderType.Excel), StringComparison.InvariantCultureIgnoreCase))
            {
                return nameof(ReportTypeExtensions.xlsx);
            }

            return nameof(ReportTypeExtensions.pdf);
        }

        /// <summary>
        /// Não usado
        /// </summary>
        /// <param name="reportName"></param>
        /// <param name="reportType"></param>
        /// <param name="dsName"></param>
        /// <param name="reportList"></param>
        /// <returns></returns>
        public byte[] GenerateReportAsync(string reportName, string reportType, string dsName = null, object reportList = null)
        {
            string pdfDirPath = Path.Combine(_env.WebRootPath, "Reports_RDLC");
            string rdlcFilePath = $"{pdfDirPath}\\{reportName}.rdlc";
            var parameters = new Dictionary<string, string>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding.GetEncoding("windows-1252");

            var report = new LocalReport(rdlcFilePath);
            if (dsName != null && reportList != null)
            {
                report.AddDataSource(dsName, reportList);
            }
            var result = report.Execute(GetRenderType(reportType), 1, parameters);

            return result.MainStream;
        }

        public byte[] GenerateAuthorsReportAsync(int opcao, string reportType, object reportList = null)
        {
            string rdlcFile = "";
            var parameters = new Dictionary<string, string>();

            switch (opcao)
            {
                case 0:
                    rdlcFile = "rptAutores_Publicacoes";
                    break;
                case 1:
                    rdlcFile = "rptAutoresSemPublicacoes";
                    break;
            }

            var result = ExecuteReport(rdlcFile, reportType, reportList, parameters);
            return result;


            //BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            //FieldInfo field = report.GetType().GetField("localReport", bindFlags);
            //object rptObj = field.GetValue(report);
            //Type type = rptObj.GetType();
            //PropertyInfo pi = type.GetProperty("EnableExternalImages");
            //pi.SetValue(rptObj, true, null);
        }

        public byte[] GenerateCategoriesReportAsync(string reportType, object reportList = null)
        {
            string rdlcFile = "rptCategoriasGenerico";

            var parameters = new Dictionary<string, string>();

            var result = ExecuteReport(rdlcFile, reportType, reportList, parameters);
            return result;
        }

        public byte[] GenerateLookupTablesReportAsync(string rdlcFile, string reportType, object reportList = null)
        {
            //string rdlcFile = "rptFormatosGenerico";

            var parameters = new Dictionary<string, string>();

            var result = ExecuteReport(rdlcFile, reportType, reportList, parameters);
            return result;
        }


        public byte[] GeneratePublishersReportAsync(string reportType, object reportList = null)
        {
            string rdlcFile = "rptEditorasGenerico";

            var parameters = new Dictionary<string, string>();

            var result = ExecuteReport(rdlcFile, reportType, reportList, parameters);
            return result;
        }

        public byte[] GeneratePublicationsReportAsync(string reportType, object reportList = null)
        {
            string rdlcFile = "rptPublicacoesGenerico";

            var parameters = new Dictionary<string, string>();
            var result = ExecuteReport(rdlcFile, reportType, reportList, parameters);
            return result;
        }

        public byte[] GenerateMediaFilesReportAsync(AppDefinitions.Query_TipoMedia reportOption, string reportType, object reportList = null)
        {
            string rdlcFile = reportOption == AppDefinitions.Query_TipoMedia.Others ? "rptMediaFormato" : "rptMediaGenero";
            string agrupamentoParam = reportOption == AppDefinitions.Query_TipoMedia.Others ? "LP, CD, DVD"
                : reportOption == AppDefinitions.Query_TipoMedia.Audio ? "Por url (Audio)"
                : reportOption == AppDefinitions.Query_TipoMedia.Video ? "Por url (Video/Tutoriais, etc.)"
                : "Todos os registos";

            var parameters = new Dictionary<string, string>
            {
                { "pAgrupamento", agrupamentoParam }
            };


            var result = ExecuteReport(rdlcFile, reportType, reportList, parameters);
            return result;
        }


        private byte[] ExecuteReport(string rdlcFile, string reportType, object reportList, Dictionary<string, string> parameters)
        {
            try
            {
                string pdfDirPath = Path.Combine(_env.WebRootPath, "Reports_RDLC");
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("windows-1252");
                string rdlcFilePath = $"{pdfDirPath}\\{rdlcFile}.rdlc";
                string dsName = ReportHelper.GetReportDs(rdlcFile);
                var report = new LocalReport(rdlcFilePath);
                
                if (dsName != null && reportList != null)
                {
                    report.AddDataSource(dsName, reportList);
                }

                var result = report.Execute(GetRenderType(reportType), 1, parameters, "");

                return result.MainStream;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao gerar pdf (a partir do ficheiro {rdlcFile}) - Mensagem: {ex.Message}");
                return null;
            }
        }

        private RenderType GetRenderType(string reportType)
        {
            return (reportType.ToLower()) switch
            {
                nameof(ReportTypeParams.word) => RenderType.WordOpenXml,
                nameof(ReportTypeParams.excel) => RenderType.ExcelOpenXml,
                _ => RenderType.Pdf,
            };
        }

    }
}
