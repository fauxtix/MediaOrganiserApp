using Dapper;
using MediaOrganizerApp.Application.AppGlobal;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    public class PublicacoesRepository : SQLServerBaseRepository<Publicacao>, IPublicacoesRepository
    {
        private readonly UserRole userRole = SessionParameters.UserRole;
        private readonly int iUserID = SessionParameters.UserID == 0 ? 1 : SessionParameters.UserID;
        private readonly StringBuilder sbData = new();
        string storedProcedure = string.Empty;

        private readonly IDataSecurity m_dataSecurity;
        private readonly IUtilizadorRepository m_utilizador;
        private readonly ILogger<PublicacoesRepository> _logger;
        private readonly IDapperContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PublicacoesRepository(IDataSecurity dataSecurity,
            IUtilizadorRepository utilizador,
            ILogger<PublicacoesRepository> logger,
            IDapperContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            m_dataSecurity = dataSecurity;
            m_utilizador = utilizador;
            _logger = logger;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IEnumerable<PubsWithDescriptions>> GetAllPubs_Extended(string userId, string role)
        {
            string procedureName;

            if (role.ToLower().Contains(UserRole.Admin.ToString().ToLower()))
            {
                procedureName = "usp_Publicacao_GetAll_Extended";
                try
                {
                    using (var connection = _context.CreateConnection())
                    {
                        var output = await connection.QueryAsync<PubsWithDescriptions>(procedureName,
                            commandType: CommandType.StoredProcedure);
                        return output;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro na leitura de publicações do repositório ({ex.Message})");
                    return null;
                }

            }
            else
            {
                procedureName = "usp_Publicacao_GetAll_Extended_ByUserId";
                try
                {
                    using (var connection = _context.CreateConnection())
                    {
                        var output = await connection.QueryAsync<PubsWithDescriptions>(procedureName, new { CreatedBy = userId },
                            commandType: CommandType.StoredProcedure);
                        return output;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro na leitura de publicações do repositório ({ex.Message})");
                    return null;
                }

            }

        }


        public async Task AtualizaTituloOriginal(int Id, string TituloOriginal)
        {


            storedProcedure = "usp_AtualizaTituloOriginal";
            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(storedProcedure, new { Id, TituloOriginal }, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<bool> Atualiza_CapaPublicacao(int Id, string ImagemCapa)
        {
            storedProcedure = "usp_Atualiza_CapaPublicacao";

            using (var connection = _context.CreateConnection())
            {
                int iRet = await connection.ExecuteAsync(storedProcedure, new { Id, ImagemCapa },
                    commandType: CommandType.StoredProcedure);
                return iRet > 0;
            }
        }

        public async Task<bool> Atualiza_Publicacao(int Id, string DigitalPath, string ImagemCapa, int idCategoria)
        {
            storedProcedure = "usp_Atualiza_Publicacao";

            using (var connection = _context.CreateConnection())
            {
                int iRet = await connection.ExecuteAsync(storedProcedure, new { Id, DigitalPath, ImagemCapa, IdCategoria = idCategoria },
                    commandType: CommandType.StoredProcedure);
                return iRet > 0;
            }
        }

        public async Task<PubsWithDescriptions> Query_ById_Extended(int Id)
        {
            storedProcedure = "usp_Publicacao_GetSingle_Extended";
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryAsync<PubsWithDescriptions>(storedProcedure, new { Id }, commandType: CommandType.StoredProcedure);

                    return result.FirstOrDefault();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro na leitura de publicação do repositório ({Id}) ({ex.Message})");
                return null;
            }
        }

        public async Task<IEnumerable<PubsWithDescriptions>> Query_Async_Extended(string where)
        {
            var query = $"SELECT * FROM vwPubs ";

            if (!string.IsNullOrEmpty(where))
                query += $"WHERE {where}";

            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryAsync<PubsWithDescriptions>(query);
            }
        }

        public async Task<int> CountPublications_ByCategory(int IdCategoria)
        {
            storedProcedure = "usp_CountPublications_ByCategory";
            int iRet;

            using (var connection = _context.CreateConnection())
            {
                iRet = (await connection.QueryAsync<int>(storedProcedure, new { IdCategoria },
                    commandType: CommandType.StoredProcedure))
                    .FirstOrDefault();
                return iRet;
            }
        }


        public async Task<List<FileCheckResult>> PubsNotFoundOnDisk(string webRoot)
        {
            storedProcedure = "usp_Publicacao_FilesNotOnDisk";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    List<FileCheckResult> lstFiles = (await connection.QueryAsync<FileCheckResult>(storedProcedure,
                        new { BasePath = webRoot },
                        commandType: CommandType.StoredProcedure))
                        .ToList();
                    return lstFiles;
                }

            }
            catch
            {

                throw;
            }
        }

        public async Task<List<PublisherVM>> EditorasSemLivros()
        {
            storedProcedure = "usp_EditorasSemLivros";

            using (var connection = _context.CreateConnection())
            {
                List<PublisherVM> lstEditoras = (await connection.QueryAsync<PublisherVM>(storedProcedure,
                    commandType: CommandType.StoredProcedure))
                    .ToList();
                return lstEditoras;
            }
        }

        public async Task<int> GetFirstIdLivro()
        {
            if (userRole == UserRole.Admin)
            {
                return GetLastId();
            }

            string sProcedName = "usp_Publicacao_GetFirstId_ByUserId";

            using (var connection = _context.CreateConnection())
            {
                int iRet = (await connection.QueryAsync<int>(sProcedName, new { UserID = iUserID },
                    commandType: CommandType.StoredProcedure))
                 .FirstOrDefault();

                return iRet;
            }
        }

        public async Task<int> GetLastIdLivro()
        {
            if (userRole == UserRole.Admin)
            {
                return GetLastId();
            }

            string sProcedName = "usp_Publicacao_GetLastId_ByUserId";  // generaluser
            using (var connection = _context.CreateConnection())
            {
                int iRet = (await connection.QueryAsync<int>(sProcedName, new
                {
                    UserID = iUserID
                }, commandType: CommandType.StoredProcedure))
                 .FirstOrDefault();

                return iRet;
            }
        }

        public async Task<List<PubsWithDescriptions>> GetID_ByCapa(string ImagemCapa)
        {
            string sProcedName = "usp_Publicacao_Get_ByCapa";

            sbData.Clear();
            sbData.Append(GetBooksQuery());

            using (var connection = _context.CreateConnection())
            {

                if (userRole == UserRole.GeneralUser)
                {
                    sProcedName = "usp_Publicacao_Get_ByUserIdAndCapa";

                    return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, new
                    {
                        UserID = iUserID,
                        ImagemCapa
                    }, commandType: CommandType.StoredProcedure))
                        .ToList();
                }
                else
                {
                    return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, new
                    {
                        ImagemCapa
                    }, commandType: CommandType.StoredProcedure))
                        .ToList();
                }
            }
        }

        public async Task<int> GetID_ByPath(string sTitulo)
        {
            string sProcedName = "usp_Publicacao_GetId_ByPath";
            using (var connection = _context.CreateConnection())
            {
                int iRet = (await connection.QueryAsync<int>(sProcedName, new
                {
                    TituloOriginal = sTitulo
                }, commandType: CommandType.StoredProcedure))
                    .SingleOrDefault();

                return iRet;
            }
        }

        public async Task<string> GetLastTitle()
        {
            string sProcedName = "usp_Publicacao_GetLastTitle";

            sbData.Clear();



            sbData.Append("SELECT Titulo from publicacao ");

            using (var connection = _context.CreateConnection())
            {
                string sRet;
                if (userRole == UserRole.GeneralUser)
                {
                    sProcedName = "usp_Publicacao_GetLastTitle_ByUserId";
                    sRet = (await connection.QueryAsync<string>(sProcedName, new
                    {
                        UserID = iUserID
                    }, commandType: CommandType.StoredProcedure))
                        .SingleOrDefault();
                }
                else
                {
                    sRet = (await connection.QueryAsync<string>(sProcedName,
                        commandType: CommandType.StoredProcedure))
                        .SingleOrDefault();
                }

                return sRet;
            }
        }

        public string GetPubUserName(int iUserID)
        {
            string sUserName = m_dataSecurity.Decrypt(m_utilizador.GetUserName(iUserID));
            if (sUserName.ToLower().Contains("admin"))
            {
                return "";
            }
            else
                return sUserName;
        }

        public async Task<bool> IDLivroExiste_BD(int Id)
        {
            return await RecInUse_Async(Id);
        }

        public async Task<List<PubsWithDescriptions>> Listar(Agrupamento grupo)
        {
            sbData.Clear();
            sbData.Append(GetBooksQuery());

            if (userRole == UserRole.GeneralUser)
                sbData.Append(" WHERE vwPubs.UserID = @UserID  ");

            switch (grupo)
            {
                case Agrupamento.Autor:
                    sbData.Append("ORDER BY vwPubs.Autor, vwPubs.Titulo");
                    break;
                case Agrupamento.Editora:
                    sbData.Append("ORDER BY vwPubs.Editora, vwPubs.Titulo");
                    break;
                case Agrupamento.Todos:
                case Agrupamento.Genero:
                    sbData.Append("ORDER BY vwPubs.Categoria, vwPubs.Titulo");
                    break;
                case Agrupamento.Formato:
                    sbData.Append("ORDER BY vwPubs.Formato, vwPubs.Titulo");
                    break;
            }



            using (var connection = _context.CreateConnection())
            {
                var output = await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new { UserID = iUserID });

                return output.ToList();
            }

        }

        public async Task<List<PubsWithDescriptions>> Listar_Duplicados()
        {
            string sProcedName = "";


            if (userRole == UserRole.GeneralUser)
            {
                sProcedName = "usp_Publicacao_ListarDuplicados_ByUserId";
                using (var connection = _context.CreateConnection())
                {
                    return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, new
                    {
                        UserID = iUserID
                    }, commandType: CommandType.StoredProcedure))
                        .ToList();
                }
            }
            else
            {
                sProcedName = "usp_Publicacao_ListarDuplicados";
                using (var connection = _context.CreateConnection())
                {
                    var output = await connection.QueryAsync<PubsWithDescriptions>(sProcedName, commandType: CommandType.StoredProcedure);
                    return output.ToList();
                }
            }
        }

        public async Task<PubsWithDescriptions> Listar_ID_Extended(int Id)
        {
            string sProcedName = "usp_Publicacao_Listar_ID_Extended";


            using (var connection = _context.CreateConnection())
            {
                return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName.ToString(), new
                {
                    Id
                }, commandType: CommandType.StoredProcedure))
                    .SingleOrDefault();
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_IDAutor(int IdAutor)
        {
            string sProcedName = "usp_Publicacao_Listar_IDAutor_ById";
            try
            {


                using (var connection = _context.CreateConnection())
                {
                    if (userRole == UserRole.GeneralUser)
                    {
                        sProcedName = "usp_Publicacao_Listar_IDAutor_ByIdAndUserId";

                        return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, new
                        {
                            IdAutor,
                            UserId = iUserID
                        }, commandType: CommandType.StoredProcedure))
                        .ToList();
                    }
                    else
                    {
                        return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, new
                        {
                            IdAutor
                        }, commandType: CommandType.StoredProcedure))
                        .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new List<PubsWithDescriptions>();
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_IDCategoria(int IdCategoria)
        {
            return await GetPubsFromID(IdCategoria, iUserID);
        }

        public async Task<List<PubsWithDescriptions>> Listar_IDEditora(int IdEditora)
        {
            string sProcedName = "usp_Publicacao_Listar_IDEditora_ById";


            try
            {
                using (var connection = _context.CreateConnection())
                {
                    if (userRole == UserRole.GeneralUser)
                    {
                        sProcedName = "usp_Publicacao_Listar_IDEditora_ByIdAndUserId";

                        return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, new { IdEditora, Userid = iUserID },
                            commandType: CommandType.StoredProcedure))
                            .ToList();
                    }
                    else
                    {
                        return connection.Query<PubsWithDescriptions>(sProcedName, new { IdEditora },
                            commandType: CommandType.StoredProcedure)
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new List<PubsWithDescriptions>();
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_IDFormato(int IdFormato)
        {


            string sProcedName = "usp_Publicacao_Listar_IDFormato_ById";
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    if (userRole == UserRole.GeneralUser)
                    {
                        sProcedName = "usp_Publicacao_Listar_IDFormato_ByIdAndUserId";

                        return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, new { IdFormato, UserID = iUserID },
                            commandType: CommandType.StoredProcedure))
                            .ToList();
                    }
                    else
                    {
                        return connection.Query<PubsWithDescriptions>(sProcedName, new { IdFormato },
                            commandType: CommandType.StoredProcedure).
                            ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new List<PubsWithDescriptions>();
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_IDLocalArquivo(int IdLocalArquivo)
        {
            string sProcedName = "usp_Publicacao_Listar_IDLocalArquivo_ById";


            try
            {
                using (var connection = _context.CreateConnection())
                {
                    if (userRole == UserRole.GeneralUser)
                    {
                        sProcedName = "usp_Publicacao_Listar_IDLocalArquivo_ByIdAndUserId";

                        return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName,
                            new { IdLocalArquivo, UserId = iUserID },
                            commandType: CommandType.StoredProcedure))
                            .ToList();
                    }
                    else
                    {
                        return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, new
                        {
                            IdLocalArquivo
                        }, commandType: CommandType.StoredProcedure))
                        .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new List<PubsWithDescriptions>();
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_Lidos(bool blnLido)
        {
            int Lido = blnLido ? 1 : 0;
            string sProcedName = "usp_Publicacao_Listar_Lidos_ByLido";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    if (userRole == UserRole.GeneralUser)
                    {
                        sProcedName = "usp_Publicacao_Listar_Lidos_ByLidoAndUserId";

                        return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, new { Lido, UserID = iUserID },
                            commandType: CommandType.StoredProcedure))
                            .ToList();
                    }
                    else
                    {
                        return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, new { Lido },
                            commandType: CommandType.StoredProcedure))
                        .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new List<PubsWithDescriptions>();
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_Pagos(bool Pago)
        {
            string filterExpr;
            if (Pago)
                filterExpr = "[Preco] > 0";
            else
                filterExpr = "[Preco] = 0";

            sbData.Clear();

            sbData.Append(GetBooksQuery());
            using (var connection = _context.CreateConnection())
            {
                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append($"WHERE {filterExpr} AND UserID = @UserID");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(),
                        new { UserID = iUserID }))
                        .ToList();
                }
                else
                {
                    sbData.Append($"WHERE {filterExpr}");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString()))
                        .ToList();
                }
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_PalavraTitulo(string sWhereParam)
        {
            sbData.Clear();
            sbData.Append(GetBooksQuery());

            using (var connection = _context.CreateConnection())
            {
                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append($" WHERE {sWhereParam} AND UserID = @UserID");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(),
                        new { UserID = iUserID }))
                        .ToList();
                }
                else
                {
                    sbData.Append($" WHERE {sWhereParam}");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString()))
                        .ToList();
                }
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_SemCapa()
        {
            // 11-08-2021 - criada função (dbo.fc_FileExists(ImagemCapa)) na BD para verificar se ficheiro existe; incluída na expressão WHERE
            // 11-09-2021 - removida função WHERE, já que conteúdo do campo não tem path completa...
            string sProcedName = "usp_Publicacao_Listar_SemCapa";


            try
            {
                using (var connection = _context.CreateConnection())
                {
                    return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName,
                        commandType: CommandType.StoredProcedure))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new List<PubsWithDescriptions>();
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_AnoEdicao(int parInt)
        {
            string sProcedName = "usp_Publicacao_Listar_AnoEdicao";


            try
            {
                using (var connection = _context.CreateConnection())
                {

                    return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, new { parInt },
                        commandType: CommandType.StoredProcedure))
                        .ToList();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new List<PubsWithDescriptions>();
            }
        }
        public async Task<List<PubsWithDescriptions>> Listar_SemPaginas()
        {
            string sProcedName = "usp_Publicacao_Listar_SemPaginas";


            try
            {
                using (var connection = _context.CreateConnection())
                {
                    if (userRole == UserRole.GeneralUser)
                    {
                        sProcedName = "usp_Publicacao_Listar_SemPaginas_ByUserId";

                        return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, new { UserId = iUserID },
                            commandType: CommandType.StoredProcedure))
                            .ToList();
                    }
                    else
                    {
                        return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, commandType: CommandType.StoredProcedure))
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new List<PubsWithDescriptions>();
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_Titulo(string Titulo)
        {
            string[] aTitulos;
            string sWhereTitulos = "";

            sbData.Clear();
            sbData.Append(GetBooksQuery());

            aTitulos = Titulo.Split(';');
            foreach (string sTitle in aTitulos)
            {
                sWhereTitulos += $"Titulo LIKE '%{sTitle}%' OR ";
            }
            sWhereTitulos = sWhereTitulos.Substring(0, sWhereTitulos.Length - 3);

            using (var connection = _context.CreateConnection())
            {
                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append($" WHERE UserID = @UserID AND ({sWhereTitulos})");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(),
                        new { UserID = iUserID }))
                        .ToList();
                }
                else
                {
                    sbData.Append($"WHERE {sWhereTitulos}");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString()))
                        .ToList();
                }
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_UserID(int iUserID)
        {


            using (var connection = _context.CreateConnection())
            {
                string sProcedName = "usp_Publicacao_Listar_UserId";

                return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, new { UserId = iUserID },
                    commandType: CommandType.StoredProcedure))
                    .ToList();
            }
        }

        public bool LivroExiste_BD(string sArquivo)
        {
            bool blnExists = FileExists(@"C:\Publicacoes\", sArquivo);
            return blnExists;
        }

        public async Task<bool> Publicacao_Ja_Incluida(string TituloOriginal)
        {
            string sProcedName = "usp_Publicacao_JaIncluida";

            using (var connection = _context.CreateConnection())
            {
                int iRet = (await connection.QueryAsync<int>(sProcedName, new { UserID = iUserID, TituloOriginal },
                    commandType: CommandType.StoredProcedure))
                    .FirstOrDefault();

                return iRet > 0;
            }

        }

        public StringBuilder QueryDuplicates(string startFolder)
        {
            DirectoryInfo dir = new(startFolder);

            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            int charsToSkip = startFolder.Length;

            var queryDupNames =
                from file in fileList
                group file.FullName.Substring(charsToSkip) by file.Name into fileGroup
                where fileGroup.Count() > 1
                select fileGroup;

            var list = queryDupNames.ToList();

            StringBuilder sb = new();
            foreach (var item in list)
            {
                sb.AppendLine(item.Key.ToString());
            }
            return sb;
        }

        public async Task<bool> TituloExiste_BD(string TituloOriginal)
        {
            string sProcedName = "usp_Publicacao_TituloExiste_BD";


            using (var connection = _context.CreateConnection())
            {
                int iRet = (await connection.QueryAsync<int>(sProcedName, new
                {
                    UserID = iUserID,
                    TituloOriginal
                }, commandType: CommandType.StoredProcedure))
                    .FirstOrDefault();

                return iRet > 0;
            }
        }

        public async Task<List<PubsWithDescriptions>> GetPubsData(TipoPesquisa iTipo, int idCodigo)
        {
            List<PubsWithDescriptions> publicacoes = new();

            try
            {
                switch (iTipo)
                {
                    case TipoPesquisa.Todos:
                        publicacoes = await Listar(Agrupamento.Todos);
                        break;
                    case TipoPesquisa.Autor:
                        publicacoes = await Listar_IDAutor(idCodigo);
                        break;
                    case TipoPesquisa.Editora:
                        publicacoes = await Listar_IDEditora(idCodigo);
                        break;
                    case TipoPesquisa.Genero:
                        publicacoes = await Listar_IDCategoria(idCodigo);
                        break;
                    case TipoPesquisa.Formato:
                        publicacoes = await Listar_IDFormato(idCodigo);
                        break;
                    case TipoPesquisa.Arquivo:
                        publicacoes = await Listar_IDLocalArquivo(idCodigo);
                        break;
                    case TipoPesquisa.Pago:
                        publicacoes = await Listar_Pagos(Convert.ToBoolean(idCodigo));
                        break;
                    case TipoPesquisa.Lido:
                        publicacoes = await Listar_Lidos(Convert.ToBoolean(idCodigo));
                        break;
                    case TipoPesquisa.SemPaginas:
                        publicacoes = await Listar_SemPaginas();
                        break;
                    case TipoPesquisa.AnoEdicao:
                        publicacoes = await Listar_AnoEdicao(idCodigo);
                        break;
                    case TipoPesquisa.SemCapa:
                        publicacoes = await Listar_SemCapa();
                        break;
                    case TipoPesquisa.Duplicados:
                        publicacoes = await Listar_Duplicados();
                        break;
                    case TipoPesquisa.Utilizador:
                        publicacoes = await Listar_UserID(idCodigo);
                        break;
                }

                return publicacoes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new List<PubsWithDescriptions>();
            }
        }

        public async Task<List<PubsWithDescriptions>> GetAllPubs_Grid()
        {
            using (var connection = _context.CreateConnection())
            {
                var output = (await connection.QueryAsync<PubsWithDescriptions>("SELECT * FROM vwPubs",
                    commandType: CommandType.Text))
                    .ToList();

                return output;
            }
        }

        public async Task<IEnumerable<PubsWithDescriptions>> GetAllPubs_Grid_Async()
        {


            using (var connection = _context.CreateConnection())
            {
                var output = await connection.QueryAsync<PubsWithDescriptions>("SELECT * FROM vwPubs",
                    commandType: CommandType.Text);


                return output.ToList();
            }
        }

        public async Task<List<AutorDTO>> AutoresSemLivros()
        {
            storedProcedure = "usp_AutoresSemLivros";


            using (var connection = _context.CreateConnection())
            {
                List<AutorDTO> lstAutores = (await connection
                    .QueryAsync<AutorDTO>(storedProcedure, commandType: CommandType.StoredProcedure))
                    .ToList();
                return lstAutores;
            }
        }

        public async Task<bool> AutorComLivros(int IdAutor)
        {
            string sProcedName = "usp_Publicacao_AutorComLivros";


            using (var connection = _context.CreateConnection())
            {
                int iRet = (await connection.QueryAsync<int>(sProcedName, new { UserID = iUserID, IdAutor },
                    commandType: CommandType.StoredProcedure))
                    .FirstOrDefault();

                return iRet > 0;
            }
        }

        public List<PubsWithDescriptions> ExecProc(string sProcedName)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {

                    return connection.Query<PubsWithDescriptions>(sProcedName,
                        commandType: CommandType.StoredProcedure).
                        ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new List<PubsWithDescriptions>();
            }
        }

        public async Task<List<PubsWithDescriptions>> GetPubsByTitle(string sTitulo)
        {
            return await Listar_Titulo(sTitulo);
        }

        public string GetDuplicateFiles()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ImageCategoryLocations>> GetImageLocations(int idCategoria)
        {
            string sProcedName = "usp_Publicacao_GetImageLocationsByCategory";


            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var output = await connection.QueryAsync<ImageCategoryLocations>(sProcedName,
                       new { IdCategoria = idCategoria },
                       commandType: CommandType.StoredProcedure);
                    return output;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return Enumerable.Empty<ImageCategoryLocations>();
            }

        }

        public async Task<bool> CanAuthorBeDeleted(int authorId)
        {
            string sProcedName = "usp_Publicacao_PubsWithOneAuthor";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var output = await connection.QueryFirstOrDefaultAsync<bool>(sProcedName,
                       new { IdAutor = authorId },
                       commandType: CommandType.StoredProcedure);
                    return output;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return false;
            }
        }
        public async Task<bool> UpdatePubsFromContextMenu(string sUpdateQuery)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    int iCodigo = await connection.ExecuteAsync(sUpdateQuery);
                    return iCodigo > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return false;
            }
        }

        public async Task<int> GetPubFormat(int pubId)
        {
            string sProcedName = "usp_Publicacao_GetPubFormat";
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    int iRet = (await connection.QueryAsync<int>(sProcedName, new { IdCategoria = pubId },
                        commandType: CommandType.StoredProcedure))
                        .FirstOrDefault();
                    return iRet;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return 0;
            }
        }

        public async Task<bool> StorePdfNotDeleted(PdfsAndPngNotDeleted fileName)
        {
            StringBuilder sbInsert = new StringBuilder();
            sbInsert.Append("INSERT INTO PdfsAndPngNotDeleted(");
            sbInsert.Append($"Filename) VALUES('{fileName}')");
            using (var connection = _context.CreateConnection())
            {
                int retCode = await connection.ExecuteAsync(sbInsert.ToString());
                return retCode > 0;
            }
        }

        private string GetBooksQuery()
        {
            StringBuilder sbQuery = new();
            sbQuery.Append("SELECT * FROM vwPubs ");
            return sbQuery.ToString();
        }

        private async Task<List<PubsWithDescriptions>> GetPubsFromID(int IdCategoria, int iUserID)
        {
            string sProcedName = "usp_Publicacao_GetPubs_ByCategory";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    if (userRole == UserRole.GeneralUser)
                    {
                        sProcedName = "usp_Publicacao_GetPubs_ByCategoryAndUserId";

                        return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, new
                        {
                            IdCategoria,
                            UserId = iUserID
                        }, commandType: CommandType.StoredProcedure)).
                            ToList();
                    }
                    else
                    {
                        return (await connection.QueryAsync<PubsWithDescriptions>(sProcedName, new
                        {
                            IdCategoria
                        }, commandType: CommandType.StoredProcedure)).
                            ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new List<PubsWithDescriptions>();
            }
        }


        private bool FileExists(string rootpath, string filename)
        {
            if (!Directory.Exists(rootpath))
                return false;

            if (File.Exists(Path.Combine(rootpath, filename)))
                return true;

            foreach (string subDir in Directory.GetDirectories(rootpath, "*", SearchOption.AllDirectories))
            {
                if (File.Exists(Path.Combine(subDir, filename)))
                    return true;
            }

            return false;
        }

    }
}
