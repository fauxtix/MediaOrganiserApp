using MediaOrganizerApp.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface IFormatos_MediaService
    {
        Task<bool> CreateMediaFormat(FormatoMediaVM formatoPublicacaoVM);
        Task<bool> DeleteMediaFormat(int id);
        Task<IEnumerable<FormatoMediaVM>> GetAllAsync();
        Task<FormatoMediaVM> GetFormatosMediaById(int id);
        Task<bool> UpdateMediaFormat(int id, FormatoMediaVM formatoFormatDTO);
        Task<List<FormatoMediaVM>> Listar();
        Task<int> GetIdFormato(string description);
        Task<IEnumerable<PublicacoesVM>> PublicacoesCatalogadas(int id);
        string RegistoComErros(FormatoMediaVM formato);
        List<MediaVM> Media_CatalogadosByFormato(int CodFormato);
    }
}