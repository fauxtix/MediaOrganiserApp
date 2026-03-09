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

namespace MediaOrganizerApp.Persistence.Repositories.RawSQLRepository
{
    public class MediaRepository : BaseRepository<Media>, IMediaRepository
    {
        readonly StringBuilder sbData = new();
        private IDbConnection sqlConnection;

        public MediaRepository(IDbConnectionFactory dbFactory)
        {
            sqlConnection = dbFactory.CreateSqlConnection("SqlServerCon");
        }
        public async Task<bool> CreateMediaFile(Media mediaFile)
        {

            sbData.Clear();
            sbData.Append("Insert Into [Publicacoes].[dbo].[Media] ");
            sbData.Append("(FileUrl, FilePath, DataMov, Visualizado, ");
            sbData.Append("fileName, Rating, AnoEdicao, Notas, Autor, ");
            sbData.Append("TipoMedia, idGenero, GuardaEmDisco, IdFormato_Media) ");
            sbData.Append("Values ");
            sbData.Append("(@FileUrl, @FilePath, @DataMov, @Visualizado, ");
            sbData.Append("@fileName, @Rating, @AnoEdicao, @Notas, @Autor, ");
            sbData.Append("@TipoMedia, @idGenero, @GuardaEmDisco, @IdFormato_Media) ");


            using (IDbConnection connection = sqlConnection)
            {
                await connection.ExecuteAsync(sbData.ToString(), mediaFile,
                    commandType: CommandType.StoredProcedure);
            }
            return true;
        }

        public async Task<bool> UpdateMediaFile(Media mediaFile)
        {
            sbData.Clear();
            sbData.Append("Insert Into [Publicacoes].[dbo].[Media] ");
            sbData.Append("(FileUrl, FilePath, DataMov, Visualizado, ");
            sbData.Append("fileName, Rating, AnoEdicao, Notas, Autor, ");
            sbData.Append("TipoMedia, idGenero, GuardaEmDisco, IdFormato_Media) ");
            sbData.Append("Values ");
            sbData.Append("(@FileUrl, @FilePath, @DataMov, @Visualizado, ");
            sbData.Append("@fileName, @Rating, @AnoEdicao, @Notas, @Autor, ");
            sbData.Append("@TipoMedia, @idGenero, @GuardaEmDisco, @IdFormato_Media) ");

            using (IDbConnection connection = sqlConnection)
            {
                int iId = (await connection.ExecuteAsync(sbData.ToString(), mediaFile));
                return iId > 0;
            }
        }
        public async Task UpdateMediaFile_RegPath(int Id)
        {
            sbData.Clear();
            sbData.Append("UPDATE [Media] ");
            sbData.Append("SET FilePath = ''");
            sbData.Append("WHERE [Id] = @Id");
            using (IDbConnection connection = sqlConnection)
            {
                await connection.ExecuteAsync(sbData.ToString(), new { Id });
            }
        }

        public async Task UpdateMediaFile_Cover(int Id, string coverUrl)
        {
            DynamicParameters paramCollection = new();
            paramCollection.Add("@Id", Id);
            paramCollection.Add("@CoverFile", coverUrl);
            sbData.Clear();
            sbData.Append("Update [Publicacoes].[dbo].[Media]");
            sbData.Append("Set CoverFile = @CoverUrl ");
            sbData.Append("WHERE [Id] = @Id");
            using (IDbConnection connection = sqlConnection)
            {
                await connection.ExecuteAsync(sbData.ToString(), param: paramCollection);
            }
        }


        public async Task<IEnumerable<MediaVM>> GetAllMediaFromDB_Report()
        {
            sbData.Clear();
            sbData.Append("SELECT M.Id, M.FileName, M.Autor, M.TipoMedia, F.Descricao AS Formato_Media ");
            sbData.Append("FROM Media M ");
            sbData.Append("INNER JOIN Formato_Media F ON ");
            sbData.Append("M.IdFormato_Media = F.Id ");
            sbData.Append("ORDER BY M.FileName");

            using (IDbConnection connection = sqlConnection)
            {
                var mediaFiles = await connection.QueryAsync<MediaVM>(sbData.ToString());
                return mediaFiles;
            }
        }

