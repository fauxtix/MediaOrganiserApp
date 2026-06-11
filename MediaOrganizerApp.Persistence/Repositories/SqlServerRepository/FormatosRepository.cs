using Dapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    public class FormatosRepository : SQLServerBaseRepository<Formato>, IFormatosRepository
    {
        private readonly IPublicacoesRepository m_repoPubs;
        private readonly IDapperContext _context;

        public FormatosRepository(IPublicacoesRepository repoPubs, IDapperContext context)
        {
            m_repoPubs = repoPubs ?? throw new ArgumentNullException(nameof(repoPubs));
            _context = context;
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
            return  await RecInUse_Async(Id);
        }

        public async Task<int> GetIdFormato(string sFormato)
        {
            return await GetIdByDescription_Async(sFormato);
        }

        public async Task<int> GetLastIdFormato()
        {
            return await GetLastId_Async();
        }

        public async Task<string> GetNomeFormato(int Id)
        {
            return await GetDescriptionById_Async(Id);
        }

        public async Task<List<Formato>> Listar_Formato(string Descricao)
        {
            IEnumerable<Formato> formatos = await ListByDescription_Async(Descricao);
            return formatos.ToList();
        }

        public List<PublicacoesVM> PublicacoesCatalogadas(int IdFormato)
        {
            List<PublicacoesVM> publicacoes;
            string sProcedName = "usp_Formato_PublicacoesCatalogadas_ById";

                using (var connection = _context.CreateConnection())
            {
                publicacoes = connection.Query<PublicacoesVM>(sProcedName, new { IdFormato },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                return publicacoes;
            }
        }
    }
}
