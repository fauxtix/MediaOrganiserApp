using BlazorInputFile;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services
{
    public interface IFileUpload
    {
        Task UploadAsync(IFileListEntry arquivo);
    }
}