        public async Task<IEnumerable<MediaVM>> GetFilesByMediaFormat(int iMediaFormatCode)
        {
            sbData.Clear();
            sbData.Append(GetMainQueryString());
            sbData.Append("WHERE M.IdFormato_Media = @iMediaFormatCode ");
            sbData.Append("ORDER BY M.Id DESC");

            using (IDbConnection connection = sqlConnection)
            {
                var mediaFiles = await connection.QueryAsync<MediaVM>(sbData.ToString(), new { iMediaFormatCode });
                return mediaFiles;
            }
        }

        public IEnumerable<MediaVM> GetFilesByMediaCategory(int iMediaCategoryCode)
        {
            sbData.Clear();
            sbData.Append(GetMainQueryString());
            sbData.Append("WHERE M.IdGenero = @iMediaCategoryCode ");
            sbData.Append("ORDER BY M.Id DESC");

            using (IDbConnection connection = sqlConnection)
            {
                var mediaFiles = connection.Query<MediaVM>(sbData.ToString(),
                    new { iMediaCategoryCode });
                return mediaFiles;
            }
        }

        public async Task<IEnumerable<MediaVM>> GetAllMedia()
        {
            sbData.Clear();
            sbData.Append(GetMainQueryString());
            sbData.Append("ORDER BY M.Id DESC");

            using (IDbConnection connection = sqlConnection)
            {
                var mediaFiles = await connection.QueryAsync<MediaVM>(sbData.ToString());
                return mediaFiles;
            }
        }

        public async Task<IEnumerable<MediaVM>> GetAllMedia_Async()
        {
            sbData.Clear();
            sbData.Append(GetMainQueryString());
            sbData.Append("ORDER BY M.Id DESC");

            using (IDbConnection connection = sqlConnection)
            {
                IEnumerable<MediaVM> mediaFiles = await connection.QueryAsync<MediaVM>(sbData.ToString());
                return mediaFiles;
            }

        }

        public async Task<IEnumerable<MediaVM>> GetFilesByMediaType(Query_TipoMedia Tipo)
        {
            int TipoMedia = (int)(Tipo);

            sbData.Clear();
            sbData.Append(GetMainQueryString());
            sbData.Append("WHERE TipoMedia = @TipoMedia ");
            sbData.Append("ORDER BY M.Id DESC");

            using (IDbConnection connection = sqlConnection)
            {
                var mediaFiles = await connection.QueryAsync<MediaVM>(sbData.ToString(), new { TipoMedia });
                return mediaFiles;
            }
        }

        private string GetMainQueryString()
        {
            StringBuilder sbDummy = new();

            sbDummy.Append("SELECT M.Id, M.FileName, M.DataMov, M.FilePath, ");
            sbDummy.Append("M.FileUrl, M.TipoMedia, M.Visualizado, ");
            sbDummy.Append("M.Rating, M.Notas, M.IdGenero, M.Autor, M.AnoEdicao, M.GuardaEmdisco, ");
            sbDummy.Append("M.IdFormato_Media, C.Descricao AS Genero, F.Descricao AS Formato_Media, ");
            sbDummy.Append(GetDurationQueryString());
            sbDummy.Append(GetSizeQueryString());
            sbDummy.Append("FROM Media M ");
            sbDummy.Append("INNER JOIN Categoria C ON M.IdGenero = C.Id ");
            sbDummy.Append("INNER JOIN Formato_Media F ON M.IdFormato_Media = F.Id ");

            return sbDummy.ToString();
        }

        private string GetDurationQueryString()
        {
            StringBuilder sbDuracao = new();
            var dbType = sqlConnection.ConnectionString;
            if (dbType.ToLower().Contains(".db"))  // sqlite
            {
                sbDuracao.Append("CASE WHEN INSTR(Notas, 'Velocidade') > 0 THEN ");
                sbDuracao.Append("LTRIM(SUBSTR(Notas, INSTR(Notas, 'Tempo') + 7, 11)) ELSE ");
                sbDuracao.Append("'00;00;00' END AS Tempo, ");  // vígula no fim, uma vez que, a seguir, vem a coluna 'Tamanho'
            }
            else // SQL Server 
            {
                sbDuracao.Append("CASE ");
                sbDuracao.Append("WHEN CHARINDEX('Velocidade', Notas) > 0 THEN ");
                sbDuracao.Append("LTRIM(SUBSTRING(Notas, CHARINDEX('Tempo', Notas) + 7, 11)) ");
                sbDuracao.Append("ELSE '00:00:00' ");
                sbDuracao.Append("END AS Tempo,  "); // vígula no fim, uma vez que, a seguir, vem a coluna 'Tamanho'
            }

            string sDuracao = sbDuracao.ToString();
            return sDuracao;
        }

