using MediaOrganizerApp.Application.DTOs.AI;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Web.Blazor.Pages.Tools
{
    public partial class TranslateText
    {
        [Parameter] public string Title { get; set; } = "Translate Text";
        [Parameter] public bool Visible { get; set; }
        [Parameter] public string InitialText { get; set; } = string.Empty;
        [Parameter] public string InitialSourceLanguage { get; set; } = string.Empty;
        [Parameter] public string InitialTargetLanguage { get; set; } = string.Empty;
        [Parameter] public Func<TranslateRequestDto, System.Threading.CancellationToken, Task<TranslateResponseDto?>>? TranslateFunc { get; set; }
        [Parameter] public EventCallback<TranslateRequestDto> OnTranslateRequested { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }


        string InputText { get; set; } = string.Empty;
        string SourceTextNoHtml { get; set; } = string.Empty;
        string SourceLanguage { get; set; } = string.Empty;
        string TargetLanguage { get; set; } = string.Empty;
        string TranslatedText { get; set; } = string.Empty;
        double? Confidence { get; set; }
        string ErrorMessage { get; set; } = string.Empty;
        bool IsTranslating { get; set; } = false;

        List<LanguageItem> languages = new()
    {
        new LanguageItem("pt","Português"),
        new LanguageItem("en","English"),
        new LanguageItem("es","Español"),
        new LanguageItem("fr","Français")
    };

        IEnumerable<LanguageItem> targetLanguages => languages.Where(l => l.Value != SourceLanguage);

        bool CanTranslate => !string.IsNullOrWhiteSpace(InputText) && !string.IsNullOrWhiteSpace(SourceLanguage) && !string.IsNullOrWhiteSpace(TargetLanguage) && SourceLanguage != TargetLanguage;

        protected override async Task OnParametersSetAsync()
        {
            // Copy incoming parameters to internal fields first to avoid races
            // between the synchronous and asynchronous lifecycle methods.
            if (!string.IsNullOrWhiteSpace(InitialText))
            {
                InputText = InitialText;
            }

            if (!string.IsNullOrWhiteSpace(InitialSourceLanguage))
            {
                SourceLanguage = InitialSourceLanguage;
            }

            if (!string.IsNullOrWhiteSpace(InitialTargetLanguage))
            {
                TargetLanguage = InitialTargetLanguage;
            }

            // Existing async initialization when dialog becomes visible
            if (Visible)
            {
                SourceTextNoHtml = StripHtml(InputText ?? string.Empty);
                await InvokeAsync(StateHasChanged);
            }
        }
        private async Task Translate()
        {
            IsTranslating = true;
            ErrorMessage = string.Empty;
            await InvokeAsync(StateHasChanged);
            await Task.Delay(50);
            TranslatedText = string.Empty;
            Confidence = null;

            TranslatedText = "";

            if (!CanTranslate)
            {
                ErrorMessage = L["msgFillAllFields"] ?? "Please fill text, source and target languages and ensure they differ.";
                return;
            }

            var req = new TranslateRequestDto { Text = SourceTextNoHtml, SourceLanguage = SourceLanguage, TargetLanguage = TargetLanguage };

            try
            {
                TranslateResponseDto? resp = null;
                if (TranslateFunc is not null)
                {
                    resp = await TranslateFunc(req, System.Threading.CancellationToken.None);
                }
                else if (OnTranslateRequested.HasDelegate)
                {
                    await OnTranslateRequested.InvokeAsync(req);
                }

                if (resp is not null)
                {
                    if (!string.IsNullOrWhiteSpace(resp.ErrorMessage))
                    {
                        // API returned an error message (for example Ollama server unreachable)
                        ErrorMessage = resp.ErrorMessage;
                        TranslatedText = string.Empty;
                        Confidence = resp.Confidence;
                    }
                    else
                    {
                        TranslatedText = resp.TranslatedText ?? string.Empty;
                        Confidence = resp.Confidence;
                        ErrorMessage = string.Empty;
                    }
                }
                else
                {
                    // No response (non-success status or exception in client). Show a friendly message.
                    ErrorMessage = L["msgTranslateServiceUnavailable"] ?? "Translation service is unavailable (server error or network issue).";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsTranslating = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        private Task Copy() => Task.CompletedTask;

        private async Task ClearError()
        {
            ErrorMessage = string.Empty;
            await InvokeAsync(StateHasChanged);
        }

        private static string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            try
            {
                return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
            }
            catch
            {
                return input;
            }
        }

        private async Task ApplyTranslated()
        {
            if (!string.IsNullOrWhiteSpace(TranslatedText))
            {
                var dto = new TranslateRequestDto { Text = TranslatedText, SourceLanguage = SourceLanguage, TargetLanguage = TargetLanguage };
                if (OnTranslateRequested.HasDelegate)
                {
                    await OnTranslateRequested.InvokeAsync(dto);
                }
            }
        }

        private async Task OnExit()
        {
            Visible = false;
            if (OnClose.HasDelegate)
                await OnClose.InvokeAsync();
        }

        private class LanguageItem
        {
            public LanguageItem(string v, string t) { Value = v; Text = t; }
            public string Value { get; set; }
            public string Text { get; set; }
        }
    }
}