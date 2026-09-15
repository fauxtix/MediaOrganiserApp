using System.Text.Json;
using MediaOrganizerApp.Application.DTOs.AI.CloudChat;

namespace MediaOrganizerApp.WebApi.Services.CloudChat;

public static class ChatChunkParser
{
    public static CloudChatChunk? ParseOpenRouterChunk(string line)
    {
        try
        {
            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;

            string? text = null;
            string? reasoning = null;
            if (root.TryGetProperty("choices", out var choices) &&
                choices.GetArrayLength() > 0 &&
                choices[0].TryGetProperty("delta", out var delta))
            {
                if (delta.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                    text = content.GetString();

                if ((delta.TryGetProperty("reasoning", out var reasoningProp) ||
                     delta.TryGetProperty("reasoning_content", out reasoningProp)) &&
                    reasoningProp.ValueKind == JsonValueKind.String)
                    reasoning = reasoningProp.GetString();
            }

            string? model = null;
            if (root.TryGetProperty("model", out var modelProp) && modelProp.ValueKind == JsonValueKind.String)
            {
                model = modelProp.GetString();
            }

            int? prompt = null, completion = null, total = null;
            if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
            {
                prompt = TryGetInt(usage, "prompt_tokens");
                completion = TryGetInt(usage, "completion_tokens");
                total = TryGetInt(usage, "total_tokens");
            }

            if (text is null && reasoning is null && model is null && prompt is null && completion is null && total is null)
                return null;

            return new CloudChatChunk
            {
                Text = text ?? string.Empty,
                Reasoning = reasoning,
                Model = model,
                PromptTokens = prompt,
                CompletionTokens = completion,
                TotalTokens = total
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static int? TryGetInt(JsonElement obj, string name)
    {
        if (obj.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.Number)
        {
            if (prop.TryGetInt32(out var value))
                return value;
        }

        return null;
    }
}