using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.DTOs.AI.CloudChat;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.WebApi.Services.CloudChat;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/cloud-chat")]
    public class CloudChatController : ControllerBase
    {
        private readonly OpenRouterChatService _chatService;
        private readonly OpenRouterCatalogService _catalogService;
        private readonly ICloudChatUsageService _usageService;
        private readonly IAIChatSettingsRepository _settingsRepository;
        private readonly ILogger<CloudChatController> _logger;

        public CloudChatController(
            OpenRouterChatService chatService,
            OpenRouterCatalogService catalogService,
            ICloudChatUsageService usageService,
            IAIChatSettingsRepository settingsRepository,
            ILogger<CloudChatController> logger)
        {
            _chatService = chatService;
            _catalogService = catalogService;
            _usageService = usageService;
            _settingsRepository = settingsRepository;
            _logger = logger;
        }

        private string? TryUserId()
        {
            var value = Request.Headers["X-UserId"].ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private bool TryIsAdmin()
        {
            var value = Request.Headers["X-IsAdmin"].ToString();
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        [HttpPost("stream")]
        public async Task Stream([FromBody] CloudChatStreamRequest request, CancellationToken ct)
        {
            if (request?.Payload is null ||
                string.IsNullOrWhiteSpace(request.Payload.Model) ||
                request.Payload.Messages is null ||
                request.Payload.Messages.Count == 0)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                await Response.WriteAsync("{\"error\":\"payload.model and payload.messages are required.\"}", ct);
                return;
            }

            Response.ContentType = "application/x-ndjson";
            Response.StatusCode = StatusCodes.Status200OK;

            try
            {
                var chunks = _chatService.StreamChatAsync(request.Payload, TryUserId(), request.Language, ct);
                await foreach (var chunk in chunks)
                {
                    await Response.WriteAsync(JsonSerializer.Serialize(chunk), ct);
                    await Response.WriteAsync("\n", ct);
                    await Response.Body.FlushAsync(ct);
                }
            }
            catch (OperationCanceledException)
            {
                // client disconnected - stop streaming gracefully
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CloudChat] Erro durante streaming.");
                if (!Response.HasStarted)
                {
                    Response.StatusCode = StatusCodes.Status502BadGateway;
                    await Response.WriteAsync(JsonSerializer.Serialize(new { error = SanitizeMessage(ex) }), ct);
                }
            }
        }

        [HttpGet("models")]
        public async Task<ActionResult> Models(CancellationToken ct) =>
            Ok(await _catalogService.GetFreeModelsAsync(ct));

        [HttpGet("settings")]
        public async Task<ActionResult<CloudChatSettingsDto>> GetSettings(CancellationToken ct)
        {
            if (!TryIsAdmin()) return Forbid();

            var dto = new CloudChatSettingsDto
            {
                ApiKeyConfigured = !string.IsNullOrWhiteSpace(
                    await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.OpenRouterApiKey, null, ct)),
                DefaultModel = await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.DefaultModel, null, ct)
                    ?? CloudChatSettingsKeys.DefaultModelValue,
                EnableReasoning = bool.TryParse(await _settingsRepository.GetValueAsync(
                    CloudChatSettingsKeys.EnableReasoning, null, ct), out var enabled) && enabled,
                AllowPaidModels = bool.TryParse(await _settingsRepository.GetValueAsync(
                    CloudChatSettingsKeys.AllowPaidModels, null, ct), out var allowPaid) && allowPaid,
                FreeDailyCap = int.TryParse(await _settingsRepository.GetValueAsync(
                    CloudChatSettingsKeys.FreeDailyCap, null, ct), out var freeCap) && freeCap > 0
                    ? freeCap
                    : null,
                AuthorBooksUseCloud = bool.TryParse(await _settingsRepository.GetValueAsync(
                    CloudChatSettingsKeys.AuthorBooksUseCloud, null, ct), out var useCloud)
                    ? useCloud
                    : true
            };

            return Ok(dto);
        }

        [HttpGet("usage")]
        public async Task<ActionResult<CloudChatUsageDto>> GetUsage(CancellationToken ct)
        {
            if (!TryIsAdmin()) return Forbid();
            return Ok(await _usageService.GetUsageAsync(ct));
        }

        [HttpGet("runtime-settings")]
        public async Task<ActionResult<CloudChatRuntimeSettingsDto>> GetRuntimeSettings(CancellationToken ct)
        {
            var allowPaid = bool.TryParse(
                await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.AllowPaidModels, null, ct),
                out var paid) && paid;

            var defaultModel = await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.DefaultModel, null, ct)
                ?? CloudChatSettingsKeys.DefaultModelValue;

            var dto = new CloudChatRuntimeSettingsDto
            {
                DefaultModel = CloudChatSettingsKeys.ApplyPaidModelGate(defaultModel, allowPaid),
                EnableReasoning = bool.TryParse(await _settingsRepository.GetValueAsync(
                    CloudChatSettingsKeys.EnableReasoning, null, ct), out var enabled) && enabled
            };

            return Ok(dto);
        }

        [HttpPut("settings")]
        public async Task<ActionResult> UpdateSettings(
            [FromBody] CloudChatSettingsUpdateDto dto,
            CancellationToken ct)
        {
            if (!TryIsAdmin()) return Forbid();
            if (dto is null) return BadRequest("Settings payload is required.");

            if (!string.IsNullOrWhiteSpace(dto.ApiKey))
            {
                await _settingsRepository.SetValueAsync(CloudChatSettingsKeys.OpenRouterApiKey,
                    dto.ApiKey.Trim(), null, ct);
            }

            if (!string.IsNullOrWhiteSpace(dto.DefaultModel))
            {
                await _settingsRepository.SetValueAsync(CloudChatSettingsKeys.DefaultModel,
                    dto.DefaultModel.Trim(), null, ct);
            }

            if (dto.EnableReasoning.HasValue)
            {
                await _settingsRepository.SetValueAsync(CloudChatSettingsKeys.EnableReasoning,
                    dto.EnableReasoning.Value ? "true" : "false", null, ct);
            }

            if (dto.AllowPaidModels.HasValue)
            {
                await _settingsRepository.SetValueAsync(CloudChatSettingsKeys.AllowPaidModels,
                    dto.AllowPaidModels.Value ? "true" : "false", null, ct);
            }

            if (dto.FreeDailyCap.HasValue)
            {
                if (dto.FreeDailyCap.Value > 0)
                {
                    await _settingsRepository.SetValueAsync(CloudChatSettingsKeys.FreeDailyCap,
                        dto.FreeDailyCap.Value.ToString(System.Globalization.CultureInfo.InvariantCulture), null, ct);
                }
                else
                {
                    await _settingsRepository.DeleteValueAsync(CloudChatSettingsKeys.FreeDailyCap, null, ct);
                }
            }

            if (dto.AuthorBooksUseCloud.HasValue)
            {
                await _settingsRepository.SetValueAsync(CloudChatSettingsKeys.AuthorBooksUseCloud,
                    dto.AuthorBooksUseCloud.Value ? "true" : "false", null, ct);
            }

            return Ok();
        }

        private static string SanitizeMessage(Exception ex)
        {
            var message = ex.Message;
            if (string.IsNullOrWhiteSpace(message)) return "Ocorreu um erro ao comunicar com o OpenRouter.";
            return message.Length > 300 ? $"{message[..300]}..." : message;
        }
    }
}