using Markdig;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Notifications;
using System.Text.Json;
using Web.Blazor.Services.ApiWrappers;

namespace Web.Blazor.Pages.AI
{
    public partial class Chat : IAsyncDisposable
    {

        [Inject] private ConversationsClientService ConversationsClient { get; set; } = null!;
        [Inject] private IJSRuntime JS { get; set; } = null!;

        [Inject] private IConfiguration Configuration { get; set; } = null!;
        [Inject] private IAppSettingsService AppSettingsService { get; set; } = null!;
        [Inject] private IAiModelsService AiModelsService { get; set; } = null!;
        [Inject] private Web.Blazor.Services.Common.IAiModelResolver AiModelResolver { get; set; } = null!;
        [Inject] private IStringLocalizer<App> L { get; set; } = null!;

        private bool ShowSpinnerOverlay;
        private CancellationTokenSource spinnerCts;

        private List<ChatMessage> messages = new();
        private string userInput = "";
        private ChatMessage? currentAssistantMessage;
        private ElementReference messagesDiv;
        private bool isConnecting = false;
        private bool isProcessing = false;
        private bool ignoreHubChunks = false;
        private CancellationTokenSource? chatCts;

        private bool userAtBottom = true;

        private string modeloAtual = "";
        private int limiteTokens { get; set; }
        private int tokensEstimados { get; set; }
        private int tokensUsados { get; set; } = 0;
        private bool ShowTokenWarning => limiteTokens > 0 && ((double)tokensEstimados / limiteTokens) >= 0.80;


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

        private string? InputStyle { get; set; }
        protected bool AlertVisibility { get; set; } = false;

        private string? lastUserPrompt;

        protected override async Task OnInitializedAsync()
        {
            await LoadConversations();
            modeloAtual = await AiModelResolver.ResolveModelAsync();

            // Carrega os modelos reais instalados no Ollama para obter limites de contexto dinâmicos.
            // Modelos desconhecidos usam o limite padrão.
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
                // Fallback: mantém os limites conhecidos quando a API de modelos não está acessível.
                ModelContextLimits = new Dictionary<string, int>(KnownModelContextLimits, StringComparer.OrdinalIgnoreCase);
            }
        }

        //protected override async Task OnAfterRenderAsync(bool firstRender)
        //{
        //    if (firstRender)
        //    {
        //        dotNetRef = DotNetObjectReference.Create(this);
        //    }

        //    await Task.Yield();

