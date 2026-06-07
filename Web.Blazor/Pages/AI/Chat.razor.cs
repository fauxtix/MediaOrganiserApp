using MediaOrganizerApp.Application.DTOs.AI;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Notifications;
using Web.Blazor.Services.ApiWrappers;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Syncfusion.Blazor.DropDowns;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace Web.Blazor.Pages.AI
{
    public partial class Chat : IAsyncDisposable
    {

        [Inject] private ConversationsClientService ConversationsClient { get; set; } = null!;
        [Inject] private IJSRuntime JS { get; set; } = null!;

        [Inject] private IConfiguration Configuration { get; set; } = null!;
        [Inject] private IAppSettingsService AppSettingsService { get; set; } = null!;
        [Inject] private IStringLocalizer<App> Localizer { get; set; } = null!;

        private bool ShowSpinnerOverlay;
        private CancellationTokenSource spinnerCts;

        private List<ChatMessage> messages = new();
        private string userInput = "";
        private ChatMessage? currentAssistantMessage;
        private ElementReference messagesDiv;
        // Using HTTP streaming only
        private bool isConnecting = false;
        private bool isProcessing = false;
        private bool ignoreHubChunks = false;
        private CancellationTokenSource? chatCts;

        private bool userAtBottom = true;


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



        private string? InputStyle { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await LoadConversations(); // load from server before render
        }



        //protected override async Task OnAfterRenderAsync(bool firstRender)
        //{
        //    if (!firstRender) return;

        //    // attach client-side JS handlers once after first render to avoid
        //    // duplicate handlers / race conditions that could cause Enter/Shift+Enter
        //    // to misbehave.
        //    dotNetRef = DotNetObjectReference.Create(this);
        //    try { await JS.InvokeVoidAsync("chatInput.attachHandlers", chatInputRef, dotNetRef); } catch { }
        //}

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                dotNetRef = DotNetObjectReference.Create(this);
            }

            // Aguarda um ciclo do CPU para garantir que o DOM do browser está 100% atualizado
            await Task.Yield();

            try
            {
                // Chamamos sempre para garantir que o input ganha os handlers, mesmo mudando de conversa
                await JS.InvokeVoidAsync("chatInput.attachHandlers", chatInputRef, dotNetRef);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao anexar handlers do chat: {ex.Message}");
            }
        }
        private void OnProgressReceived(string? message)
        {
            // show informational progress coming from the server (HTTP streaming)
            ProgressStatus = message ?? string.Empty;

            // Update UI and ensure the messages area scrolls so the status is visible
            _ = InvokeAsync(async () =>
            {
                StateHasChanged();
                try { await SafeScrollToBottomAsync(); } catch { }
            });
        }

        private async Task SafeScrollToBottomAsync()
        {
            try
            {
                // Keep messages scroll behavior stable; scroll after content appended.
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

        async Task Send()
        {
            if (string.IsNullOrWhiteSpace(userInput) || isConnecting)
                return;

            messages.Add(new ChatMessage { Role = "user", Content = userInput });

            currentAssistantMessage = new ChatMessage
            {
                Role = "assistant",
                Content = ""
            };

            messages.Add(currentAssistantMessage);

            // scroll único inicial (sem duplicação)
            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("scrollMessagesToBottom", messagesDiv);

            chatCts?.Cancel();
            chatCts = new CancellationTokenSource();

            userInput = string.Empty;

            InputStyle = "height: auto;";

            // 3. Renderiza o HTML limpo no browser
            await InvokeAsync(StateHasChanged);

            // 4. Chama o JS apenas para limpar resíduos físicos locais
            try { await JS.InvokeVoidAsync("chatInput.resetHeight", chatInputRef); } catch { }

            // 5. Restaura a propriedade para nula para que o CSS volte a mandar a 100% no futuro
            InputStyle = null;

            try
            {
                var dtoList = messages
                    .Select(m => new ChatMessageDto { Role = m.Role, Content = m.Content })
                    .ToList();

                string? language = null;

                try
                {
                    var settings = await AppSettingsService.GetSettingsAsync();
                    language = settings?.Language ?? settings?.DefaultLanguage;

                    if (!string.IsNullOrWhiteSpace(language))
                        language = language.Split(new[] { '-', '_' })[0];
                }
                catch
                {
                    language = null;
                }

                ProgressStatus = "Thinking...";
                isProcessing = true;
                await InvokeAsync(StateHasChanged);

                var firstChunkReceived = false;

                await foreach (var chunk in AiClient.StreamChatAsync(dtoList, null, null, language, chatCts.Token))
                {
                    if (!firstChunkReceived)
                    {
                        firstChunkReceived = true;
                        ProgressStatus = string.Empty;
                        await InvokeAsync(StateHasChanged);
                    }

                    if (currentAssistantMessage is null)
                    {
                        currentAssistantMessage = new ChatMessage
                        {
                            Role = "assistant",
                            Content = ""
                        };

                        messages.Add(currentAssistantMessage);
                    }

                    currentAssistantMessage.Content += chunk;

                    await InvokeAsync(StateHasChanged);

                    // scroll CONTROLADO (não agressivo)
                    if (userAtBottom)
                    {
                        await JS.InvokeVoidAsync("scrollMessagesToBottom", messagesDiv);
                    }
                }

                await SaveCurrentConversationAsync();

                if (toastObj != null)
                {
                    ToastTitle = "AI";
                    ToastContent = "Response received";
                    ToastCssClass = "e-toast-success";
                    await toastObj.ShowAsync();
                }
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

                if (toastObj != null)
                {
                    ToastTitle = "Error";
                    ToastCssClass = "e-toast-danger";
                    ToastTimeOut = 4000;
                    ToastContent = currentAssistantMessage.Content;
                    await InvokeAsync(StateHasChanged);
                    await toastObj.ShowAsync();
                }
            }
            finally
            {
                chatCts?.Dispose();
                chatCts = null;

                isProcessing = false;
                ProgressStatus = string.Empty;

                try { await JS.InvokeVoidAsync("focusElement", chatInputRef); } catch { }

                await InvokeAsync(StateHasChanged);
            }
        }
        [JSInvokable]
        public async Task OnEnterPressedFromJs()
        {
            await Task.Delay(10);
            await Send();
        }

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

            messages.Clear(); currentAssistantMessage = null;

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
                        ToastTitle = Localizer["TituloApagarRegisto"];
                        ToastContent = Localizer["SuccessDelete"];
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
        private Task MigrateLocalConversationsAsync() => Task.CompletedTask;


        public async ValueTask DisposeAsync()
        {
            chatCts?.Cancel();
            chatCts?.Dispose();
            try { await JS.InvokeVoidAsync("chatInput.detachHandlers", chatInputRef); } catch { }
            dotNetRef?.Dispose();
        }

        private async Task OnScroll()
        {
            var state = await JS.InvokeAsync<ScrollState>("chatScroll.getScrollState", messagesDiv);

            userAtBottom = (state.ScrollHeight - state.ScrollTop - state.ClientHeight) < 80;
        }

        public class ScrollState
        {
            public double ScrollTop { get; set; }
            public double ScrollHeight { get; set; }
            public double ClientHeight { get; set; }
        }
    }
}
