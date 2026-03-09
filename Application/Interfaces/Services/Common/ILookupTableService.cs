using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.LookupTables;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Common
{
    public interface ILookupTableService
    {
        Task<bool> CheckIfRecordExist(string description, string tableName);
        Task<string> GetDescription(int id, string tableName);
        Task<IEnumerable<LookupTableVM>> GetLookupTableData(string tableName);

        Task<bool> ActualizaDetalhes(int Codigo, string Descricao, string Tabela);
        Task<bool> CheckRegistoExist(string Descricao, string Tabela);
        Task<bool> CriaNovoRegisto(string Descricao, string Tabela);
        Task<bool> DeleteRegisto(int iCodigo, string Tabela);

        Task<int> GetCodByDescricao(string sDescr, string sTabela);
        int GetId(string Descricao, string Tabela);

        Task<bool> CheckFKInUse(int IdFK, string fieldToCheck, string tableToCheck);
        Task<int> GetLastInsertedId(string tableToCheck);
        List<string> RegistoComErros<T>(T model);
    }
}
