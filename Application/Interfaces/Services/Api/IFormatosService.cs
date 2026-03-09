using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface IFormatosService
    {
        Task<bool> CreatePublicationFormat(FormatoPublicacaoVM formatoPublicacaoVM);
        Task<bool> DeletePublicationFormat(int id);
        Task<IEnumerable<FormatoPublicacaoVM>> GetAllAsync();
        Task<FormatoPublicacaoVM> GetFormatosPublicacoesById(int id);
        Task<bool> UpdatePublicationFormat(int id, FormatoPublicacaoVM publicationFormatDTO);
        Task<List<FormatoPublicacaoVM>> Listar();
        Task<List<FormatoPublicacaoVM>> Listar_Formato(string sDescricao);
        Task<int> GetIdFormato(string description);
        IEnumerable<PublicacoesVM> PublicacoesCatalogadas(int CodAutor);
        string RegistoComErros(FormatoPublicacaoVM formato);

    }
}