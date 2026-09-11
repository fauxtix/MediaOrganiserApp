using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using System.IO;
using MediaOrganizerApp.WebApi.Services;

namespace PropertyManagerFL.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AppSettingsController : ControllerBase
{
    private readonly IAppSettingsRepository _appSettingsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AppSettingsController>? _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IAiSettingsProvider _aiSettingsProvider;


    public AppSettingsController(IAppSettingsRepository appSettingsRepository,
                                 IMapper mapper,
                                 IWebHostEnvironment environment,
                                 IAiSettingsProvider aiSettingsProvider)
    {
        _appSettingsRepository = appSettingsRepository;
        _mapper = mapper;
        _environment = environment;
        _aiSettingsProvider = aiSettingsProvider;
    }

    /// <summary>
    /// Lê settings
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettingsAsync()
    {
        var location = GetControllerActionNames();

        try
        {
            var settings = await _appSettingsRepository.GetSettingsAsync();
            if (settings is not null)
            {
                var clientSettings = _mapper.Map<ApplicationSettingsDto>(settings);
                return Ok(clientSettings);
            }
            else
            {
                return NotFound();
            }

        }
        catch (Exception e)
        {
            return InternalError($"{location}: {e.Message} - {e.InnerException}");
        }
    }

    /// <summary>
    /// Altera settings do e-mail (outlook)
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    [HttpPut("updatesettings")]
    public async Task<IActionResult> UpdateSettingsAsync(ApplicationSettingsDto settings)
    {
        var location = GetControllerActionNames();
        try
        {
            if (settings == null)
            {
                string msg = "As definições passadas são incorretas.";
                _logger?.LogWarning("As definições passadas são incorretas.");
                return BadRequest(msg);
            }

            var entitySettings = _mapper.Map<ApplicationSettings>(settings);

            await _appSettingsRepository.UpdateSettingsAsync(entitySettings);
            _aiSettingsProvider.Invalidate();
            return NoContent();

        }
        catch (Exception e)
        {
            _logger?.LogError($"{location}: {e.Message} - {e.InnerException}");
            return InternalError($"{location}: {e.Message} - {e.InnerException}");
        }
    }


    /// <summary>
    /// Altera settings (outros)
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    [HttpPatch("updatelanguage")]
    public async Task<IActionResult> UpdateLanguage([FromBody] string languageToUpdate)
    {
        var location = GetControllerActionNames();

        if (string.IsNullOrEmpty(languageToUpdate))
        {
            string msg = "Campo 'Idioma' incorreto (Api AppSettings)";
            _logger?.LogWarning(msg);
            return BadRequest(msg);
        }

        try
        {
            await _appSettingsRepository.UpdateLanguageAsync(languageToUpdate);
            return NoContent();

        }
        catch (Exception e)
        {
            _logger?.LogError($"{location}: {e.Message} - {e.InnerException}");
            return InternalError($"{location}: {e.Message} - {e.InnerException}");
        }
    }


    private string GetControllerActionNames()
    {
        var controller = ControllerContext.ActionDescriptor.ControllerName;
        var action = ControllerContext.ActionDescriptor.ActionName;

        return $"{controller} - {action}";
    }

    private ObjectResult InternalError(string message)
    {
        _logger?.LogError(message);
        return StatusCode(500, "Algo de errado ocorreu. Contacte o Administrador");
    }
}
