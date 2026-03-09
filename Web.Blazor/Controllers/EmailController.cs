using Microsoft.AspNetCore.Mvc;
using MsgReader;
using Syncfusion.HtmlConverter;
using System.IO;
using System.Runtime.InteropServices;
using System;
using Syncfusion.Pdf;
using Microsoft.AspNetCore.Hosting;

namespace Web.Blazor.Controllers
{
    public class EmailController : Controller
    {
        public string basePath;

        public EmailController(IWebHostEnvironment hostingEnvironment)
        {
            basePath = hostingEnvironment.ContentRootPath;
        }
        public ActionResult ExportToPDF(string email)
        {
            //Indicates whether the current application is running in Windows or Linux.
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            //string fullPath = System.IO.Path.GetFullPath(@"Data/MsgReaderContent.msg");
            string fullPath = System.IO.Path.GetFullPath(@"Data/Input.eml");
            byte[] finalBytes = null;
            string filename = "";

            try
            {
                //Convert EML or MSG to HTML using a third-party reader.
                var msgReader = new Reader();
                string tempFolder = Path.Combine(basePath,"Files", "Uploads", "Desconhecida") ;
                //File contains the HTML file converted from MSG.
                var files = msgReader.ExtractToFolder(fullPath, tempFolder);
                var error = msgReader.GetErrorMessage();
                if (!string.IsNullOrEmpty(error))
                    throw new Exception(error);
                //Initialize HTML to PDF converter.
                HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();
                if (!string.IsNullOrEmpty(files[0]))
                {
                    if (isLinux)
                    {
                        filename = "LinuxOutput.pdf";
                        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
                        //Initialize HTML to PDF converter.
                        htmlConverter = new HtmlToPdfConverter();
                        BlinkConverterSettings blinkConverterSettings = new BlinkConverterSettings();
                        //Set command line arguments to run without sandbox.
                        blinkConverterSettings.CommandLineArguments.Add("--no-sandbox");
                        blinkConverterSettings.CommandLineArguments.Add("--disable-setuid-sandbox");
                        htmlConverter.ConverterSettings = blinkConverterSettings;
                    }
                    else if (isWindows)
                    {
                        filename = "WindowsOutput.pdf";
                        //Initialize HTML to PDF converter.
                        htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink);
                        BlinkConverterSettings settings = new BlinkConverterSettings();
                        //Assign Blink converter settings to HTML converter.
                        htmlConverter.ConverterSettings = settings;
                    }
                }

                //Create a new PDF document. 
                PdfDocument pdFDocument = new PdfDocument();
                //Create a memory stream to save a PDF document. 
                MemoryStream outputStream = new MemoryStream();
                //Convert HTML file to a PDF document. 
                pdFDocument = htmlConverter.Convert(System.IO.File.ReadAllText(files[0]), tempFolder);
                //Save a PDF document. 
                pdFDocument.Save(outputStream);
                finalBytes = outputStream.ToArray();
                //Close the document.
                pdFDocument.Close(true);
                //Close the stream. 
                outputStream.Close();
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return File(finalBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, filename);
        }
    }
}
