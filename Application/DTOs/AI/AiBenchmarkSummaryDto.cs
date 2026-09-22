using System.Collections.Generic;

namespace MediaOrganizerApp.Application.DTOs.AI
{
    /// <summary>
    /// Aggregate usage/performance summary of recorded AI responses
    /// (GET /api/ai-benchmark/summary). Only the cloud chat records responses today.
    /// </summary>
    public class AiBenchmarkSummaryDto
    {
        public int TotalRespostas { get; set; }
        public int Ultimos7Dias { get; set; }
        public double? LatenciaMediaSegundos { get; set; }
        public double? TokensPorSegundoMedio { get; set; }
        public double? TamanhoTokensMedio { get; set; }
        public List<ModeloUsageContagem> ModelosMaisUsados { get; set; } = new();
    }

    public class ModeloUsageContagem
    {
        public string Modelo { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public int Contagem { get; set; }

        /// <summary>
        /// OpenRouter model ids always include a vendor/model segment (e.g.
        /// "openrouter/free", "nex-agi/nex-n2.5-pro:free"); local Ollama names
        /// (e.g. "phi4-mini:latest") do not.
        /// </summary>
        public static string ProviderFromModelName(string model)
            => !string.IsNullOrWhiteSpace(model) && model.Contains('/')
                ? "OpenRouter"
                : "Ollama";
    }
}