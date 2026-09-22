using Markdig;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Notifications;
using System.Globalization;
using System.Text.Json;
using Web.Blazor.Components.Chat;
using Web.Blazor.Services.ApiWrappers;
using Web.Blazor.Services.CloudChat;
using Web.Blazor.Services.Common;

namespace Web.Blazor.Pages.AI
{
    public partial class Chat : IAsyncDisposable
    {
        [Inject] private ConversationsClientService ConversationsClient { get; set; } = null!;
        [Inject] private ChatFeedbackClientService ChatFeedbackClient { get; set; } = null!;
        [Inject] private AiClientService AiClient { get; set; } = null!;
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private IAppSettingsService AppSettingsService { get; set; } = null!;
        [Inject] private IAiModelsService AiModelsService { get; set; } = null!;
        [Inject] private IAiModelResolver AiModelResolver { get; set; } = null!;
        [Inject] private IStringLocalizer<App> L { get; set; } = null!;

        private readonly List<ChatUiMessage> messages = new();
        private ChatUiMessage? currentAssistantMessage;
        private string userInput = "";
        private string? lastUserPrompt;
        private string welcomeMessage = "";

        private string _selectedModel = "";

        private int _contextLength = DefaultContextLimit;
        private int _contextUsedTokens;
        private bool _showContextBar;

        private bool isProcessing;
        private bool isConnecting;
        private double _elapsed;
        private System.Threading.Timer? _elapsedTimer;
        private CancellationTokenSource? chatCts;
        private DotNetObjectReference<Chat>? dotNetRef;
        private ElementReference messagesDiv;
        private ElementReference chatInputRef;
        private bool userAtBottom = true;

        private HistoryPanel? historyPanel;

        private List<ConversationItem> conversations = new();
        private string? selectedConversationId;

        private readonly Dictionary<int, int> _feedbackByIndex = new();

        private SfToast? toastObj;
        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";
        public int ToastTimeOut { get; set; } = 3000;

        private const int DefaultContextLimit = 8192;

        private static readonly Dictionary<string, int> KnownModelContextLimits = new(StringComparer.OrdinalIgnoreCase)
        {
            { "mistral:latest", 32768 },
            { "qwen3.5:latest", 128000 },
            { "phi4-mini:latest", 128000 }
        };

        private Dictionary<string, int> ModelContextLimits = new(KnownModelContextLimits, StringComparer.OrdinalIgnoreCase);

        protected override async Task OnInitializedAsync()
        {
            welcomeMessage = L["TestChatWelcome"];

            _selectedModel = await AiModelResolver.ResolveModelAsync();

            try
            {
                var models = await AiModelsService.GetAllAsync();
                var dynamicLimits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var m in models)
                {
                    if (!string.IsNullOrWhiteSpace(m.Descricao) && !dynamicLimits.ContainsKey(m.Descricao))
                        dynamicLimits[m.Descricao] = DefaultContextLimit;
                }

                foreach (var kv in KnownModelContextLimits)
                    dynamicLimits[kv.Key] = kv.Value;

                ModelContextLimits = dynamicLimits;
            }
            catch
            {
                ModelContextLimits = new Dictionary<string, int>(KnownModelContextLimits, StringComparer.OrdinalIgnoreCase);
            }

            _contextLength = ResolveContextLength(_selectedModel);

            await LoadConversations();

            messages.Add(new ChatUiMessage { Role = "assistant", Content = welcomeMessage, IsWelcome = true });
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

        private int ResolveContextLength(string model)
        {
            if (string.IsNullOrWhiteSpace(model)) return DefaultContextLimit;
            return ModelContextLimits.TryGetValue(model, out var limit) && limit > 0 ? limit : DefaultContextLimit;
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
                    L["ChatRoomTitle"],
                    string.Format(L["TestChatContextExceededDetail"], _contextLength));
                return;
            }

            var userMsg = new ChatUiMessage { Role = "user", Content = userText };
            messages.Add(userMsg);

