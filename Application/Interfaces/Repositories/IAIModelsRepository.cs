using MediaOrganizerApp.Domain.Entities;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Repositories;

public interface IAIModelsRepository : IBaseRepository<AiModel>
{
    Task<bool> AIModelExist(int Id);
    Task<int> GetModelId(string modelNameDescription);
    Task<string> GetModelName(int Id);
}
