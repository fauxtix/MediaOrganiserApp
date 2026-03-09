using Dapper;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text;

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    public class FormsHelper : IFormsHelper
    {
        private readonly ILogger<FormsHelper> _logger;
        private readonly IDapperContext _context;

        public FormsHelper(ILogger<FormsHelper> logger, IDapperContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IDataReader GetCategoriasPublicacoes()
        {
            StringBuilder sb = new();
            sb.Append("SELECT c.Descricao from publicacao l ");
            sb.Append("INNER JOIN Categoria c ");
            sb.Append("ON l.IdCategoria = c.Id ");
            sb.Append("GROUP by c.descricao ");
            sb.Append("ORDER BY c.descricao");

                using (var connection = _context.CreateConnection())
            {
                return connection.ExecuteReader(sb.ToString());
            }
        }

        public string GetDescricao(int Id, string sTabela)
        {
            StringBuilder sb = new();
            sb.Append("SELECT Descricao ");
            sb.Append($"FROM {sTabela} ");
            sb.Append("WHERE Id = @Id");
                using (var connection = _context.CreateConnection())
            {
                var sDesc = connection.QuerySingle<string>(sb.ToString(), new { Id });
                return sDesc;
            }
        }

        // Obtém descrição das tabelas auxialiares (para preenchimento/posicionamento das combo-boxes)
        public string GetDescricaoCategoria(int Id, int TipoCategoria)
        {
            var sp = "usp_Categoria_GetDescription_ByIdAndType";
            try
            {
                    using (var connection = _context.CreateConnection())
                {
                    var sDesc = connection.QuerySingle<string>(sp, new { Id, TipoCategoria }, commandType: CommandType.StoredProcedure);
                    return sDesc;
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message);
                return "";
            }
        }
    }
}
