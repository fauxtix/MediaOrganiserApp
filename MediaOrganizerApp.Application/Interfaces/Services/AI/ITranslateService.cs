using MediaOrganizerApp.Application.DTOs.AI;

namespace MediaOrganizerApp.Application.Interfaces.Services.AI;

public interface ITranslateService
{
    Task<TranslateResponseDto> TranslateAsync(TranslateRequestDto request, CancellationToken cancellationToken);
}
