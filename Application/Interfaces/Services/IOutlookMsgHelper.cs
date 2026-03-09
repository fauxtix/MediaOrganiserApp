using MediaOrganizerApp.Application.Enums;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services
{
    public interface IOutlookMsgHelper
    {
        Task<string> ConvertMsgToPdfAsync(string msgFilePath);
    }
}