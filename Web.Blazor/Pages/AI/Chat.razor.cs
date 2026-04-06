using MediaOrganizerApp.Application.DTOs.AI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Notifications;
using System.Text.Json;
using MediaOrganizerApp.Domain.Entities;
using Web.Blazor.Services.ApiWrappers;
using Syncfusion.EJ2.Notifications;

namespace Web.Blazor.Pages.AI
{
    public partial class Chat
    {
        private bool ShowSpinnerOverlay;
        private CancellationTokenSource spinnerCts;

        private List<ChatMessage> messages = new();
        private string userInput = "";
        private ChatMessage? currentAssistantMessage;
        private ElementReference messagesDiv;
        private string? connectionId;
        private bool isConnecting = false;
        private bool isProcessing = false;
        private CancellationTokenSource? chatCts;
        private bool ignoreHubChunks = false;

        private ElementReference chatInputRef;
        private DotNetObjectReference<Chat>? dotNetRef;
        private bool showConfirmNewConversation = false;
        private bool showConfirmDeleteConversation = false;
        private List<ConversationItem> conversations = new();
        private string? selectedConversationId;
        private readonly DropDownListFieldSettings convFields = new() { Text = "Title", Value = "Id" };

        private SfToast? toastObj;
        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";
        public int ToastTimeOut { get; set; } = 1500;


        private string SpinnerLabel { get; set; } = "Loading...";

        // Keep a small UI DTO for the dropdown
        public class ConversationItem { public string Id { get; set; } = ""; public string Title { get; set; } = ""; public string Json { get; set; } = ""; }
        public class ChatMessage { public string Role { get; set; } = ""; public string Content { get; set; } = ""; }

        [Inject] private ConversationsClientService ConversationsClient { get; set; } = null!;
        [Inject] private IJSRuntime JS { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            await LoadConversations(); // load from server before render
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            BooksProgressClient.ChunkReceived += OnChunkReceived;
            BooksProgressClient.DoneReceived += OnDoneReceived;

            isConnecting = true;
            await InvokeAsync(StateHasChanged);

            try
            {
                await BooksProgressClient.StartAsync("https://localhost:44303");
                connectionId = BooksProgressClient.ConnectionId;
            }
            catch { connectionId = null; }
            finally
            {
                isConnecting = false;
                await InvokeAsync(StateHasChanged);
            }

            dotNetRef = DotNetObjectReference.Create(this);
            try { await JS.InvokeVoidAsync("chatInput.attachHandlers", chatInputRef, dotNetRef); } catch { }
        }

        private async void OnChunkReceived(string chunk)
        {
            if (ignoreHubChunks) return;

            await InvokeAsync(async () =>
            {
                if (currentAssistantMessage is null)
                {
                    currentAssistantMessage = new ChatMessage { Role = "assistant", Content = "" };
                    messages.Add(currentAssistantMessage);
                }

                currentAssistantMessage.Content += chunk;
                StateHasChanged();
                await SafeScrollToBottomAsync();
            });
        }

        private async void OnDoneReceived()
        {
            if (ignoreHubChunks) return;

            await InvokeAsync(() => { currentAssistantMessage = null; StateHasChanged(); });
        }

        private async Task SafeScrollToBottomAsync()
        {
            try
            {
                // Avoid forcing scroll to bottom for the first message(s) so the initial prompt appears near the top.
                if (messages.Count <= 1) return;
                await Task.Delay(50);
                await JS.InvokeVoidAsync("scrollToBottom", messagesDiv);
            }
            catch { }
        }

        private string FormatMessage(string content, bool showCursor)
        {
            var escaped = System.Net.WebUtility.HtmlEncode(content).Replace("\n", "<br>");
            if (showCursor) escaped += "<span class='cursor'>|</span>";
            return escaped;
        }

