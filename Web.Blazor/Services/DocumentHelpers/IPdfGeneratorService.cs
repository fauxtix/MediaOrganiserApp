using MediaOrganizerApp.Application.DTOs;

namespace Web.Blazor.Services.DocumentHelpers
{
    public interface IPdfGeneratorService
    {
        byte[] GeneratePublicacoesPdf(List<PubsWithDescriptions> publicacoes);
        byte[] CreateMediaPdf(List<MediaVM> mediaList);
        byte[] CreateAuthorsPdf(List<AutorDTO> authorsList);
    }
}