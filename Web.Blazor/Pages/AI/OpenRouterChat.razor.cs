using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Notifications;
using System.Globalization;
using System.Text.Json;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.DTOs.AI.CloudChat;
using MediaOrganizerApp.Application.Utilities;
using Web.Blazor.Components.Chat;
using Web.Blazor.Services.ApiWrappers;
using Web.Blazor.Services.CloudChat;

namespace Web.Blazor.Pages.AI
{
    public partial class OpenRouterChat : IAsyncDisposable
    {
        private const string ModeloRouterFree = "openrouter/free";

        // Coalescing do stream: acumula deltas SSE e só faz render quando o texto
        // pendente atinge o limiar de caracteres OU decorreu o intervalo desde o último
        // render — mesma cadência de rendering do Chat.razor (Ollama), evitando
        // StateHasChanged/FormatMessage/scroll por delta minúsculo.
        private const int CoalesceMinChars = 400;
        private const int CoalesceIntervalMs = 50;

        [Inject] private ConversationsClientService ConversationsClient { get; set; } = null!;
        [Inject] private CloudChatClientService CloudChatClient { get; set; } = null!;
        [Inject] private ChatFeedbackClientService ChatFeedbackClient { get; set; } = null!;
        [Inject] private CurrentUserIdentity UserIdentity { get; set; } = null!;
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private IStringLocalizer<App> Localizer { get; set; } = null!;
        [Inject] private ILogger<OpenRouterChat> Logger { get; set; } = null!;

        private readonly List<CloudChatUiMessage> messages = new();
        private CloudChatUiMessage? currentAssistantMessage;
        private string userInput = "";
        private string? lastUserPrompt;
        private string welcomeMessage = "";
        private string _systemPrompt = "You are a helpful assistant. Respond concisely.";

        private string _selectedModel = ModeloRouterFree;
        private bool _enableReasoning;

        private int _contextLength = ContextWindowResolver.GetDefaultLimit();
        private int _contextUsedTokens;
        private bool _showContextBar;
        private string _contextResolvedForModel = ModeloRouterFree;

        private Dictionary<string, int> _contextLengthCatalog = new(StringComparer.OrdinalIgnoreCase);
        private List<OpenRouterCatalogModel> _catalogModels = new();

        private bool isProcessing;
        private bool isConnecting;
        private double _elapsed;
        private System.Threading.Timer? _elapsedTimer;
        private CancellationTokenSource? chatCts;
        private DotNetObjectReference<OpenRouterChat>? dotNetRef;
        private ElementReference messagesDiv;
        private ElementReference chatInputRef;
        private bool userAtBottom = true;

        private HistoryPanel? historyPanel;

        private List<ConversationItem> conversations = new();
        private string? selectedConversationId;

        private readonly Dictionary<int, int> _feedbackByIndex = new();

        private readonly Dictionary<int, bool> _reasoningToggled = new();

        private SfToast? toastObj;
        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";
        public int ToastTimeOut { get; set; } = 3000;

