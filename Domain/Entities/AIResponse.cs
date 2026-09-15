using System;

namespace MediaOrganizerApp.Domain.Entities
{
    /// <summary>
    /// Auto-recorded model response (AIResponses table). Single-slot layout with
    /// the 12 jury metrics present (nullable) for future benchmark evaluations.
    /// </summary>
    public class AIResponse
    {
        public int Id { get; set; }
        public int PromptId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string NomeModelo { get; set; } = string.Empty;
        public string? ModeloResolvido { get; set; }
        public string TextoResposta { get; set; } = string.Empty;
        public double? TempoProcessamento { get; set; }
        public double? TokensPorSegundo { get; set; }
        public double? TempoPuroMs { get; set; }
        public double? TempoCargaMs { get; set; }
        public int? TamanhoTokens { get; set; }
        public string? IdiomaSessao { get; set; }
        public DateTime DataCriacao { get; set; }

        public int? FactualRating { get; set; }
        public int? FormattingRating { get; set; }
        public int? ComplianceRating { get; set; }
        public int? RelevanceRating { get; set; }
        public int? ToneRating { get; set; }
        public int? ConcisenessRating { get; set; }
        public int? ClarityRating { get; set; }
        public int? ReadabilityRating { get; set; }
        public int? HaloEffectRating { get; set; }
        public int? SafetyRating { get; set; }
        public int? LanguageConsistencyRating { get; set; }
        public int? LoopDetectionRating { get; set; }
    }
}