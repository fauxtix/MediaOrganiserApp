using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IMediaRepository : IBaseRepository<Media>
    {
        Task<bool> CreateMediaFile(Media mediaFile);
        Task<bool> UpdateMediaFile(Media mediaFile);
        Task<IEnumerable<MediaVM>> GetAllMedia(string userId, string role);
        Task<IEnumerable<MediaVM>> GetAllMedia_Async(string userId, string role);
        Task<IEnumerable<MediaVM>> GetAllMediaFromDB_Report();
        Task<IEnumerable<MediaVM>> GetFilesByMediaType(Query_TipoMedia Tipo);
        Task<IEnumerable<MediaVM>> GetFilesByMediaFormat(int iMediaFormatCode);
        IEnumerable<MediaVM> GetFilesByMediaCategory(int iMediaCategoryCode);
        Task<string> GetFromDataBase(string fileName);
        Task<MediaVM> GetMediaByID(int iFileId);
        Task<IEnumerable<MediaVM>> GetTitlesFromDataBase(string sCriteria);
        Task<bool> MediaFileExists(string mediaTile, string mediaAuthor);
        Task UpdateMediaFile_RegPath(int ID);
        Task<int> GetFirstId_Media();
        Task<int> GetLastId_Media();
        Task<decimal> GetTotalEspacoOcupado_Media(string s);
        Task<IEnumerable<MediaVM>> ListarMedia_PorID(int Id);

        Task<MediaVM> GetMediaByTitle(string sFileName);
        Task<IEnumerable<Formato_Media>> GetFormatos_Media();
        Task<string> GetFormatos_Media(int idMedia);
        Task<int> GetMediaFormatByDescription(string FormatDescription);
        Task<int> GetMediaCategoryByDescription(string Descricao);
        Task<IEnumerable<VideoCategoryLocations>> GetVideoLocations(int idCategoria);
        Task<IEnumerable<VideoCategoryLocations>> GetMusicMedia(int idCategoria);
        Task<IEnumerable<VideoCategoryLocations>> GetAllMusicTitles();
        Task UpdateMediaFile_Cover(int Id, string coverUrl);
        Task<VideoCategoryLocations> GetMusicFile(int Id);
    }
}