        protected override async Task OnInitializedAsync()
        {
            welcomeMessage = Localizer["ChatWelcome"];

            var (userId, _) = await UserIdentity.GetAsync();

            _systemPrompt = $"You are a helpful assistant. Respond in {TargetLanguageResolver.GetTargetLanguage()}. Be concise and accurate. Never output content-safety or moderation classifications (such as \"User Safety: safe\"). Always answer the user's request directly.";

            await LoadConversations();

            try
            {
                _catalogModels = await CloudChatClient.GetModelsAsync();
                _contextLengthCatalog = _catalogModels
                    .Where(m => m.ContextLength is > 0)
                    .GroupBy(m => m.Id, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().ContextLength!.Value, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Nao foi possivel obter o catalogo OpenRouter para o indicador de contexto.");
            }

            try
            {
                var runtimeSettings = await CloudChatClient.GetRuntimeSettingsAsync();
                if (runtimeSettings is not null)
                {
                    _selectedModel = string.IsNullOrWhiteSpace(runtimeSettings.DefaultModel) ? ModeloRouterFree : runtimeSettings.DefaultModel;
                    _enableReasoning = runtimeSettings.EnableReasoning;
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Nao foi possivel obter as definicoes runtime cloud chat.");
            }

            _contextLength = ContextWindowResolver.GetContextLength("OpenRouter", _selectedModel, _contextLengthCatalog);
            _contextResolvedForModel = _selectedModel;

            messages.Add(new CloudChatUiMessage { Role = "assistant", Content = welcomeMessage, IsWelcome = true });
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                dotNetRef = DotNetObjectReference.Create(this);
                await Task.Delay(50);
                await JS.InvokeVoidAsync("chatInput.attachHandlers", chatInputRef, dotNetRef);
            }
        }

        private string FormatMessage(string? content, bool showCursor)
        {
            content ??= string.Empty;

            if (string.IsNullOrEmpty(content))
                return showCursor ? "<span class='cursor'>|</span>" : "";

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseSoftlineBreakAsHardlineBreak()
                .Build();

            var html = Markdown.ToHtml(content, pipeline);
            html = html.TrimEnd('\n', '\r', ' ');
            html = html.Replace("<p>", "<div>").Replace("</p>", "</div>");

            if (showCursor)
            {
                var trimmed = html.TrimEnd();

                if (trimmed.EndsWith("</div>", StringComparison.OrdinalIgnoreCase))
                {
                    html = trimmed[..^6] + "<span class='cursor'>|</span></div>";
                }
                else
                {
                    html += "<span class='cursor'>|</span>";
                }
            }

            return html;
        }

        private async Task Send()
        {
            if (string.IsNullOrWhiteSpace(userInput) || isProcessing || isConnecting)
                return;

            var userText = userInput.Trim();
            userInput = string.Empty;
            lastUserPrompt = userText;

            if (_contextLength > 0 && _contextUsedTokens + ChatComposerService.EstimateTokens(userText) > _contextLength)
            {
                await ShowWarningToastAsync(
                    Localizer["ChatContextExceeded"],
                    string.Format(Localizer["ChatContextExceededDetail"], _contextLength));
                return;
            }

            var userMsg = new CloudChatUiMessage { Role = "user", Content = userText };
            messages.Add(userMsg);

            var prepared = ChatComposerService.Prepare(
                messages.Select(m => new CloudChatMessage { Role = m.Role, Content = m.Content }).ToList(),
                userText,
                _systemPrompt,
                welcomeMessage,
                string.IsNullOrWhiteSpace(_selectedModel) ? ModeloRouterFree : _selectedModel,
                _contextLength,
                _enableReasoning);

            if (prepared is null)
                return;

            userMsg.Temperature = Math.Round(prepared.Temperature, 2);

            _contextUsedTokens += ChatComposerService.EstimateTokens(userText);
            _showContextBar = true;

            currentAssistantMessage = new CloudChatUiMessage
            {
                Role = "assistant",
                Content = "..."
            };
            messages.Add(currentAssistantMessage);

            await InvokeAsync(StateHasChanged);
            await SafeScrollToBottomAsync();

            chatCts?.Cancel();
            chatCts?.Dispose();
            chatCts = new CancellationTokenSource();
            var localToken = chatCts.Token;

            isProcessing = true;
            _elapsed = 0;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _elapsedTimer = new System.Threading.Timer(_ =>
            {
                if (!isProcessing) return;

                stopwatch.Stop();
                var seconds = stopwatch.Elapsed.TotalSeconds;
                stopwatch.Start();
                _elapsed = seconds;

                if (currentAssistantMessage is not null)
                {
                    currentAssistantMessage.ElapsedTime = seconds < 10
                        ? $"{seconds:F2}s"
                        : $"{seconds:F1}s";
                }

                _ = InvokeAsync(StateHasChanged);
            }, null, 0, 150);

            await InvokeAsync(StateHasChanged);
            await SafeScrollToBottomAsync();

            try
            {
                var firstChunkReceived = false;
                string? servedModel = null;
                var pendingText = new System.Text.StringBuilder();
                var pendingReasoning = new System.Text.StringBuilder();
                var lastRenderAt = DateTime.UtcNow;

                await foreach (var chunk in CloudChatClient.StreamChatAsync(prepared.Payload, LanguageCode, localToken))
                {
                    if (!firstChunkReceived)
                    {
                        firstChunkReceived = true;
                    }

                    if (currentAssistantMessage is null)
                    {
                        currentAssistantMessage = new CloudChatUiMessage { Role = "assistant", Content = "" };
                        messages.Add(currentAssistantMessage);
                    }

                    if (!string.IsNullOrEmpty(chunk.Text))
                    {
                        pendingText.Append(chunk.Text);
                        _contextUsedTokens += ChatComposerService.EstimateTokens(chunk.Text);
                    }

                    if (!string.IsNullOrWhiteSpace(chunk.Reasoning))
                    {
                        pendingReasoning.Append(chunk.Reasoning);
                        _contextUsedTokens += ChatComposerService.EstimateTokens(chunk.Reasoning);
                    }

                    ApplyRealUsage(chunk);
                    ResolveRealModelContext(chunk);

                    if (!string.IsNullOrWhiteSpace(chunk.Model))
                    {
                        servedModel = chunk.Model;
                        if (!string.Equals(servedModel, prepared.Payload.Model, StringComparison.OrdinalIgnoreCase))
                            currentAssistantMessage.RealModelName = servedModel;
                    }

                    _elapsed = stopwatch.Elapsed.TotalSeconds;

                    var now = DateTime.UtcNow;
                    var renderDue = pendingText.Length >= CoalesceMinChars
                        || pendingReasoning.Length >= CoalesceMinChars
                        || ((pendingText.Length > 0 || pendingReasoning.Length > 0) && (now - lastRenderAt).TotalMilliseconds >= CoalesceIntervalMs);

                    if (!renderDue)
                        continue;

FlushPendingText(currentAssistantMessage, pendingText);
                    FlushPendingReasoning(currentAssistantMessage, pendingReasoning);
                    lastRenderAt = now;

                    await InvokeAsync(StateHasChanged);

                    if (userAtBottom)
                    {
                        try { await JS.InvokeVoidAsync("scrollMessagesToBottom", messagesDiv); } catch { }
                    }
                }

                FlushPendingText(currentAssistantMessage, pendingText);

                var hasAnswer = false;
                if (currentAssistantMessage is not null)
                {
                    if (currentAssistantMessage.Content == "...")
                        currentAssistantMessage.Content = string.Empty;

                    var (_, cleanText) = ChatComposerService.ExtractSummaryFromResponse(currentAssistantMessage.Content);
                    string finalContent;
                    if (ChatSafetyArtifactFilter.IsSafetyOnly(cleanText))
                    {
                        finalContent = Localizer["ChatSafetyArtifactHint"];
                    }
                    else
                    {
                        finalContent = ChatSafetyArtifactFilter.StripSafetyLines(cleanText);
                        if (string.IsNullOrWhiteSpace(finalContent))
                            finalContent = Localizer["ChatEmptyResponseHint"];
                        else
                            hasAnswer = true;
                    }

                    currentAssistantMessage.Content = finalContent;
                    currentAssistantMessage.IsError = !hasAnswer;
                    currentAssistantMessage.LanguageWarning = LanguageWarningFor(finalContent);
                }

                await SaveCurrentConversationAsync();

                if (hasAnswer)
                    await ShowSuccessToastAsync(Localizer["ChatResponseReceived"]);
                else
                    await ShowWarningToastAsync(Localizer["Chat"], currentAssistantMessage?.Content ?? Localizer["ChatEmptyResponseHint"]);
            }
            catch (OperationCanceledException)
            {
                if (currentAssistantMessage is not null)
                {
                    if (currentAssistantMessage.Content == "...") currentAssistantMessage.Content = string.Empty;
                    currentAssistantMessage.Content += "\n\n⚠️ " + Localizer["ChatCancelled"];
                }

                await ShowWarningToastAsync(Localizer["Chat"], Localizer["ChatCancelled"]);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro durante o streaming cloud chat.");

                if (currentAssistantMessage is not null)
                {
                    if (currentAssistantMessage.Content == "...") currentAssistantMessage.Content = string.Empty;
                    currentAssistantMessage.Content += "\n\n⚠️ " + string.Format(Localizer["ChatError"], ex.Message);
                    currentAssistantMessage.IsError = true;
                }

                await ShowWarningToastAsync(Localizer["Chat"], ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                _elapsed = stopwatch.Elapsed.TotalSeconds;
                _elapsedTimer?.Dispose();
                _elapsedTimer = null;

                if (currentAssistantMessage is not null)
                {
                    currentAssistantMessage.ElapsedTime = _elapsed < 10
                        ? $"{_elapsed:F2}s"
                        : $"{_elapsed:F1}s";
                }

                currentAssistantMessage = null;

                chatCts?.Dispose();
                chatCts = null;
                isProcessing = false;

                try { await JS.InvokeVoidAsync("focusElement", chatInputRef); } catch { }

                await InvokeAsync(StateHasChanged);
                await SafeScrollToBottomAsync();
            }
        }

        private string LanguageCode
        {
            get
            {
                try
                {
                    return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                }
                catch
                {
                    return "pt";
                }
            }
        }

        private string? LanguageWarningFor(string? text)
        {
            var detected = ResponseLanguageChecker.Detect(text);

            if (!ResponseLanguageChecker.IsMismatch(detected, LanguageCode))
                return null;

            return string.Format(Localizer["ChatLanguageWarning"], TargetLanguageResolver.GetTargetLanguage());
        }

        private static void FlushPendingText(CloudChatUiMessage? message, System.Text.StringBuilder pendingText)
        {
            if (message is null || pendingText.Length == 0)
                return;

            var text = pendingText.ToString();
            pendingText.Clear();
            message.Content = message.Content == "..." ? text : message.Content + text;
        }

        private static void FlushPendingReasoning(CloudChatUiMessage? message, System.Text.StringBuilder pendingReasoning)
        {
            if (message is null || pendingReasoning.Length == 0)
                return;

            var reasoning = pendingReasoning.ToString();
            pendingReasoning.Clear();
            message.Reasoning = (message.Reasoning ?? string.Empty) + reasoning;
        }

        private void ApplyRealUsage(CloudChatChunk chunk)
        {
            if (chunk.TotalTokens is > 0)
            {
                _contextUsedTokens = chunk.TotalTokens.Value;
            }
            else if (chunk.PromptTokens is > 0 || chunk.CompletionTokens is > 0)
            {
                _contextUsedTokens = (chunk.PromptTokens ?? 0) + (chunk.CompletionTokens ?? 0);
            }
        }

        private void ResolveRealModelContext(CloudChatChunk chunk)
        {
            if (string.IsNullOrWhiteSpace(chunk.Model))
                return;

            if (string.Equals(chunk.Model, _contextResolvedForModel, StringComparison.OrdinalIgnoreCase))
                return;

            if (!ContextWindowResolver.TryGetKnownContextLength("OpenRouter", chunk.Model, _contextLengthCatalog, out var realLength))
                return;

            _contextLength = realLength;
            _contextResolvedForModel = chunk.Model;
        }

        private string GetContextPercent()
        {
            if (_contextLength <= 0) return "0%";
            double ratio = Math.Clamp((double)_contextUsedTokens / _contextLength, 0, 1);
            return (ratio * 100).ToString("0.##", CultureInfo.InvariantCulture) + "%";
        }

        private string GetContextColor()
        {
            if (_contextLength <= 0) return "var(--app-primary)";
            double ratio = (double)_contextUsedTokens / _contextLength;
            if (ratio >= 0.9) return "var(--app-accent-yellow)";
            return "var(--app-primary)";
        }

        private int ContextRemaining => Math.Max(0, _contextLength - _contextUsedTokens);

        private async Task SafeScrollToBottomAsync()
        {
            try
            {
                await Task.Delay(50);
                if (messagesDiv.Context != null)
                    await JS.InvokeVoidAsync("scrollMessagesToBottom", messagesDiv);
            }
            catch { }
        }

        private async Task OnScroll()
        {
            try
            {
                if (messagesDiv.Context is null) return;
                var state = await JS.InvokeAsync<ScrollState>("chatScroll.getScrollState", messagesDiv);
                if (state is null) return;
                userAtBottom = (state.ScrollHeight - state.ScrollTop - state.ClientHeight) < 80;
            }
            catch { }
        }

        [JSInvokable]
        public async Task OnEnterPressedFromJs()
        {
            await Task.Delay(10);
            await Send();
        }

        private Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "ArrowUp")
            {
                if (!string.IsNullOrWhiteSpace(lastUserPrompt) && string.IsNullOrWhiteSpace(userInput))
                {
                    userInput = lastUserPrompt;
                }
            }
            else if (e.Key == "ArrowDown")
            {
                if (userInput == lastUserPrompt)
                {
                    userInput = string.Empty;
                }
            }

            return Task.CompletedTask;
        }