        private string GetSizeQueryString()
        {
            StringBuilder sbTamanho = new();
            var dbType = sqlConnection.ConnectionString;
            if (dbType.ToLower().Contains(".db"))  // sqlite
            {
                sbTamanho.Append("CASE WHEN INSTR(Notas, 'MB') > 0 THEN ");
                sbTamanho.Append("CAST(REPLACE(SUBSTR(Notas, instr(Notas, 'Tamanho') + 10, ");
                sbTamanho.Append("(INSTR(Notas, 'MB')) - (INSTR(Notas, 'Tamanho') + 10)), ',', '.') AS DECIMAL) ELSE ");
                sbTamanho.Append("0.00 END Tamanho ");
            }
            else // SQL SERVER
            {
                sbTamanho.Append("CASE WHEN  CHARINDEX(' MB', Notas) > 0 THEN ");
                sbTamanho.Append("CAST(REPLACE(SUBSTRING(Notas, CHARINDEX('Tamanho', Notas) + 10, ");
                sbTamanho.Append("(CHARINDEX(' MB', Notas)) - (CHARINDEX('Tamanho', Notas) + 10)), ',', '.') AS DECIMAL) ELSE ");
                sbTamanho.Append("0.00 END AS Tamanho ");
            }

            string sTamanho = sbTamanho.ToString();
            return sTamanho;
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
            using (IDbConnection connection = sqlConnection)
            {
                int iID = await connection.QuerySingleAsync(sbData.ToString());
                return iID;
            }
        }

        public async Task<IEnumerable<Formato_Media>> GetFormatos_Media()
        {
            sbData.Clear();
            sbData.Append("SELECT * FROM Formato_Media ");
            sbData.Append("ORDER BY Id ");

            using (IDbConnection connection = sqlConnection)
            {
                var mediaFormats = await connection.QueryAsync<Formato_Media>(sbData.ToString());
                return mediaFormats;
            }
        }

        public async Task<string> GetFormatos_Media(int Id)
        {
            sbData.Clear();
            sbData.Append("SELECT Descricao FROM Formato_Media ");
            sbData.Append("WHERE Id = @Id");

            using (IDbConnection connection = sqlConnection)
            {
                var mediaFormat = (await connection.QueryAsync<string>(sbData.ToString(), new { Id }))
                    .SingleOrDefault();
                return mediaFormat;
            }
        }

        public async Task<string> GetFromDataBase(string fileName)
        {
            sbData.Clear();
            sbData.Append("SELECT FilePath FROM Media ");
            sbData.Append("WHERE FilePath = @fileName");
            using (IDbConnection connection = sqlConnection)
            {
                string sPath = (await connection.QueryAsync<string>(sbData.ToString(), new { fileName }))
                    .FirstOrDefault();
                return sPath;
            }
        }

        public async Task<int> GetLastId_Media()
        {
            sbData.Append("SELECT Id FROM Media ORDER BY Id DESCENDING LIMIT 1");
            using (IDbConnection connection = sqlConnection)
            {
                int iID = (await connection.QuerySingleAsync(sbData.ToString()));
                return iID;
            }
        }

        public async Task<MediaVM> GetMediaByID(int FileId)
        {
            sbData.Clear();
            sbData.Append("SELECT M.*, c.Descricao Genero, f.Descricao Formato, ");
            sbData.Append(GetDurationQueryString());
            sbData.Append(GetSizeQueryString());
            sbData.Append("FROM Media M ");
            sbData.Append("INNER JOIN Categoria c ON M.IdGenero = c.Id ");
            sbData.Append("INNER JOIN Formato_Media f ON M.IdFormato_Media = f.Id ");
            sbData.Append("WHERE M.Id = @FileId");

            using (IDbConnection connection = sqlConnection)
            {
                var mediaFile = (await connection.QueryAsync<MediaVM>(sbData.ToString(), new { FileId }))
                    .SingleOrDefault();
                return mediaFile;
            }
        }

