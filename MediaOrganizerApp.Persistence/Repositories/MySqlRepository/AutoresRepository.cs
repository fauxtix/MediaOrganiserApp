using Dapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Repositories.MySqlRepository
{
    public class AutoresRepository : SQLServerBaseRepository<Autor>, IAutoresRepository
    {
        private readonly IPublicacoesRepository repoPubs;
        private readonly IDbConnection sqlConnection;

        public AutoresRepository(IPublicacoesRepository pubsRepository, 
            IDbConnectionFactory dbFactory)
        {
            repoPubs = pubsRepository ?? throw new ArgumentNullException(nameof(pubsRepository));
            sqlConnection = dbFactory.CreateMySqlConnection("MySqlCon");
        }

        public async Task<bool> AutorExiste(string NomeAutor)
        {
            string sProcName = "usp_AutorExiste";

            using (IDbConnection connection = sqlConnection)
            {
                int iRet = (await connection.QueryAsync<int>(sProcName, new { NomeAutor }, commandType: CommandType.StoredProcedure))
                    .FirstOrDefault();
                return iRet > 0;
            }
        }

        public async Task<AutorDTO> GetAuthorById_Async(int Id)
        {
            string uSP = "usp_Autor_GetSingle";

            using (IDbConnection connection = sqlConnection)
            {
                var result = await connection.QueryAsync<AutorDTO>(uSP, new { Id }, commandType: CommandType.StoredProcedure);

                return result.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<AutorDTO>> GetAutores_Extended_Async()
        {
            string sProcName = "usp_Autor_GetAll_Extended";
            using (IDbConnection connection = sqlConnection)
            {
                var autores = await connection.QueryAsync<AutorDTO>(sProcName, commandType: CommandType.StoredProcedure);
                return autores;
            }
        }
        public IEnumerable<PublicacoesVM> PublicacoesCatalogadas(int Id)
        {
            string sProcName;

            using (IDbConnection connection = sqlConnection)
            {
                if (Id > 0)
                {
                    sProcName = "usp_Autor_PublicacoesCatalogadas_ByAuthorId";
                    var output = connection.Query<PublicacoesVM>(sProcName, new { Id }, commandType: CommandType.StoredProcedure);
                    return output;
                }
                else
                {
                    sProcName = "usp_Autor_PublicacoesCatalogadas"; // TODO - Não utilizada?
                    var output = connection.Query<PublicacoesVM>(sProcName, commandType: CommandType.StoredProcedure);
                    return output;
                }
            }
        }

        public async Task<IEnumerable<PublicacoesVM>> PublicacoesCatalogadas_Async(int Id)
        {
            string sProcName;

            using (IDbConnection connection = sqlConnection)
            {
                if (Id > 0)
                {
                    sProcName = "usp_Autor_PublicacoesCatalogadas_ByAuthorId";
                    return await connection.QueryAsync<PublicacoesVM>(sProcName, new { Id }, commandType: CommandType.StoredProcedure);
                }
                else
                {
                    sProcName = "usp_Autor_PublicacoesCatalogadas";
                    return await connection.QueryAsync<PublicacoesVM>(sProcName, commandType: CommandType.StoredProcedure);
                }
            }
        }

        public async Task<IEnumerable<AutorDTO>> AutoresSemLivros()
        {
            return await repoPubs.AutoresSemLivros();
        }
        public async Task<bool> AutorComLivros(int IdAutor)
        {
            return await repoPubs.AutorComLivros(IdAutor);
        }
    }
}
