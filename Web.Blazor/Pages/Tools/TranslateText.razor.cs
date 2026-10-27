using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.DTOs.AI;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Web.Blazor.Services.Common;

namespace Web.Blazor.Pages.Tools
{
    public partial class TranslateText : IAsyncDisposable
    {
        [Inject] public IConfiguration Configuration { get; set; } = default!;
        [Inject] public NavigationManager NavigationManager { get; set; } = default!;
        [Inject] public BooksInfoProgressClient ProgressClient { get; set; } = default!;

        [Parameter] public bool Visible { get; set; }
        [Parameter] public string InitialText { get; set; } = string.Empty;
        [Parameter] public string InitialSourceLanguage { get; set; } = string.Empty;
        [Parameter] public string InitialTargetLanguage { get; set; } = string.Empty;
        [Parameter] public Func<TranslateRequestDto, System.Threading.CancellationToken, Task<TranslateResponseDto?>>? TranslateFunc { get; set; }
        [Parameter] public EventCallback<TranslateRequestDto> OnTranslateRequested { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        string Title = "";
        string InputText { get; set; } = string.Empty;
        string SourceTextNoHtml { get; set; } = string.Empty;
        string SourceLanguage { get; set; } = string.Empty;
        string TargetLanguage { get; set; } = string.Empty;
        string TranslatedText { get; set; } = string.Empty;
        string ProgressMessage { get; set; } = string.Empty;
        double? Confidence { get; set; }
        string ErrorMessage { get; set; } = string.Empty;
        bool IsTranslating { get; set; } = false;

        // Set when the user cancels: the animated "waiting" block is replaced by a
        // static "cancelled by the user" note until the next attempt or dialog close.
        bool IsCancelled { get; set; } = false;
        private bool _progressClientOwned;
        private string? _activeCorrelationId;
        private bool _wasVisible;
        private bool _progressHandlersAttached;

        // Provider/model that produced the finished translation, straight from the response
        // DTO. Authoritative: it survives a lost progress event.
        string? ResultProvider { get; set; }
        string? ResultModel { get; set; }

        // Provider/model that was in flight when the user cancelled. A cancellation never
        // returns a response body (the request is aborted), so the progress channel is the
        // only source - this is what explains a slow translation that fell back to Ollama.
        string? AttemptedProvider { get; set; }
        string? AttemptedModel { get; set; }

        bool HasResultModel => !string.IsNullOrWhiteSpace(ResultModel);
        bool HasAttemptedModel => !string.IsNullOrWhiteSpace(AttemptedModel);

        /// <summary>
        /// The finished answer wins over the model that was merely in flight, and the chip stays
        /// hidden while translating because the progress message already names the model.
        /// </summary>
        bool ShowModelChip => !IsTranslating && (HasResultModel || HasAttemptedModel);
        string? ShownProvider => HasResultModel ? ResultProvider : AttemptedProvider;
        string? ShownModel => HasResultModel ? ResultModel : AttemptedModel;

        /// <summary>OpenRouter and Ollama are product names and are never localized.</summary>
        string ProviderLabel(string? provider) => provider switch
        {
            "openrouter" => "OpenRouter",
            "ollama" => "Ollama",
            _ => provider ?? string.Empty
        };

        string ProviderIcon(string? provider) => provider == "ollama" ? "fa fa-desktop" : "fa fa-cloud";

        /// <summary>
        /// Falls back to "Automatic" when the answer can only be attributed to the
        /// openrouter/free router, matching the model pill in the chats.
        /// </summary>
        string ModelLabel(string? model)
            => string.IsNullOrWhiteSpace(model)
                ? string.Empty
                : string.Equals(model, CloudChatSettingsKeys.DefaultModelValue, StringComparison.OrdinalIgnoreCase)
                    ? L["CloudChatAutomaticModel"].Value
                    : model;

        string ModelTooltip(string? provider, string? model, bool attempted)
        {
            if (string.IsNullOrWhiteSpace(model))
                return string.Empty;

            var label = attempted
                ? string.Format(L["TraducaoModeloTentado"].Value, model)
                : string.Format(L["TraducaoModelo"].Value, model);
            var providerLabel = ProviderLabel(provider);
            return string.IsNullOrWhiteSpace(providerLabel) ? label : $"{providerLabel} - {label}";
        }

        // Owns the token of the in-flight translation so the user (or the dialog close /
        // component disposal) can abort it. The request is abandoned immediately on the UI;
        // any late answer from a superseded call is discarded by the guard in Translate().
        private CancellationTokenSource? _cts;

        List<LanguageItem> languages = new()
    {
        new LanguageItem("pt","Português"),
        new LanguageItem("en","English"),
        new LanguageItem("es","Español"),
        new LanguageItem("fr","Français")
    };

        IEnumerable<LanguageItem> targetLanguages => languages.Where(l => l.Value != SourceLanguage);

        bool CanTranslate => !string.IsNullOrWhiteSpace(InputText) && !string.IsNullOrWhiteSpace(SourceLanguage) && !string.IsNullOrWhiteSpace(TargetLanguage) && SourceLanguage != TargetLanguage;

        protected override async Task OnInitializedAsync()
        {
            Title = L["TituloTraduzirTexto"];
            await base.OnInitializedAsync();
        }
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
            else if (_wasVisible)
            {
                // dialog was closed: drop the "cancelled" note and abort anything still in flight
                CancelPendingSilently();
                await CleanupProgressClientAsync();
            }

            _wasVisible = Visible;
        }
        private async Task Translate()
        {
            if (!CanTranslate)
            {
                ErrorMessage = L["msgFillAllFields"] ?? "Please fill text, source and target languages and ensure they differ.";
                return;
            }

            IsTranslating = true;
            IsCancelled = false;
            ErrorMessage = string.Empty;
            ProgressMessage = string.Empty;
            ClearModelInfo();
            await InvokeAsync(StateHasChanged);
            await Task.Delay(50);
            TranslatedText = string.Empty;
            Confidence = null;

            TranslatedText = "";

            var cts = new CancellationTokenSource();
            _cts = cts;

            try
            {
                await EnsureProgressClientAsync();
                _activeCorrelationId = Guid.NewGuid().ToString("N");
                await ProgressClient.JoinCorrelationAsync(_activeCorrelationId);

                var req = new TranslateRequestDto
                {
                    Text = SourceTextNoHtml,
                    SourceLanguage = SourceLanguage,
                    TargetLanguage = TargetLanguage,
                    ConnectionId = ProgressClient.ConnectionId,
                    CorrelationId = _activeCorrelationId
                };

                TranslateResponseDto? resp = null;
                if (TranslateFunc is not null)
                {
                    await InvokeAsync(StateHasChanged);
                    await Task.Yield();
                    resp = await TranslateFunc(req, cts.Token);
                }
                else if (OnTranslateRequested.HasDelegate)
                {
                    await OnTranslateRequested.InvokeAsync(req);
                }

                // A cancelled or superseded call must never write state: the user already moved on
                // (or closed the dialog) and a late answer would overwrite the text/field.
                if (cts.IsCancellationRequested || !ReferenceEquals(_cts, cts))
                    return;

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
                        // Who actually answered: the sweep makes this unpredictable, and a
                        // local fallback is the usual reason for a slow translation.
                        ResultProvider = resp.Provider;
                        ResultModel = resp.Model;
                    }
                }
                else
                {
                    // No response (non-success status or exception in client). Show a friendly message.
                    ErrorMessage = L["msgTranslateServiceUnavailable"] ?? "Translation service is unavailable (server error or network issue).";
                }
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                // Deliberate cancel: no error message, the UI already shows "cancelled by the user".
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(_activeCorrelationId))
                {
                    try
                    {
                        await ProgressClient.LeaveCorrelationAsync(_activeCorrelationId);
                    }
                    catch (Exception)
                    {
                        // after an immediate cancel + close the progress hub may already be gone
                    }
                    _activeCorrelationId = null;
                }

