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
        private string? lastHubEvent;
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
                ChatProgressClient.ProgressReceived += OnProgressReceived;
                ChatProgressClient.ChunkReceived += OnChunkReceived;
                ChatProgressClient.DoneReceived += OnDoneReceived;

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
                // subscribe to reconnect events so UI knows when connection id changes
                ChatProgressClient.Reconnecting += OnHubReconnecting;
                ChatProgressClient.Reconnected += OnHubReconnected;
                ChatProgressClient.Closed += OnHubClosed;

                await ChatProgressClient.StartAsync(hubBase);
                connectionId = ChatProgressClient.ConnectionId;
                ProgressStatus = string.Empty;
                if (string.IsNullOrEmpty(ChatProgressClient.ConnectionId))
                {
                    ProgressStatus = "Progress hub unreachable. Server progress messages will not appear.";
                    lastHubEvent = "unavailable";
                }
                else
                {
                    lastHubEvent = "connected";
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

        private void OnHubReconnecting()
        {
            // show a small connection hint while reconnecting
            isConnecting = true;
            lastHubEvent = "reconnecting";
            _ = InvokeAsync(StateHasChanged);
        }

        private void OnHubReconnected(string? newId)
        {
            connectionId = newId;
            isConnecting = false;
            lastHubEvent = "reconnected";
            _ = InvokeAsync(StateHasChanged);
        }

        private void OnHubClosed(Exception? ex)
        {
            connectionId = null;
            isConnecting = false;
            lastHubEvent = "closed";
            _ = InvokeAsync(StateHasChanged);
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
            lastHubEvent = "progress";

            // Update UI and ensure the messages area scrolls so the status is visible
            _ = InvokeAsync(async () =>
            {
                StateHasChanged();
                try { await SafeScrollToBottomAsync(); } catch { }
            });
        }

        private async void OnChunkReceived(string chunk)
        {
            if (ignoreHubChunks) return;
            lastHubEvent = "chunk";

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

            lastHubEvent = "done";

            await InvokeAsync(async () =>
            {
                currentAssistantMessage = null;
                ProgressStatus = string.Empty;
                // mark processing finished when server indicates done
                isProcessing = false;
                StateHasChanged();
                try { await SaveCurrentConversationAsync(); } catch { }

                // show a small toast to indicate completion
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
            });
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

            try { await SafeScrollToBottomAsync(); } catch { }

            chatCts?.Cancel();
            chatCts = new CancellationTokenSource();

            userInput = string.Empty;
            await InvokeAsync(StateHasChanged);

            try
            {
                try { await SafeScrollToBottomAsync(); } catch { }

                // Try hub-first: invoke StartChat on the hub so server uses the caller connection id.
                var dtoList = messages.Select(m => new ChatMessageDto { Role = m.Role, Content = m.Content }).ToList();
                var language = Configuration["Language"] ?? null;

                connectionId = ChatProgressClient.ConnectionId;

                if (!string.IsNullOrWhiteSpace(ChatProgressClient.ConnectionId) && ChatProgressClient.IsConnected && !ChatProgressClient.IsReconnecting)
                {
                    // allow hub chunks
                    ignoreHubChunks = false;
                    ProgressStatus = "Thinking...";
                    isProcessing = true;
                    await InvokeAsync(StateHasChanged);

                    var started = await ChatProgressClient.StartChatAsync(dtoList, language);
                    if (started)
                    {
                        // wait for the first hub chunk within a short window
                        var firstChunkTcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
                        void LocalChunkHandler(string _) => firstChunkTcs.TrySetResult(null);

                        ChatProgressClient.ChunkReceived += LocalChunkHandler;
                        try
                        {
                            var delayMs = 2000;
                            var delayTask = Task.Delay(delayMs, chatCts.Token);
                            var finished = await Task.WhenAny(firstChunkTcs.Task, delayTask);

                            if (finished != firstChunkTcs.Task)
                            {
                                // no hub chunks in time: cancel server job via hub and fall back to HTTP streaming
                                try { await ChatProgressClient.CancelChatAsync(); } catch { }

                                ChatProgressClient.ChunkReceived -= LocalChunkHandler;

                                ignoreHubChunks = true;
                                ProgressStatus = "Thinking...";
                                await InvokeAsync(StateHasChanged);

                                var firstChunkReceived = false;
                                await foreach (var chunk in AiClient.StreamChatAsync(dtoList, null, chatCts.Token))
                                {
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

                                try { await SaveCurrentConversationAsync(); } catch { }
                                return;
                            }
                        }
                        finally
                        {
                            ChatProgressClient.ChunkReceived -= LocalChunkHandler;
                        }

                        // seen at least one hub chunk — wait for done
                        var tcs = new TaskCompletionSource<object?>();
                        void DoneHandler() => tcs.TrySetResult(null);

                        ChatProgressClient.DoneReceived += DoneHandler;
                        try
                        {
                            using (chatCts.Token.Register(() => tcs.TrySetCanceled()))
                            {
                                await tcs.Task;
                            }
                        }
                        finally
                        {
                            ChatProgressClient.DoneReceived -= DoneHandler;
                        }
                    }
                    else
                    {
                        // hub invocation failed: fallback to HTTP streaming
                        ignoreHubChunks = true;
                        ProgressStatus = "Thinking...";
                        await InvokeAsync(StateHasChanged);

                        var firstChunkReceived = false;
                        await foreach (var chunk in AiClient.StreamChatAsync(dtoList, null, chatCts.Token))
                        {
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
                    }
                }
                else
                {
                    // no stable hub connection: fallback to HTTP streaming
                    ignoreHubChunks = true;
                    ProgressStatus = "Thinking...";
                    isProcessing = true;
                    await InvokeAsync(StateHasChanged);

                    var firstChunkReceived = false;
                    await foreach (var chunk in AiClient.StreamChatAsync(dtoList, null, chatCts.Token))
                    {
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
                }

                try { await SaveCurrentConversationAsync(); } catch { }

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
            // Do not auto-save here: conversations are saved automatically when a response completes.
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
            // Require at least one non-empty message to save
            if (!messages.Any(m => !string.IsNullOrWhiteSpace(m.Content))) return;

            var json = JsonSerializer.Serialize(messages);

            if (string.IsNullOrEmpty(selectedConversationId))
            {
                var id = Guid.NewGuid().ToString();
                // Derive a friendly title from the first user message (fallback to timestamp)
                var firstUser = messages.FirstOrDefault(m => m.Role == "user" && !string.IsNullOrWhiteSpace(m.Content))?.Content?.Trim();
                var title = !string.IsNullOrEmpty(firstUser)
                    ? (firstUser.Length > 60 ? firstUser.Substring(0, 60).TrimEnd() + "..." : firstUser)
                    : ("Conversation " + DateTime.Now.ToString("g"));
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
                ChatProgressClient.Reconnected -= OnHubReconnected;
                ChatProgressClient.Reconnecting -= OnHubReconnecting;
                ChatProgressClient.Closed -= OnHubClosed;
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