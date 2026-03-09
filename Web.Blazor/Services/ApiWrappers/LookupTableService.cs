using FluentValidation;
using FluentValidation.Results;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.Common;
using MediaOrganizerApp.Application.LookupTables;
using MediaOrganizerApp.Shared.Validators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Blazor.Services.ApiWrappers
{
    public interface ILookupTableService
    {
        Task<bool> ActualizaDetalhes(int Codigo, string Descricao, string Tabela);
        Task<bool> CheckFKInUse(int IdFK, string fieldToCheck, string tableToCheck);
        Task<bool> CheckIfRecordExist(string description, string tableName);
        Task<bool> CheckRegistoExist(string Descricao, string Tabela);
        Task<bool> CriaNovoRegisto(string Descricao, string Tabela);
        Task<bool> DeleteRegisto(int iCodigo, string Tabela);
        Task<int> GetCodByDescricao(string sDescr, string sTabela);
        string GetDescription(int id, string tableName);
        int GetId(string Descricao, string Tabela);
        Task<int> GetLastInsertedId(string tableToCheck);
        Task<IEnumerable<LookupTableVM>> GetLookupTableData(string tableName);
        List<string> RegistoComErros<T>(T model);
    }

    public class LookupTableService : ILookupTableService
    {
        private readonly ILookupTableRepository _repoLookupTable;
        public LookupTableService(ILookupTableRepository repoLookupTable)
        {
            _repoLookupTable = repoLookupTable;
        }

        public Task<bool> ActualizaDetalhes(int Codigo, string Descricao, string Tabela)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckFKInUse(int IdFK, string fieldToCheck, string tableToCheck)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckIfRecordExist(string description, string tableName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckRegistoExist(string Descricao, string Tabela)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CriaNovoRegisto(string Descricao, string Tabela)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteRegisto(int iCodigo, string Tabela)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCodByDescricao(string sDescr, string sTabela)
        {
            throw new NotImplementedException();
        }

        public string GetDescription(int id, string tableName)
        {
            return _repoLookupTable.GetDescription(id, tableName);
        }

        public int GetId(string Descricao, string Tabela)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetLastInsertedId(string tableToCheck)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<LookupTableVM>> GetLookupTableData(string tableName)
        {
            return await _repoLookupTable.GetLookupTableData(tableName);
        }

        Task<string> ILookupTableService.GetDescription(int id, string tableName)
        {
            throw new NotImplementedException();
        }
        public List<string> RegistoComErros<T>(T model)
        {
            throw new NotImplementedException();
        }

    }
}
