using MediaOrganizerApp.Application.Enums;
using System.Collections.Generic;
using System.IO;

namespace MediaOrganizerApp.Application.Interfaces.Services
{
    public interface IDocxHelper
    {
        int ContaPaginasDocWord(string ficheiro);
        string ConvertToPdf(string docxFile);
        Dictionary<AppDefinitions.Metadados, string> GetDocxMetadata(FileStream sFicheiro);
    }
}