using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.AI
{
    public interface IAiSettingsProvider
    {
        Task<(string BaseUrl, string Model)> GetSettingsAsync(CancellationToken cancellationToken = default);
        void Invalidate();
    }
}
