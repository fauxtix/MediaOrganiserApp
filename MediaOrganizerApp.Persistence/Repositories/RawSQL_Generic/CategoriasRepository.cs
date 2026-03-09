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
    public class CategoriasRepository : BaseRepository<Categoria>, ICategoriasRepository
    {
        readonly IPublicacoesRepository repoPubs;
        readonly StringBuilder sbData = new();
        private readonly IDbConnection sqlConnection;

        public CategoriasRepository(IPublicacoesRepository pubsRepository,
            IDbConnectionFactory dbFactory)
        {
            repoPubs = pubsRepository ?? throw new ArgumentNullException(nameof(pubsRepository));
            sqlConnection = dbFactory.CreateSqlConnection("SqlServerCon");
        }

        public async Task<bool> CategoriaComRegistos(int IdCategoria, bool m_CalledFromBooks = false)  
        {
            List<PubsWithDescriptions> lstPubs = await repoPubs.Listar_IDCategoria(IdCategoria);
            if (lstPubs.Count > 0)
                return true;
            else
                return false;
        }

        public async Task<bool> CategoriaExiste(int tipoCategoria)
        {
            return await RecInUse_Async(tipoCategoria);
        }
        public async Task<bool> CategoriaExiste(int tipo, string description)
        {
            return await RecInUse_ByDescription_Async(tipo, description);
        }

        public async Task<string> GeNomeGenero(int Id)
        {
            return await GetDescriptionById_Async(Id);
        }

        public Task<IEnumerable<CategoryVM>> GetCategoriasAsync(int tipo)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<CategoryVM>> GetCategorias_ExtendedAsync(int tipo)
        {
            sbData.Clear();
            sbData.Append("SELECT C.Id, C.Descricao, T.Descricao DescricaoTipoCategoria ");
            sbData.Append("FROM[Publicacoes].[dbo].[Categoria] C ");
            sbData.Append("INNER JOIN TipoCategoria T ");
            sbData.Append("ON C.TipoCategoria = T.Id ");
            sbData.Append("WHERE TipoCategoria = @tipo ");
            sbData.Append("ORDER BY T.Descricao");

            using (IDbConnection connection = sqlConnection)
            {
                var output = await connection.QueryAsync<CategoryVM>(sbData.ToString(), new { TipoCategoria = tipo });
                return output;
            }
        }

        public async Task<IEnumerable<CategoryVM>> GetCategoriesWithMediaEntries()
        {
            sbData.Clear();
            sbData.Append("SELECT C.id, c.Descricao ");
            sbData.Append("FROM Categoria C ");
            sbData.Append("WHERE TipoCategoria = 2 ");
            sbData.Append("AND EXISTS( ");
            sbData.Append("SELECT * FROM Media WHERE ");
            sbData.Append("IdGenero = C.Id ");
            sbData.Append("ORDER BY C.Descricao");
            using (IDbConnection connection = sqlConnection)
            {
                return await connection.QueryAsync<CategoryVM>(sbData.ToString());
            }
        }

        public async Task<IEnumerable<CategoryVM>> GetCategoriesWithPublications()
        {
            sbData.Clear();
            sbData.Append("SELECT C.id, c.Descricao ");
            sbData.Append("FROM Categoria C ");
            sbData.Append("WHERE TipoCategoria = 1 ");
            sbData.Append("AND EXISTS( ");
            sbData.Append("SELECT * from publicacao WHERE ");
            sbData.Append("IdCategoria = C.Id ");
            sbData.Append("ORDER BY C.Descricao");
            using (IDbConnection connection = sqlConnection)
            {
                return await connection.QueryAsync<CategoryVM>(sbData.ToString());
            }
        }

        public async Task<int> GetFirstIdCategoria(bool CalledFromBooks)
        {
            int iTipoCategoria = 1;
            if (!CalledFromBooks)
                iTipoCategoria = 2;

            sbData.Clear();
            sbData.Append("SELECT Id FROM Categoria ");
            sbData.Append("WHERE TipoCategoria = @TipoCategoria ");
            sbData.Append("ORDER BY Id ASC ");
            using (IDbConnection connection = sqlConnection)
            {
                int iRet = (await connection.QueryAsync<int>(sbData.ToString(), new { TipoCategoria = iTipoCategoria }))
                    .First();
                return iRet;
            }
        }

        public async Task<int> GetIdCategoria(string Descricao)
        {
            return await GetIdByDescription_Async(Descricao);
        }

        public async Task<int> GetLastIdCategoria()
        {
            return await GetLastId_Async();
        }

        public async Task<List<Categoria>> Listar(bool CalledFromBooks)
        {
            int iTipoCategoria = 1;
            if (!CalledFromBooks)
                iTipoCategoria = 2;

            sbData.Clear();
            sbData.Append("SELECT Categoria.* ");
            sbData.Append("FROM Categoria ");
            sbData.Append("WHERE TipoCategoria = @TipoCategoria ");
            sbData.Append("ORDER BY Descricao");

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<Categoria>(sbData.ToString(), new { TipoCategoria = iTipoCategoria }))
                    .ToList();
            }
        }

        public async Task<List<Categoria>> Listar_Categoria(string sDescricao, int iTipoCat)
        {
            sbData.Clear();
            sbData.Append("SELECT c.*, t.Descricao AS DescricaoTipoCategoria ");
            sbData.Append("FROM Categoria c INNER JOIN TipoCategoria t ");
            sbData.Append("ON c.TipoCategoria = t.Id ");
            sbData.AppendFormat("WHERE c.Descricao LIKE '%{0}%' AND c.TipoCategoria = @TipoCategoria ", sDescricao);
            sbData.Append("ORDER BY Descricao");

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<Categoria>(sbData.ToString(), new { TipoCategoria = iTipoCat }))
                    .ToList();
            }
        }

        public async Task<IDataReader> Listar_Categoria_DR(int iTipoCat)
        {
            sbData.Clear();
            sbData.Append("SELECT Categoria.* FROM Categoria WHERE ");
            sbData.Append("TipoCategoria = @TipoCategoria ");
            sbData.Append("ORDER BY Descricao");

            var connection = sqlConnection;

            return await connection.ExecuteReaderAsync(sbData.ToString(), new { TipoCategoria = iTipoCat });

        }

        public async Task<List<CategoryVM>> Listar_Categoria_List(string sDescricao, bool CalledFromPubs)
        {
            int iTipoCategoria = 1;
            if (!CalledFromPubs)
                iTipoCategoria = 2;

            sbData.Clear();
            sbData.Append("SELECT c.Id, c.Descricao, t.Descricao AS DescricaoTipoCategoria ");
            sbData.Append("FROM Categoria c INNER JOIN TipoCategoria t ");
            sbData.Append("ON c.TipoCategoria = t.Id ");
            sbData.AppendFormat("WHERE c.Descricao LIKE '%{0}%' AND ", sDescricao);
            sbData.Append("TipoCategoria = @iTipoCategoria ");
            sbData.Append("ORDER BY c.Descricao");

            using (IDbConnection connection = sqlConnection)
            {
                var output = (await connection.QueryAsync<CategoryVM>(sbData.ToString(), new { iTipoCategoria }))
                    .ToList();

                return output;
            }
        }

        public async Task<List<Categoria>> Listar_ID(int Id, bool CalledFromBooks)
        {
            int iTipoCategoria = 1;
            if (!CalledFromBooks)
                iTipoCategoria = 2;

            sbData.Clear();
            sbData.Append("SELECT Categoria.* ");
            sbData.Append("FROM Categoria ");
            sbData.Append("WHERE Id = @Id AND TipoCategoria = @TipoCategoria");

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<Categoria>(sbData.ToString(), new
                {
                    Id,
                    TipoCategoria = iTipoCategoria
                }))
                .ToList();
            }
        }

        public async Task<IEnumerable<MediaVM>> MediaCatalogados(int IdGenero)
        {
            sbData.Clear();

            sbData.Append("SELECT filename AS Titulo, Autor AS NomeAutor ");
            sbData.Append("FROM Media ");

            using (IDbConnection connection = sqlConnection)
            {
                if (IdGenero > 0)
                {
                    sbData.Append("WHERE IdGenero = @IdGenero ");
                    sbData.Append("ORDER BY filename");

                    var output = (await connection.QueryAsync<MediaVM>(sbData.ToString(), new
                    {
                        IdGenero
                    })).ToList();

                    return output;
                }
                else
                {
                    sbData.Append("ORDER BY filename");

                    return (await connection.QueryAsync<MediaVM>(sbData.ToString()))
                        .ToList();
                }
            }
        }

        public async Task<List<PublicacoesVM>> PublicacoesCatalogadas(int IdCategoria)
        {
            sbData.Clear();

            sbData.Append("SELECT e.Descricao, l.Titulo, l.DigitalPath, a.Descricao AS NomeAutor ");
            sbData.Append("from publicacao l ");
            sbData.Append("INNER JOIN Editora e ON l.IdEditora = e.Id ");
            sbData.Append("INNER JOIN Autor a ON l.IdAutor = a.Id ");

            using (IDbConnection connection = sqlConnection)
            {
                if (IdCategoria > 0)
                {
                    sbData.Append("WHERE l.IdCategoria = @IdCategoria ");
                    sbData.Append("ORDER BY l.titulo");

                    return (await connection.QueryAsync<PublicacoesVM>(sbData.ToString(), new
                    {
                        IdCategoria
                    }))
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
