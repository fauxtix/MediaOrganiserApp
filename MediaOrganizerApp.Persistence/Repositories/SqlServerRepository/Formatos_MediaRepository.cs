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

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    public class Formatos_MediaRepository : SQLServerBaseRepository<Formato_Media>, IFormatos_MediaRepository
    {
        private readonly IPublicacoesRepository m_repoPubs;
        private readonly StringBuilder sbData = new();
        private readonly IDapperContext _context;

        public Formatos_MediaRepository(IPublicacoesRepository repoPubs, IDapperContext context)
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
            return await RecInUse_Async(Id);
        }

        public async Task<int> GetIdFormato(string sFormato_Media)
        {
            return await GetIdByDescription_Async(sFormato_Media);
        }

        public async Task<int> GetLastIdFormato()
        {
            return await GetLastId_Async();
        }

        public async Task<string> GetNomeFormato(int IdFormato_Media)
        {
            return await GetDescriptionById_Async(IdFormato_Media);
        }

        public async Task<List<Formato_Media>> Listar_Formato(string Descricao)
        {
            IEnumerable<Formato_Media> formatos = await ListByDescription_Async(Descricao);
            return formatos.ToList();
        }

        public async Task<List<MediaVM>> Media_CatalogadosByFormato(int IdFormato_Media)
        {
            List<MediaVM> mediaFiles;
            string sProcedName = "usp_Formato_Media_MediaCatalogados_ById";

                using (var connection = _context.CreateConnection())
            {
                mediaFiles = (await connection.QueryAsync<MediaVM>(sProcedName, new { IdFormato_Media },
                    commandType: CommandType.StoredProcedure))
                    .ToList();

                return mediaFiles;
            }
        }
        public async Task<List<MediaVM>> Media_Catalogados()
        {
            List<MediaVM> mediaFiles;
            string sProcedName = "usp_Formato_Media_MediaCatalogados";

                using (var connection = _context.CreateConnection())
            {
                mediaFiles = (await connection.QueryAsync<MediaVM>(sProcedName,
                    commandType: CommandType.StoredProcedure))
                    .ToList();

                return mediaFiles;
            }
        }
    }
}
