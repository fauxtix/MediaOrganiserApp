using System.Text.Json;
using System.Text.Json.Serialization;
using MediaOrganizerApp.Application.DTOs.AI.CloudChat;

namespace MediaOrganizerApp.WebApi.Services.CloudChat
{
    /// <summary>
    /// Builds the OpenRouter chat completions body. The reasoning flag is omitted
    /// when disabled to avoid 400 errors on models that do not support it.
    /// </summary>
    public static class CloudRequestBodyBuilder
    {
        public static Dictionary<string, object> BuildOpenRouterBody(CloudChatPayload payload)
        {
            var body = new Dictionary<string, object>
            {
                ["model"] = payload.Model,
                ["messages"] = payload.Messages
                    .Select(m => new { role = m.Role, content = m.Content })
                    .ToArray(),
                ["stream"] = true,
                ["stream_options"] = new { include_usage = true },
                ["temperature"] = payload.Temperature,
                ["max_tokens"] = payload.MaxTokens ?? 2048
            };

            if (payload.EnableReasoning)
            {
                body["reasoning"] = new { enabled = true };
            }

            return body;
        }

        public static string Serialize(CloudChatPayload payload)
        {
            return JsonSerializer.Serialize(BuildOpenRouterBody(payload));
        }
    }
}