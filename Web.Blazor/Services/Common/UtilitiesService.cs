using Dapper;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces.Services;
using Microsoft.Extensions.Localization;
using System.ComponentModel;
using System.Data;
using System.Text;

namespace Web.Blazor.Services.Common
{
    public class UtilitiesService : IUtilitiesService
    {
        private readonly ILogger<UtilitiesService> _logger;
        private readonly IStringLocalizer<App> localizerApp;
        public UtilitiesService(ILogger<UtilitiesService> logger)
        {
            _logger = logger;
        }
        public bool TabelasAuxuliaresComRegistos()
        {
            string sQuery = "";
            bool bRet = true;
            string[] aTabelas = new string[] { "LocalArquivo", "Autor", "Categoria", "Editora", "Formato" };
            int[] aCnt = new int[5];
            int i = 0;
            int iCnt;
            try
            {
                foreach (string sTabela in aTabelas)
                {
                    sQuery = $"SELECT COUNT(*) FROM {sTabela}";

                    using (var connection = ConnectionManager.GetConnection())
                    {
                        iCnt = connection.Query<int>(sQuery).SingleOrDefault();
                        aCnt[i] = iCnt;
                        i++;
                    }
                }
                for (int x = 0; x < aTabelas.Length; x++)
                {
                    if (aCnt[x] == 0)
                    {
                        bRet = false;
                        break;
                    }
                }

            }
            catch (Exception exc)
            {
                ////Logger.WriteLog(exc);
                throw new ApplicationException(exc.Message);
            }
            return bRet;

        }

        public int GetPkValueFromTable(string sTabela, string sColuna1, string sColuna2, string sFind)
        {
            StringBuilder sb = new();
            sb.Append($"SELECT COUNT(*) FROM  {sTabela} ");
            sb.Append($"WHERE {sColuna2} LIKE '%{sFind}%'");
            try
            {
                using (var connection = ConnectionManager.GetConnection())
                {
                    int iRet = connection.ExecuteScalar<int>(sb.ToString());
                    if (iRet > 0)
                    {
                        sb.Clear();
                        sb.Append($"SELECT {sColuna1} FROM {sTabela} ");
                        sb.Append($"WHERE {sColuna2} LIKE '%{sFind}%'");
                        iRet = connection.Query<int>(sb.ToString()).FirstOrDefault();  // singleordefault
                        return iRet;
                    }
                    else
                        return 0;
                }
            }
            catch (Exception ex)
            {
                ////Logger.WriteLog(ex);
                throw new ApplicationException(ex.StackTrace);
            }
        }

        public DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
               TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;

        }

        public void SetupSqliteTables()
        {
            string sDelete;
            string sReset = "";
            string[] aTabelas = new string[] { "Publicacao", "Media", "LocalArquivo",
                "Autor", "Categoria", "Editora", "Formato" };

            try
            {
                foreach (string sTabela in aTabelas)
                {
                    sDelete = $"DELETE FROM {sTabela}";
                    sReset = $"DELETE FROM SQLITE_SEQUENCE WHERE name = '{sTabela}'";
                    using (var connection = ConnectionManager.GetConnection())
                    {
                        connection.Execute(sDelete.ToString());
                        connection.Execute(sReset.ToString());
                    }
                }

                using (var connection = ConnectionManager.GetConnection())
                {
                    connection.Execute("DELETE FROM user_info WHERE Id > 1"); // temos que manter o user Admin (User_id = 1)
                }

                // Valores 'default' para tabelas auxiliares
                InicializaTabela("LocalArquivo", "Descricao", "Desconhecido");
                InicializaTabela("Autor", "Descricao", "Desconhecido");
                InicializaTabela("Categoria", "Descricao", "Desconhecida");
                InicializaTabela("Editora", "Descricao", "Desconhecida");
                InicializaTabela("Formato", "Descricao", "Desconhecido");
            }
            catch (Exception exc)
            {
                ////Logger.WriteLog(exc);
                throw new ApplicationException(exc.Message);
            }
        }

        private void InicializaTabela(string sTabela, string sCampo, string InitialValue)
        {
            try
            {
                string sQuery = $"INSERT INTO {sTabela} ({sCampo}) VALUES ('{InitialValue}')";
                using (var connection = ConnectionManager.GetConnection())
                {
                    connection.Execute(sQuery);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int GetCodeFromTable(string sQuery)
        {
            try
            {
                using (var connection = ConnectionManager.GetConnection())
                {
                    int iCodigo = connection.Query<int>(sQuery).FirstOrDefault();
                    return iCodigo;
                }
            }
            catch (Exception ex)
            {
                ////Logger.WriteLog(ex);
                throw new ApplicationException(ex.StackTrace);
            }
        }


        /// <summary>
        ///  replaced by service / controller action / repository entry
        /// </summary>
        /// <param name="sUpdateQuery"></param>
        /// <returns></returns>
        public bool UpdatePubsFromContextMenu(string sUpdateQuery)
        {
            try
            {
                using (var connection = ConnectionManager.GetConnection())
                {
                    int iCodigo = (int)connection.Execute(sUpdateQuery);
                    return iCodigo > 0;
                }

            }
            catch (Exception ex)
            {
                ////Logger.WriteLog(ex);
                throw new ApplicationException(ex.StackTrace);
            }
        }


        public async Task<bool> BackupDatabase(string backupPath)
        {
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            StringBuilder sb = new();
            sb.Append("BACKUP DATABASE ");
            sb.Append($"Publicacoes_Prod TO DISK='{backupPath}");
            sb.Append($@"\{DateTime.Now:ddMMyyyy_HHmmss}.Bak'");

            try
            {
                using (var connection = ConnectionManager.GetConnection())
                {
                    await connection.ExecuteAsync(sb.ToString());
                    return true;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($@"Verifique premissões para 'NT Service\MSSQL$SQLEXPRESS' para a pasta de backups - {ex.Message}", ex);
                return false;
            }
        }

        public async Task SetupSqlServerTables()
        {
            string sDelete;
            string sReset = "";
            string[] aTabelas = new string[] { "Publicacao", "Media", "LocalArquivo",
                "Autor", "Categoria", "Editora", "Formato", "Formato_Media"  };

            try
            {
                foreach (string sTabela in aTabelas)
                {
                    sDelete = string.Format("DELETE FROM {0}", sTabela);
                    sReset = string.Format("DBCC CHECKIDENT ('{0}', RESEED, 0)", sTabela);

                    using (var connection = ConnectionManager.GetConnection())
                    {
                        await connection.ExecuteAsync(sDelete.ToString());

                        // Iniciar identity/autonumber = 1
                        await connection.ExecuteAsync(sReset.ToString());
                    }
                }

                //using (var connection = ConnectionManager.GetConnection())
                //{
                //    await connection.ExecuteAsync("DELETE FROM user_info WHERE User_id > 1"); // temos que manter o user Admin (User_id = 1)
                //}


                // Valores 'default' para tabelas auxiliares (livros/ebooks)
                InicializaTabela("LocalArquivo", "Descricao", "Desconhecido");
                InicializaTabela("Autor", "Descricao", "Desconhecido");
                InicializaTabela("Categoria", "Descricao", "Desconhecida");
                InicializaTabela("Editora", "Descricao", "Desconhecida");
                InicializaTabela("Formato", "Descricao", "Desconhecido");
                InicializaTabela("Formato_Media", "Descricao", "Desconhecido");
            }
            catch (Exception exc)
            {
                _logger.LogError(exc.Message);
                throw new ApplicationException(exc.Message);
            }
        }
    }
}
