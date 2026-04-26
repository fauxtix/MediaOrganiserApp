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

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    public class AutoresRepository : SQLServerBaseRepository<Autor>, IAutoresRepository
    {
        private readonly IPublicacoesRepository repoPubs;
        private readonly IDapperContext _context;

        public AutoresRepository(IPublicacoesRepository pubsRepository,
            IDapperContext context)
        {
            repoPubs = pubsRepository ?? throw new ArgumentNullException(nameof(pubsRepository));
            _context = context;
        }

        public async Task<bool> AutorExiste(string NomeAutor)
        {
            string sProcName = "usp_AutorExiste";

                using (var connection = _context.CreateConnection())
            {
                int iRet = (await connection.QueryAsync<int>(sProcName, new { NomeAutor }, commandType: CommandType.StoredProcedure))
                    .FirstOrDefault();
                return iRet > 0;
            }
        }

        public async Task<AutorDTO> GetAuthorById_Async(int Id)
        {
            string uSP = "usp_Autor_GetSingle";

                using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryFirstOrDefaultAsync<AutorDTO>(uSP, 
                    new { Id }, 
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }

        public async Task<IEnumerable<AutorDTO>> GetAutores_Extended_Async()
        {
            string sProcName = "usp_Autor_GetAll_Extended";
                using (var connection = _context.CreateConnection())
            {
                var autores = await connection.QueryAsync<AutorDTO>(sProcName, commandType: CommandType.StoredProcedure);
                return autores;
            }
        }
        public IEnumerable<PublicacoesVM> PublicacoesCatalogadas(int Id)
        {
            string sProcName;

                using (var connection = _context.CreateConnection())
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

                using (var connection = _context.CreateConnection())
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

        public async Task UpdateAutorAsync(Autor entity)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();

            parameters.Add("@Id", entity.Id);
            parameters.Add("@Descricao", entity.Descricao);
            parameters.Add("@Sobre", entity.Sobre);
            parameters.Add("@ImagemAutor", entity.ImagemAutor);

            parameters.Add("@DataNascimento",
                entity.DataNascimento,
                DbType.DateTime2);

            await connection.ExecuteAsync(
                "usp_Autor_Update",
                parameters,
                commandType: CommandType.StoredProcedure);
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
