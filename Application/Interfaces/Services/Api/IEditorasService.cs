using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface IEditorasService
    {
        Task<bool> CreateAuthor(PublisherVM publisherVM);
        Task<bool> DeleteAuthor(int id);
        Task<IEnumerable<PublisherVM>> GetAllAsync();
        Task<PublisherVM> GetAuthorsById(int id);
        Task<bool> UpdatePublisher(int id, PublisherVM publisherDTO);
        Task<List<PublisherVM>> Listar();
        Task<int> GetIdEditora(string description);
        IEnumerable<PublicacoesVM> PublicacoesCatalogadas(int CodAutor);
        Task<IEnumerable<PublisherVM>> EditorasSemLivros();
        Task<bool> EditoraComLivros(int id);
        string RegistoComErros(PublisherVM editora);
        Task<List<PublisherVM>> Listar_Editora(string Descricao);

    }
}