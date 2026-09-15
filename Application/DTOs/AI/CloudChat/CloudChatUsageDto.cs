using System;

namespace MediaOrganizerApp.Application.DTOs.AI.CloudChat;

/// <summary>
/// Aggregate usage/limits state for the OpenRouter cloud chat, surfaced to the
/// admin settings page (GET /api/cloud-chat/usage).
/// </summary>
public class CloudChatUsageDto
{
    public bool ChaveConfigurada { get; set; }
    public bool EhFreeTier { get; set; }
    public bool PodeUsarModelosPagos { get; set; }
    public string? Erro { get; set; }

    public double? TotalCredits { get; set; }
    public double? SaldoRestante { get; set; }
    public double? Usados { get; set; }
    public double? UsoHoje { get; set; }
    public double? UsoSemana { get; set; }
    public double? UsoMes { get; set; }
    public double? LimiteKey { get; set; }
    public double? LimiteKeyRestante { get; set; }

    public int UsadosGratisHoje { get; set; }
    public int TetoDiario { get; set; }
    public double PercentagemUsada { get; set; }
    public DateTime? ResetUTC { get; set; }
    public bool TetoAutomatico { get; set; }
}