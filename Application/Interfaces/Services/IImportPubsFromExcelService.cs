using System;
using System.Data;

namespace MediaOrganizerApp.Application.Interfaces.Services
{
    public interface IImportPubsFromExcelService
    {
        event Action<int, string> ProgressChanged;

        DataTable GetTabelaFromExcel(string sExcelFileName);
        int TrataDadosExcel(DataTable dt);
        int TrataDadosExcel2(DataTable dt);
    }
}