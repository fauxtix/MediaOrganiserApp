using MediaOrganizerApp.Application.Enums;
using System;
using System.Threading.Tasks;
using Web.Blazor.Reporting.BusinessObjects;

namespace Web.Blazor.Reporting.Helpers
{
    public class ReportLists : IReportLists
    {
        private readonly IBO_Autores _autoresSvc;
        private readonly IBO_Categorias _categoriasSvc;
        private readonly IBO_Formatos _formatosSvc;
        private readonly IBO_LocalArquivo _locaisArquivoSvc;
        private readonly IBO_Editoras _editorasSvc;
        private readonly IBO_Media _mediaFilesSvc;
        private readonly IBO_Publicacoes _publicacoesSvc;

        public ReportLists(IBO_Autores autoresSvc,
            IBO_Categorias categoriasSvc,
            IBO_Formatos formatosSvc,
            IBO_LocalArquivo locaisArquivoSvc,
            IBO_Editoras editorasSvc,
            IBO_Publicacoes publicacoesSvc,
            IBO_Media mediaFilesSvc)
        {
            _autoresSvc = autoresSvc;
            _categoriasSvc = categoriasSvc;
            _formatosSvc = formatosSvc;
            _locaisArquivoSvc = locaisArquivoSvc;
            _editorasSvc = editorasSvc;
            _publicacoesSvc = publicacoesSvc;
            _mediaFilesSvc = mediaFilesSvc;
        }

        public async Task<object> GetAuthorsList(int opcao)
        {
            switch (opcao)
            {
                case 0:
                    return await Task.Run(() => _autoresSvc.GetBO_AuthorsPubs());
                case 1:
                    return await Task.Run(() => _autoresSvc.GetBO_AuthorsWithoutPubs());
                case 2:
                    return await Task.Run(() => _autoresSvc.GetBO_AuthorsBooksAndPubs());
            }

            return null;
        }

        public async Task<object> GetCategoriesList()
        {
            return await _categoriasSvc.GetBO_Categories(true);
        }

        public async Task<object> GetFormatsList()
        {
            return await _formatosSvc.GetFormats();
        }
        public async Task<object> GetStoragePlacesList()
        {
            return await _locaisArquivoSvc.GetLocaisArquivo();
        }

        public async Task<object> GetPublishersList()
        {
            return await _editorasSvc.GetPublishers();
        }

        public async Task<object> GetPublicationsList()
        {
            return await _publicacoesSvc.GetPublications(AppDefinitions.TipoPesquisa.Todos, 0, "Todos");
        }

        public async Task<object> GetMediaFilesList(AppDefinitions.Query_TipoMedia reportOption)
        {
            var output = await _mediaFilesSvc.GetMediaFiles(reportOption);
            return output;
        }


        /// <summary>
        /// Not used (caller: report.controller)
        /// </summary>
        /// <param name="reportName"></param>
        /// <returns></returns>
        public async Task<object> GetReportList(string reportName)
        {
            object data = null;
            if (reportName.Equals(nameof(ReportNames.rptAutores_Publicacoes), StringComparison.InvariantCultureIgnoreCase))
            {
                data = await GetAuthorsList(1); // 1 = Publicacoes
            }
            else if (reportName.Equals(nameof(ReportNames.rptCategoriasGenerico), StringComparison.InvariantCultureIgnoreCase))
            {
                data = await GetCategoriesList();
            }
            return data is not null ? data : null;
        }
    }
}