        private async Task CancelSend()
        {
            try { chatCts?.Cancel(); } catch { }
            try { chatCts?.Dispose(); chatCts = null; } catch { }
            isProcessing = false;
            await InvokeAsync(StateHasChanged);
        }

        private int GetFeedback(int index)
            => _feedbackByIndex.TryGetValue(index, out var value) ? value : 0;

        private bool IsReasoningExpanded(int index, CloudChatUiMessage msg)
            => _reasoningToggled.TryGetValue(index, out var expanded) && expanded;

        private void ToggleReasoning(int index)
        {
            if (index < 0 || index >= messages.Count) return;
            _reasoningToggled[index] = !IsReasoningExpanded(index, messages[index]);
        }

        private async Task CopyMessageAsync(CloudChatUiMessage msg)
        {
            if (string.IsNullOrWhiteSpace(msg.Content)) return;

            try
            {
                var ok = await JS.InvokeAsync<bool>("copyText", msg.Content);
                if (ok)
                    await ShowSuccessToastAsync(Localizer["ChatResponseCopied"]);
                else
                    await ShowWarningToastAsync(Localizer["Chat"], Localizer["ChatCopyFailed"]);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Falha ao copiar resposta do chat.");
                await ShowWarningToastAsync(Localizer["Chat"], Localizer["ChatCopyFailed"]);
            }
        }

