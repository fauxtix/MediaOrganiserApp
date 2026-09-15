using System;

namespace MediaOrganizerApp.Domain.Entities
{
    /// <summary>
    /// A key/value record from the AIChatSettings table.
    /// UserId is NULL for global (admin-managed) settings.
    /// </summary>
    public class AIChatSetting
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string Chave { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}