        //    try
        //    {
        //        await JS.InvokeVoidAsync("chatInput.attachHandlers", chatInputRef, dotNetRef);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Erro ao anexar handlers do chat: {ex.Message}");
        //    }
        //}

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                dotNetRef = DotNetObjectReference.Create(this);
                await Task.Delay(50);
                await JS.InvokeVoidAsync("chatInput.attachHandlers", chatInputRef, dotNetRef);
            }
        }
        private void OnProgressReceived(string? message)
        {
            ProgressStatus = message ?? string.Empty;

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
                await Task.Delay(50);
                if (messagesDiv.Context != null)
                    await JS.InvokeVoidAsync("scrollMessagesToBottom", messagesDiv);
            }
            catch { }
        }

        //private string FormatMessage(string content, bool showCursor)
        //{
        //    var escaped = System.Net.WebUtility.HtmlEncode(content).Replace("\n", "<br>");
        //    if (showCursor) escaped += "<span class='cursor'>|</span>";
        //    return escaped;
        //}

        private string FormatMessage(string content, bool showCursor)
        {
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

        async Task Send()
        {

            //if (!IsPromptValid(userInput))
            //{
            //    ToastContent = "O prompt não foi usado corretamente. Por favor, faça uma pergunta relacionada com autores, publicações, músicas, vídeos ou livros";
            //    AlertVisibility = true;
            //    return;
            //}

            if (string.IsNullOrWhiteSpace(userInput) || isConnecting)
                return;

            limiteTokens = ModelContextLimits.TryGetValue(modeloAtual, out var limit) ? limit : DefaultContextLimit;
            var simulacaoMensagens = messages.ToList();
            simulacaoMensagens.Add(new ChatMessage { Role = "user", Content = userInput });

            tokensEstimados = EstimateTokens(simulacaoMensagens);

            // 4. Validar se ultrapassou o limite
            if (tokensEstimados > limiteTokens)
            {
                ToastContent = $"Atenção: A conversa atual possui aproximadamente {tokensEstimados} tokens, o que ultrapassa o limite de {limiteTokens} do modelo {modeloAtual}. Limpe o chat ou inicie uma nova conversa para evitar alucinações.";
                AlertVisibility = true;

                await InvokeAsync(StateHasChanged);
                return; // Bloqueia o envio para proteger a API e evitar respostas erradas
            }


            messages.Add(new ChatMessage { Role = "user", Content = userInput });

            currentAssistantMessage = new ChatMessage
            {
                Role = "assistant",
                Content = ""
            };

            messages.Add(currentAssistantMessage);

            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("scrollMessagesToBottom", messagesDiv);

            chatCts?.Cancel();
            chatCts?.Dispose();

            chatCts = new CancellationTokenSource();
            var localToken = chatCts.Token;

            lastUserPrompt = userInput;

            userInput = string.Empty;
            await InvokeAsync(StateHasChanged);

            // 🔥 Esperar o Blazor terminar o re-render
            await Task.Delay(10);

            // 🔥 Agora sim, o textarea está realmente vazio
            try { await JS.InvokeVoidAsync("chatInput.resetHeight", chatInputRef); } catch { }

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

                await foreach (var chunk in AiClient.StreamChatAsync(dtoList, null, null, language, localToken))
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

                    if (userAtBottom)
                    {
                        await JS.InvokeVoidAsync("scrollMessagesToBottom", messagesDiv);
                    }
                }

                tokensUsados = EstimateTokens(messages);
                await InvokeAsync(StateHasChanged);

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
                // Garantir que existe uma mensagem do assistente
                if (currentAssistantMessage is null)
                {
                    currentAssistantMessage = new ChatMessage
                    {
                        Role = "assistant",
                        Content = ""
                    };
                    messages.Add(currentAssistantMessage);
                }

                // Mensagem amigável para cancelamento ou timeout
                currentAssistantMessage.Content += "\n\n⚠️ O pedido foi cancelado ou interrompido. Tente novamente.";

                ProgressStatus = string.Empty;

                if (toastObj != null)
                {
                    ToastTitle = "AI";
                    ToastCssClass = "e-toast-warning";
                    ToastTimeOut = 3000;
                    ToastContent = "O pedido foi cancelado ou interrompido.";
                    await InvokeAsync(StateHasChanged);
                    await toastObj.ShowAsync();
                }
            }
            catch (Exception)
            {
                // Garantir que existe uma mensagem do assistente
                if (currentAssistantMessage is null)
                {
                    currentAssistantMessage = new ChatMessage
                    {
                        Role = "assistant",
                        Content = ""
                    };
                    messages.Add(currentAssistantMessage);
                }

                // Mensagem amigável para o utilizador
                currentAssistantMessage.Content += "\n\n⚠️ Não foi possível obter resposta da IA neste momento. Tente novamente dentro de alguns segundos.";

                ProgressStatus = string.Empty;

                if (toastObj != null)
                {
                    ToastTitle = "AI";
                    ToastCssClass = "e-toast-warning";
                    ToastTimeOut = 4000;
                    ToastContent = "A resposta foi interrompida. Tente novamente.";
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
            if (messages.Count > 0)
            {
                showConfirmNewConversation = true;
                await InvokeAsync(StateHasChanged);
            }
            else
            {
                await NewConversation();
            }
        }
        void ShowConfirmDeleteConversation() => showConfirmDeleteConversation = true;

        async Task OnConfirmYes()
        {
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
                var conv = new ConversationCreateDto { Id = id, Title = title, Json = "[]" };
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
            if (!messages.Any(m => !string.IsNullOrWhiteSpace(m.Content))) return;

            var json = JsonSerializer.Serialize(messages);

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
                var conv = new ConversationUpdateDto { Id = selectedConversationId, Title = conversations.FirstOrDefault(c => c.Id == selectedConversationId)?.Title ?? ("Conversation " + DateTime.Now.ToString("g")), Json = json };
                await ConversationsClient.SaveAsync(conv);
                await LoadConversations();
            }
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
                try
                {
                    if (toastObj != null)
                    {
                        ToastTitle = L["TituloApagarRegisto"];
                        ToastContent = L["SuccessDelete"];
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

        private async void HandleKeyDown(KeyboardEventArgs e)
        {
            if (isProcessing)
                return;

            if (e.Key == "ArrowUp")
            {
                if (!string.IsNullOrWhiteSpace(lastUserPrompt) &&
                    string.IsNullOrWhiteSpace(userInput))
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

            if (e.Key == "ArrowDown")
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
        private int EstimateTokens(List<ChatMessage> messageList)
        {
            int total = 0;

            foreach (var msg in messageList)
            {
                if (!string.IsNullOrEmpty(msg.Role))
                    total += msg.Role.Length;

                if (!string.IsNullOrEmpty(msg.Content))
                    total += msg.Content.Length;
            }

            // Média realista: 1 token ≈ 4 caracteres (línguas latinas)
            return (int)Math.Ceiling(total / 4.0);
        }
        private string GetTokenPercent()
        {
            if (limiteTokens <= 0) return "0%";
            double ratio = (double)tokensEstimados / limiteTokens;
            ratio = Math.Clamp(ratio, 0, 1);
            return (ratio * 100).ToString("0.##") + "%";
        }

        private string GetTokenClass()
        {
            if (limiteTokens <= 0) return "";

            double ratio = (double)tokensEstimados / limiteTokens;

            if (ratio < 0.40)
                return "token-low";      // verde
            else if (ratio < 0.75)
                return "token-medium";   // amarelo
            else
                return "token-high";     // vermelho
        }

        private string GetWarningClass()
        {
            if (limiteTokens <= 0) return "";

            double ratio = (double)tokensEstimados / limiteTokens;

            if (ratio >= 0.90)
                return "token-warning-high";   // vermelho forte
            else
                return "token-warning-medium"; // amarelo
        }


        private bool IsPromptValid(string userPrompt)
        {
            // Lista de palavras-chave obrigatórias
            var keywords = new[]
            {
                "autor",
                "author",
                "publica",
                "mp3",
                "mp4",
                "video",
                "lp",
                "cd",
                "dvd",
                "livro",
                "book",
                "top",
                "obras",
                "works",
                "artist"
            };

            string lowerPrompt = userPrompt.ToLower();

            return keywords.Any(keyword => lowerPrompt.Contains(keyword));
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

        private const int DefaultContextLimit = 8192;

        private static readonly Dictionary<string, int> KnownModelContextLimits = new(StringComparer.OrdinalIgnoreCase)
        {
            { "mistral:latest", 32768 },
            { "qwen3.5:latest", 128000 },
            { "phi4-mini:latest", 128000 }
        };

        private Dictionary<string, int> ModelContextLimits = new(KnownModelContextLimits, StringComparer.OrdinalIgnoreCase);
    }
}