        private async Task RateMessageAsync(int index, CloudChatUiMessage msg, int rating)
        {
            var current = GetFeedback(index);
            var effective = current == rating ? 0 : rating;
            _feedbackByIndex[index] = effective;

            try
            {
                var model = !string.IsNullOrWhiteSpace(msg.RealModelName) ? msg.RealModelName : _selectedModel;
                await ChatFeedbackClient.UpsertAsync(new ChatFeedbackDto
                {
                    Interface = "openrouter",
                    ConversationId = string.IsNullOrEmpty(selectedConversationId) ? null : selectedConversationId,
                    MessageIndex = index,
                    PromptText = LastUserPromptBefore(index),
                    ResponseText = msg.Content,
                    ModelName = string.IsNullOrWhiteSpace(model) ? null : model,
                    Rating = effective
                });

                if (effective == 0)
                    await ShowSuccessToastAsync(Localizer["ChatRatingRemoved"]);
                else if (current != 0)
                    await ShowSuccessToastAsync(Localizer["ChatRatingUpdated"]);
                else
                    await ShowSuccessToastAsync(Localizer["ChatRatingSaved"]);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Falha ao registar avaliação no chat.");
                _feedbackByIndex[index] = current;
                await ShowWarningToastAsync(Localizer["Chat"], Localizer["ChatRatingFailed"]);
            }

            await InvokeAsync(StateHasChanged);
        }

