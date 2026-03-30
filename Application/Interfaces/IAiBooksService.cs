using MediaOrganizerApp.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces
{
    public interface IAiBooksService
    {
        // Query books for an author. Do not require a 'maxResults' parameter here;
        // implementations should use their own defaults or internal limits.
        Task<List<GoogleBookInfo>> GetBooksByAuthorAsync(string author, string language, CancellationToken cancellationToken = default);
    }
}
