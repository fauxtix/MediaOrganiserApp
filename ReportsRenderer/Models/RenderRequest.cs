using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ReportsRenderer.Models
{
    public class RenderRequest
    {
        public string ReportName { get; set; } = default!;    // e.g. "rptPublicacoesGenerico"
        public string DataSetName { get; set; } = default!;   // e.g. "dsPublicacoes"
        public JArray? Data { get; set; }                     // JSON array of objects
        public Dictionary<string, string>? Parameters { get; set; } // optional report parameters
    }
}