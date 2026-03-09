using MediaOrganizerApp.Application.DTOs.Reporting;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Interfaces.Services.Reporting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Helpers.BusinessObjects
{
    public class BO_Media : IBO_Media
    {
        readonly IMediaRepository _repoMedia;
  
        public BO_Media(IMediaRepository repoMedia)
        {
            _repoMedia = repoMedia;
        }

        public async Task<List<MediaReportModel>> GetMediaFiles(AppDefinitions.Query_TipoMedia reportOption)
        {
            var mediaFiles = reportOption == AppDefinitions.Query_TipoMedia.Todos ? await _repoMedia.GetAllMediaFromDB_Report()
                : reportOption == AppDefinitions.Query_TipoMedia.Audio ? await _repoMedia.GetFilesByMediaType(AppDefinitions.Query_TipoMedia.Audio)
                : reportOption == AppDefinitions.Query_TipoMedia.Video ? await _repoMedia.GetFilesByMediaType(AppDefinitions.Query_TipoMedia.Video)
                : await _repoMedia.GetFilesByMediaType(AppDefinitions.Query_TipoMedia.Others);

            List <MediaReportModel> ListaMediaFiles = new();
            foreach (var item in mediaFiles)
            {
                ListaMediaFiles.Add(new MediaReportModel
                {
                    IdMedia = item.Id,
                    Titulo = item.FileName,
                    Autor = item.Autor,
                    TipoMedia = item.TipoMedia,
                    Formato_Media = item.FormatoMedia,
                    Genero = item.Genero
                });
            }
            return ListaMediaFiles;
        }
    }
}