        private string? LastUserPromptBefore(int index)
        {
            for (var i = index - 1; i >= 0; i--)
            {
                if (string.Equals(messages[i].Role, "user", StringComparison.OrdinalIgnoreCase))
                    return messages[i].Content;
            }
            return null;
        }

        private async Task NewChat()
        {
            try { chatCts?.Cancel(); } catch { }
            chatCts?.Dispose(); chatCts = null;

            messages.Clear();
            currentAssistantMessage = null;
            messages.Add(new CloudChatUiMessage { Role = "assistant", Content = welcomeMessage, IsWelcome = true });
            _contextUsedTokens = 0;
            _showContextBar = false;
            _elapsed = 0;
            selectedConversationId = null;
            _feedbackByIndex.Clear();
            _reasoningToggled.Clear();

            await InvokeAsync(StateHasChanged);

            try
            {
                await JS.InvokeVoidAsync("focusElement", chatInputRef);
            }
            catch { }
        }

        private void OpenHistory()
        {
            if (historyPanel is not null)
            {
                _ = historyPanel.Open();
            }
        }

        /// <summary>
        /// The history panel deleted every stored conversation (both chats share the same
        /// list). Reset the open chat, otherwise selectedConversationId would keep pointing
        /// at a row that no longer exists and the next save would update zero rows.
        /// </summary>
        private async Task OnHistoryClearedAsync()
        {
            await NewChat();
            await LoadConversations();
        }

        private async Task LoadConversationAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            selectedConversationId = id;

            messages.Clear();
            currentAssistantMessage = null;
            _contextUsedTokens = 0;
            _showContextBar = false;
            _elapsed = 0;
            _feedbackByIndex.Clear();
            _reasoningToggled.Clear();

            var conv = conversations.FirstOrDefault(c => c.Id == id);
            if (conv is not null && !string.IsNullOrEmpty(conv.Json))
            {
                try
                {
                    var restored = JsonSerializer.Deserialize<List<HistoryMessage>>(conv.Json);
                    if (restored != null)
                    {
                        foreach (var m in restored)
                        {
                            messages.Add(new CloudChatUiMessage
                            {
                                Role = m.Role,
                                Content = m.Content,
                                Reasoning = m.Reasoning,
                                Temperature = m.Temperature,
                                ElapsedTime = m.ElapsedTime
                            });
                        }
                    }
                }
                catch { }
            }

