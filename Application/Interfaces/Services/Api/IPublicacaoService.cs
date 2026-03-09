using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface IPublicacaoService
    {
        Task<bool> DeletePubAsync(int id);
        Task<IEnumerable<PubsWithDescriptions>> GetAllAsync(string userId, string userRole);
        Task<IEnumerable<ImageCategoryLocations>> GetImageLocations(int idCategoria);
        Task<PubsWithDescriptions> GetByIdAsync_Extended(int id);
        Task<bool> InsertPubAsync(PubsWithDescriptions publicationDTO);
        Task AtualizaTituloOriginal(int ID, string sTituloOriginal);
        Task Atualiza_CapaPublicacao(int Pub_ID, string sImageCoverPath);
        Task Atualiza_Publicacao(int id, PubsWithDescriptions pub);
        Task<List<AutorDTO>> AutoresSemLivros();
        Task<bool> AutorComLivros(int IdAutor);
        Task<List<PublisherVM>> EditorasSemLivros();
        Task<int> GetFirstIdLivro();

        Task<int> GetPubFormat(int pubId);

        Task<PubsWithDescriptions> Query_ById_Extended(int Id); // new

        Task<IEnumerable<PubsWithDescriptions>> Query_Async_Extended(string where); // new
        Task<List<PubsWithDescriptions>> GetID_ByCapa(string Capa);
        Task<int> GetID_ByPath(string sTitulo);
        Task<int> GetLastIdLivro();
        Task<string> GetLastTitle();
        Task<string> GetPubUserName(int iUserID);
        Task<List<PubsWithDescriptions>> GetPubsByTitle(string sTitle);
        Task<bool> IDLivroExiste_BD(int ID_LIvro);
        Task<List<PubsWithDescriptions>> Listar(Agrupamento grupo);
        Task<IEnumerable<PubsWithDescriptions>> Listar_Duplicados();

        Task<PubsWithDescriptions> Listar_ID_Extended(int Id);

        Task<List<PubsWithDescriptions>> Listar_IDAutor(int IdAutor);
        Task<List<PubsWithDescriptions>> Listar_IDCategoria(int IdCategoria);
        Task<List<PubsWithDescriptions>> Listar_IDEditora(int IdEditora);
        Task<List<PubsWithDescriptions>> Listar_IDFormato(int IdFormato);
        Task<List<PubsWithDescriptions>> Listar_IDLocalArquivo(int IdLocalArquivo);
        Task<List<PubsWithDescriptions>> Listar_PalavraTitulo(string sWhereParam);

        Task<List<PubsWithDescriptions>> Listar_AnoEdicao(int parInt);

        Task<List<PubsWithDescriptions>> Listar_Lidos(bool Lido);
        Task<List<PubsWithDescriptions>> Listar_Pagos(bool Pago);
        Task<List<PubsWithDescriptions>> Listar_SemCapa();
        Task<List<PubsWithDescriptions>> Listar_SemPaginas();

        Task<List<PubsWithDescriptions>> Listar_Titulo(string Titulo);
        Task<List<PubsWithDescriptions>> Listar_UserID(int iUserID);

        Task<bool> TituloExiste_BD(string sTituloOriginal);

        Task<IEnumerable<BooksViewModel>> GetAllPubs_Grid_Async();
        List<string> RegistoComErros(PubsWithDescriptions publicacao);
        Task<bool> UpdatePubsFromContextMenu(string sUpdateQuery);
        Task<List<FileCheckResult>> PubsNotFoundOnDisk(string webRoot);
        Task UpdatePubAsync(int id, PubsWithDescriptions publicationDTO);
        Task<bool> StorePdfNotDeleted(PdfsAndPngNotDeleted fileName);
    }
}