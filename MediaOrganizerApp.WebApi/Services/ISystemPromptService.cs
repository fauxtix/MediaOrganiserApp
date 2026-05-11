namespace MediaOrganizerApp.WebApi.Services
{
    public interface ISystemPromptService
    {
        Task<string> GetSystemPrompt(CancellationToken cancellationToken);
    }
}