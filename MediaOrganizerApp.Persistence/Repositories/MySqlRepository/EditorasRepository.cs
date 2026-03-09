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
    public class EditorasRepository : SQLServerBaseRepository<Editora>, IEditorasRepository
    {
        private readonly IPublicacoesRepository m_repoPublicacoes;
        private readonly IDbConnection sqlConnection;

        public EditorasRepository(IPublicacoesRepository repoPublicacoes, IDbConnectionFactory dbFactory)
        {
            m_repoPublicacoes = repoPublicacoes ?? throw new ArgumentNullException(nameof(repoPublicacoes));
            sqlConnection = dbFactory.CreateMySqlConnection("MySqlCon");
        }

        public async Task<bool> EditoraComLivros(int IdEditora)
        {
            return (await m_repoPublicacoes.Listar_IDEditora(IdEditora))
                .ToList().Count > 0;
        }

        public async Task<bool> EditoraExiste(int Id)
        {
            return await RecInUse_Async(Id);
        }

        public async Task<List<PublisherVM>> EditorasSemLivros()
        {
            return await  m_repoPublicacoes.EditorasSemLivros();
        }

        public async Task<int> GetIdEditora(string sEditora)
        {
            return await GetIdByDescription_Async(sEditora);
        }

        public async Task<int> GetLastIdEditora()
        {
            return await GetLastId_Async();
        }

        public async Task<string> GetDescricao(int IdEditora)
        {
            return await GetDescriptionById_Async(IdEditora);
        }

        public async Task<List<Editora>> Listar_Editora(string Descricao)
        {
            IEnumerable<Editora> editoras = await ListByDescription_Async(Descricao);
            return editoras.ToList();
        }

        public async Task<IDataReader> Listar_Editora_DR(string Descricao)
        {
            return await ListByDescription_Reader_Async(Descricao);
        }

        public List<PublicacoesVM> PublicacoesCatalogadas(int IdEditora)
        {
            string sProcedName = "usp_Editora_PublicacoesCatalogadas";
            using (IDbConnection connection = sqlConnection)
            {
                if (IdEditora > 0)
                {
                    sProcedName = "usp_Editora_PublicacoesCatalogadas_ByPublisherId";
                    return connection.Query<PublicacoesVM>(sProcedName, new { IdEditora },
                         commandType: CommandType.StoredProcedure)
                        .ToList();
                }
                else
                {
                    return connection.Query<PublicacoesVM>(sProcedName, 
                        commandType: CommandType.StoredProcedure)
                        .ToList();
                }
            }
        }

        public async Task<List<PublicacoesVM>> PublicacoesCatalogadas_Async(int IdEditora)
        {
            string sProcedName = "usp_Editora_PublicacoesCatalogadas";
            using (IDbConnection connection = sqlConnection)
            {
                if (IdEditora > 0)
                {
                    sProcedName = "usp_Editora_PublicacoesCatalogadas_ByPublisherId";
                    return (await connection.QueryAsync<PublicacoesVM>(sProcedName, new { IdEditora },
                         commandType: CommandType.StoredProcedure)).ToList();
                       
                }
                else
                {
                    return (await connection.QueryAsync<PublicacoesVM>(sProcedName,
                        commandType: CommandType.StoredProcedure))
                        .ToList();
                }
            }
        }
    }
}
