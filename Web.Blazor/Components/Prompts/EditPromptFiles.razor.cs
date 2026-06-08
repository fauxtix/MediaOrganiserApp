using Ganss.Xss;
using Markdig;
using Syncfusion.Blazor.Notifications;

namespace Web.Blazor.Components.Prompts
{
    public partial class EditPromptFiles
    {
        List<string> PromptFiles = new();
        string SelectedPromptFile;
        private string _editorContent;
        public string EditorContent
        {
            get => _editorContent;
            set
            {
                if (_editorContent != value)
                {
                    _editorContent = value;
                    Logger.LogInformation("EditorContent updated: {Content}", _editorContent);
                }
            }
        }
        string ReadmeMd;
        protected SfToast ToastObj;
        protected string ToastTitle = "Prompt Editor";
        protected string ToastContent = "";
        protected string ToastCssClass = "";

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var files = await PromptService.GetPromptFilesAsync();
                PromptFiles = files.Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)).ToList();
                var readme = await PromptService.GetReadmeContentAsync();
                var pipeline = new Markdig.MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .Build();

                var raw = readme?.Replace("\r\n", "\n") ?? string.Empty;
                var html = Markdig.Markdown.ToHtml(raw, pipeline);

                var sanitizer = new HtmlSanitizer();
                sanitizer.AllowedAttributes.Add("target");
                sanitizer.AllowedAttributes.Add("rel");
                sanitizer.AllowedAttributes.Add("class");
                sanitizer.AllowedSchemes.Add("http");
                sanitizer.AllowedSchemes.Add("https");
                sanitizer.AllowedSchemes.Add("mailto");

                var safe = sanitizer.Sanitize(html);

                ReadmeMd = $"<article class=\"markdown-body\">{safe}</article>";
            }

            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading prompt files or README.md");
                ReadmeMd = $"Erro ao carregar README.md: {ex.Message}";
            }
        }


        private async Task OnPromptSelected(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, string> e)
        {

            try
            {
                SelectedPromptFile = e.Value?.ToString();
                if (!string.IsNullOrEmpty(SelectedPromptFile))
                {
                    var content = await PromptService.GetPromptFileContentAsync(SelectedPromptFile);
                    EditorContent = content;
                }
                else
                {
                    EditorContent = null;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving prompt atual");

                ToastContent = "Erro ao gravar prompt atual";
                ToastCssClass = "e-danger";
                ToastTitle = "Editor de Prompts";
            }
        }

        private async Task OnSaved()
        {
            ToastContent = "Prompt salvo com sucesso!";
            ToastCssClass = "e-success";
            ToastTitle = "Editor de Prompts";
            await ToastObj.ShowAsync();
        }

        private async Task OnSave()
        {
            try
            {
                Logger.LogInformation("OnSave triggered. EditorContent: {Content}", EditorContent);

                if (!string.IsNullOrEmpty(SelectedPromptFile))
                {
                    await PromptService.SavePromptFileAsync(SelectedPromptFile, EditorContent);
                    await OnSaved();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in saving prompt");

                ToastContent = "Erro ao gravar ficheiro";
                ToastCssClass = "e-danger";
                ToastTitle = "Editor de Prompts";
                await ToastObj.ShowAsync();
            }
        }

        private void OnCancel()
        {
            _ = ReloadPromptContentAsync();
        }

        private async Task ReloadPromptContentAsync()
        {
            if (!string.IsNullOrEmpty(SelectedPromptFile))
            {
                EditorContent = await PromptService.GetPromptFileContentAsync(SelectedPromptFile);

                ToastContent = "Ficheiro recarregado com sucesso!";
                ToastCssClass = "e-warning";
                ToastTitle = "Editor de Prompts";
                StateHasChanged();
                await ToastObj.ShowAsync();
            }
        }

    }
}