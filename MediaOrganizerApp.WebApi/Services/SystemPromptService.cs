namespace MediaOrganizerApp.WebApi.Services
{
    public class SystemPromptService : ISystemPromptService
    {
        private readonly IPromptTemplateProvider _prompts;
        public SystemPromptService(IPromptTemplateProvider prompts)
        {
            _prompts = prompts;
        }

        public async Task<string> GetSystemPrompt(CancellationToken cancellationToken)
        {
            var systemTemplate = await _prompts.GetTemplateAsync("system.prompt.txt", cancellationToken);

            return PromptTemplateRenderer.Render(systemTemplate, new Dictionary<string, string?>());
        }
    }
}
