using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Domain.Entities;

namespace Web.Blazor.Services.ApiWrappers
{
    public interface IMediaServiceWrapper
    {
        int ConvertIMDbRatingToFiveScale(string imdbRating);
        DateTime ConvertIOMDbStringAsDate(string input);
        string ConvertIOMDbTimeStringAsTime(string input);
        Task<bool> CreateMediaFile(MediaVM mediaFileDTO);
        Task<bool> DeleteMediaFile(int id);
        Task<MediaVM> GetAlbumInfo(string artist, string album);
        Task<IEnumerable<MediaVM>> GetAllMediaFromDB_Report();
        Task<IEnumerable<MediaVM>> GetAllMedia_Async(string userId, string role);
        Task<IEnumerable<MediaVM>> GetFilesByMediaCategory(int id);
        Task<IEnumerable<MediaVM>> GetFilesByMediaFormat(int id);
        Task<IEnumerable<MediaVM>> GetFilesByMediaType(AppDefinitions.Query_TipoMedia Tipo);
        Task<int> GetFirstId_Media();
        Task<IEnumerable<Formato_Media>> GetFormatos_Media();
        Task<string> GetFormatos_Media(int id);
        Task<string> GetFromDataBase(string fileName);
        Task<int> GetLastId_Media();
        Task<MediaVM> GetMediaByID(int id);
        Task<MediaVM> GetMediaByTitle(string sFileName);
        Task<int> GetMediaCategoryByDescription(string descricao);
        Task<int> GetMediaFormatByDescription(string formatDescription);
        Task<IEnumerable<VideoCategoryLocations>> GetMusicMedia(int catId);
        Task<IMDbModel> GetOMDbInfo(string title, string actor);
        Task<IEnumerable<MediaVM>> GetTitlesFromDataBase(string sCriteria);
        Task<decimal> GetTotalEspacoOcupado_Media(string s);
        Task<IEnumerable<VideoCategoryLocations>> GetVideoLocations(int idCategoria);
        Task<IEnumerable<VideoCategoryLocations>> GetYoutubeMusicMedia(int catId);
        Task<IEnumerable<MediaVM>> ListarMedia_PorID(int id);
        Task<bool> MediaFileExists(string mediaTitle, string mediaAuthor);
        List<string> RegistoComErros(MediaVM mediaFile);
        Task<bool> UpdateMediaFile(MediaVM mediaFileDTO);
        Task<bool> UpdateMediaFile_RegPath(int id);
    }
}