        async Task Send()
        {
            if (string.IsNullOrWhiteSpace(userInput) || connectionId == null || isConnecting) return;

            messages.Add(new ChatMessage { Role = "user", Content = userInput });
            currentAssistantMessage = new ChatMessage { Role = "assistant", Content = "" };
            messages.Add(currentAssistantMessage);

            var dtoList = messages.Select(m => new ChatMessageDto { Role = m.Role, Content = m.Content }).ToList();

            chatCts?.Cancel();
            chatCts = new CancellationTokenSource();

            userInput = "";
            StateHasChanged();

            try
            {
                ignoreHubChunks = true;
                isProcessing = true;
                // show spinner while waiting for the first AI chunk

                await ShowSpinnerWithDelay();
                await InvokeAsync(StateHasChanged);

                await foreach (var chunk in AiClient.StreamChatAsync(dtoList, connectionId, chatCts.Token))
                {
                    if (currentAssistantMessage is null) { currentAssistantMessage = new ChatMessage { Role = "assistant", Content = "" }; messages.Add(currentAssistantMessage); }
                    // hide spinner as soon as we receive the first chunk

                    HideSpinner();
                    currentAssistantMessage.Content += chunk;
                    await InvokeAsync(StateHasChanged);
                    await SafeScrollToBottomAsync();
                }

                try
                {
                    if (toastObj != null)
                    {
                        ToastTitle = "AI";
                        ToastContent = "Response received";
                        ToastCssClass = "e-toast-success";
                        await toastObj.ShowAsync();
                    }
                }
                catch { }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (currentAssistantMessage is null)
                {
                    currentAssistantMessage = new ChatMessage { Role = "assistant", Content = "" };
                    messages.Add(currentAssistantMessage);
                }
                currentAssistantMessage.Content += $"\n\n[Error receiving response: {ex.Message}]";
                try
                {
                    if (toastObj != null)
                    {
                        ToastTitle = "Error";
                        ToastCssClass = "e-toast-danger";
                        ToastContent = currentAssistantMessage.Content;
                        await InvokeAsync(StateHasChanged);
                        await toastObj.ShowAsync();
                    }
                }
                catch { }
            }
            finally
            {
                chatCts?.Dispose();
                chatCts = null;
                ignoreHubChunks = false;
                isProcessing = false;
                HideSpinner();

                try { await JS.InvokeVoidAsync("focusElement", chatInputRef); } catch { }
            }
        }

        [JSInvokable] public async Task OnEnterPressedFromJs() => await Send();

        async Task ShowConfirmNewConversation()
        {
            // Only show confirmation if there are messages in the UI
            if (messages.Count > 0)
            {
                showConfirmNewConversation = true;
                await InvokeAsync(StateHasChanged);
            }
            else
            {
                // No messages: start a new conversation immediately
                await NewConversation();
            }
        }
        void ShowConfirmDeleteConversation() => showConfirmDeleteConversation = true;

        async Task OnConfirmYes()
        {
            // If there are messages, save them before starting a new conversation.
            // SaveCurrentConversationAsync handles both create (when no selectedConversationId)
            // and update (when a conversation is selected).
            if (messages.Count > 0)
            {
                await SaveCurrentConversationAsync();
            }

            showConfirmNewConversation = false;
            await NewConversation();
        }

        void OnConfirmNo() => showConfirmNewConversation = false;
        void OnConfirmDeleteNo() => showConfirmDeleteConversation = false;

        async Task OnConfirmDeleteYes()
        {
            showConfirmDeleteConversation = false;
            await DeleteSelectedConversationAsync();
        }

        async Task NewConversation()
        {
            try { chatCts?.Cancel(); } catch { }
            chatCts?.Dispose(); chatCts = null;

            // Clear UI messages and prepare to start a fresh conversation context.
            messages.Clear(); currentAssistantMessage = null; ignoreHubChunks = false;

            await InvokeAsync(StateHasChanged);

            try
            {
                await JS.InvokeVoidAsync("focusElement", chatInputRef);
                var id = Guid.NewGuid().ToString();
                var title = "Conversation " + DateTime.Now.ToString("g");
                // create empty conversation record on server and select it so new prompts attach to it
                var conv = new MediaOrganizerApp.Domain.Entities.Conversation { Id = id, Title = title, Json = "[]" };
                await ConversationsClient.CreateAsync(conv);
                await LoadConversations();
                // ensure dropdown selects the newly created conversation
                selectedConversationId = id;
                await InvokeAsync(StateHasChanged);
                await JS.InvokeVoidAsync("showToast", "Started new conversation");
            }
            catch { }
        }

