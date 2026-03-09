using System.Data;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface ISQLFormsHelper
    {
        IDataReader GetCategoriasPublicacoes();
        string GetDescricao(int Id, string sTabela);
        string GetDescricaoCategoria(int Id, int TipoCategoria);
    }
}