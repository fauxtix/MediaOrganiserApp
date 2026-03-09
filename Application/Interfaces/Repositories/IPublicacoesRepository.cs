using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IPublicacoesRepository : IBaseRepository<Publicacao>
    {
        Task AtualizaTituloOriginal(int ID, string sTituloOriginal);
        Task<bool> Atualiza_CapaPublicacao(int Pub_ID, string sImageCoverPath);
        Task<bool> Atualiza_Publicacao(int Pub_ID, string sPubPath, string sImageCoverPath, int idCategoria);
        Task<List<AutorDTO>> AutoresSemLivros();
        Task<bool> AutorComLivros(int IdAutor);
        Task<List<PublisherVM>> EditorasSemLivros();
        Task<int> GetFirstIdLivro();

        Task<int> GetPubFormat(int pubId);

        Task<IEnumerable<PubsWithDescriptions>> GetAllPubs_Extended(string userId, string userRole);
        Task<IEnumerable<ImageCategoryLocations>> GetImageLocations(int idCategoria);

        Task<PubsWithDescriptions> Query_ById_Extended(int Id); // new

        Task<IEnumerable<PubsWithDescriptions>> Query_Async_Extended(string where); // new
        Task<List<PubsWithDescriptions>> GetID_ByCapa(string Capa);
        Task<int> GetID_ByPath(string sTitulo);
        Task<int> GetLastIdLivro();
        Task<string> GetLastTitle();
        string GetPubUserName(int iUserID);
        Task<List<PubsWithDescriptions>> GetPubsByTitle(string sTitle);
        Task<bool> IDLivroExiste_BD(int ID_LIvro);
        Task<List<PubsWithDescriptions>> Listar(Agrupamento grupo);
        Task<List<PubsWithDescriptions>> Listar_Duplicados();

        Task<PubsWithDescriptions> Listar_ID_Extended(int Id);
        Task<List<PubsWithDescriptions>> Listar_IDAutor(int IdAutor);
        Task<List<PubsWithDescriptions>> Listar_IDCategoria(int IdCategoria);
        Task<List<PubsWithDescriptions>> Listar_IDEditora(int IdEditora);
        Task<List<PubsWithDescriptions>> Listar_IDFormato(int IdFormato);
        Task<List<PubsWithDescriptions>> Listar_IDLocalArquivo(int IdLocalArquivo);

        Task<List<PubsWithDescriptions>> Listar_Lidos(bool Lido);
        Task<List<PubsWithDescriptions>> Listar_Pagos(bool Pago);
        Task<List<PubsWithDescriptions>> Listar_PalavraTitulo(string sWhereParam);
        Task<List<PubsWithDescriptions>> Listar_SemCapa();
        Task<List<PubsWithDescriptions>> Listar_AnoEdicao(int parInt);
        Task<List<PubsWithDescriptions>> Listar_SemPaginas();
        Task<List<PubsWithDescriptions>> Listar_Titulo(string Titulo);
        Task<List<PubsWithDescriptions>> Listar_UserID(int iUserID);

        Task<List<PubsWithDescriptions>> GetPubsData(TipoPesquisa iTipo, int idCodigo);

        bool LivroExiste_BD(string sArquivo);
        Task<bool> Publicacao_Ja_Incluida(string sTituloOriginal);
        Task<bool> TituloExiste_BD(string sTituloOriginal);
        Task<int> CountPublications_ByCategory(int idCategoria);
        StringBuilder QueryDuplicates(string startFolder);

        Task<List<PubsWithDescriptions>> GetAllPubs_Grid();
        Task<IEnumerable<PubsWithDescriptions>> GetAllPubs_Grid_Async();
        Task<bool> UpdatePubsFromContextMenu(string sUpdateQuery);
        Task<List<FileCheckResult>> PubsNotFoundOnDisk(string webRoot);
        Task<bool> StorePdfNotDeleted(PdfsAndPngNotDeleted fileName);
        Task<bool> CanAuthorBeDeleted(int authorId);
    }
}