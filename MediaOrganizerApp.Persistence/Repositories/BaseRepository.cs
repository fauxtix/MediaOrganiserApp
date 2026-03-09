using Dapper;
using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.Exceptions;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Utilities;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Repositories.RawSQLRepository
{
    public abstract class BaseRepository<T> : IBaseRepository<T>
    {
        //protected string connectionString = ApplicationConfiguration.ConnectionString;
        public async Task<long> Insert_Async(T entity)
        {
            var columns = GetColumns();
            var stringOfColumns = string.Join(", ", columns);
            var stringOfParameters = string.Join(", ", columns.Select(e => "@" + e));
            var query = $"INSERT INTO {typeof(T).Name} ({stringOfColumns}) VALUES ({stringOfParameters})";
            long RecordInserted = 0;
            try
            {
                using (var connection = ConnectionManager.GetConnection())
                {
                    var regs = await connection.ExecuteAsync(query, entity);
                    if (!typeof(T).Name.ToLower().Contains("categoria")) // tablehasdata requires one extra parameter (tipocategoria)
                        RecordInserted = await GetLastId_Async();
                }

            }
            catch (DataAccessException ex)
            {
                ex.DataAccessStatusInfo.Status = "Erro";
                ex.DataAccessStatusInfo.OperationSucceeded = false;
                ex.DataAccessStatusInfo.CustomMessage = $"Erro na inserção de registo (tabela '{typeof(T).Name}').";
                ex.DataAccessStatusInfo.ExceptionMessage = ex.Message;
                ex.DataAccessStatusInfo.StackTrace = ex.StackTrace;
                throw;
            }

            return RecordInserted;
        }

        public async Task Update_Async(T entity)
        {
            var columns = GetColumns();
            var stringOfColumns = string.Join(", ", columns.Select(e => $"{e} = @{e}"));
            var query = $"UPDATE {typeof(T).Name} SET {stringOfColumns} WHERE Id = @Id";

            try
            {
                using var connection = ConnectionManager.GetConnection();
                await connection.ExecuteAsync(query, entity);
            }
            catch (DataAccessException ex)
            {
                ex.DataAccessStatusInfo.Status = "Erro";
                ex.DataAccessStatusInfo.OperationSucceeded = false;
                ex.DataAccessStatusInfo.CustomMessage = $"Erro na atualização de registo (tabela '{typeof(T).Name}').";
                ex.DataAccessStatusInfo.ExceptionMessage = ex.Message;
                ex.DataAccessStatusInfo.StackTrace = ex.StackTrace;
                throw;
            }
        }

        public async Task DeleteById_Async(int Id)
        {
            var query = $"DELETE FROM {typeof(T).Name} WHERE Id = @Id";

            try
            {
                using var connection = ConnectionManager.GetConnection();
                await connection.ExecuteAsync(query, new { Id });
            }
            catch (DataAccessException ex)
            {
                ex.DataAccessStatusInfo.Status = "Erro";
                ex.DataAccessStatusInfo.OperationSucceeded = false;
                ex.DataAccessStatusInfo.CustomMessage = $"Erro na anulação de registo (tabela '{typeof(T).Name}').";
                ex.DataAccessStatusInfo.ExceptionMessage = ex.Message;
                ex.DataAccessStatusInfo.StackTrace = ex.StackTrace;
                throw;
            }
        }

        public virtual long Insert(T entity)
        {
            var columns = GetColumns();
            var stringOfColumns = string.Join(", ", columns);
            var stringOfParameters = string.Join(", ", columns.Select(e => "@" + e));
            var query = $"INSERT INTO {typeof(T).Name} ({stringOfColumns}) VALUES ({stringOfParameters})";
            long RecordInserted = 0;
            try
            {
                using (var connection = ConnectionManager.GetConnection())
                {
                    var regs = connection.Execute(query, entity);
                    RecordInserted = GetLastId();
                }

            }
            catch (DataAccessException ex)
            {
                ex.DataAccessStatusInfo.Status = "Erro";
                ex.DataAccessStatusInfo.OperationSucceeded = false;
                ex.DataAccessStatusInfo.CustomMessage = $"Erro na inserção de registo (tabela '{typeof(T).Name}').";
                ex.DataAccessStatusInfo.ExceptionMessage = ex.Message;
                ex.DataAccessStatusInfo.StackTrace = ex.StackTrace;
                throw;
            }

            return RecordInserted;
        }

        public virtual void DeleteById(int Id)
        {
            var query = $"DELETE FROM {typeof(T).Name} WHERE Id = @Id";

            try
            {
                using (var connection = ConnectionManager.GetConnection())
                {
                    connection.Execute(query, new { Id });
                }
            }
            catch (DataAccessException ex)
            {
                ex.DataAccessStatusInfo.Status = "Erro";
                ex.DataAccessStatusInfo.OperationSucceeded = false;
                ex.DataAccessStatusInfo.CustomMessage = $"Erro na anulação de registo (tabela '{typeof(T).Name}').";
                ex.DataAccessStatusInfo.ExceptionMessage = ex.Message;
                ex.DataAccessStatusInfo.StackTrace = ex.StackTrace;
                throw;
            }
        }

        public virtual void Update(T entity)
        {
            var columns = GetColumns();
            var stringOfColumns = string.Join(", ", columns.Select(e => $"{e} = @{e}"));
            var query = $"UPDATE {typeof(T).Name} SET {stringOfColumns} WHERE Id = @Id";

            try
            {
                using (var connection = ConnectionManager.GetConnection())
                {
                    connection.Execute(query, entity);
                }
            }
            catch (DataAccessException ex)
            {
                ex.DataAccessStatusInfo.Status = "Erro";
                ex.DataAccessStatusInfo.OperationSucceeded = false;
                ex.DataAccessStatusInfo.CustomMessage = $"Erro na atualização de registo (tabela '{typeof(T).Name}').";
                ex.DataAccessStatusInfo.ExceptionMessage = ex.Message;
                ex.DataAccessStatusInfo.StackTrace = ex.StackTrace;
                throw;
            }
        }

        public virtual IEnumerable<T> Query(string where = null)
        {
            var query = $"SELECT * FROM {typeof(T).Name} ";

            if (!string.IsNullOrWhiteSpace(where))
            {
                query += $"WHERE {where}";
            }

            using (var connection = ConnectionManager.GetConnection())
            {
                return connection.Query<T>(query);
            }
        }

        public virtual T Query_ById(int Id)
        {

            using (IDbConnection connection = ConnectionManager.GetConnection())
            {
                return connection.Query<T>($"SELECT * FROM {typeof(T).Name} WHERE Id=@Id",
                    new { Id }).FirstOrDefault();
            }
        }


        private static IEnumerable<string> GetColumns()
        {
            return typeof(T)
                    .GetProperties()
                    .Where(e => e.Name != "Id" && !e.PropertyType.GetTypeInfo().IsGenericType)
                    .Select(e => e.Name);
        }


        public virtual bool EntradaExiste_BD(string campo, string str)
        {
            using (IDbConnection connection = ConnectionManager.GetConnection())
            {
                var output = Query($"{campo} = '{str}'");
                if (output.Any())
                    return true;
            }
            return false;
        }

        public bool TableHasData()
        {
            using (var connection = ConnectionManager.GetConnection())
            {
                string sql = $"SELECT * FROM {typeof(T).Name}";
                int result = connection.Query<int>(sql).Count();
                return result > 0;
            }
        }

        public int GetFirstId()
        {
            string sql = "";
            if (TableHasData())
            {
                using (var connection = ConnectionManager.GetConnection())
                {
                    if (Utilitarios.IsSQLServer() || Utilitarios.IsAccess())
                        sql = $"SELECT TOP 1 Id FROM {typeof(T).Name} ORDER BY Id ASC";
                    else if (Utilitarios.IsSQLite() || Utilitarios.IsMySQL())
                        sql = $"SELECT Id FROM {typeof(T).Name} ORDER BY Id ASC LIMIT 1";

                    int result = connection.Query<int>(sql).FirstOrDefault();
                    return result;
                }
            }
            else
                return 0;
        }

        public bool RecInUse(int Id)
        {
            var query = $"SELECT COUNT(1) FROM {typeof(T).Name} WHERE Id = @Id";

            using (var connection = ConnectionManager.GetConnection())
            {
                bool exists = connection.Query<bool>(query, new { Id }).SingleOrDefault();
                return exists;
            }
        }

        public bool RecInUse_ByDescription(int tipo, string description)
        {
            var query = $"SELECT COUNT(1) FROM {typeof(T).Name} WHERE Tipo = @Tipo AND Descricao = @Descricao";

            using (var connection = ConnectionManager.GetConnection())
            {
                bool exists = connection.Query<bool>(query, new { Tipo = tipo, Descricao = description }).SingleOrDefault();
                return exists;
            }
        }

        public int GetIdByDescription(string Description)
        {
            var query = $"SELECT Id FROM {typeof(T).Name} WHERE Descricao = @Description";

            using var connection = ConnectionManager.GetConnection();
            int Id = connection.Query<int>(query, new { Description }).SingleOrDefault();
            return Id;
        }

        public int GetLastId()
        {
            string sql = "";
            if (TableHasData())
            {
                using (var connection = ConnectionManager.GetConnection())
                {
                    if (Utilitarios.IsSQLServer() || Utilitarios.IsAccess())
                        sql = $"SELECT TOP 1 Id FROM {typeof(T).Name} ORDER BY Id DESC";
                    else if (Utilitarios.IsSQLite() || Utilitarios.IsMySQL())
                        sql = $"SELECT Id FROM {typeof(T).Name} ORDER BY Id DESCENDING LIMIT 1";

                    int lastId = connection.Query<int>(sql).FirstOrDefault();
                    return lastId;
                }
            }
            else
                return 0;
        }

        public List<T> Listar()
        {
            StringBuilder sbData = new();
            sbData.Clear();
            sbData.Append("SELECT * ");
            sbData.Append($"FROM {typeof(T).Name} ");
            sbData.Append("ORDER BY Descricao");
            using (var connection = ConnectionManager.GetConnection())
            {
                return connection.Query<T>(sbData.ToString()).ToList();
            }
        }

        public IEnumerable<T> ListByDescription(string Descricao)
        {
            StringBuilder sbData = new();

            sbData.Clear();
            sbData.Append("SELECT * ");
            sbData.Append($"FROM {typeof(T).Name} ");
            sbData.Append("WHERE Descricao LIKE '%" + Descricao + "%'");
            using (var connection = ConnectionManager.GetConnection())
            {
                return connection.Query<T>(sbData.ToString()).ToList();
            }
        }

        public IDataReader ListByDescription_Reader(string Descricao)
        {
            StringBuilder sbData = new();

            sbData.Clear();
            sbData.Append("SELECT * ");
            sbData.Append($"FROM {typeof(T).Name} ");
            if (!string.IsNullOrEmpty(Descricao))
                sbData.AppendFormat("WHERE Descricao LIKE '%{0}%' ", Descricao);

            var connection = ConnectionManager.GetConnection();

            return connection.ExecuteReader(sbData.ToString());
        }

        public IEnumerable<T> Listar_ID(int Id)
        {
            StringBuilder sbData = new();

            sbData.Append("SELECT * ");
            sbData.Append($"FROM {typeof(T).Name} ");
            sbData.Append("WHERE Id = @Id ");
            using (var connection = ConnectionManager.GetConnection())
            {
                return connection.Query<T>(sbData.ToString(), new { Id });
            }
        }

        public IDataReader Listar_ID_DR(int Id)
        {
            StringBuilder sbData = new();

            sbData.Append("SELECT * ");
            sbData.Append($"FROM {typeof(T).Name} ");
            sbData.Append("WHERE Id = @Id ");
            var connection = ConnectionManager.GetConnection();

            return connection.ExecuteReader(sbData.ToString(), new { Id });
        }

        public string GetDescriptionById(int Id)
        {
            StringBuilder sbData = new();

            sbData.Clear();
            sbData.Append("SELECT Descricao ");
            sbData.Append($"FROM {typeof(T).Name} ");
            sbData.Append("WHERE Id = @Id");

            using (var connection = ConnectionManager.GetConnection())
            {
                string sRet = connection.Query<string>(sbData.ToString(), new { Id }).SingleOrDefault();
                return sRet;
            }
        }

        public async Task<IEnumerable<T>> Query_Async(string where = null)
        {
            var query = $"SELECT * FROM {typeof(T).Name} ";

            if (!string.IsNullOrWhiteSpace(where))
            {
                query += $"WHERE {where}";
            }

            using (var connection = ConnectionManager.GetConnection())
            {
                return await connection.QueryAsync<T>(query);
            }
        }

        public async Task<T> Query_ById_Async(int Id)
        {
            using (IDbConnection connection = ConnectionManager.GetConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<T>($"SELECT * FROM {typeof(T).Name} WHERE Id=@Id",
                    new { Id });
            }
        }

        public async Task<int> GetIdByDescription_Async(string Description)
        {
            var query = $"SELECT Id FROM {typeof(T).Name} WHERE Descricao = @Description";

            using (var connection = ConnectionManager.GetConnection())
            {
                int Id = await connection.QuerySingleOrDefaultAsync<int>(query, new { Description });
                return Id;
            }
        }

        public async Task<string> GetDescriptionById_Async(int Id)
        {
            StringBuilder sbData = new();

            sbData.Clear();
            sbData.Append("SELECT Descricao ");
            sbData.Append($"FROM {typeof(T).Name} ");
            sbData.Append("WHERE Id = @Id");

            using (var connection = ConnectionManager.GetConnection())
            {
                string sRet = await connection.QuerySingleOrDefaultAsync<string>(sbData.ToString(), new { Id });
                return sRet;
            }
        }

        public async Task<List<T>> Listar_Async()
        {
            StringBuilder sbData = new();
            sbData.Clear();
            sbData.Append("SELECT * ");
            sbData.Append($"FROM {typeof(T).Name} ");
            sbData.Append("ORDER BY Descricao");
            using (var connection = ConnectionManager.GetConnection())
            {
                return (await connection.QueryAsync<T>(sbData.ToString())).ToList();
            }
        }

        public async Task<IEnumerable<T>> ListByDescription_Async(string Descricao)
        {
            StringBuilder sbData = new();

            sbData.Clear();
            sbData.Append("SELECT * ");
            sbData.Append($"FROM {typeof(T).Name} ");
            sbData.Append("WHERE Descricao LIKE '%" + Descricao + "%'");
            using (var connection = ConnectionManager.GetConnection())
            {
                return (await connection.QueryAsync<T>(sbData.ToString())).ToList();
            }
        }

        public async Task<IDataReader> ListByDescription_Reader_Async(string Descricao)
        {
            StringBuilder sbData = new();

            sbData.Clear();
            sbData.Append("SELECT * ");
            sbData.Append($"FROM {typeof(T).Name} ");
            if (!string.IsNullOrEmpty(Descricao))
                sbData.AppendFormat("WHERE Descricao LIKE '%{0}%' ", Descricao);

            var connection = ConnectionManager.GetConnection();

            return await connection.ExecuteReaderAsync(sbData.ToString());
        }

        public async Task<IEnumerable<T>> Listar_ID_Async(int Id)
        {
            StringBuilder sbData = new();

            sbData.Append("SELECT * ");
            sbData.Append($"FROM {typeof(T).Name} ");
            sbData.Append("WHERE Id = @Id ");
            using (var connection = ConnectionManager.GetConnection())
            {
                return await connection.QueryAsync<T>(sbData.ToString(), new { Id });
            }
        }

        public async Task<IDataReader> Listar_ID_DR_Async(int Id)
        {
            StringBuilder sbData = new();

            sbData.Append("SELECT * ");
            sbData.Append($"FROM {typeof(T).Name} ");
            sbData.Append("WHERE Id = @Id ");
            var connection = ConnectionManager.GetConnection();

            return await connection.ExecuteReaderAsync(sbData.ToString(), new { Id });
        }

        public async Task<bool> EntradaExiste_BD_Async(string campo, string str)
        {
            using (IDbConnection connection = ConnectionManager.GetConnection())
            {
                var output = await Query_Async($"{campo} = '{str}'");
                if (output.Any())
                    return true;
            }
            return false;
        }

        public async Task<bool> RecInUse_Async(int Id)
        {
            var query = $"SELECT COUNT(1) FROM {typeof(T).Name} WHERE Id = @Id";

            using (var connection = ConnectionManager.GetConnection())
            {
                bool exists = (await connection.QueryAsync<bool>(query, new { Id })).SingleOrDefault();
                return exists;
            }
        }

        public async Task<bool> RecInUse_ByDescription_Async(int tipo, string description)
        {
            var query = $"SELECT COUNT(1) FROM {typeof(T).Name} WHERE Tipo = @Tipo AND Descricao = @Descricao";

            using (var connection = ConnectionManager.GetConnection())
            {
                bool exists = (await connection.QueryAsync<bool>(query, new { Tipo = tipo, Descricao = description })).SingleOrDefault();
                return exists;
            }
        }

        public async Task<bool> TableHasData_Async()
        {
            using (var connection = ConnectionManager.GetConnection())
            {
                string sql = $"SELECT * FROM {typeof(T).Name}";
                int result = (await connection.QueryAsync<int>(sql)).Count();
                return result > 0;
            }
        }

        public async Task<int> GetFirstId_Async()
        {
            if (await TableHasData_Async())
            {
                string sql = "";
                if (TableHasData())
                {
                    using (var connection = ConnectionManager.GetConnection())
                    {
                        if (Utilitarios.IsSQLServer() || Utilitarios.IsAccess())
                            sql = $"SELECT TOP 1 Id FROM {typeof(T).Name} ORDER BY Id ASC";
                        else if (Utilitarios.IsSQLite() || Utilitarios.IsMySQL())
                            sql = $"SELECT Id FROM {typeof(T).Name} ORDER BY Id ASC LIMIT 1";

                        int result = (await connection.QueryAsync<int>(sql)).FirstOrDefault();
                        return result;
                    }
                }
                else
                    return 0;
            }
            else
                return 0;
        }

        public async Task<int> GetLastId_Async()
        {
            string sql;
            if (await TableHasData_Async())
            {
                using (var connection = ConnectionManager.GetConnection())
                {
                    sql = $"SELECT Id FROM {typeof(T).Name} ORDER BY Id DESCENDING LIMIT 1";
                    int lastId = (await connection.QueryAsync<int>(sql)).FirstOrDefault();
                    return lastId;
                }
            }
            else
                return 0;
        }

        public async Task<IEnumerable<SummaryData>> GetCount()
        {
            StringBuilder sb = new();
            sb.Append("SELECT JT.Descricao, COUNT(1) ");
            sb.Append("from publicacao P ");
            sb.Append($"INNER JOIN {typeof(T).Name} JT ");
            sb.Append("ON P.IdCategoria = JT.Id ");
            sb.Append("GROUP BY JT.Descricao ");
            sb.Append("ORDER BY COUNT(1) DESC");

            try
            {
                using (var connection = ConnectionManager.GetConnection())
                {
                    var list = await connection.QueryAsync<SummaryData>(sb.ToString());
                    return list;
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