            if (messages.Count == 0)
            messages.Add(new CloudChatUiMessage { Role = "assistant", Content = welcomeMessage, IsWelcome = true });

            try
            {
                var ratings = await ChatFeedbackClient.GetByConversationAsync("openrouter", id);
                foreach (var r in ratings)
                {
                    if (r.Rating == 1 || r.Rating == -1)
                        _feedbackByIndex[r.MessageIndex] = r.Rating;
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Falha ao restaurar avaliações da conversa.");
            }

            await InvokeAsync(StateHasChanged);
            await SafeScrollToBottomAsync();
        }

        async Task LoadConversations()
        {
            try
            {
                var arr = await ConversationsClient.GetAllAsync();
                conversations.Clear();
                conversations = arr
                    .Select(c => new ConversationItem { Id = c.Id, Title = c.Title, Json = c.Json })
                    .ToList();
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading conversations: " + ex.Message);
            }
        }

        private async Task SaveCurrentConversationAsync()
        {
            if (!messages.Any(m => !string.IsNullOrWhiteSpace(m.Content) && m.Content != "...")) return;

            var json = JsonSerializer.Serialize(messages.Select(m => new HistoryMessage
            {
                Role = m.Role,
                Content = m.Content,
                Reasoning = m.Reasoning,
                Temperature = m.Temperature,
                ElapsedTime = m.ElapsedTime
            }));

            if (string.IsNullOrEmpty(selectedConversationId))
            {
                var id = Guid.NewGuid().ToString();
                var firstUser = messages.FirstOrDefault(m => m.Role == "user" && !string.IsNullOrWhiteSpace(m.Content))?.Content?.Trim();
                var title = !string.IsNullOrEmpty(firstUser)
                    ? (firstUser.Length > 60 ? firstUser.Substring(0, 60).TrimEnd() + "..." : firstUser)
                    : ("Conversation " + DateTime.Now.ToString("g"));
                var conv = new ConversationCreateDto { Id = id, Title = title, Json = json };
                await ConversationsClient.CreateAsync(conv);
                selectedConversationId = id;
                await LoadConversations();
            }
            else
            {
                var conv = new ConversationUpdateDto
                {
                    Id = selectedConversationId,
                    Title = conversations.FirstOrDefault(c => c.Id == selectedConversationId)?.Title ?? ("Conversation " + DateTime.Now.ToString("g")),
                    Json = json
                };
                await ConversationsClient.SaveAsync(conv);
                await LoadConversations();
            }
        }

        private async Task ShowSuccessToastAsync(string content)
        {
            ToastTitle = Localizer["Chat"];
            ToastContent = content;
            ToastCssClass = "e-toast-success";
            await ShowToastAsync();
        }

        private async Task ShowWarningToastAsync(string title, string content)
        {
            ToastTitle = title;
            ToastContent = content;
            ToastCssClass = "e-toast-warning";
            await ShowToastAsync();
        }

        private async Task ShowToastAsync()
        {
            try
            {
                ToastTimeOut = 3000;
                await InvokeAsync(StateHasChanged);
                if (toastObj != null)
                    await toastObj.ShowAsync();
            }
            catch { }
        }

        public async ValueTask DisposeAsync()
        {
            chatCts?.Cancel();
            chatCts?.Dispose();
            _elapsedTimer?.Dispose();
            try { await JS.InvokeVoidAsync("chatInput.detachHandlers", chatInputRef); } catch { }
            dotNetRef?.Dispose();
        }

        public class ScrollState
        {
            public double ScrollTop { get; set; }
            public double ScrollHeight { get; set; }
            public double ClientHeight { get; set; }
        }

        private sealed class HistoryMessage
        {
            public string Role { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public string? Reasoning { get; set; }
            public double Temperature { get; set; }
            public string? ElapsedTime { get; set; }
            public bool IsWelcome { get; set; }
        }

        public class CloudChatUiMessage
        {
            public string Role { get; set; } = "assistant";
            public string Content { get; set; } = string.Empty;
            public string? Reasoning { get; set; }
            public bool IsError { get; set; }
            public bool IsWelcome { get; set; }
            public DateTime Timestamp { get; set; } = DateTime.Now;
            public double Temperature { get; set; }
            public string? ElapsedTime { get; set; }
            public string? RealModelName { get; set; }
            public string? LanguageWarning { get; set; }
        }
    }
}