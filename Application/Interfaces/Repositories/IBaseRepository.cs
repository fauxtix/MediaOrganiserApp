using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IBaseRepository<T>
    {
        long Insert(T entity);
        void Update(T entity);
        void DeleteById(int Id);

        Task<long> Insert_Async(T entity);
        Task Update_Async(T entity);
        Task DeleteById_Async(int Id);

        Task<IEnumerable<SummaryData>> GetCount();
        bool EntradaExiste_BD(string campo, string str);
        Task<bool> EntradaExiste_BD_Async(string campo, string str);

        bool RecInUse(int Id);
        bool RecInUse_ByDescription(int tipo, string sDescricao);
        bool TableHasData();
        Task<bool> RecInUse_Async(int Id);
        Task<bool> RecInUse_ByDescription_Async(int tipo, string sDescricao);
        Task<bool> TableHasData_Async();

        int GetFirstId();
        int GetLastId();
        Task<int> GetFirstId_Async();
        Task<int> GetLastId_Async();

        IEnumerable<T> Query(string where = null);
        Task<IEnumerable<T>> Query_Async(string where = null);
        T Query_ById(int Id);
        Task<T> Query_ById_Async(int Id);
        int GetIdByDescription(string Description);
        string GetDescriptionById(int Id);
        Task<int> GetIdByDescription_Async(string Description);
        Task<string> GetDescriptionById_Async(int Id);


        List<T> Listar();
        IEnumerable<T> ListByDescription(string Description);
        IDataReader ListByDescription_Reader(string Descricao);
        IEnumerable<T> Listar_ID(int Id);
        IDataReader Listar_ID_DR(int Id);

        Task<List<T>> Listar_Async();
        Task<IEnumerable<T>> ListByDescription_Async(string Description);
        Task<IDataReader> ListByDescription_Reader_Async(string Descricao);
        Task<IEnumerable<T>> Listar_ID_Async(int Id);
        Task<IDataReader> Listar_ID_DR_Async(int Id);
    }
}