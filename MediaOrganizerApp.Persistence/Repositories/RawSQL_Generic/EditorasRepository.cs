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
    public class EditorasRepository : BaseRepository<Editora>, IEditorasRepository
    {
        private readonly IPublicacoesRepository m_repoPublicacoes;
        readonly StringBuilder sbData = new();
        private readonly IDbConnection sqlConnection;

        public EditorasRepository(IPublicacoesRepository repoPublicacoes,
            IDbConnectionFactory dbFactory)
        {
            m_repoPublicacoes = repoPublicacoes ?? throw new ArgumentNullException(nameof(repoPublicacoes));
            sqlConnection = dbFactory.CreateSqlConnection("SqlServerCon");
        }

        public async Task<bool> EditoraExiste(int Id)
        {
            sbData.Clear();
            sbData.Append("SELECT COUNT(*) ");
            sbData.Append("FROM Editora ");
            sbData.Append("WHERE Id = @Id");

            using (IDbConnection connection = sqlConnection)
            {
                bool Exist = (await connection.QueryAsync<bool>(sbData.ToString(), new { Id }))
                    .SingleOrDefault();
                return Exist;
            }
        }

        public async Task<bool> EditoraComLivros(int IdEditora)
        {
            return (await m_repoPublicacoes.Listar_IDEditora(IdEditora)).ToList().Count > 0;
        }

        public async Task<List<PublisherVM>> EditorasSemLivros()
        {
            return await m_repoPublicacoes.EditorasSemLivros();
        }

        public async Task<int> GetIdEditora(string sNome)
        {
            sbData.Clear();
            sbData.Append("SELECT Id FROM ");
            sbData.Append("Editora ");
            sbData.Append("WHERE Descricao = @Nome");

            using (IDbConnection connection = sqlConnection)
            {
                int IdEditora = (await connection.QueryAsync<int>(sbData.ToString(), new { Nome = sNome }))
                    .SingleOrDefault();
                return IdEditora;
            }
        }

        public async Task<int> GetLastIdEditora()
        {
            sbData.Clear();
            sbData.Append("SELECT Id FROM Editora ");
            sbData.Append("ORDER BY Id DESC ");
            using (IDbConnection connection = sqlConnection)
            {
                int IdEditora = (await connection.QueryAsync<int>(sbData.ToString()))
                    .FirstOrDefault();
                return IdEditora;
            }
        }

        public async Task<string> GetDescricao(int Id)
        {
            sbData.Clear();

            sbData.Append("SELECT Descricao FROM ");
            sbData.Append("Editora ");
            sbData.Append("WHERE Id = @Id");
            using (IDbConnection connection = sqlConnection)
            {
                string sEditora = (await connection.QueryAsync<string>(sbData.ToString(), new
                {
                    Id
                })).SingleOrDefault();
                return sEditora;
            }
        }

        public async Task<List<Editora>> Listar_Editora(string Descricao)
        {
            sbData.Clear();
            sbData.Append("SELECT * ");
            sbData.Append("FROM Editora ");
            sbData.Append("WHERE Descricao LIKE '%" + Descricao + "%' ");
            sbData.Append("ORDER BY Descricao");

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<Editora>(sbData.ToString()))
                    .ToList();
            }
        }

        public async Task<IDataReader> Listar_Editora_DR(string Descricao)
        {
            sbData.Clear();
            sbData.Append("SELECT * ");
            sbData.Append("FROM Editora ");
            sbData.AppendFormat("WHERE Descricao LIKE '%{0}%' ", Descricao);
            sbData.Append("ORDER BY Descricao");

            var connection = ConnectionManager.GetConnection();

            return await connection.ExecuteReaderAsync(sbData.ToString());
        }

        public List<PublicacoesVM> PublicacoesCatalogadas(int CodEditora)
        {
            sbData.Clear();

            sbData.Append("SELECT e.Descricao, l.Titulo, l.DigitalPath, a.Descricao AS NomeAutor ");
            sbData.Append("from publicacao l ");
            sbData.Append("INNER JOIN Editora e ON l.IdEditora = e.Id ");
            sbData.Append("INNER JOIN Autor a ON l.IdAutor = a.Id ");

            using (IDbConnection connection = sqlConnection)
            {
                if (CodEditora > 0)
                {
                    sbData.Append("WHERE l.IdEditora = @IdEditora ");
                    sbData.Append("ORDER BY l.titulo");

                    return connection.Query<PublicacoesVM>(sbData.ToString(), new
                    {
                        IdEditora = CodEditora
                    })
                    .ToList();
                }
                else
                {
                    sbData.Append("ORDER BY l.titulo");
                    return connection.Query<PublicacoesVM>(sbData.ToString())
                        .ToList();
                }
            }
        }

        public async Task<List<PublicacoesVM>> PublicacoesCatalogadas_Async(int CodEditora)
        {
            sbData.Clear();

            sbData.Append("SELECT e.Descricao, l.Titulo, l.DigitalPath, a.Descricao AS NomeAutor ");
            sbData.Append("from publicacao l ");
            sbData.Append("INNER JOIN Editora e ON l.IdEditora = e.Id ");
            sbData.Append("INNER JOIN Autor a ON l.IdAutor = a.Id ");

            using (IDbConnection connection = sqlConnection)
            {
                if (CodEditora > 0)
                {
                    sbData.Append("WHERE l.IdEditora = @IdEditora ");
                    sbData.Append("ORDER BY l.titulo");

                    return (await connection.QueryAsync<PublicacoesVM>(sbData.ToString(), new { IdEditora = CodEditora }))
                        .ToList();
                }
                else
                {
                    sbData.Append("ORDER BY l.titulo");
                    return (await connection.QueryAsync<PublicacoesVM>(sbData.ToString()))
                        .ToList();
                }
            }
        }
    }
}
