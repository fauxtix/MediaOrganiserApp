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
    public class FormatosRepository : BaseRepository<Formato>, IFormatosRepository
    {
        private readonly IPublicacoesRepository m_repoPubs;
        readonly StringBuilder sbData = new();
        private readonly IDbConnection sqlConnection;

        public FormatosRepository(IPublicacoesRepository repoPubs,
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
            sbData.Append("SELECT COUNT(*) FROM Formato ");
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
            sbData.Append("Formato ");
            sbData.Append("WHERE Descricao = @Nome");

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<int>(sbData.ToString(), new { Nome = sNome }))
                    .SingleOrDefault();
            }
        }

        public async Task<int> GetLastIdFormato()
        {
            sbData.Clear();
            sbData.Append("SELECT Id FROM Formato ");
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
            sbData.Append("Formato ");
            sbData.Append("WHERE Id = @IdFormato");

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<string>(sbData.ToString(), new { Id }))
                    .SingleOrDefault();
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
                return (await connection.QueryAsync<Autor>(sbData.ToString()))
                    .ToList();
            }
        }

        public async Task<List<Formato>> Listar_Formato(string sDescricao)
        {
            sbData.Clear();
            sbData.Append("SELECT Formato.* FROM Formato ");
            sbData.Append("WHERE Descricao LIKE '%" + sDescricao + "%' ");
            sbData.Append("ORDER BY Descricao");

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<Formato>(sbData.ToString()))
                    .ToList();
            }
        }

        public List<PublicacoesVM> PublicacoesCatalogadas(int IdFormato)
        {
            sbData.Clear();

            sbData.Append("SELECT e.Descricao, l.Titulo, l.DigitalPath, e.Descricao AS Editora, a.Descricao AS NomeAutor ");
            sbData.Append("from publicacao l ");
            sbData.Append("INNER JOIN Editora e ON e.id = l.IdEditora ");
            sbData.Append("INNER JOIN Autor a ON a.id = l.IdAutor ");

            using (IDbConnection connection = sqlConnection)
            {
                if (IdFormato > 0)
                {
                    sbData.Append("WHERE l.IdFormato = @IdFormato ");
                    sbData.Append("ORDER BY l.titulo");

                    return connection.Query<PublicacoesVM>(sbData.ToString(), new
                    {
                        IdFormato
                    })
                    .ToList();
                }
                else
                {
                    sbData.Append("ORDER BY l.titulo");
                    return  connection.Query<PublicacoesVM>(sbData.ToString())
                        .ToList();
                }
            }
        }
    }
}
