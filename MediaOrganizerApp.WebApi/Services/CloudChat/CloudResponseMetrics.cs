namespace MediaOrganizerApp.WebApi.Services.CloudChat;

/// <summary>Token/metric heuristics for auto-recorded benchmark rows.</summary>
public static class CloudResponseMetrics
{
    /// <summary>Rough token estimate: ~3 characters per token.</summary>
    public static int EstimateTokens(string? text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return Math.Max(1, (int)Math.Ceiling(text.Length / 3.0));
    }

    /// <summary>Short human-readable label for a prompt row.</summary>
    public static string SmartFallbackDescription(string? prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt)) return "Consulta";
        var firstLine = prompt.Split('\n')[0].Trim();
        return firstLine.Length > 80 ? $"{firstLine[..80]}..." : firstLine;
    }

    /// <summary>
    /// Tokens/s físicos de geração: tokens de saída a dividir pela janela de geração
    /// (tempo total do stream - tempo até ao primeiro token). Respostas sub-segundo
    /// (cache/burst), sem tokens de saída ou com janela nula/negativa não têm taxa válida.
    /// </summary>
    public static double? ComputeTokensPerSecond(int completionTokens, double seconds, double firstChunkSeconds)
    {
        var generationWindow = seconds - Math.Max(0, firstChunkSeconds);
        if (completionTokens <= 0 || generationWindow < 1.0)
            return null;
        return Math.Round(completionTokens / generationWindow, 2);
    }
}