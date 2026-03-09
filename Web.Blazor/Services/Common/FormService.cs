using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services;
using System.Data;


namespace Web.Blazor.Services.Common
{
    public class FormService : IFormService
    {
        private readonly IFormsHelper _formsHelper;
        public FormService(IFormsHelper formsHelper)
        {
            _formsHelper = formsHelper;
        }

        /// <summary>
        /// Obtém descrição das tabelas auxialiares (para preenchimento/posicionamento das combo-boxes) 
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="sTabela">Tabela fonye </param>
        /// <returns>Conteúdo do campo Descrição</returns>
        public string GetDescricao(int Id, string sTabela)
        {
            return _formsHelper.GetDescricao(Id, sTabela);
        }

        public string GetDescricaoCategoria(int Id, int TipoCategoria)
        {
            return _formsHelper.GetDescricaoCategoria(Id, TipoCategoria);
        }

        public IDataReader GetCategoriasPublicacoes()
        {
            return _formsHelper.GetCategoriasPublicacoes();
        }
    }
}
