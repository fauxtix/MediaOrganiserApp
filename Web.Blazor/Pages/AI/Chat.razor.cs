using MediaOrganizerApp.Application.DTOs.AI;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Notifications;
using Web.Blazor.Services.ApiWrappers;
using Syncfusion.Blazor.DropDowns;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Web.Blazor.Pages.AI
{
    public partial class Chat : IAsyncDisposable
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

        private string ProgressStatus { get; set; } = "";

        public class ConversationItem { public string Id { get; set; } = ""; public string Title { get; set; } = ""; public string Json { get; set; } = ""; }
        public class ChatMessage { public string Role { get; set; } = ""; public string Content { get; set; } = ""; }

        [Inject] private ConversationsClientService ConversationsClient { get; set; } = null!;
        [Inject] private IJSRuntime JS { get; set; } = null!;

        [Inject] private IConfiguration Configuration { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            await LoadConversations(); // load from server before render

            // subscribe to hub events early so we receive messages as soon as connection opens
            try
            {
                ChatProgressClient.ProgressReceived += OnProgressReceived;
                ChatProgressClient.ChunkReceived += OnChunkReceived;
                ChatProgressClient.DoneReceived += OnDoneReceived;
            }
            catch
            {
                // defensive: if BooksProgressClient isn't available yet, continue without throwing
            }

            isConnecting = true;
            await InvokeAsync(StateHasChanged);

            // Derive hub base from configured API BaseUrl when available. Navigation base may point
            // to the Blazor host which is not necessarily the API hosting the hub.
            var hubBase = Configuration["BaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrEmpty(hubBase))
            {
                // fallback to NavigationManager if BaseUrl not configured
                hubBase = (Nav?.BaseUri ?? "").TrimEnd('/');
            }

            // final fallback to localhost used previously
            if (string.IsNullOrEmpty(hubBase))
                hubBase = "https://localhost:44303";

            try
            {
                // Await the hub start so the component has a deterministic connection id
                await ChatProgressClient.StartAsync(hubBase);
                connectionId = ChatProgressClient.ConnectionId;
                ProgressStatus = string.Empty;
                if (string.IsNullOrEmpty(ChatProgressClient.ConnectionId))
                {
                    ProgressStatus = "Progress hub unreachable. Server progress messages will not appear.";
                }
            }
            catch (Exception ex)
            {
                // keep the UI usable, surface a brief message in the messages area
                ProgressStatus = "Progress hub unreachable. Server progress messages will not appear.";
                connectionId = ChatProgressClient.ConnectionId;
                Console.WriteLine("BooksProgressClient.StartAsync failed: " + ex.Message);
            }
            finally
            {
                isConnecting = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            // attach client-side JS handlers once after first render to avoid
            // duplicate handlers / race conditions that could cause Enter/Shift+Enter
            // to misbehave.
            dotNetRef = DotNetObjectReference.Create(this);
            try { await JS.InvokeVoidAsync("chatInput.attachHandlers", chatInputRef, dotNetRef); } catch { }
        }

        private void OnProgressReceived(string message)
        {
            // show informational progress coming from the server/hub
            ProgressStatus = message ?? string.Empty;
            _ = InvokeAsync(StateHasChanged);
        }

        private async void OnChunkReceived(string chunk)
        {
            if (ignoreHubChunks) return;

            // clear any progress hint as soon as assistant starts streaming
            ProgressStatus = string.Empty;

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

            await InvokeAsync(() => { currentAssistantMessage = null; ProgressStatus = string.Empty; StateHasChanged(); });
        }

        private async Task SafeScrollToBottomAsync()
        {
            try
            {
                // Keep messages scroll behavior stable; scroll after content appended.
                await Task.Delay(50);
                if (messagesDiv.Context != null)
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
            if (string.IsNullOrWhiteSpace(userInput) || isConnecting) return;

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
                // When doing client-streaming, prevent hub chunks from being applied simultaneously.
                ignoreHubChunks = true;

                // show a friendly status while the AI pipeline runs; hub may replace this with more specific messages
                ProgressStatus = "Thinking...";
                isProcessing = true;

                await InvokeAsync(StateHasChanged);
                // Ensure the messages area is scrolled so the user can see the status message immediately
                try { await SafeScrollToBottomAsync(); } catch { }

                var firstChunkReceived = false;
                await foreach (var chunk in AiClient.StreamChatAsync(dtoList, connectionId, chatCts.Token))
                {
                    // clear the progress status on first chunk
                    if (!firstChunkReceived)
                    {
                        firstChunkReceived = true;
                        ProgressStatus = string.Empty;
                        await InvokeAsync(StateHasChanged);
                    }

                    if (currentAssistantMessage is null)
                    {
                        currentAssistantMessage = new ChatMessage { Role = "assistant", Content = "" };
                        messages.Add(currentAssistantMessage);
                    }

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
            catch (OperationCanceledException)
            {
                // cancelled by user
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
                ProgressStatus = string.Empty;
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
                ProgressStatus = string.Empty;

                try { await JS.InvokeVoidAsync("focusElement", chatInputRef); } catch { }
                await InvokeAsync(StateHasChanged);
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

            messages.Clear(); currentAssistantMessage = null; ignoreHubChunks = false;

            await InvokeAsync(StateHasChanged);

            try
            {
                await JS.InvokeVoidAsync("focusElement", chatInputRef);
                var id = Guid.NewGuid().ToString();
                var title = "Conversation " + DateTime.Now.ToString("g");
                var conv = new MediaOrganizerApp.Domain.Entities.Conversation { Id = id, Title = title, Json = "[]" };
                await ConversationsClient.CreateAsync(conv);
                await LoadConversations();
                selectedConversationId = id;
                await InvokeAsync(StateHasChanged);
                try
                {
                    if (toastObj != null)
                    {
                        ToastTitle = "Conversation";
                        ToastContent = "Started new conversation";
                        ToastCssClass = "e-toast-success";
                        await InvokeAsync(StateHasChanged);
                        await toastObj.ShowAsync();
                    }
                }
                catch { }
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
                try
                {
                    if (toastObj != null)
                    {
                        ToastTitle = "Conversation";
                        ToastContent = "Loaded conversation: " + conv.Title;
                        ToastCssClass = "e-toast-success";
                        ToastTimeOut = 2000;
                        await InvokeAsync(StateHasChanged);
                        await toastObj.ShowAsync();
                    }
                }
                catch { }
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
                try
                {
                    if (toastObj != null)
                    {
                        ToastTitle = "Conversation";
                        ToastContent = "Conversation deleted";
                        ToastCssClass = "e-toast-success";
                        await InvokeAsync(StateHasChanged);
                        await toastObj.ShowAsync();
                    }
                }
                catch { }
            }
            catch
            {
                try
                {
                    if (toastObj != null)
                    {
                        ToastTitle = "Error";
                        ToastContent = "Error deleting conversation";
                        ToastCssClass = "e-toast-danger";
                        await InvokeAsync(StateHasChanged);
                        await toastObj.ShowAsync();
                    }
                }
                catch { }
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

        async Task CancelSend()
        {
            try { chatCts?.Cancel(); } catch { }
            try { chatCts?.Dispose(); chatCts = null; } catch { }

            ignoreHubChunks = false;
            isProcessing = false;
            ProgressStatus = string.Empty;

            await InvokeAsync(StateHasChanged);
        }
        // Migration helper intentionally disabled: localStorage helpers were removed.
        // If you still have local data and need to migrate it, restore the getConversations JS
        // helpers temporarily and re-enable this method to import them to the server.
        private Task MigrateLocalConversationsAsync() => Task.CompletedTask;


        public async ValueTask DisposeAsync()
        {
            try
            {
                ChatProgressClient.ProgressReceived -= OnProgressReceived;
                ChatProgressClient.ChunkReceived -= OnChunkReceived;
                ChatProgressClient.DoneReceived -= OnDoneReceived;
            }
            catch { }

            chatCts?.Cancel();
            chatCts?.Dispose();
                try { await JS.InvokeVoidAsync("chatInput.detachHandlers", chatInputRef); } catch { }
            dotNetRef?.Dispose();
            try { await ChatProgressClient.StopAsync(); } catch { }
        }
    }
}