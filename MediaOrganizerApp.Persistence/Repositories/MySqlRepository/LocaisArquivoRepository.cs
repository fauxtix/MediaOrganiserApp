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

namespace MediaOrganizerApp.Persistence.Repositories.MySqlRepository
{
    public class LocaisArquivoRepository : SQLServerBaseRepository<LocalArquivo>, ILocalArquivoRepository
    {
        private readonly IPublicacoesRepository m_repoPubs;
        private IDbConnection sqlConnection;

        public LocaisArquivoRepository(IPublicacoesRepository repoPubs, IDbConnectionFactory dbFactory)
        {
            m_repoPubs = repoPubs ?? throw new ArgumentNullException(nameof(repoPubs));
            sqlConnection = dbFactory.CreateMySqlConnection("MySqlCon");
        }

        public async Task<IEnumerable<LocalArquivo>> GetAllAsync()
        {
            string sProcedName = "usp_LocalArquivo_GetAll";

            using (IDbConnection connection = sqlConnection)
            {
                var locais = await connection.QueryAsync<LocalArquivo>(sProcedName,
                    commandType: CommandType.StoredProcedure);
                return locais;
            }
        }

        public async Task<int> GetIdLocalArquivo(string sNome)
        {
            return await GetIdByDescription_Async(sNome);
        }

        public async Task<int> GetLastIdLocalArquivo()
        {
            return await GetLastId_Async();
        }

        public async Task<string> GetNomeLocalArquivo(int Id)
        {
            return await GetDescriptionById_Async(Id);
        }

        public async Task<List<LocalArquivo>> Listar_LocalArquivo(string Descricao)
        {
            IEnumerable<LocalArquivo> locais = await ListByDescription_Async(Descricao);
            return locais.ToList();
        }

        public async Task<bool> LocalArquivoComLivros(int IdLocalArquivo)
        {
            List<PubsWithDescriptions> lstPubs =await  m_repoPubs.Listar_IDLocalArquivo(IdLocalArquivo);
            return lstPubs.Count > 0;
        }

        public async Task<bool> LocalArquivoExiste(int Id)
        {
            return await RecInUse_Async(Id);
        }

        public async Task<List<PublicacoesVM>> PublicacoesCatalogadas(int IdLocalArquivo)
        {
            List<PublicacoesVM> publicacoes;
            string sProcedName = "usp_LocalArquivo_PublicacoesCatalogadas_ById";

            using (IDbConnection connection = sqlConnection)
            {
                publicacoes = (await connection.QueryAsync<PublicacoesVM>(sProcedName, new { IdLocalArquivo },
                    commandType: CommandType.StoredProcedure))
                    .ToList();

                return publicacoes;
            }
        }
    }
}
