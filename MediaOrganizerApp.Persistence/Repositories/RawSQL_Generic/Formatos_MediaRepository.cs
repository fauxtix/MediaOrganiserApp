using Dapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Repositories.RawSQLRepository
{
    public class Formatos_MediaRepository : BaseRepository<Formato_Media>, IFormatos_MediaRepository
    {
        private readonly IPublicacoesRepository m_repoPubs;
        private readonly StringBuilder sbData = new();
        private readonly IDbConnection sqlConnection;

        public Formatos_MediaRepository(IPublicacoesRepository repoPubs,
            IDbConnectionFactory dbFactory)
        {
            m_repoPubs = repoPubs ?? throw new ArgumentNullException(nameof(repoPubs));
            sqlConnection = dbFactory.CreateSqlConnection("SqlServerCon");
        }

        public async Task<bool> FormatoComLivros(int IdFormato)
        {
            List<PubsWithDescriptions> lstPubs = await m_repoPubs.Listar_IDFormato(IdFormato);
            if (lstPubs.Count > 0)
                return true;
            else
                return false;
        }

        public async Task<bool> FormatoExiste(int Id)
        {
            sbData.Clear();
            sbData.Append("SELECT COUNT(*) FROM Formato_Media ");
            sbData.Append(" WHERE Id = @Id");

            using (IDbConnection connection = sqlConnection)
            {
                int res = (await connection.QueryAsync<int>(sbData.ToString(), new { Id }))
                    .SingleOrDefault();
                return res > 0;
            }
        }

        public async Task<int> GetIdFormato(string sNome)
        {
            sbData.Clear();

            sbData.Append("SELECT Id FROM ");
            sbData.Append("Formato_Media ");
            sbData.Append("WHERE Descricao = @Nome");

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<int>(sbData.ToString(), new
                { Nome = sNome }))
                .SingleOrDefault();
            }
        }

        public async Task<int> GetLastIdFormato()
        {
            sbData.Clear();
            sbData.Append("SELECT Id FROM Formato_Media ");
            sbData.Append("ORDER BY Id DESC ");
            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<int>(sbData.ToString()))
                    .FirstOrDefault();
            }
        }

        public async Task<string> GetNomeFormato(int Id)
        {
            sbData.Clear();
            sbData.Append("SELECT Descricao FROM ");
            sbData.Append("Formato_Media ");
            sbData.Append("WHERE Id = @IdFormato");

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<string>(sbData.ToString(), new
                {
                    Id
                })).SingleOrDefault();
            }
        }

        public async Task<List<Autor>> Listar_Autor(string sDescricao)
        {
            sbData.Clear();
            sbData.Append("SELECT Autor.* FROM Autor ");
            sbData.Append("WHERE Descricao LIKE '%" + sDescricao + "%' ");
            sbData.Append("ORDER BY Descricao");

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<Autor>(sbData.ToString())).ToList();
            }
        }

        public async Task<List<Formato_Media>> Listar_Formato(string sDescricao)
        {
            sbData.Clear();
            sbData.Append("SELECT * FROM Formato_Media ");
            sbData.Append("WHERE Descricao LIKE '%" + sDescricao + "%' ");
            sbData.Append("ORDER BY Descricao");

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<Formato_Media>(sbData.ToString()))
                    .ToList();
            }
        }

        public async Task<List<MediaVM>> Media_CatalogadosByFormato(int IdFormato_Media)
        {
            List<MediaVM> mediaFiles;
            sbData.Clear();

            sbData.Append("SELECT fileName, Autor, C.Descricao AS Genero ");
            sbData.Append("FROM Media ");
            sbData.Append("INNER JOIN Categoria C ON Media.IdGenero = C.Id ");
            sbData.Append("WHERE idFormato_media = @IdFormato_Media  ");
            sbData.Append("ORDER BY FileName");

            using (IDbConnection connection = sqlConnection)
            {
                mediaFiles = (await connection.QueryAsync<MediaVM>(sbData.ToString(), new
                {
                    IdFormato_Media
                })).ToList();
                return mediaFiles;
            }
        }
        public async Task<List<MediaVM>> Media_Catalogados()
        {
            List<MediaVM> mediaFiles;
            sbData.Clear();

            sbData.Append("SELECT fileName, Autor, C.Descricao AS Genero ");
            sbData.Append("FROM Media ");
            sbData.Append("INNER JOIN Categoria C ON Media.IdGenero = C.Id ");
            sbData.Append("ORDER BY ORDER BY FileName");

            using (IDbConnection connection = sqlConnection)
            {
                mediaFiles = (await connection.QueryAsync<MediaVM>(sbData.ToString())).ToList();
                return mediaFiles;
            }
        }
    }
}
