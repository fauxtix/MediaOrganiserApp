using System.Data;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IFormsHelper
    {
        IDataReader GetCategoriasPublicacoes();
        string GetDescricao(int Id, string sTabela);
        string GetDescricaoCategoria(int Id, int TipoCategoria);
    }
}