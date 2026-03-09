using Dapper;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Utilities;
using MediaOrganizerApp.Shared.DTOs;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Repositories.MySqlRepository
{
    public class StatsRepository : IStatsRepository
    {
        private readonly StringBuilder sb = new();
        private readonly IDbConnection sqlConnection;
        private int iRet;

        public StatsRepository(IDbConnectionFactory dbFactory)
        {
            sqlConnection = dbFactory.CreateSqlConnection("SqlServerCon");
        }
        public async Task<List<StatRecord>> GetTitulosDuplicados()
        {
            string sProcedName = "usp_Stats_GetTitulosDuplicados";

            try
            {
                using (IDbConnection connection = sqlConnection)
                {
                    var res = (await connection.QueryAsync<StatRecord>(sProcedName,
                        commandType: CommandType.StoredProcedure))
                        .ToList();
                    return res;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> GetCountFromTableNotClassified(string sTabela, string sCampoGrupo, string sCampoAux, string sToCheck)
        {
            sb.Clear();
            sb.Append("SELECT Count(*) AS Contagem ");
            sb.Append("from publicacao ");
            sb.Append($"INNER JOIN {sTabela} ON {sTabela}.Id = Publicacao.{sCampoGrupo} ");
            sb.Append($"WHERE {sTabela}.{sCampoAux} = {sToCheck}");

            try
            {
                using (IDbConnection connection = sqlConnection)
                {
                    iRet = await connection.QuerySingleAsync<int>(sb.ToString());
                    return iRet;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<StatRecord>> GetStatEntities()
        {
            string sProcedName = "usp_Stats_GetCountFromTablesNotClassified";
            try
            {
                var p = new DynamicParameters();
                p.Add("TotalEditoras_NC", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
                p.Add("TotalAutores_NC", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
                p.Add("TotalCategorias_NC", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
                p.Add("TotalFormatos_NC", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
                p.Add("TotalLocaisArquivo_NC", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

                p.Add("TotalEditoras", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
                p.Add("TotalAutores", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
                p.Add("TotalCategorias", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
                p.Add("TotalFormatos", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
                p.Add("TotalLocaisArquivo", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

                int autoresSemPubs = await GetCountFromAuthorsWithoutPubs();
                int editorasSemPubs = await GetCountFromEditorasWithoutPubs();
                int categoriasSemPubs = await GetCountFromGenerosWithoutPubs();
                int formatosSemPubs = await GetCountFromFormatosWithoutPubs();
                int locaisRiscoSemPubs = await GetCountFromLocaisArquivoWithoutPubs();

                using (IDbConnection connection = sqlConnection)
                {
                    await connection.QueryAsync<int>(sProcedName, p, commandType: CommandType.StoredProcedure);

                    List<StatRecord> statRecords = new()
                    {
                        new StatRecord()
                        {
                            Titulo = "- Editoras",
                            Contagem = p.Get<int>("TotalEditoras").ToString(),
                            NaoClassificados = p.Get<int>("TotalEditoras_NC").ToString(),
                            SemPublicacoes = editorasSemPubs.ToString()
                        },
                        new StatRecord()
                        {
                            Titulo = "- Autores",
                            Contagem = p.Get<int>("TotalAutores").ToString(),
                            NaoClassificados = p.Get<int>("TotalAutores_NC").ToString(),
                            SemPublicacoes = autoresSemPubs.ToString()
                        },
                        new StatRecord()
                        {
                            Titulo = "- Categorias",
                            Contagem = p.Get<int>("TotalCategorias").ToString(),
                            NaoClassificados = p.Get<int>("TotalCategorias_NC").ToString(),
                            SemPublicacoes = categoriasSemPubs.ToString()
                        },
                        new StatRecord()
                        {
                            Titulo = "- Formatos",
                            Contagem = p.Get<int>("TotalFormatos").ToString(),
                            NaoClassificados = p.Get<int>("TotalFormatos_NC").ToString(),
                            SemPublicacoes = formatosSemPubs.ToString()
                        },
                        new StatRecord()
                        {
                            Titulo = "- Locais de Arquivo",
                            Contagem = p.Get<int>("TotalLocaisArquivo").ToString(),
                            NaoClassificados = p.Get<int>("TotalLocaisArquivo_NC").ToString(),
                            SemPublicacoes = locaisRiscoSemPubs.ToString()
                        }
                    };

                    return statRecords;

                }
            }
            catch
            {
                throw;
            }

        }

        public async Task<int> GetCountFromPubsWithoutPages()
        {
            string sProcedName = "usp_Stats_GetCountFromPubsWithoutPages";
            return await ExecuteSP(sProcedName);
        }

        public async Task<int> GetCountFromPubsWithoutCover()
        {
            string sProcedName = "usp_Stats_GetCountFromPubsWithoutCover";
            return await ExecuteSP(sProcedName);
        }

        public async Task<int> GetCountFromAuthorsWithoutPubs()
        {
            string sProcedName = "usp_Stats_GetCountFromAuthorsWithoutPubs";
            return await ExecuteSP(sProcedName);
        }

        public async Task<int> GetCountFromGenerosWithoutPubs()
        {
            string sProcedName = "usp_Stats_GetCountFromGenerosWithoutPubs";
            return await ExecuteSP(sProcedName);
        }

        public async Task<int> GetCountFromFormatosWithoutPubs()
        {
            string sProcedName = "usp_Stats_GetCountFromFormatosWithoutPubs";
            return await ExecuteSP(sProcedName);
        }

        public async Task<int> GetCountFromEditorasWithoutPubs()
        {
            string sProcedName = "usp_Stats_GetCountFromEditorasWithoutPubs";
            return await ExecuteSP(sProcedName);
        }

        public async Task<int> GetCountFromLocaisArquivoWithoutPubs()
        {
            string sProcedName = "usp_Stats_GetCountFromLocaisArquivoWithoutPubs";
            return await ExecuteSP(sProcedName);
        }

        public async Task<int> ExecuteSP(string spName)
        {
            try
            {
                using (IDbConnection connection = sqlConnection)
                {
                    var res = await connection.QuerySingleAsync<int>(spName,
                        commandType: CommandType.StoredProcedure);
                    return res;
                }
            }
            catch
            {
                throw;
            }
        }
    }
}

