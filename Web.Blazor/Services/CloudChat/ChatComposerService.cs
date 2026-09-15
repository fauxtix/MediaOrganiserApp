using MediaOrganizerApp.Application.DTOs.AI.CloudChat;

namespace Web.Blazor.Services.CloudChat
{
    /// <summary>
    /// Builds the request payload for the OpenRouter chat: trims history to the
    /// model context window, recommends a temperature, and extracts summaries.
    /// </summary>
    public sealed class ChatComposerService
    {
        public const string SummaryMarker = "SUMMARY:";
        public const string SummarySeparator = "---";

        public static PreparedCloudRequest? Prepare(
            IReadOnlyList<CloudChatMessage> messages,
            string userPrompt,
            string systemInstructions,
            string welcomeMessage,
            string modelName,
            int contextLength = 0,
            bool enableReasoning = false)
        {
            if (string.IsNullOrWhiteSpace(userPrompt))
                return null;

            var history = BuildHistory(messages, systemInstructions, welcomeMessage, contextLength);
            if (history.Count == 0 || history[^1].Role != "user")
                return null;

            double temperature = ChatMeasureTemperature.ObterTemperaturaRecomendada(userPrompt);

            var payload = new CloudChatPayload
            {
                Model = modelName,
                Messages = history,
                Temperature = Math.Round(temperature, 2),
                MaxTokens = 2048,
                Stream = true,
                EnableReasoning = enableReasoning
            };

            return new PreparedCloudRequest
            {
                Payload = payload,
                Temperature = temperature
            };
        }

        public static List<CloudChatMessage> BuildHistory(
            IReadOnlyList<CloudChatMessage> messages,
            string systemInstructions,
            string welcomeMessage,
            int contextLength = 0)
        {
            var history = new List<CloudChatMessage>();

            if (!string.IsNullOrWhiteSpace(systemInstructions))
                history.Add(new CloudChatMessage { Role = "system", Content = systemInstructions });

            foreach (var msg in messages)
            {
                if (string.Equals(msg.Role, "system", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (string.Equals(msg.Role, "user", StringComparison.OrdinalIgnoreCase))
                {
                    history.Add(new CloudChatMessage { Role = "user", Content = msg.Content });
                }
                else if (msg.Content != welcomeMessage && msg.Content != "...")
                {
                    history.Add(new CloudChatMessage { Role = "assistant", Content = msg.Content });
                }
            }

            if (contextLength > 0)
                history = TrimHistoryToContext(history, contextLength);

            return history;
        }

        private static List<CloudChatMessage> TrimHistoryToContext(List<CloudChatMessage> history, int contextLength)
        {
            int outputReserve = Math.Min(2048, contextLength / 4);
            int budget = contextLength - outputReserve;
            if (budget <= 0) return history;

            int totalTokens = history.Sum(m => EstimateTokens(m.Content));
            if (totalTokens <= budget) return history;

            var trimmed = new List<CloudChatMessage>(history);

            int i = 0;
            while (i < trimmed.Count - 1 && trimmed.Sum(m => EstimateTokens(m.Content)) > budget)
            {
                if (trimmed[i].Role == "system")
                {
                    i++;
                    continue;
                }

                trimmed.RemoveAt(i);
                if (i > 0 && trimmed[i - 1].Role == "user")
                {
                    trimmed.RemoveAt(i - 1);
                    i = Math.Max(0, i - 1);
                }
            }

            if (trimmed.Count == 0 || trimmed[^1].Role != "user")
                return history;

            return trimmed;
        }

        public static int EstimateTokens(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return Math.Max(1, (int)Math.Ceiling(text.Length / 4.0));
        }

        public static (string? Summary, string CleanText) ExtractSummaryFromResponse(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
                return (null, responseText);

            var lines = responseText.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith(SummaryMarker, StringComparison.OrdinalIgnoreCase))
                {
                    var summary = line[SummaryMarker.Length..].Trim();
                    var remaining = string.Join('\n', lines.Skip(i + 1)).Trim();
                    if (!remaining.Contains(SummarySeparator))
                        return (summary, remaining);
                }
            }

            return (null, responseText);
        }

        public static string SmartFallbackDescription(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt)) return string.Empty;
            var firstSentence = prompt.Split(['.', '\n', '\r'])[0].Trim();
            if (firstSentence.Length <= 50) return firstSentence;
            var lastSpace = firstSentence.LastIndexOf(' ', 50);
            return lastSpace > 0 ? firstSentence[..lastSpace] : firstSentence[..50];
        }
    }

    public sealed class PreparedCloudRequest
    {
        public CloudChatPayload Payload { get; set; } = new();
        public double Temperature { get; set; }
    }
}