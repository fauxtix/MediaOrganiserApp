using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services
{
    public interface IUtilitiesService
    {
        DataTable ConvertToDataTable<T>(IList<T> data);
        int GetCodeFromTable(string sQuery);
        int GetPkValueFromTable(string sTabela, string sColuna1, string sColuna2, string sFind);
        void SetupSqliteTables();
        Task SetupSqlServerTables();
        bool TabelasAuxuliaresComRegistos();
        bool UpdatePubsFromContextMenu(string sQuery);
        Task<bool> BackupDatabase(string backupPath);
    }
}