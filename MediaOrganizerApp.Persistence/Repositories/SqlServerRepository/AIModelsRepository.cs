using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    public class AIModelsRepository : SQLServerBaseRepository<AiModel>, IAIModelsRepository
    {
        private readonly IDapperContext _context;
        public AIModelsRepository(IDapperContext context)
        {
            _context = context;
        }

        public async Task<int> GetModelId(string modelNameDescription)
        {
            return await GetIdByDescription_Async(modelNameDescription);
        }

        public async Task<string> GetModelName(int Id)
        {
            return await GetDescriptionById_Async(Id);
        }

        public async Task<bool> AIModelExist(int Id)
        {
            return await RecInUse_Async(Id);
        }

    }
}