            _contextUsedTokens += ChatComposerService.EstimateTokens(userText);
            _showContextBar = true;

            currentAssistantMessage = new ChatUiMessage
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
                var dtoList = messages
                    .Where(m => !m.IsWelcome)
                    .Select(m => new ChatMessageDto { Role = m.Role, Content = m.Content })
                    .ToList();

                var language = await ResolveRequestLanguageAsync();

                await foreach (var chunk in AiClient.StreamChatAsync(dtoList, null, null, language, localToken))
                {
                    if (currentAssistantMessage is null)
                    {
                        currentAssistantMessage = new ChatUiMessage { Role = "assistant", Content = "" };
                        messages.Add(currentAssistantMessage);
                    }

                    if (!string.IsNullOrEmpty(chunk))
                    {
                        if (currentAssistantMessage.Content == "...")
                            currentAssistantMessage.Content = chunk;
                        else
                            currentAssistantMessage.Content += chunk;

                        _contextUsedTokens += ChatComposerService.EstimateTokens(chunk);
                    }

                    _elapsed = stopwatch.Elapsed.TotalSeconds;
                    await InvokeAsync(StateHasChanged);

                    if (userAtBottom)
                    {
                        try { await JS.InvokeVoidAsync("scrollMessagesToBottom", messagesDiv); } catch { }
                    }
                }

                if (currentAssistantMessage is not null)
                {
                    if (currentAssistantMessage.Content == "...")
                        currentAssistantMessage.Content = string.Empty;

                    currentAssistantMessage.RealModelName = _selectedModel;
                    currentAssistantMessage.LanguageWarning = LanguageWarningFor(currentAssistantMessage.Content);
                }

                await SaveCurrentConversationAsync();

