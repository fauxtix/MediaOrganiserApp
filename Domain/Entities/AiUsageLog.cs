using System;

namespace MediaOrganizerApp.Domain.Entities
{
    /// <summary>
    /// Auto-recorded internal AI usage (AIUsageLog table). Tracks every non-chat
    /// AI operation performed from within the app (translate, media notes/metadata,
    /// edition year, author books, book summary, author profile) regardless of the
    /// provider used (OpenRouter cloud or local Ollama fallback).
    /// Chat exchanges are intentionally NOT recorded here — they live in AIResponses.
    /// </summary>
    public class AiUsageLog
    {
        public int AiUsageId { get; set; }
        public string? UserId { get; set; }
        public string Operacao { get; set; } = string.Empty;
        /// <summary>'openrouter' | 'ollama' | 'openai-sdk'</summary>
        public string Provider { get; set; } = string.Empty;
        public string? Modelo { get; set; }
        public string? Contexto { get; set; }
        public string? Scenario { get; set; }
        public double DurationMs { get; set; }
        public int? TokenCount { get; set; }
        /// <summary>True when Provider is the local Ollama fallback.</summary>
        public bool EraFallback { get; set; }
        public bool Sucesso { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}