                if (ReferenceEquals(_cts, cts))
                    _cts = null;

                cts.Dispose();

                IsTranslating = false;

                try
                {
                    await InvokeAsync(StateHasChanged);
                }
                catch (ObjectDisposedException)
                {
                    // the dialog was closed right after cancelling: nothing left to paint
                }
            }
        }

        /// <summary>
        /// Aborts the in-flight translation. The UI stops immediately - it never waits for the
        /// provider to acknowledge the cancel (local Ollama may keep inferring on its own).
        /// </summary>
        private async Task CancelTranslate()
        {
            var cts = _cts;
            if (cts is null || cts.IsCancellationRequested)
                return;

            IsTranslating = false;
            IsCancelled = true;
            ProgressMessage = string.Empty;
            cts.Cancel();

            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Cancels the pending translation without touching UI state (dialog is closing or the
        /// component is being disposed). Disposal is left to the owning Translate() call.
        /// </summary>
        private void CancelPendingSilently()
        {
            var cts = _cts;
            _cts = null;
            IsCancelled = false;
            ClearModelInfo();

            if (cts is null)
                return;

            try
            {
                cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // the owning call already finished and disposed the source
            }
        }

        private void ClearModelInfo()
        {
            ResultProvider = null;
            ResultModel = null;
            AttemptedProvider = null;
            AttemptedModel = null;
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
            // Abort before closing so a late answer cannot write into the closed dialog
            // (it would otherwise overwrite the field the translation was requested for).
            CancelPendingSilently();
            await CleanupProgressClientAsync();
            Visible = false;
            if (OnClose.HasDelegate)
                await OnClose.InvokeAsync();
        }

        public async ValueTask DisposeAsync()
        {
            CancelPendingSilently();
            await CleanupProgressClientAsync();
        }

        private async Task EnsureProgressClientAsync()
        {
            var wasConnected = !string.IsNullOrWhiteSpace(ProgressClient.ConnectionId);

            var apiBase = Configuration["BaseUrl"]?.TrimEnd('/') ?? NavigationManager.BaseUri.TrimEnd('/');
            if (apiBase.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                apiBase = apiBase.Substring(0, apiBase.Length - 4);
            }

            await ProgressClient.StartAsync(apiBase);
            if (!wasConnected && !string.IsNullOrWhiteSpace(ProgressClient.ConnectionId))
            {
                _progressClientOwned = true;
            }

            EnsureProgressSubscriptions();
        }

        private async Task CleanupProgressClientAsync()
        {
            RemoveProgressSubscriptions();

            if (_progressClientOwned)
            {
                await ProgressClient.StopAsync();
                _progressClientOwned = false;
            }
        }

        private void EnsureProgressSubscriptions()
        {
            if (_progressHandlersAttached)
                return;

            ProgressClient.ProgressReceived += OnProgressReceived;
            ProgressClient.ProgressEventReceived += OnProgressEventReceived;
            _progressHandlersAttached = true;
        }

        private void RemoveProgressSubscriptions()
        {
            if (!_progressHandlersAttached)
                return;

            ProgressClient.ProgressReceived -= OnProgressReceived;
            ProgressClient.ProgressEventReceived -= OnProgressEventReceived;
            _progressHandlersAttached = false;
        }

        private void OnProgressReceived(string message)
        {
            _ = UpdateProgressMessageAsync(message);
        }

        private void OnProgressEventReceived(ProgressUpdateDto update)
        {
            if (update is null)
                return;

            TrackModelInFlight(update);

            var text = update.Stage switch
            {
                "preparing_prompt" or "request_sent" => L["AIProcessingPreparing"].Value,
                "waiting_response" => L["AIProcessingWaiting"].Value,
                "response_received" or "parsing" => L["AIProcessingParsing"].Value,
                "completed" => L["AIProcessingDone"].Value,
                "failed" => L["AIProcessingFailed"].Value,
                "cancelled" => L["AIProcessingCancelled"].Value,
                "cloud_try" => string.Format(L["AIProcessingCloudTrying"].Value, update.Message),
                "cloud_failed" => string.Format(L["AIProcessingCloudFailed"].Value, update.Message),
                "cloud_empty" => string.Format(L["AIProcessingCloudEmpty"].Value, update.Message),
                _ => update.Message
            };
            _ = UpdateProgressMessageAsync(text);
        }

        /// <summary>
        /// Remembers which provider/model is handling the request right now, so a cancellation
        /// can report it (the aborted request never returns a body). Falls back to the stage
        /// message when the structured field is missing.
        /// </summary>
        private void TrackModelInFlight(ProgressUpdateDto update)
        {
            var model = update.Model;
            if (string.IsNullOrWhiteSpace(model)
                && update.Stage is "cloud_try" or "cloud_success"
                && !string.IsNullOrWhiteSpace(update.Message))
            {
                model = update.Message;
            }

            if (string.IsNullOrWhiteSpace(model))
                return;

            AttemptedModel = model;
            AttemptedProvider = update.Provider;
        }

        private async Task UpdateProgressMessageAsync(string? message)
        {
            try
            {
                await InvokeAsync(() =>
                {
                    ProgressMessage = message ?? string.Empty;
                    StateHasChanged();
                });
            }
            catch (ObjectDisposedException)
            {
                // ignore during component disposal
            }
        }

        private class LanguageItem
        {
            public LanguageItem(string v, string t) { Value = v; Text = t; }
            public string Value { get; set; }
            public string Text { get; set; }
        }
    }
}
