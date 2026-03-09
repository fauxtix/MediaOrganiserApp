using Dapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
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

namespace MediaOrganizerApp.Persistence.Repositories.MySqlRepository
{
    public class CategoriasRepository : SQLServerBaseRepository<Categoria>, ICategoriasRepository
    {
        private readonly IPublicacoesRepository repoPubs;
        private IDbConnection sqlConnection;


        public CategoriasRepository(IPublicacoesRepository pubsRepository, IDbConnectionFactory dbFactory)
        {
            repoPubs = pubsRepository ?? throw new ArgumentNullException(nameof(pubsRepository));
            sqlConnection = dbFactory.CreateMySqlConnection("MySqlCon");
        }

        public async Task<IEnumerable<CategoryVM>> GetCategorias_ExtendedAsync(int tipo) // 1 = publicações - 2 = Média
        {
            string sProcedName = "usp_Categoria_GetAll_Extended";
            DynamicParameters paramCollection = new();
            paramCollection.Add("@TipoCategoria", tipo);
            using (IDbConnection connection = ConnectionManager.GetConnection())
            {
                var result = await connection.QueryAsync<CategoryVM>(sProcedName, param: paramCollection,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }
        public async Task<bool> CategoriaComRegistos(int IdCategoria, bool m_CalledFromBooks = false)
        {
            List<PubsWithDescriptions> lstPubs = await repoPubs.Listar_IDCategoria(IdCategoria);
            if (lstPubs.Any())
                return true;
            else
                return false;
        }

        public async Task<bool> CategoriaExiste(int iCategoria)
        {
            return await RecInUse_Async(iCategoria);
        }

        public async Task<bool> CategoriaExiste(int tipo, string description)
        {
            return await RecInUse_ByDescription_Async(tipo, description);
        }

        public async Task<string> GeNomeGenero(int Id)
        {
            return await GetDescriptionById_Async(Id);
        }


        public async Task<IEnumerable<CategoryVM>> GetCategoriesWithPublications()
        {
            string sProcedName = "usp_Categoria_GetCategoriesWithPublications";
            using (IDbConnection connection = sqlConnection)
            {
                var result = await connection.QueryAsync<CategoryVM>(sProcedName,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }

        public async Task<IEnumerable<CategoryVM>> GetCategoriesWithMediaEntries()
        {
            string sProcedName = "usp_Categoria_GetCategoriesWithMediaEntries";
            using (IDbConnection connection = sqlConnection)
            {
                var result = await connection.QueryAsync<CategoryVM>(sProcedName,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }



        public async Task<int> GetFirstIdCategoria(bool CalledFromBooks)
        {
            string sProcedName = "usp_Categoria_GetFirstId";
            int TipoCategoria = 1;

            if (!CalledFromBooks)
                TipoCategoria = 2;

            using (IDbConnection connection = sqlConnection)
            {
                int iRet = (await connection.QueryAsync<int>(sProcedName, new { TipoCategoria },
                    commandType: CommandType.StoredProcedure))
                    .FirstOrDefault();
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
            string sProcedName = "usp_CategoriaList_ByType";
            int TipoCategoria = 1;

            if (!CalledFromBooks)
                TipoCategoria = 2;

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<Categoria>(sProcedName, new { TipoCategoria },
                    commandType: CommandType.StoredProcedure))
                    .ToList();
            }
        }

        public async Task<List<Categoria>> Listar_Categoria(string Descricao, int TipoCategoria)
        {
            string sProcedName = "usp_CategoriaList_ByTypeAndLikeDescription";

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<Categoria>(sProcedName, new { Descricao, TipoCategoria },
                    commandType: CommandType.StoredProcedure))
                    .ToList();
            }
        }

        public async Task<List<Categoria>> Listar_Categoria(int TipoCategoria)
        {
            string sProcedName = "usp_CategoriaList_ByType";

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<Categoria>(sProcedName, new { TipoCategoria },
                    commandType: CommandType.StoredProcedure))
                    .ToList();
            }
        }

        public async Task<IDataReader> Listar_Categoria_DR(int TipoCategoria)
        {
            string sProcedName = "usp_CategoriaList_ByType";

            var connection = ConnectionManager.GetConnection();

            return await connection.ExecuteReaderAsync(sProcedName, new { TipoCategoria },
                  commandType: CommandType.StoredProcedure);
        }

        public async Task<List<CategoryVM>> Listar_Categoria_List(string Descricao, bool CalledFromPubs)
        {
            string sProcedName = "usp_Categoria_List_Report";

            int TipoCategoria = 1;
            if (!CalledFromPubs)
                TipoCategoria = 2;

            using (IDbConnection connection = sqlConnection)
            {
                var output = (await connection.QueryAsync<CategoryVM>(sProcedName, new { Descricao, TipoCategoria },
                    commandType: CommandType.StoredProcedure))
                    .ToList();

                return output;
            }
        }

        public async Task<List<Categoria>> Listar_ID(int Id, bool CalledFromBooks)
        {
            string sProcedName = "usp_Categoria_ListByIdAndCategory";

            int TipoCategoria = 1;
            if (!CalledFromBooks)
                TipoCategoria = 2;

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<Categoria>(sProcedName, new
                {
                    Id,
                    TipoCategoria
                }, commandType: CommandType.StoredProcedure))
                    .ToList();
            }
        }

        public Task<List<CategoryVM>> ListByType(AppDefinitions.Query_TipoMedia tipoMedia)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IEnumerable<MediaVM>> MediaCatalogados(int genero)
        {
            string sProcedName = "usp_Media_MediaCatalogados";

            using (IDbConnection connection = sqlConnection)
            {
                if (genero > 0)
                {
                    sProcedName = "usp_Media_MediaCatalogados_ByCategoryId";
                    var output = (await connection.QueryAsync<MediaVM>(sProcedName, new { IdGenero = genero },
                        commandType: CommandType.StoredProcedure)).ToList();

                    return output;
                }
                else
                {
                    var output = (await connection.QueryAsync<MediaVM>(sProcedName,
                        commandType: CommandType.StoredProcedure))
                        .ToList();

                    return output;
                }
            }
        }

        public async Task<List<PublicacoesVM>> PublicacoesCatalogadas(int IdCategoria)
        {
            string sProcedName = "usp_Categoria_PublicacoesCatalogadas";
            using (IDbConnection connection = sqlConnection)
            {
                if (IdCategoria > 0)
                {
                    sProcedName = "usp_Categoria_PublicacoesCatalogadas_ByCategoryId";

                    return (await connection.QueryAsync<PublicacoesVM>(sProcedName, new
                    {
                        IdCategoria
                    }, commandType: CommandType.StoredProcedure))
                        .ToList();
                }
                else
                {
                    return (await connection.QueryAsync<PublicacoesVM>(sProcedName, commandType: CommandType.StoredProcedure))
                        .ToList();
                }
            }
        }

        public async Task<IEnumerable<CategoryVM>> GetCategoriasAsync(int tipo)
        {
            string sProcedName = "usp_Categoria_GetAll_Ordered";
            DynamicParameters paramCollection = new();
            paramCollection.Add("@TipoCategoria", tipo);
            using (IDbConnection connection = ConnectionManager.GetConnection())
            {
                var result = await connection.QueryAsync<CategoryVM>(sProcedName, param: paramCollection,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }
    }
}
