using System;

namespace MediaOrganizerApp.Application.DTOs.AI.CloudChat;

public class OpenRouterCatalogModel
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public int? ContextLength { get; set; }
    public bool EhRouter { get; set; }
    public bool EhGratuito { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime? ReleaseDate { get; set; }
    public double? IntelligenceIndex { get; set; }
    public double? CodingIndex { get; set; }
    public double? AgenticIndex { get; set; }
}