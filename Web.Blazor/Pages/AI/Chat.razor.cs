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
        private List<ConversationItem> conversations = new();
        private string? selectedConversationId;
        private readonly DropDownListFieldSettings convFields = new() { Text = "Title", Value = "Id" };

        private SfToast? toastObj;
        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";
        public int ToastTimeOut { get; set; } = 1500;

        private bool SpinnerVisible { get; set; }
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
            try { await Task.Delay(50); await JS.InvokeVoidAsync("scrollToBottom", messagesDiv); } catch { }
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
                await InvokeAsync(StateHasChanged);

                await foreach (var chunk in AiClient.StreamChatAsync(dtoList, connectionId, chatCts.Token))
                {
                    if (currentAssistantMessage is null) { currentAssistantMessage = new ChatMessage { Role = "assistant", Content = "" }; messages.Add(currentAssistantMessage); }
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
                await InvokeAsync(StateHasChanged);

                try { await JS.InvokeVoidAsync("focusElement", chatInputRef); } catch { }
            }
        }

        [JSInvokable] public async Task OnEnterPressedFromJs() => await Send();

        void ShowConfirmNewConversation() => showConfirmNewConversation = true;

        async Task OnConfirmYes()
        {
            // Only save if there are messages
            if (messages.Count > 0)
            {
                await SaveCurrentConversationAsync();
            }

            showConfirmNewConversation = false;
            await NewConversation();
        }

        void OnConfirmNo() => showConfirmNewConversation = false;

        async Task NewConversation()
        {
            try { chatCts?.Cancel(); } catch { }
            chatCts?.Dispose(); chatCts = null;

            messages.Clear(); currentAssistantMessage = null; ignoreHubChunks = false;

            if (!string.IsNullOrEmpty(selectedConversationId))
            {
                try
                {
                    // delete from server
                    await ConversationsClient.DeleteAsync(selectedConversationId);
                    await LoadConversations();
                }
                catch { }
                selectedConversationId = null;
            }

            await InvokeAsync(StateHasChanged);

            try
            {
                await JS.InvokeVoidAsync("focusElement", chatInputRef);
                var id = Guid.NewGuid().ToString();
                var title = "Conversation " + DateTime.Now.ToString("g");
                // create empty conversation record on server
                var conv = new MediaOrganizerApp.Domain.Entities.Conversation { Id = id, Title = title, Json = "[]" };
                await ConversationsClient.CreateAsync(conv);
                await LoadConversations();
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

                    selectedConversationId = conversations.FirstOrDefault()?.Id; // optional: select first
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