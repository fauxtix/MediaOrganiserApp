using System.Data;

namespace MediaOrganizerApp.Application.Interfaces.Services
{
    public interface IFormService
    {
        IDataReader GetCategoriasPublicacoes();
        string GetDescricao(int Id, string sTabela);
        string GetDescricaoCategoria(int Id, int TipoCategoria);
    }
}