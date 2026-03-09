using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Blazor.Controllers
{
    public class PubsReportController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnv;
        public PubsReportController(IWebHostEnvironment webHostEnv)
        {
            _webHostEnv = webHostEnv;
        }
      public IActionResult ReportPubs_Simple()
        {
            return null;
        }
    }
}
