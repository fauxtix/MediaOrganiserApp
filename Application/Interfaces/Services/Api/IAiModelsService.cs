using MediaOrganizerApp.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface IAiModelsService
    {
        Task<bool> AIModelExist(int Id);
        Task<int> GetModelId(string modelNameDescription);
        Task<string> GetModelName(int Id);
        Task<IEnumerable<AIModelVM>> GetAllAsync();
        string RegistoComErros(AIModelVM aiModel);
        Task<bool> CreateAIModel(AIModelVM aiModel);
        Task<bool> DeleteModel(int id);
        Task<AIModelVM> GetModelById(int id);
        Task<bool> UpdateAIModel(int id, AIModelVM aiModel);
    }
}
