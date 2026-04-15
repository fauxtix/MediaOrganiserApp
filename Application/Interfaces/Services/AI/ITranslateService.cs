using MediaOrganizerApp.Application.DTOs.AI;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.AI;

public interface ITranslateService
{
    Task<TranslateResponseDto> TranslateAsync(TranslateRequestDto request, CancellationToken cancellationToken);
}
