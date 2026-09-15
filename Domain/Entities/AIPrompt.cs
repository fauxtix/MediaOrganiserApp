using System;

namespace MediaOrganizerApp.Domain.Entities
{
    /// <summary>
    /// Auto-recorded benchmark prompt (AIPrompts table).
    /// </summary>
    public class AIPrompt
    {
        public int Id { get; set; }
        public string? Descricao { get; set; }
        public string TextoPrompt { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public double? Temperatura { get; set; }
    }
}