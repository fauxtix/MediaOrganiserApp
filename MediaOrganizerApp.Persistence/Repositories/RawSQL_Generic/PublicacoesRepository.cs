using Dapper;
using MediaOrganizerApp.Application.AppGlobal;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.Persistence.Repositories.RawSQLRepository
{
    public class PublicacoesRepository : BaseRepository<Publicacao>, IPublicacoesRepository
    {
        private readonly UserRole userRole = SessionParameters.UserRole;
        private readonly int iUserID = SessionParameters.UserID;
        private readonly StringBuilder sbData = new();
        private readonly IDbConnection sqlConnection;

        private readonly IDataSecurity m_dataSecurity;
        private readonly IUtilizadorRepository m_utilizador;

        public PublicacoesRepository(IDataSecurity dataSecurity,
            IUtilizadorRepository utilizador,
            IDbConnectionFactory dbFactory)
        {
            m_dataSecurity = dataSecurity;
            m_utilizador = utilizador;
            sqlConnection = dbFactory.CreateSqlConnection("SqlServerCon");
        }

        public async Task<IEnumerable<PubsWithDescriptions>> GetAllPubs_Extended(string userId, string role)
        {
            sbData.Clear();
            sbData.Append("SELECT P.Id, DataMovimento, Titulo, TituloOriginal, Resumo, IdAutor, IdCategoria, ");
            sbData.Append("IdEditora, IdFormato, IdLocalArquivo, Preco, Paginas, AnoEdicao, ImagemCapa, DigitalPath, ");
            sbData.Append("Rating, Lido, UserID, isbn, autor.Descricao Autor, categoria.Descricao Categoria, ");
            sbData.Append("Editora.Descricao Editora, formato.Descricao Formato, LocalArquivo.Descricao Local ");
            sbData.Append("FROM[Publicacoes].[dbo].[Publicacao] P ");
            sbData.Append("INNER JOIN Autor ON p.IdAutor = autor.Id ");
            sbData.Append("INNER JOIN Formato ON P.IdFormato = dbo.Formato.Id ");
            sbData.Append("INNER JOIN dbo.Editora ON P.IdEditora = dbo.Editora.Id ");
            sbData.Append("INNER JOIN dbo.Categoria ON P.IdCategoria = dbo.Categoria.Id ");
            sbData.Append("INNER JOIN dbo.LocalArquivo ON P.IdLocalArquivo = dbo.LocalArquivo.Id");

            try
            {
                using (IDbConnection connection = sqlConnection)
                {
                    var output = await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString());
                    return output;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task AtualizaTituloOriginal(int Id, string TituloOriginal)
        {
            sbData.Clear();
            sbData.Append("UPDATE Publicacao ");
            sbData.Append("SET TituloOriginal = @TituloOriginal ");
            sbData.Append("WHERE Id = @Id");
            using (IDbConnection connection = sqlConnection)
            {
                await connection.ExecuteAsync(sbData.ToString(), new { Id, TituloOriginal });
            }
        }

        public async Task<bool> Atualiza_CapaPublicacao(int Id, string sImageCoverPath)
        {
            sbData.Clear();
            sbData.Append("UPDATE Publicacao ");
            sbData.Append("SET ImagemCapa = @ImagemCapa ");
            sbData.Append("WHERE Id = @Id");
            using (IDbConnection connection = sqlConnection)
            {
                int iRet = await connection.ExecuteAsync(sbData.ToString(), new { Id, ImagemCaoa = sImageCoverPath });
                return iRet > 0;
            }
        }

        public async Task<bool> Atualiza_Publicacao(int Id, string DigitalPath, string ImagemCapa, int idCategoria)
        {
            sbData.Clear();
            sbData.Append("UPDATE Publicacao ");
            sbData.Append("SET DigitalPath = @DigitalPath, ImagemCapa = @ImagemCapa, IdCategoria = @IdCategoria ");
            sbData.Append("WHERE Id = @Id");

            using (IDbConnection connection = sqlConnection)
            {
                int iRet = await connection.ExecuteAsync(sbData.ToString(), new
                {
                    Id,
                    DigitalPath,
                    ImagemCapa,
                    IdCategoria = idCategoria
                });
                return iRet > 0;
            }
        }

        public async Task<bool> AutorComLivros(int IdAutor)
        {
            sbData.Clear();
            sbData.Append("SELECT 1 from publicacao ");
            sbData.Append("WHERE UserID = @UserID AND IdAutor = @IdAutor ");
            using (IDbConnection connection = sqlConnection)
            {
                int iRet = (await connection.QueryAsync<int>(sbData.ToString(), new { UserID = iUserID, IdAutor }))
                    .FirstOrDefault();

                return iRet > 0;
            }
        }

        public async Task<List<AutorDTO>> AutoresSemLivros()
        {
            sbData.Clear();
            sbData.Append("SELECT * ");
            sbData.Append("FROM vwAutoresSemPublicacoes ");
            sbData.Append("ORDER BY Descricao");
            using (IDbConnection connection = sqlConnection)
            {
                List<AutorDTO> lstAutores = (await connection.QueryAsync<AutorDTO>(sbData.ToString())).ToList();
                return lstAutores;
            }
        }

        public async Task<PubsWithDescriptions> Query_ById_Extended(int Id)
        {
            string sql = "SELECT * FROM vwPubs WHERE Id = @Id";

            using (IDbConnection connection = ConnectionManager.GetConnection())
            {
                var result = await connection.QueryAsync<PubsWithDescriptions>(sql, new { Id });

                return result.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<PubsWithDescriptions>> Query_Async_Extended(string where)
        {
            var query = $"SELECT * FROM vwPubs ";
            if (!string.IsNullOrEmpty(where))
                query += $"WHERE {where}";

            using (IDbConnection connection = sqlConnection)
            {
                return await connection.QueryAsync<PubsWithDescriptions>(query);
            }
        }
        public async Task<int> CountPublications_ByCategory(int IdCategoria)
        {
            int iRet;
            sbData.Clear();
            sbData.Append("SELECT COUNT(*) ");
            sbData.Append("from publicacao ");
            sbData.Append("WHERE IdCategoria = @IdCategoria");

            using (IDbConnection connection = sqlConnection)
            {
                iRet = (await connection.QueryAsync<int>(sbData.ToString(), new { IdCategoria })).FirstOrDefault();
                return iRet;
            }
        }

        public async Task<List<PublisherVM>> EditorasSemLivros()
        {
            sbData.Clear();
            sbData.Append("SELECT * ");
            sbData.Append("FROM vwEditorasSemPublicacoes");
            using (IDbConnection connection = sqlConnection)
            {
                List<PublisherVM> lstEditoras = (await connection.QueryAsync<PublisherVM>(sbData.ToString())).ToList();
                return lstEditoras;
            }
        }

        public async Task<int> GetFirstIdLivro()
        {
            sbData.Clear();
            sbData.Append("SELECT Id ");
            sbData.Append("from publicacao ");

            using (IDbConnection connection = sqlConnection)
            {
                int iRet;
                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append(" WHERE UserID = @UserID  ");
                    sbData.Append("ORDER BY Id ");
                    iRet = (await connection.QueryAsync<int>(sbData.ToString(), new
                    {
                        UserID = iUserID
                    })).FirstOrDefault();
                }
                else
                {
                    sbData.Append("ORDER BY Id ");
                    iRet = (await connection.QueryAsync<int>(sbData.ToString())).FirstOrDefault();
                }

                return iRet;
            }
        }

        public async Task<List<PubsWithDescriptions>> GetID_ByCapa(string ImagemCapa)
        {
            sbData.Clear();
            sbData.Append(GetBooksQuery());

            using (IDbConnection connection = sqlConnection)
            {

                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append(" WHERE UserID = @UserID AND ImagemCapa = @ImagemCapa");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(),
                        new { UserID = iUserID, ImagemCapa }))
                        .ToList();
                }
                else
                {
                    sbData.Append(" WHERE ImagemCapa = @ImagemCapa");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(),
                        new { UserID = iUserID, ImagemCapa }))
                        .ToList();
                }
            }
        }

        public async Task<int> GetID_ByPath(string sTitulo)
        {
            sbData.Clear();
            sbData.Append("SELECT vwPubs.Id FROM vwPubs ");
            sbData.Append(" WHERE TituloOriginal = @TituloOriginal");
            using (IDbConnection connection = sqlConnection)
            {
                int iRet = (await connection.QueryAsync<int>(sbData.ToString(), new
                {
                    TituloOriginal = sTitulo
                })).SingleOrDefault();

                return iRet;
            }
        }

        public async Task<int> GetLastIdLivro()
        {
            int iRet;

            sbData.Clear();
            sbData.Append("SELECT Id FROM vwPubs ");

            using (IDbConnection connection = sqlConnection)
            {
                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append(" WHERE UserID = @UserID  ");
                    sbData.Append("ORDER BY Id DESC");
                    iRet = (await connection.QueryAsync<int>(sbData.ToString(), new
                    {
                        UserID = iUserID
                    })).FirstOrDefault();
                }
                else
                {
                    sbData.Append("ORDER BY Id DESC ");
                    iRet = (await connection.QueryAsync<int>(sbData.ToString())).FirstOrDefault();
                }

                return iRet;
            }
        }

        public async Task<string> GetLastTitle()
        {
            sbData.Clear();

            sbData.Append("SELECT Titulo from publicacao ");

            using (IDbConnection connection = sqlConnection)
            {
                string sRet;
                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append(" WHERE UserID = @UserID  ");
                    sbData.Append("ORDER BY Id DESC ");

                    sRet = (await connection.QueryAsync<string>(sbData.ToString(), new { UserID = iUserID })).SingleOrDefault();
                }
                else
                {
                    sbData.Append("ORDER BY Id DESC ");
                    sRet = (await connection.QueryAsync<string>(sbData.ToString())).SingleOrDefault();
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
            sbData.Clear();
            sbData.Append("SELECT COUNT(1) from publicacao ");
            sbData.Append("WHERE Id = @Id ");

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<bool>(sbData.ToString(), new { Id }))
                    .SingleOrDefault();
            }
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
                case Agrupamento.Genero:
                    sbData.Append("ORDER BY vwPubs.Categoria, vwPubs.Titulo");
                    break;
                case Agrupamento.Formato:
                    sbData.Append("ORDER BY vwPubs.Formato, vwPubs.Titulo");
                    break;
            }

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new { UserID = iUserID }))
                    .ToList();
            }

        }

        public async Task<List<PubsWithDescriptions>> Listar_Duplicados()
        {
            sbData.Clear();
            sbData.Append(GetBooksQuery());
            StringBuilder sbGeneralUser = sbData;

            if (userRole == UserRole.GeneralUser)
            {
                sbGeneralUser.Append("WHERE vwPubs.UserID = @UserID AND vwpubs.TituloOriginal IN(");
                sbGeneralUser.Append("SELECT TituloOriginal FROM vwPubs ");
                sbGeneralUser.Append("WHERE TituloOriginal IS NOT NULL AND TituloOriginal != '' ");
                sbGeneralUser.Append("GROUP BY TituloOriginal ");
                sbGeneralUser.Append("HAVING COUNT(TituloOriginal) > 1) ");
                sbGeneralUser.Append("ORDER BY vwpubs.TituloOriginal");
            }
            else
            {
                sbData.Append("WHERE vwpubs.TituloOriginal IN(");
                sbData.Append("SELECT TituloOriginal FROM vwPubs ");
                sbData.Append("WHERE TituloOriginal IS NOT NULL AND TituloOriginal != '' ");
                sbData.Append("GROUP BY TituloOriginal ");
                sbData.Append("HAVING COUNT(TituloOriginal) > 1) ");
                sbData.Append("ORDER BY vwpubs.TituloOriginal");
            }


            using (IDbConnection connection = sqlConnection)
            {
                if (userRole == UserRole.GeneralUser)
                {
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbGeneralUser.ToString(), new { UserID = iUserID }))
                        .ToList();
                }
                else
                {
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString()))
                        .ToList();
                }
            }

        }

        public async Task<PubsWithDescriptions> Listar_ID_Extended(int Id)
        {
            sbData.Clear();

            sbData.Append(GetBooksQuery());
            sbData.Append(" WHERE Id = @Id");
            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new
                { Id })).SingleOrDefault();
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_IDAutor(int Id)
        {

            try
            {
                sbData.Clear();
                sbData.Append(GetBooksQuery());

                using (IDbConnection connection = sqlConnection)
                {

                    if (userRole == UserRole.GeneralUser)
                    {
                        sbData.Append(" WHERE IdAutor = @Id AND UserID = @iUserID");
                        return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new
                        {
                            Id,
                            iUserID
                        })).ToList();
                    }
                    else
                    {
                        sbData.Append(" WHERE IdAutor = @Id");
                        return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new
                        {
                            Id
                        })).ToList();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_IDCategoria(int IdCategoria)
        {
            return await GetPubsFromID(IdCategoria, iUserID);
        }

        public async Task<List<PubsWithDescriptions>> Listar_IDEditora(int IdEditora)
        {
            sbData.Clear();
            sbData.Append(GetBooksQuery());

            using (IDbConnection connection = sqlConnection)
            {

                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append(" WHERE IdEditora = @IdEditora AND UserID = @UserID");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new { IdEditora, UserID = iUserID }))
                        .ToList();
                }
                else
                {
                    sbData.Append(" WHERE IdEditora = @IdEditora ");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new
                    {
                        IdEditora
                    })).ToList();
                }
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_IDFormato(int IdFormato)
        {
            sbData.Clear();
            sbData.Append(GetBooksQuery());

            using (IDbConnection connection = sqlConnection)
            {

                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append(" WHERE IdFormato = @IdFormato AND UserID = @UserID");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new { IdFormato, UserID = iUserID }))
                        .ToList();
                }
                else
                {
                    sbData.Append(" WHERE IdFormato = @IdFormato ");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new
                    {
                        IdFormato
                    })).ToList();
                }
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_IDLocalArquivo(int Id)
        {
            sbData.Clear();
            sbData.Append(GetBooksQuery());

            using (IDbConnection connection = sqlConnection)
            {
                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append("WHERE Id = @Id AND UserID = @UserID");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(),
                        new { Id, UserID = iUserID }))
                        .ToList();
                }
                else
                {
                    sbData.Append("WHERE Id = @Id ");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(),
                        new { Id }))
                        .ToList();
                }
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_Lidos(bool Lido)
        {
            sbData.Clear();
            sbData.Append(GetBooksQuery());

            using (IDbConnection connection = sqlConnection)
            {
                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append("WHERE Lido = @Lido AND UserID = @UserID");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(),
                        new { Lido, UserID = iUserID }))
                        .ToList();
                }
                else
                {
                    sbData.Append("WHERE Lido = @Lido");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new
                    {
                        Lido
                    })).ToList();
                }
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_Pagos(bool Pago)
        {
            string sExpr;
            if (Pago)
                sExpr = "[Preco] > 0";
            else
                sExpr = "[Preco] = 0";

            sbData.Clear();
            sbData.Append(GetBooksQuery());
            using (IDbConnection connection = sqlConnection)
            {
                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append($"WHERE {sExpr} AND UserID = @UserID");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new { UserID = iUserID }))
                        .ToList();
                }
                else
                {
                    sbData.Append($"WHERE {sExpr}");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString()))
                        .ToList();
                }
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_PalavraTitulo(string sWhereParam)
        {
            sbData.Clear();
            sbData.Append(GetBooksQuery());

            using (IDbConnection connection = sqlConnection)
            {
                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append($"WHERE {sWhereParam} AND UserID = @UserID");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new { UserID = iUserID }))
                        .ToList();
                }
                else
                {
                    sbData.Append($"WHERE {sWhereParam}");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString()))
                        .ToList();
                }
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_SemCapa()
        {
            sbData.Clear();
            sbData.Append(GetBooksQuery());

            using (IDbConnection connection = sqlConnection)
            {
                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append("WHERE ImagemCapa = '' AND UserID = @UserID");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new { UserID = iUserID }))
                        .ToList();
                }
                else
                {
                    sbData.Append("WHERE ImagemCapa = ''");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString())).ToList();
                }
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_SemPaginas()
        {
            sbData.Clear();
            sbData.Append(GetBooksQuery());

            using (IDbConnection connection = sqlConnection)
            {
                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append("WHERE Paginas = 0 AND UserID = @UserID");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new { UserID = iUserID }))
                        .ToList();
                }
                else
                {
                    sbData.Append("WHERE Paginas = 0");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString())).ToList();
                }
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

            using (IDbConnection connection = sqlConnection)
            {
                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append($" WHERE UserID = @UserID AND ({sWhereTitulos})");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new { UserID = iUserID }))
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
            sbData.Clear();

            sbData.Append(GetBooksQuery());
            sbData.Append(" WHERE UserID = @UserID");

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new { UserID = iUserID }))
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
            sbData.Clear();
            sbData.Append("SELECT 1 from publicacao ");
            sbData.Append("WHERE UserID = @UserID AND TituloOriginal = @TituloOriginal ");

            using (IDbConnection connection = sqlConnection)
            {
                int iRet = (await connection.QueryAsync<int>(sbData.ToString(), new { UserID = iUserID, TituloOriginal }))
                    .FirstOrDefault();

                return iRet > 0;
            }

        }

        public StringBuilder QueryDuplicates(string startFolder)
        {
            DirectoryInfo dir = new(startFolder);

            IEnumerable<FileInfo> fileList = dir.GetFiles("*.*", SearchOption.AllDirectories);

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
            sbData.Clear();
            sbData.Append("SELECT 1 from publicacao ");
            sbData.Append("WHERE UserID = @UserID AND TituloOriginal = @TituloOriginal ");
            using (IDbConnection connection = sqlConnection)
            {
                int iRet = (await connection.QueryAsync<int>(sbData.ToString(),
                    new { UserID = iUserID, TituloOriginal }))
                    .FirstOrDefault();

                return iRet > 0;
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
            sbData.Clear();
            sbData.Append(GetBooksQuery());
            using (IDbConnection connection = sqlConnection)
            {
                if (userRole == UserRole.GeneralUser)
                {
                    sbData.Append(" WHERE IdCategoria = @IdCategoria AND UserID = @UserID");
                    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new
                    {
                        IdCategoria,
                        UserID = iUserID
                    })).ToList();
                }
                else
                {
                    sbData.Append(" WHERE IdCategoria = @IdCategoria ");
                    return connection.Query<PubsWithDescriptions>(sbData.ToString(), new { IdCategoria }).ToList();
                }
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
            catch
            {
                throw;
            }
        }


        public async Task<List<PubsWithDescriptions>> GetPubsByTitle(string sTitulo)
        {
            return await Listar_Titulo(sTitulo);
        }

        public bool CopiaPublicacaoParaRepositorio(PublicacaoACopiar publicacao)
        {
            throw new NotImplementedException();
        }

        public string GetDuplicateFiles()
        {
            throw new NotImplementedException();
        }

        public async Task<List<PubsWithDescriptions>> Listar_AnoEdicao(int AnoEdicao)
        {
            sbData.Clear();
            sbData.Append("SELECT * ");
            sbData.Append("FROM vwPubs ");
            sbData.Append("WHERE AnoEdicao = @AnoEdicao");

            using (IDbConnection connection = sqlConnection)
            {
                return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), new { AnoEdicao }))
                    .ToList();
            }
        }

        public async Task<List<PubsWithDescriptions>> GetAllPubs_Grid()
        {
            using (IDbConnection connection = sqlConnection)
            {
                var output = (await connection.QueryAsync<PubsWithDescriptions>("SELECT * FROM vwPubs",
                    commandType: CommandType.Text))
                    .ToList();

                return output;
            }
        }
        public async Task<IEnumerable<PubsWithDescriptions>> GetAllPubs_Grid_Async()
        {
            return await GetAllPubs_Grid_Async();
        }

        public async Task<IEnumerable<ImageCategoryLocations>> GetImageLocations(int idCategoria)
        {
            sbData.Clear();
            sbData.Append("Id, IdCategoria, ImagemCapa ");
            sbData.Append("FROM [Publicacoes].[dbo].[Publicacao] ");
            sbData.Append("IdCategoria = @IdCategoria");

            try
            {
                using (IDbConnection connection = sqlConnection)
                {
                    var output = (await connection.QueryAsync<ImageCategoryLocations>(sbData.ToString(),
                        new { IdCategoria = idCategoria }));

                    return output;

                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdatePubsFromContextMenu(string sUpdateQuery)
        {
            try
            {
                using (IDbConnection connection = sqlConnection)
                {
                    int iCodigo = await connection.ExecuteAsync(sUpdateQuery);
                    return iCodigo > 0;
                }
            }
            catch
            {
                throw;
            }
        }

        public Task<List<FileCheckResult>> PubsNotFoundOnDisk(string webRoot)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StorePdfNotDeleted(PdfsAndPngNotDeleted fileName)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetPubFormat(int pubId)
        {
            sbData.Clear();
            sbData.Append("SELECT F.Id FROM Formato F ");
            sbData.Append("INNER JOIN Publicacao P ON F.Id = P.IdFormato ");
            sbData.Append("WHERE Id = @pubId");

            try
            {
                using (IDbConnection connection = sqlConnection)
                {
                    var output = (await connection.QueryFirstOrDefaultAsync<int>(sbData.ToString(),
                        new { id = pubId }));

                    return output;

                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<bool> CanAuthorBeDeleted(int authorId)
        {
            throw new NotImplementedException();
        }
    }
}
