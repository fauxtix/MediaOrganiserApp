using MediaOrganizerApp.Application.Enums;
using System.Collections.Generic;
using System.Drawing;

namespace MediaOrganizerApp.Application.Interfaces.Services
{
    public interface IPdfHelper
    {
        int getNumberOfPdfPages(string ficheiro);
        Image GetPdfCover_Sync(string sPathImagensPubs, string ficheiro, out string sFichGravado);
        Dictionary<AppDefinitions.Metadados, string> GetPdfMetadata_Sync(string sFicheiro);
    }
}