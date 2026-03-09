using Dapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Utilities;
using MediaOrganizerApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    public class MediaRepository : SQLServerBaseRepository<Media>, IMediaRepository
    {
        readonly StringBuilder sbData = new();
        private readonly IDapperContext _context;

        public MediaRepository(IDapperContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<MediaVM>> GetAllMediaFromDB_Report()
        {
            string sProcedName = "usp_Media_GetAllMediaFromDB_Report";

            using (var connection = _context.CreateConnection())
            {
                var mediaFiles = await connection.QueryAsync<MediaVM>(sProcedName,
                    commandType: CommandType.StoredProcedure);
                return mediaFiles;
            }
        }

        public Task<IEnumerable<MediaVM>> GetFilesByMediaFormat(int iMediaFormatCode)
        {
            string sProcedName = "usp_Media_GetFilesByMediaFormat";

            using (var connection = _context.CreateConnection())
            {
                var mediaFiles = connection.QueryAsync<MediaVM>(sProcedName, new { iMediaFormatCode },
                    commandType: CommandType.StoredProcedure);
                return mediaFiles;
            }
        }

        public IEnumerable<MediaVM> GetFilesByMediaCategory(int iMediaCategoryCode)
        {
            string sProcedName = "usp_Media_GetFilesByMediaCategory";

            using (var connection = _context.CreateConnection())
            {
                var mediaFiles = connection.Query<MediaVM>(sProcedName, new { iMediaCategoryCode },
                    commandType: CommandType.StoredProcedure);
                return mediaFiles;
            }
        }

        public async Task<IEnumerable<MediaVM>> GetAllMedia(string userId, string role)
        {
            string procedureName;
            if (role.ToLower().Contains(UserRole.Admin.ToString().ToLower()))
            {
                procedureName = "usp_Media_GetAllMedia";
                using (var connection = _context.CreateConnection())
                {
                    var mediaFiles = await connection.QueryAsync<MediaVM>(procedureName,
                        commandType: CommandType.StoredProcedure);
                    return mediaFiles;
                }
            }
            else
            {
                procedureName = "usp_Media_GetAllMedia_ById";
                using (var connection = _context.CreateConnection())
                {
                    var mediaFiles = await connection.QueryAsync<MediaVM>(procedureName,
                        new { CreatedBy = userId },
                        commandType: CommandType.StoredProcedure);
                    return mediaFiles;
                }
            }
        }

        public async Task<IEnumerable<MediaVM>> GetAllMedia_Async(string userId, string role)
        {
            string procedureName;
            if (role.ToLower().Contains(UserRole.Admin.ToString().ToLower()))
            {
                procedureName = "usp_Media_GetAllMedia";
                using (var connection = _context.CreateConnection())
                {
                    var mediaFiles = await connection.QueryAsync<MediaVM>(procedureName,
                        commandType: CommandType.StoredProcedure);
                    return mediaFiles;
                }
            }
            else
            {
                procedureName = "usp_Media_GetAllMedia_ById";
                using (var connection = _context.CreateConnection())
                {
                    var mediaFiles = await connection.QueryAsync<MediaVM>(procedureName,
                        new { CreatedBy = userId },
                        commandType: CommandType.StoredProcedure);
                    return mediaFiles;
                }
            }
        }

        public async Task<IEnumerable<MediaVM>> GetFilesByMediaType(Query_TipoMedia Tipo)
        {
            int TipoMedia = (int)(Tipo);

            string sProcedName = "usp_Media_GetFilesByMediaType";

            using (var connection = _context.CreateConnection())
            {
                var mediaFiles = await connection.QueryAsync<MediaVM>(sProcedName, new { TipoMedia },
                    commandType: CommandType.StoredProcedure);
                return mediaFiles;
            }
        }

        public async Task<int> GetFirstId_Media()
        {
            sbData.Clear();
            if (Utilitarios.IsSQLServer())
            {
                sbData.Append("SELECT TOP 1 Id FROM Media ");
            }
            else
            {
                sbData.Append("SELECT Id FROM Media LIMIT 1 ");
            }
            sbData.Append("ORDER BY Id ");
            using (var connection = _context.CreateConnection())
            {
                int iID = await connection.QuerySingle(sbData.ToString());
                return iID;
            }
        }

        public async Task<IEnumerable<Formato_Media>> GetFormatos_Media()
        {
            string sProcedName = "usp_Formato_Media_GetAll";

            using (var connection = _context.CreateConnection())
            {
                var mediaFormats = await connection.QueryAsync<Formato_Media>(sProcedName,
                    commandType: CommandType.StoredProcedure);
                return mediaFormats;
            }
        }

        public async Task<string> GetFormatos_Media(int Id)
        {
            string sProcedName = "usp_Formato_Media_GetDescription_ById";

            using (var connection = _context.CreateConnection())
            {
                var mediaFormat = (await connection.QueryAsync<string>(sProcedName, new { Id },
                    commandType: CommandType.StoredProcedure))
                    .SingleOrDefault();
                return mediaFormat;
            }
        }

        // SP 'usp_Media_GetPath_ByFileName' foi criada, mas não está a ser usada, uma vez que
        // os testes não devolveram qq registo (comparação de campos 'text' não funcionam no sql server)
        // provalvelmente este método não devolve qq informação
        public async Task<string> GetFromDataBase(string fileName)
        {
            sbData.Clear();
            sbData.Append("SELECT FilePath FROM Media ");
            sbData.Append("WHERE FilePath = @fileName");
            using (var connection = _context.CreateConnection())
            {
                string sPath = (await connection.QueryAsync<string>(sbData.ToString(), new { fileName }))
                    .FirstOrDefault();
                return sPath;
            }
        }

        public async Task<int> GetLastId_Media()
        {
            sbData.Clear();
            sbData.Append("SELECT TOP 1 Id FROM Media ORDER BY Id DESCENDING");
            using (var connection = _context.CreateConnection())
            {
                int iID = (await connection.QuerySingleAsync(sbData.ToString()));
                return iID;
            }
        }

        public async Task<MediaVM> GetMediaByID(int FileId)
        {
            string sProcedName = "usp_Media_GetFilesByMediaId";


            using (var connection = _context.CreateConnection())
            {
                MediaVM mediaFile = (await connection
                    .QueryAsync<MediaVM>(sProcedName, new { FileId },
                    commandType: CommandType.StoredProcedure))
                    .SingleOrDefault();
                return mediaFile;
            }
        }

        // não está a ser usada SP, uma vez que
        // os testes não devolveram qq registo (comparação de campos 'text' não funcionam no sql server)
        // provalvelmente este método não devolve qq informação

        public async Task<MediaVM> GetMediaByTitle(string FilePath)
        {
            sbData.Clear();
            sbData.Append("SELECT AnoEdicao, Autor ");
            sbData.Append("FROM Media ");
            sbData.Append("WHERE FilePath = @FilePath");
            using (var connection = _context.CreateConnection())
            {
                return (await connection.QueryAsync<MediaVM>(sbData.ToString(), new { FilePath }))
                    .FirstOrDefault();
            }
        }

        public async Task<IEnumerable<MediaVM>> GetTitlesFromDataBase(string sSearchCriteria)
        {
            sbData.Clear();
            sbData.Append("SELECT * ");
            sbData.Append("FROM vwMedia ");
            sbData.Append($"WHERE {sSearchCriteria}");
            using (var connection = _context.CreateConnection())
            {
                var mediaFiles = await connection.QueryAsync<MediaVM>(sbData.ToString());

                return mediaFiles;
            }
        }


        // Método não está a ser usado na aplicação (?)
        // comentado
        public async Task<decimal> GetTotalEspacoOcupado_Media(string sFiltro)
        {
            sbData.Clear();
            //sbData.Append("SELECT SUM( ");
            //sbData.Append($"{GetSizeQueryString()} ) ");
            //sbData.Append("FROM Media ");
            //if (!string.IsNullOrEmpty(sFiltro))
            //    sbData.Append($"WHERE {sFiltro}");

            using (var connection = _context.CreateConnection())
            {
                decimal decTotal = (await connection.QueryAsync<decimal>(sbData.ToString()))
                    .FirstOrDefault();
                return decTotal;
            }
        }

        public async Task<IEnumerable<MediaVM>> ListarMedia_PorID(int IdCategoria)
        {
            string sProcedName = "usp_Media_GetFile_ByMediaCategory";

            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryAsync<MediaVM>(sProcedName, new { IdCategoria },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<bool> MediaFileExists(string mediaTile, string mediaAuthor)
        {
            string sProcedName = "usp_Media_MediaFileExists";
            using (var connection = _context.CreateConnection())
            {
                int iId = (await connection.QueryAsync<int>(sProcedName, new { Titulo = mediaTile, Autor = mediaAuthor },
                    commandType: CommandType.StoredProcedure))
                    .FirstOrDefault();
                return iId > 0;
            }
        }

        public async Task<bool> CreateMediaFile(Media mediaFile)
        {
            string sProcedName = "usp_Media_Create";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(sProcedName, mediaFile,
                    commandType: CommandType.StoredProcedure);
            }
            return true;
        }

        public async Task<bool> UpdateMediaFile(Media mediaFile)
        {
            DynamicParameters paramCollection = new();
            paramCollection.Add("@Id", mediaFile.Id);
            paramCollection.Add("@AnoEdicao", mediaFile.AnoEdicao);
            paramCollection.Add("@Tempo", mediaFile.Tempo);
            paramCollection.Add("@Autor", mediaFile.Autor);
            paramCollection.Add("@CoverFile", mediaFile.CoverFile);
            paramCollection.Add("@FileUrl", mediaFile.FileUrl);
            paramCollection.Add("@FilePath", mediaFile.FilePath);
            paramCollection.Add("@DataMov", mediaFile.DataMov);
            paramCollection.Add("@Visualizado", mediaFile.Visualizado);
            paramCollection.Add("@fileName", mediaFile.FileName);
            paramCollection.Add("@Rating", mediaFile.Rating);
            paramCollection.Add("@Notas", mediaFile.Notas);
            paramCollection.Add("@TipoMedia", mediaFile.TipoMedia);
            paramCollection.Add("@idGenero", mediaFile.IdGenero);
            paramCollection.Add("@GuardaEmDisco", mediaFile.GuardaEmdisco);
            paramCollection.Add("@IdFormato_Media", mediaFile.IdFormato_Media);

            string sProcedName = "usp_Media_Update";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(sProcedName, param: paramCollection,
                    commandType: CommandType.StoredProcedure);
            }
            return true;
        }

        public async Task UpdateMediaFile_RegPath(int Id)
        {
            string sProcedName = "usp_Media_ResetRegPath_ById";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(sProcedName, new { Id },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task UpdateMediaFile_Cover(int Id, string coverUrl)
        {
            string sProcedName = "usp_Media_UpdateCover";

            DynamicParameters paramCollection = new();
            paramCollection.Add("@Id", Id);
            paramCollection.Add("@CoverUrl", coverUrl);

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(sProcedName, param: paramCollection,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<int> GetMediaFormatByDescription(string Descricao)
        {
            string sProcedName = "usp_Formato_Media_GetId_ByDescription";

            using (var connection = _context.CreateConnection())
            {
                int iId = (await connection.QueryAsync<int>(sProcedName, new { Descricao },
                    commandType: CommandType.StoredProcedure))
                    .FirstOrDefault();
                return iId;
            }
        }

        public async Task<int> GetMediaCategoryByDescription(string Descricao)
        {
            string sProcedName = "usp_Categoria_GetId_ByDescription";

            using (var connection = _context.CreateConnection())
            {
                int iId = (await connection.QueryAsync<int>(sProcedName, new { Descricao },
                    commandType: CommandType.StoredProcedure))
                    .FirstOrDefault();
                return iId;
            }
        }

        public async Task<IEnumerable<VideoCategoryLocations>> GetVideoLocations(int idCategoria)
        {
            string sProcedName = "usp_Media_GetVideoLocationsByCategory";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var output = await connection.QueryAsync<VideoCategoryLocations>(sProcedName,
                       new { IdCategoria = idCategoria },
                       commandType: CommandType.StoredProcedure);
                    return output;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<VideoCategoryLocations>> GetMusicMedia(int idCategoria)
        {
            string sProcedName = "usp_Media_MusicTitles";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var output = await connection.QueryAsync<VideoCategoryLocations>(sProcedName,
                              new { IdCategoria = idCategoria },
                       commandType: CommandType.StoredProcedure);
                    return output;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<VideoCategoryLocations>> GetAllMusicTitles()
        {
            //            string sProcedName = "usp_Media_GetAllMusicTitles";
            string sProcedName = "usp_Media_GetMusicTitlesWithNoCover";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var output = await connection.QueryAsync<VideoCategoryLocations>(sProcedName,
                       commandType: CommandType.StoredProcedure);
                    return output;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<VideoCategoryLocations> GetMusicFile(int Id)
        {
            string sProcedName = "usp_Media_GetMusicById";
            DynamicParameters paramCollection = new();
            paramCollection.Add("@Id", Id);

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var output = await connection
                        .QuerySingleAsync<VideoCategoryLocations>(
                        sProcedName, param: paramCollection, commandType: CommandType.StoredProcedure);
                    return output;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
