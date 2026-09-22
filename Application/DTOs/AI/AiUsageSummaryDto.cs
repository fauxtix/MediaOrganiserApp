using System;
using System.Collections.Generic;

namespace MediaOrganizerApp.Application.DTOs.AI;

public sealed class AiUsageSummaryDto
{
    public int TotalGeral { get; set; }
    public int Ultimos7 { get; set; }
    public int UsosOpenRouter { get; set; }
    public int UsosOllama { get; set; }
    public int UsosFallback { get; set; }
    public List<AiUsageOperacaoDto> PorOperacao { get; set; } = new();
    public List<AiUsageRegistoDto> UltimosRegistos { get; set; } = new();
}

public sealed class AiUsageOperacaoDto
{
    public string Operacao { get; set; } = string.Empty;
    public int Contagem { get; set; }
    public int Ultimos7 { get; set; }
    public int OpenRouter { get; set; }
    public int Ollama { get; set; }
    public int Fallback { get; set; }
}

public sealed class AiUsageRegistoDto
{
    public int Id { get; set; }
    public string Operacao { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? Contexto { get; set; }
    public string? Scenario { get; set; }
    public double DurationMs { get; set; }
    public bool Sucesso { get; set; }
    public DateTime DataCriacao { get; set; }
}