        async Task LoadConversations()
        {
            try
            {
                var arr = await ConversationsClient.GetAllAsync();
                if (arr != null)
                {
                    conversations.Clear();
                    conversations = arr
                        .Select(c => new ConversationItem { Id = c.Id, Title = c.Title, Json = c.Json })
                        .ToList();

                    // Do not auto-select a conversation. Require the user to explicitly select one from the dropdown.
                    selectedConversationId = null;
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading conversations: " + ex.Message);
            }
        }

        async Task OnConversationSelected(string? id)
        {
            selectedConversationId = id;
            if (string.IsNullOrEmpty(id)) return;
            var conv = conversations.FirstOrDefault(c => c.Id == id);
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
                try { await JS.InvokeVoidAsync("showToast", "Loaded conversation: " + conv.Title); } catch { }
                await InvokeAsync(StateHasChanged);
            }
        }

        private void OnConversationChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, ConversationItem> args)
        {
            var id = args.Value;
            _ = OnConversationSelected(id);
        }

        private async Task SaveCurrentConversationAsync()
        {
            if (messages.Count == 0) return;

            var json = JsonSerializer.Serialize(messages);

            if (string.IsNullOrEmpty(selectedConversationId))
            {
                var id = Guid.NewGuid().ToString();
                var title = "Conversation " + DateTime.Now.ToString("g");
                var conv = new MediaOrganizerApp.Domain.Entities.Conversation { Id = id, Title = title, Json = json };
                await ConversationsClient.CreateAsync(conv);
                selectedConversationId = id;
                await LoadConversations();
            }
            else
            {
                var conv = new MediaOrganizerApp.Domain.Entities.Conversation { Id = selectedConversationId, Title = conversations.FirstOrDefault(c => c.Id == selectedConversationId)?.Title ?? ("Conversation " + DateTime.Now.ToString("g")), Json = json, UpdatedAt = DateTime.UtcNow };
                await ConversationsClient.SaveAsync(conv);
                await LoadConversations();
            }
        }

        private async Task DeleteSelectedConversationAsync()
        {
            if (string.IsNullOrEmpty(selectedConversationId)) return;
            var deletedId = selectedConversationId; // capture before reloading

            try
            {
                await ConversationsClient.DeleteAsync(deletedId);
                // reload conversations from server so the dropdown is up-to-date
                await LoadConversations();

                // if the deleted conversation is no longer present, clear UI messages
                if (!conversations.Any(c => c.Id == deletedId))
                {
                    messages.Clear();
                    currentAssistantMessage = null;
                    // select the first conversation if any (or leave null)
                    selectedConversationId = conversations.FirstOrDefault()?.Id;
                }

                await InvokeAsync(StateHasChanged);
                await JS.InvokeVoidAsync("showToast", "Conversation deleted");
            }
            catch
            {
                try { await JS.InvokeVoidAsync("showToast", "Error deleting conversation"); } catch { }
            }
        }


        private async Task ShowSpinnerWithDelay(int delayMs = 300)
        {
            spinnerCts?.Cancel();
            spinnerCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(delayMs, spinnerCts.Token);
                ShowSpinnerOverlay = true;
                StateHasChanged();
            }
            catch { }
        }

        private void HideSpinner()
        {
            spinnerCts?.Cancel();
            ShowSpinnerOverlay = false;
        }
        // Migration helper intentionally disabled: localStorage helpers were removed.
        // If you still have local data and need to migrate it, restore the getConversations JS
        // helpers temporarily and re-enable this method to import them to the server.
        private Task MigrateLocalConversationsAsync() => Task.CompletedTask;

        public async ValueTask DisposeAsync()
        {
            BooksProgressClient.ChunkReceived -= OnChunkReceived;
            BooksProgressClient.DoneReceived -= OnDoneReceived;
            chatCts?.Cancel();
            chatCts?.Dispose();
            try { await JS.InvokeVoidAsync("chatInput.detachHandlers", chatInputRef); } catch { }
            dotNetRef?.Dispose();
            await BooksProgressClient.StopAsync();
        }
    }
}