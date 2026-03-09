using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Syncfusion.Blazor.DataVizCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Web.Blazor.Controllers
{
    [Route("api/[controller]")]
    public class UploadController : Controller
    {
        public IWebHostEnvironment HostingEnvironment { get; set; }
        private readonly ILogger<UploadController> _logger;

        public UploadController(IWebHostEnvironment hostingEnvironment, ILogger<UploadController> logger)
        {
            HostingEnvironment = hostingEnvironment;
            _logger = logger;
        }


        [HttpPost("[action]")]
        public void Remove(IList<IFormFile> UploadFiles)
        {
            try
            {
                var filename = HostingEnvironment.WebRootPath + $@"\{UploadFiles[0].FileName}";
                var filename2 = Path.Combine(HostingEnvironment.WebRootPath, "Files", $@"\{UploadFiles[0].FileName}");
                if (System.IO.File.Exists(filename))
                {
                    System.IO.File.Delete(filename);
                }
            }
            catch (Exception e)
            {
                Response.Clear();
                Response.StatusCode = 200;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File removed successfully";
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
            }
        }

        [HttpPost("[action]")]
        public IActionResult SavePdf(IList<IFormFile> chunkFile, IList<IFormFile> UploadFiles)
        {
            try
            {
                if (UploadFiles is null)
                    return BadRequest("Ficheiro não é pdf");

                var pdfDirectory = Path.Combine(HostingEnvironment.WebRootPath, "Files", "Uploads", "Publicacoes", "Desconhecida");
                var exists = Directory.Exists(pdfDirectory);
                if (!exists)
                {
                    Directory.CreateDirectory(pdfDirectory);
                }

                foreach (var file in UploadFiles)
                {
                    var filePath = Path.Combine(pdfDirectory, file.FileName);

                    if (!System.IO.File.Exists(filePath))
                    {
                        using (var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        {
                            file.OpenReadStream().CopyToAsync(fs);
                        }
                    }
                    else
                    {

                        //using (FileStream fs = System.IO.File.Open(filePath, FileMode.Append))
                        //{
                        //    file.CopyTo(fs);
                        //    fs.Flush();

                        //}

                        using (var fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None))
                        {
                            file.OpenReadStream().CopyToAsync(fs);
                        }
                    }
                }

                return Ok(new { Message = "File uploaded successfully" });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = e.Message });
            }
        }

        [HttpPost("[action]")]
        public void SavePdf2(IList<IFormFile> chunkFile, IList<IFormFile> UploadFiles)
        {
            try
            {
                var pdfDirectory = Path.Combine(HostingEnvironment.WebRootPath,"Files", "Uploads", "Publicacoes", "Desconhecida");
                var exists = Directory.Exists(pdfDirectory);
                if (!exists)
                {
                    Directory.CreateDirectory(pdfDirectory);
                }
                foreach (var file in UploadFiles)
                {
                    var filename = Path.Combine(pdfDirectory, file.FileName);
                    if (!System.IO.File.Exists(filename))
                    {
                        using (FileStream fs = System.IO.File.Create(filename))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }
                    }
                    else
                    {
                        using (FileStream fs = System.IO.File.Open(filename, FileMode.Append))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Response.Clear();
                Response.StatusCode = 204;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File failed to upload";
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
            }
        }

        [HttpPost("[action]")]
        public void RemovePdf(IList<IFormFile> UploadFiles)
        {
            try
            {
                var filename = Path.Combine(HostingEnvironment.WebRootPath, "Files", "Uploads", "Publicacoes", "Desconhecida" + $@"\{UploadFiles[0].FileName}");
                if (System.IO.File.Exists(filename))
                {
                    System.IO.File.Delete(filename);
                }
            }
            catch (Exception e)
            {
                Response.Clear();
                Response.StatusCode = 200;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File removed successfully";
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
            }
        }

        [HttpPost("[action]")]
        public void SavePost(IList<IFormFile> chunkFile, IList<IFormFile> UploadFiles)
        {
            try
            {
                foreach (var file in UploadFiles)
                {
                    var filename = HostingEnvironment.WebRootPath + $@"\uploads\Posts\{file.FileName}";
                    if (!System.IO.File.Exists(filename))
                    {
                        using (FileStream fs = System.IO.File.Create(filename))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }
                    }
                    else
                    {
                        using (FileStream fs = System.IO.File.Open(filename, FileMode.Append))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Response.Clear();
                Response.StatusCode = 204;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File failed to upload";
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
            }
        }

        [HttpPost("[action]")]
        public void RemovePost(IList<IFormFile> UploadFiles)
        {
            try
            {
                var filename = HostingEnvironment.WebRootPath + $@"\{UploadFiles[0].FileName}";
                if (System.IO.File.Exists(filename))
                {
                    System.IO.File.Delete(filename);
                }
            }
            catch (Exception e)
            {
                Response.Clear();
                Response.StatusCode = 200;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File removed successfully";
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
            }
        }

    }
}