        public async Task<MediaVM> GetMediaByTitle(string FilePath)
        {
            sbData.Clear();
            sbData.Append("SELECT AnoEdicao, Autor ");
            sbData.Append("FROM Media ");
            sbData.Append("WHERE FilePath = @FilePath");
            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<MediaVM>(sbData.ToString(), new { FilePath }))
                .FirstOrDefault();
            }
        }

        public string GetMediaDuration(int Id)
        {

            StringBuilder sbDuracao = new();
            sbDuracao.Append("CASE WHEN  INSTR(Notas, 'Tempo') + 7, 11) > 0 THEN ");
            sbDuracao.Append("LTRIM(SUBSTR(Notas, INSTR(Notas, 'Tempo') + 7, 11)) ELSE ");
            sbDuracao.Append("'00;00;00' END AS Tempo, ");
            string sDuracao = sbDuracao.ToString();

            sbData.Clear();
            sbData.Append("SELECT substr(Notas, instr(Notas, 'Tempo') + 7, 11) AS Tempo ");
            sbData.Append("FROM Media ");
            sbData.Append($"WHERE Id = @Id ");

            using (IDbConnection connection = sqlConnection)
            {
                string sDuration = connection.Query<string>(sbData.ToString(), new { Id }).FirstOrDefault();
                return sDuration;
            }
        }

        public async Task<IEnumerable<MediaVM>> GetTitlesFromDataBase(string sSearchCriteria)
        {
            sbData.Clear();
            sbData.Append("SELECT * ");
            sbData.Append("FROM vwMedia ");
            sbData.Append($"WHERE {sSearchCriteria}");
            using (IDbConnection connection = sqlConnection)
            {
                var mediaFiles = await connection.QueryAsync<MediaVM>(sbData.ToString());
                return mediaFiles;
            }
        }

        public async Task<decimal> GetTotalEspacoOcupado_Media(string sFiltro)
        {
            sbData.Clear();
            sbData.Append("SELECT SUM( ");
            sbData.Append($"{GetSizeQueryString()} ) ");
            sbData.Append("FROM Media ");
            if (!string.IsNullOrEmpty(sFiltro))
                sbData.Append($"WHERE {sFiltro}");

            using (IDbConnection connection = sqlConnection)
            {
                decimal decTotal = (await connection.QueryAsync<decimal>(sbData.ToString()))
                    .FirstOrDefault();
                return decTotal;
            }
        }

        public async Task<IEnumerable<MediaVM>> ListarMedia_PorID(int IdCategoria)
        {
            sbData.Clear();
            sbData.Append("SELECT * FROM Media ");
            sbData.Append(" WHERE IdGenero = @IdCategoria ");

            using (IDbConnection connection = sqlConnection)
            {
                return await connection.QueryAsync<MediaVM>(sbData.ToString(), new { IdCategoria });
            }
        }

        public async Task<bool> MediaFileExists(string Titulo, string Autor)
        {
            sbData.Clear();
            sbData.Append("SELECT Id ");
            sbData.Append("FROM Media ");
            sbData.Append("WHERE Filename = @Titulo AND Autor = @Autor");
            using (IDbConnection connection = sqlConnection)
            {
                int iId = (await connection.QueryAsync<int>(sbData.ToString(), new { Titulo, Autor }))
                    .FirstOrDefault();
                return iId > 0;
            }
        }


        public async Task<int> GetMediaFormatByDescription(string Descricao)
        {
            sbData.Clear();
            sbData.Append("SELECT Id ");
            sbData.Append("FROM Formato_Media ");
            sbData.Append("WHERE Descricao = @FormatDescription");
            using (IDbConnection connection = sqlConnection)
            {
                int iId = (await connection.QueryAsync<int>(sbData.ToString(), new { Descricao }))
                    .FirstOrDefault();
                return iId;
            }
        }

        public async Task<int> GetMediaCategoryByDescription(string Descricao)
        {
            sbData.Clear();
            sbData.Append("SELECT Id ");
            sbData.Append("FROM Categoria ");
            sbData.Append("WHERE Descricao = @Descricao");
            using (IDbConnection connection = sqlConnection)
            {
                int iId = (await connection.QueryAsync<int>(sbData.ToString(), new { Descricao }))
                    .FirstOrDefault();
                return iId;
            }
        }

        public async Task<IEnumerable<VideoCategoryLocations>> GetVideoLocations(int idCategoria)
        {
            sbData.Clear();
            sbData.Append("SELECT M.Id, M.FileName, M.idGenero, M.FileUrl, C.Descricao Categoria ");
            sbData.Append("FROM[Publicacoes].[dbo].[Media] M ");
            sbData.Append("INNER JOIN[Publicacoes].[dbo].Categoria C ");
            sbData.Append("ON M.idGenero = C.Id ");
            sbData.Append("WHERE ");
            sbData.Append("IdGenero = @IdCategoria)");

            using (IDbConnection connection = sqlConnection)
            {
                var output = (await connection.QueryAsync<VideoCategoryLocations>(sbData.ToString(), new { IdGenero = idCategoria }));
                return output;
            }

        }

        public async Task<IEnumerable<VideoCategoryLocations>> GetMusicMedia(int idCategoria)
        {
            sbData.Clear();
            sbData.Append("SELECT M.Id, M.FileName Titulo, M.idGenero IdCategoria, ");
            sbData.Append("M.FileUrl, C.Descricao Categoria ");
            sbData.Append("FROM[Publicacoes].[dbo].[Media] M ");
            sbData.Append("INNER JOIN[Publicacoes].[dbo].Categoria C ");
            sbData.Append("ON M.idGenero = C.Id ");
            sbData.Append("WHERE IdGenero = @IdCategoria AND (FileUrl = '' OR FileUrl IS NULL) AND TipoMedia = 2");

            try
            {
                using (IDbConnection connection = sqlConnection)
                {
                    var output = await connection.QueryAsync<VideoCategoryLocations>(sbData.ToString());
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
            sbData.Clear();
            sbData.Append("SELECT M.Id, M.FileName Titulo, M.Autor, ");
            sbData.Append("M.idGenero IdCategoria, M.FileUrl, C.Descricao ");
            sbData.Append("Categoria, F.Id, F.Descricao ");
            sbData.Append("FROM[Publicacoes].[dbo].[Media] M ");
            sbData.Append("INNER JOIN[Publicacoes].[dbo].Categoria C ");
            sbData.Append("ON M.idGenero = C.Id ");
            sbData.Append("INNER JOIN Formato_Media F ");
            sbData.Append("ON M.IdFormato_Media = F.Id ");
            sbData.Append("WHERE(fileurl = '' OR fileurl is null) ");
            sbData.Append("AND(FileSource = '' OR FileSource is null)) ");

            try
            {
                using (IDbConnection connection = sqlConnection)
                {
                    var output = await connection.QueryAsync<VideoCategoryLocations>(sbData.ToString());
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
            DynamicParameters paramCollection = new();
            paramCollection.Add("@Id", Id);

            sbData.Clear();
            sbData.Append("SELECT M.Id, M.FileName Titulo, M.Autor, M.idGenero IdCategoria, M.CoverFile, M.FileUrl, C.Descricao Categoria ");
            sbData.Append("FROM[Publicacoes].[dbo].[Media] M ");
            sbData.Append("INNER JOIN[Publicacoes].[dbo].Categoria C ");
            sbData.Append("ON M.idGenero = C.Id ");
            sbData.Append("INNER JOIN Formato_Media F ");
            sbData.Append("ON M.IdFormato_Media = F.Id ");
            sbData.Append("WHERE M.Id = @Id");

            try
            {
                using (IDbConnection connection = sqlConnection)
                {
                    var output = await connection.QuerySingleAsync<VideoCategoryLocations>(
                        sbData.ToString(), param: paramCollection);
                    return output;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<IEnumerable<MediaVM>> GetAllMedia_Async(string userId, string role)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MediaVM>> GetAllMedia(string userId, string role)
        {
            throw new NotImplementedException();
        }
    }
}