                await ShowSuccessToastAsync(L["TestChatResponseReceived"]);
            }
            catch (OperationCanceledException)
            {
                if (currentAssistantMessage is not null)
                {
                    if (currentAssistantMessage.Content == "...") currentAssistantMessage.Content = string.Empty;
                    currentAssistantMessage.Content += "\n\n⚠️ " + L["TestChatCancelled"];
                }

                await ShowWarningToastAsync("Chat", L["TestChatCancelled"]);
            }
            catch (Exception ex)
            {
                if (currentAssistantMessage is not null)
                {
                    if (currentAssistantMessage.Content == "...") currentAssistantMessage.Content = string.Empty;
                    currentAssistantMessage.Content += "\n\n⚠️ " + string.Format(L["TestChatError"], ex.Message);
                    currentAssistantMessage.IsError = true;
                }

                await ShowWarningToastAsync("Chat", Truncate(ex.Message));
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

                chatCts?.Dispose();
                chatCts = null;
                isProcessing = false;

                try { await JS.InvokeVoidAsync("focusElement", chatInputRef); } catch { }

                await InvokeAsync(StateHasChanged);
                await SafeScrollToBottomAsync();
            }
        }

        private static string Truncate(string message)
        {
            return string.IsNullOrWhiteSpace(message) || message.Length <= 200
                ? message
                : message[..200] + "...";
        }

        private async Task<string?> ResolveRequestLanguageAsync()
        {
            try
            {
                var settings = await AppSettingsService.GetSettingsAsync();
                var language = settings?.Language ?? settings?.DefaultLanguage;

                if (!string.IsNullOrWhiteSpace(language))
                    language = language.Split(new[] { '-', '_' })[0];

                return language;
            }
            catch
            {
                return null;
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

            return string.Format(L["TestChatLanguageWarning"], TargetLanguageResolver.GetTargetLanguage());
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

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (isProcessing)
                return;

            if (e.Key == "ArrowUp")
            {
                if (!string.IsNullOrWhiteSpace(lastUserPrompt) && string.IsNullOrWhiteSpace(userInput))
                {
                    userInput = lastUserPrompt;

                    await InvokeAsync(StateHasChanged);

                    try
                    {
                        await JS.InvokeVoidAsync("chatInput.resetHeight", chatInputRef);
                    }
                    catch { }
                }
            }
            else if (e.Key == "ArrowDown")
            {
                if (userInput == lastUserPrompt)
                {
                    userInput = string.Empty;

                    await InvokeAsync(StateHasChanged);

                    try
                    {
                        await JS.InvokeVoidAsync("chatInput.resetHeight", chatInputRef);
                    }
                    catch { }
                }
            }
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

        private async Task CopyMessageAsync(ChatUiMessage msg)
        {
            if (string.IsNullOrWhiteSpace(msg.Content)) return;

            try
            {
                var ok = await JS.InvokeAsync<bool>("copyText", msg.Content);
                if (ok)
                    await ShowSuccessToastAsync(L["ChatResponseCopied"]);
                else
                    await ShowWarningToastAsync("Chat", L["ChatCopyFailed"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error copying response: " + ex.Message);
                await ShowWarningToastAsync("Chat", L["ChatCopyFailed"]);
            }
        }

        private async Task RateMessageAsync(int index, ChatUiMessage msg, int rating)
        {
            var current = GetFeedback(index);
            var effective = current == rating ? 0 : rating;
            _feedbackByIndex[index] = effective;

            try
            {
                await ChatFeedbackClient.UpsertAsync(new ChatFeedbackDto
                {
                    Interface = "ollama",
                    ConversationId = string.IsNullOrEmpty(selectedConversationId) ? null : selectedConversationId,
                    MessageIndex = index,
                    PromptText = LastUserPromptBefore(index),
                    ResponseText = msg.Content,
                    ModelName = null,
                    Rating = effective
                });

                if (effective == 0)
                    await ShowSuccessToastAsync(L["ChatRatingRemoved"]);
                else if (current != 0)
                    await ShowSuccessToastAsync(L["ChatRatingUpdated"]);
                else
                    await ShowSuccessToastAsync(L["ChatRatingSaved"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving rating: " + ex.Message);
                _feedbackByIndex[index] = current;
                await ShowWarningToastAsync("Chat", L["ChatRatingFailed"]);
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
            messages.Add(new ChatUiMessage { Role = "assistant", Content = welcomeMessage, IsWelcome = true });
            _contextUsedTokens = 0;
            _showContextBar = false;
            _elapsed = 0;
            selectedConversationId = null;
            _feedbackByIndex.Clear();

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

        private async Task LoadConversationAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            selectedConversationId = id;

            messages.Clear();
            currentAssistantMessage = null;
            _feedbackByIndex.Clear();

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
                            messages.Add(new ChatUiMessage
                            {
                                Role = m.Role,
                                Content = m.Content,
                                Temperature = m.Temperature,
                                ElapsedTime = m.ElapsedTime,
                                IsWelcome = m.IsWelcome
                            });
                        }
                    }
                }
                catch { }
            }

            if (messages.Count == 0)
                messages.Add(new ChatUiMessage { Role = "assistant", Content = welcomeMessage, IsWelcome = true });

            _contextUsedTokens = messages.Sum(m => ChatComposerService.EstimateTokens(m.Content));
            _showContextBar = true;

            try
            {
                var ratings = await ChatFeedbackClient.GetByConversationAsync("ollama", id);
                foreach (var r in ratings)
                {
                    if (r.Rating == 1 || r.Rating == -1)
                        _feedbackByIndex[r.MessageIndex] = r.Rating;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error restoring ratings: " + ex.Message);
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
                Temperature = m.Temperature,
                ElapsedTime = m.ElapsedTime,
                IsWelcome = m.IsWelcome
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
            ToastTitle = "Chat";
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
            public double Temperature { get; set; }
            public string? ElapsedTime { get; set; }
            public bool IsWelcome { get; set; }
        }

        public class ChatUiMessage
        {
            public string Role { get; set; } = "assistant";
            public string Content { get; set; } = string.Empty;
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