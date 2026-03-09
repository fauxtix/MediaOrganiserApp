using MediaOrganizerApp.Application.DTOs;
using System.IO;
using System.Threading.Tasks;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Application.Interfaces.Services
{
    public interface IFileService
    {
        string DecodeImage(string image);
        Task<(string, string)> TransfereFicheiroParaRepositorio(PublicacaoACopiar publicacao);
        bool IsFileLocked(FileInfo file);
        Task<bool> DeletePhysicalPubPdf(PubsWithDescriptions pub);
    }
}