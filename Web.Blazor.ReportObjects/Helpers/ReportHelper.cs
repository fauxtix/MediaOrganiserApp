using System;
using System.Collections.Generic;

namespace Web.Blazor.Reporting.Helpers
{
    public enum ReportNames
    {
        rptAutores_Publicacoes,
        rptAutoresGenerico,
        rptAutoresSemPublicacoes,
        rptCategoriasGenerico,
        rptEditorasGenerico,
        rptEditorasSemPublicacoes,
        rptFormatosGenerico,
        rptHotmail,
        rptPublicacoesGenerico,
        rptLocaisArquivoGenerico,
        rptMediaFormato,
        rptMediaGenero,
        rptUtlizadoresGenerico,
    }


    public class ReportHelper
    {
        private static readonly Dictionary<string, string> ReportNameDs = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { nameof(ReportNames.rptAutoresGenerico), "dsAuthors"},
            { nameof(ReportNames.rptAutores_Publicacoes), "dsAutoresPublicacoes" },
            { nameof(ReportNames.rptAutoresSemPublicacoes), "dsAutoresSemPublicacoes" },

            { nameof(ReportNames.rptCategoriasGenerico), "dsCategorias"},

            { nameof(ReportNames.rptEditorasGenerico), "dsPublishers"},
            { nameof(ReportNames.rptEditorasSemPublicacoes), "dsEditorasSemPublicacoes" },

            { nameof(ReportNames.rptFormatosGenerico), "dsFormatos"},

            { nameof(ReportNames.rptHotmail), "dsEmail"},
            { nameof(ReportNames.rptPublicacoesGenerico), "dsPublicacoes"},
            { nameof(ReportNames.rptLocaisArquivoGenerico), "dsLocaisArquivo"},
            { nameof(ReportNames.rptMediaFormato), "dsMedia"},
            { nameof(ReportNames.rptMediaGenero), "dsMediaGenero"},
            { nameof(ReportNames.rptUtlizadoresGenerico), "dsUsers"},
        };


        public static string GetReportDs(string reportName)
        {
            if (ReportNameDs.ContainsKey(reportName))
            {
                return ReportNameDs[reportName];
            }
            return null;
        }

    }
}
