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
    public class LocaisArquivoRepository : BaseRepository<LocalArquivo>, ILocalArquivoRepository
    {
        private readonly IPublicacoesRepository m_repoPubs;
        readonly StringBuilder sbData = new();
        private readonly IDbConnection sqlConnection;

        public LocaisArquivoRepository(IPublicacoesRepository repoPubs,
            IDbConnectionFactory dbFactory)
        {
            m_repoPubs = repoPubs ?? throw new ArgumentNullException(nameof(repoPubs));
            sqlConnection = dbFactory.CreateSqlConnection("SqlServerCon");
        }

        public async Task<int> GetIdLocalArquivo(string sNome)
        {
            sbData.Clear();
            sbData.Append("SELECT Id FROM ");
            sbData.Append("LocalArquivo ");
            sbData.Append("WHERE Descricao = @Nome");

            using (IDbConnection connection = sqlConnection)
            {
                int iRet = (await connection.QueryAsync<int>(sbData.ToString(), new { Nome = sNome }))
                    .SingleOrDefault();
                return iRet;
            }
        }

        public async Task<int> GetLastIdLocalArquivo()
        {
            sbData.Clear();

            sbData.Append("SELECT Id FROM LocalArquivo ");
            sbData.Append("ORDER BY Id DESC ");

            using (IDbConnection connection = sqlConnection)
            {
                int iRet = (await connection.QueryAsync<int>(sbData.ToString()))
                    .FirstOrDefault();
                return iRet;
            }

        }

        public async Task<IEnumerable<LocalArquivo>> GetAllAsync()
        {
            sbData.Clear();
            sbData.Append("SELECT Id, Descricao ");
            sbData.Append("FROM[Publicacoes].[dbo].[LocalArquivo]");

            using (IDbConnection connection = sqlConnection)
            {
                return await connection.QueryAsync<LocalArquivo>(sbData.ToString());
            }
        }
        public async Task<string> GetNomeLocalArquivo(int Id)
        {
            sbData.Clear();

            sbData.Append("SELECT Descricao FROM ");
            sbData.Append("LocalArquivo ");
            sbData.Append("WHERE Id = @Id");

            using (IDbConnection connection = sqlConnection)
            {
                string sRet = (await connection.QueryAsync<string>(sbData.ToString(), new { Id }))
                    .SingleOrDefault();
                return sRet;
            }

        }

        public async Task<List<LocalArquivo>> Listar_LocalArquivo(string sDescricao)
        {
            sbData.Clear();
            sbData.Append("SELECT * FROM LocalArquivo ");
            sbData.Append($"WHERE Descricao LIKE '%{ sDescricao }%' ");
            sbData.Append("ORDER BY Descricao");

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<LocalArquivo>(sbData.ToString())).ToList();
            }
        }

        public async Task<bool> LocalArquivoComLivros(int IdLocalArquivo)
        {
            List<PubsWithDescriptions> lstPubs = await m_repoPubs.Listar_IDLocalArquivo(IdLocalArquivo);
            return lstPubs.Count > 0;
        }

        public async Task<bool> LocalArquivoExiste(int Id)
        {
            sbData.Clear();
            sbData.Append("SELECT COUNT(*) FROM LocalArquivo ");
            sbData.Append("WHERE Id = @Id");

            using (IDbConnection connection = sqlConnection)
            {
                return  (await connection.QueryAsync<bool>(sbData.ToString(), new
                {
                    Id
                })).SingleOrDefault();
            }
        }

        public async Task<List<PublicacoesVM>> PublicacoesCatalogadas(int IdLocalArquivo)
        {
            sbData.Clear();

            sbData.Append("SELECT e.Descricao, l.Titulo, l.DigitalPath, a.Descricao AS NomeAutor ");
            sbData.Append("from publicacao l ");
            sbData.Append("INNER JOIN Editora e ON l.IdEditora = e.Id ");
            sbData.Append("INNER JOIN Autor a ON l.IdAutor = a.Id ");

            using (IDbConnection connection = sqlConnection)
            {
                if (IdLocalArquivo > 0)
                {
                    sbData.Append("WHERE l.idLocalArquivo = @IdLocalArquivo ");
                    sbData.Append("ORDER BY l.titulo");

                    return  (await connection.QueryAsync<PublicacoesVM>(sbData.ToString(), new
                    {
                        IdLocalArquivo
                    })).ToList();
                }
                else
                {
                    sbData.Append("ORDER BY l.titulo");
                    return  (await connection.QueryAsync<PublicacoesVM>(sbData.ToString())).ToList();
                }
            }
        }
    }
}
