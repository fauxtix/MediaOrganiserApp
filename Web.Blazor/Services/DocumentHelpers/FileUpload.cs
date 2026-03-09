using BlazorInputFile;
using MediaOrganizerApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Web.Blazor.Services.DocumentHelpers
{
    public class FileUpload : IFileUpload
    {
        private readonly IWebHostEnvironment _environment;
        public FileUpload(IWebHostEnvironment env)
        {
            _environment = env;
        }

        public async Task UploadAsync(IFileListEntry arquivoEntrada)
        {
            try
            {
                var path = Path.Combine(_environment.WebRootPath + "/Uploads/Media", arquivoEntrada.Name);

                var ms = new MemoryStream();
                await arquivoEntrada.Data.CopyToAsync(ms);

                using (FileStream arquivo = new FileStream(path, FileMode.Create,
                        FileAccess.Write))
                {
                    ms.WriteTo(arquivo);
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
