using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.Api.MusicBrainz;
using MediaOrganizerApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface IMediaService
    {
        Task<bool> CreateMediaFile(MediaVM mediaFile);
        Task<bool> DeleteMediaFile(int id);
        Task<bool> UpdateMediaFile(MediaVM mediaFile);
        Task<bool> UpdateMediaFile_RegPath(int id);
        Task<IEnumerable<MediaVM>> GetAllMedia_Async(string userId, string role);
        Task<IEnumerable<MediaVM>> GetAllMediaFromDB_Report();
        Task<IEnumerable<MediaVM>> GetFilesByMediaType(Query_TipoMedia Tipo);
        Task<IEnumerable<MediaVM>> GetFilesByMediaFormat(int iMediaFormatCode);
        Task<IEnumerable<MediaVM>> GetFilesByMediaCategory(int iMediaCategoryCode);
        Task<string> GetFromDataBase(string fileName);
        Task<MediaVM> GetMediaByID(int iFileId);
        Task<IEnumerable<MediaVM>> GetTitlesFromDataBase(string sCriteria);
        Task<bool> MediaFileExists(string mediaTitle, string mediaAuthor);
        Task<int> GetFirstId_Media();
        Task<int> GetLastId_Media();
        Task<decimal> GetTotalEspacoOcupado_Media(string s);
        Task<IEnumerable<MediaVM>> ListarMedia_PorID(int Id);

        Task<MediaVM> GetMediaByTitle(string sFileName);
        Task<IEnumerable<Formato_Media>> GetFormatos_Media();
        Task<string> GetFormatos_Media(int idMedia);
        Task<int> GetMediaFormatByDescription(string FormatDescription);
        Task<int> GetMediaCategoryByDescription(string Descricao);

        List<string> RegistoComErros(MediaVM mediaFile);
        Task<IEnumerable<VideoCategoryLocations>> GetVideoLocations(int idCategoria);
        Task<IEnumerable<VideoCategoryLocations>> GetMusicMedia(int idCategoria);
        Task<IEnumerable<VideoCategoryLocations>> GetYoutubeMusicMedia(int catId);
        Task<MediaVM> GetAlbumInfo(string artist, string album);
        Task<IMDbModel> GetOMDbInfo(string title, string actor);
        string ConvertIOMDbTimeStringAsTime(string input);
        DateTime ConvertIOMDbStringAsDate(string input);
        int ConvertIMDbRatingToFiveScale(string imdbRating);
    }
}