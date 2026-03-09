using MediaOrganizerApp.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface ILocaisArquivoService
    {
        Task<bool> CreateStoragePlace(LocalArquivoVM storagePlaceVM);
        Task<bool> DeleteStoragePlace(int id);
        Task<IEnumerable<LocalArquivoVM>> GetAllAsync();
        Task<LocalArquivoVM> GetStoragePlaceById(int id);
        Task<bool> UpdateStoragePlace(int id, LocalArquivoVM storagePlaceVM);
        Task<List<LocalArquivoVM>> Listar();
        Task<List<LocalArquivoVM>> Listar_LocalArquivo(string descricao);
        Task<int> GetIdLocalArquivo(string description);
        IEnumerable<PublicacoesVM> PublicacoesCatalogadas(int CodAutor);
        string RegistoComErros(LocalArquivoVM local);

    }
}