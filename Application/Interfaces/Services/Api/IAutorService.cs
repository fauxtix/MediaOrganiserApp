using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities.Author_Api_Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface IAutorService
    {
        Task<bool> CreateAuthor(AutorDTO authorVM);
        Task<bool> UpdateAuthor(int id, AutorDTO authorDTO);
        Task<bool> DeleteAuthor(int id);
        Task<IEnumerable<AutorDTO>> GetAllAsync();
        Task<AutorDTO> GetAuthorsById(int id);
        Task<int> GetFirstId();
        Task<int> GetLastId();
        Task<int> GetIdByDescription(string Description);
        Task<string> GetDescriptionById(int Id);
        Task<List<AutorDTO>> Listar();
        Task<bool> AutorExiste(string NomeAutor);
        Task<IEnumerable<AutorDTO>> AutoresSemLivros();
        Task<bool> AutorComLivros(int IdAutor);
        IEnumerable<PublicacoesVM> PublicacoesCatalogadas(int CodAutor);
        List<string> RegistoComErros(AutorDTO autor);

        Task<AuthorModel> GetAuthorInfo_Api(string nomeAutor);
        Task<(string bio, string photo)> GetAuthorDetails(string authorName);
        Task<string> TranslateWithMyMemoryAsync(string text, string sourceLang, string targetLang);

        //Task<IEnumerable<AutorDTO>> Query(string where = null);
        //Task<AutorDTO> Query_ById(int Id);


        //Task<IEnumerable<AutorDTO>> Query_Async(string where = null);
        //Task<AutorDTO> Query_ById_Async(int Id);

        //Task<int> GetIdByDescription_Async(string Description);
        //Task<string> GetDescriptionById_Async(int Id);


        //Task<bool> RecInUse(int Id);
        //bool RecInUse(string sDescricao);
        //bool TableHasData();

        //Task<IEnumerable<AutorDTO>> ListByDescription(string Description);
        //Task<IDataReader> ListByDescription_Reader(string Descricao);
        //Task <IEnumerable<AutorDTO>> Listar_ID(int Id);
        //Task<IDataReader> Listar_ID_DR(int Id);

        //Task<List<BO_Autores.Author>> GetAuthorsAndPubs_Async();
        //Task List<BO_Autores.Author> GetAuthors();
        //Task<Task<List<BO_Autores.Author>> GetAuthors_Async();
        //Task<List<BO_Autores.Author>> GetAuthorsAndPubs();
        //Task<List<BO_Autores.Author>> GetAuthorsWithoutPubs();
    }
}