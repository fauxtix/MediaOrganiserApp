using MediaOrganizerApp.Application.DTOs.AI;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Web.Blazor.Services.ApiWrappers;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Web.Blazor.Pages.AI
{
    public partial class CopilotChat : IAsyncDisposable
    {
        public class ChatMessage { public string Role { get; set; } = ""; public string Content { get; set; } = ""; }
        private List<ChatMessage> messages = new();
        private string userInput = "";
        private string suggestion = "";
        private ChatMessage? currentAssistantMessage;
        private ElementReference messagesDiv;
        private ElementReference chatInputRef;
        private DotNetObjectReference<CopilotChat>? dotNetRef;
        private bool isProcessing = false;
        private string ProgressStatus { get; set; } = "";

        [Inject] private Web.Blazor.Services.ApiWrappers.AiClientService AiClient { get; set; } = null!;
        [Inject] private ConversationsClientService ConversationsClient { get; set; } = null!;
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private IConfiguration Configuration { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            await LoadConversations();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            dotNetRef = DotNetObjectReference.Create(this);
            try { await JS.InvokeVoidAsync("copilotInput.attachHandlers", chatInputRef, dotNetRef); } catch { }
        }

        [JSInvokable] public async Task OnAcceptSuggestionFromJs() => await AcceptSuggestionAsync();
        [JSInvokable] public Task OnDismissSuggestionFromJs() { suggestion = string.Empty; StateHasChanged(); return Task.CompletedTask; }

        private async Task AcceptSuggestionAsync()
        {
            if (string.IsNullOrEmpty(suggestion)) return;
            userInput += suggestion;
            suggestion = string.Empty;
            await InvokeAsync(StateHasChanged);
            try { await JS.InvokeVoidAsync("focusElement", chatInputRef); } catch { }
        }

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

        private string FormatMessage(string content, bool showCursor)
        {
            var escaped = System.Net.WebUtility.HtmlEncode(content).Replace("\n", "<br>");
            if (showCursor) escaped += "<span class='cursor'>|</span>";
            return escaped;
        }

        private string HtmlEncodeForPre(string text)
        {
            return System.Net.WebUtility.HtmlEncode(text).Replace("\n", "\n");
        }

        private async Task Send()
        {
            if (string.IsNullOrWhiteSpace(userInput) || isProcessing) return;

            messages.Add(new ChatMessage { Role = "user", Content = userInput });
            currentAssistantMessage = new ChatMessage { Role = "assistant", Content = "" };
            messages.Add(currentAssistantMessage);

            try { await SafeScrollToBottomAsync(); } catch { }

            var dtoList = messages.Select(m => new ChatMessageDto { Role = m.Role, Content = m.Content }).ToList();
            var language = Configuration["Language"] ?? null;

            isProcessing = true;
            suggestion = string.Empty;
            try
            {
                await foreach (var chunk in AiClient.StreamChatAsync(dtoList, language, CancellationToken.None))
                {
                    suggestion += chunk;
                    if (currentAssistantMessage is null)
                    {
                        currentAssistantMessage = new ChatMessage { Role = "assistant", Content = "" };
                        messages.Add(currentAssistantMessage);
                    }

                    currentAssistantMessage.Content = suggestion;
                    await InvokeAsync(StateHasChanged);
                    await SafeScrollToBottomAsync();
                }

                if (currentAssistantMessage != null)
                {
                    currentAssistantMessage.Content = suggestion;
                    suggestion = string.Empty;
                }

                try { await SaveCurrentConversationAsync(); } catch { }
            }
            catch (OperationCanceledException)
            {
                ProgressStatus = "Request cancelled.";
            }
            catch (Exception ex)
            {
                if (currentAssistantMessage is null)
                {
                    currentAssistantMessage = new ChatMessage { Role = "assistant", Content = "" };
                    messages.Add(currentAssistantMessage);
                }
                currentAssistantMessage.Content += $"\n\n[Error receiving response: {ex.Message}]";
            }
            finally
            {
                isProcessing = false;
                suggestion = string.Empty;
                try { await JS.InvokeVoidAsync("focusElement", chatInputRef); } catch { }
                await InvokeAsync(StateHasChanged);
            }
        }

        async Task CancelSend()
        {
            try { chatCts?.Cancel(); } catch { }
            try { chatCts?.Dispose(); chatCts = null; } catch { }

            isProcessing = false;
            ProgressStatus = string.Empty;

            await InvokeAsync(StateHasChanged);
        }

        // conversation helpers
        private List<ConversationItem> conversations = new();
        private string? selectedConversationId;
        private bool showConfirmDeleteConversation = false;
        public class ConversationItem { public string Id { get; set; } = ""; public string Title { get; set; } = ""; public string Json { get; set; } = ""; }
        // filename input removed from this component; topbar handles create/open
        private async Task NewConversation()
        {
            // Create an untitled conversation directly
            var id = Guid.NewGuid().ToString();
            var title = "Conversation " + DateTime.Now.ToString("g");
            var conv = new MediaOrganizerApp.Domain.Entities.Conversation { Id = id, Title = title, Json = "[]" };
            await ConversationsClient.CreateAsync(conv);
            await LoadConversations();
            selectedConversationId = id;
            await InvokeAsync(StateHasChanged);
        }

        private async Task DeleteSelectedConversationAsync()
        {
            if (string.IsNullOrEmpty(selectedConversationId)) return;
            var deletedId = selectedConversationId;
            try
            {
                await ConversationsClient.DeleteAsync(deletedId);
                await LoadConversations();
                if (!conversations.Any(c => c.Id == deletedId))
                {
                    messages.Clear();
                    currentAssistantMessage = null;
                    selectedConversationId = conversations.FirstOrDefault()?.Id;
                }
                await InvokeAsync(StateHasChanged);
            }
            catch { }
        }

        void ShowConfirmDeleteConversation() => showConfirmDeleteConversation = true;

        void OnConfirmDeleteNo() => showConfirmDeleteConversation = false;

        async Task OnConfirmDeleteYes()
        {
            showConfirmDeleteConversation = false;
            await DeleteSelectedConversationAsync();
        }

        private void OnConversationChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, ConversationItem> args)
        {
            selectedConversationId = args.Value;
            _ = LoadSelectedFile();
        }

        private async Task LoadSelectedFile()
        {
            if (string.IsNullOrEmpty(selectedConversationId)) return;
            var conv = conversations.FirstOrDefault(c => c.Id == selectedConversationId);
            if (conv != null)
            {
                messages.Clear();
                if (!string.IsNullOrEmpty(conv.Json))
                {
                    try
                    {
                        var restored = JsonSerializer.Deserialize<List<ChatMessage>>(conv.Json);
                        if (restored != null) messages.AddRange(restored);
                    }
                    catch { }
                }
                await InvokeAsync(StateHasChanged);
            }
        }

        async Task LoadConversations()
        {
            try
            {
                var arr = await ConversationsClient.GetAllAsync();
                if (arr != null)
                {
                    conversations = arr.Select(c => new ConversationItem { Id = c.Id, Title = c.Title, Json = c.Json }).ToList();
                }
            }
            catch { }
        }

        private async Task SaveCurrentConversationAsync()
        {
            if (!messages.Any(m => !string.IsNullOrWhiteSpace(m.Content))) return;
            var json = JsonSerializer.Serialize(messages);
            if (string.IsNullOrEmpty(selectedConversationId))
            {
                var id = Guid.NewGuid().ToString();
                var firstUser = messages.FirstOrDefault(m => m.Role == "user" && !string.IsNullOrWhiteSpace(m.Content))?.Content?.Trim();
                var title = !string.IsNullOrEmpty(firstUser) ? (firstUser.Length > 60 ? firstUser.Substring(0, 60).TrimEnd() + "..." : firstUser) : ("Conversation " + DateTime.Now.ToString("g"));
                var conv = new MediaOrganizerApp.Domain.Entities.Conversation { Id = id, Title = title, Json = json };
                await ConversationsClient.CreateAsync(conv);
                selectedConversationId = id;
            }
            else
            {
                var conv = new MediaOrganizerApp.Domain.Entities.Conversation { Id = selectedConversationId, Title = conversations.FirstOrDefault(c => c.Id == selectedConversationId)?.Title ?? ("Conversation " + DateTime.Now.ToString("g")), Json = json, UpdatedAt = DateTime.UtcNow };
                await ConversationsClient.SaveAsync(conv);
            }
        }

        private CancellationTokenSource? chatCts;

        public async ValueTask DisposeAsync()
        {
            try { await JS.InvokeVoidAsync("copilotInput.detachHandlers", chatInputRef); } catch { }
            dotNetRef?.Dispose();
        }
    }
}
