using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Reporting.NETCore;
using Newtonsoft.Json.Linq;
using ReportsRenderer.Models;
using System;
using System.Data;
using System.IO;

namespace ReportsRenderer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RenderController : ControllerBase
    {
        private readonly ILogger<RenderController> _logger;
        private readonly IHostEnvironment _env;

        public RenderController(ILogger<RenderController> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        [HttpPost]
        public IActionResult Post([FromBody] RenderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ReportName))
                return BadRequest("ReportName is required.");

            try
            {
                var reportsFolder = Environment.GetEnvironmentVariable("REPORTS_PATH")
                                    ?? Path.Combine(_env.ContentRootPath, "Reports_RDLC");

                var rdlcFilePath = Path.Combine(reportsFolder, $"{request.ReportName}.rdlc");
                if (!System.IO.File.Exists(rdlcFilePath))
                    return NotFound($"RDLC not found: {rdlcFilePath}");

                using var localReport = new LocalReport();
                using (Stream rs = new FileStream(rdlcFilePath, FileMode.Open, FileAccess.Read))
                    localReport.LoadReportDefinition(rs);

                if (request.Data != null && !string.IsNullOrWhiteSpace(request.DataSetName))
                {
                    var dt = ConvertJArrayToDataTable(request.Data);
                    localReport.DataSources.Add(new ReportDataSource(request.DataSetName, dt));
                }

                if (request.Parameters != null)
                {
                    var list = new System.Collections.Generic.List<ReportParameter>();
                    foreach (var kv in request.Parameters)
                        list.Add(new ReportParameter(kv.Key, kv.Value));
                    localReport.SetParameters(list.ToArray());
                }

                var pdf = localReport.Render("PDF");
                return File(pdf, "application/pdf", $"{request.ReportName}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering report");
                return StatusCode(500, ex.Message);
            }
        }

        private static DataTable ConvertJArrayToDataTable(JArray array)
        {
            var dt = new DataTable("Data");
            if (array.Count == 0) return dt;

            // build columns from first object
            var first = array[0] as JObject;
            foreach (var prop in first.Properties())
                dt.Columns.Add(prop.Name, typeof(string));

            foreach (var item in array)
            {
                var obj = item as JObject;
                var row = dt.NewRow();
                foreach (var prop in obj.Properties())
                {
                    var val = prop.Value.Type == JTokenType.Null ? null : prop.Value.ToString();
                    row[prop.Name] = val ?? string.Empty;
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
    }
}