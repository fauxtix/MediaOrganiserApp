using Dapper;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository;
public class AppSettingsRepository : IAppSettingsRepository
{
    private readonly IDapperContext _context;
    private readonly ILogger<AppSettingsRepository> _logger;

    public AppSettingsRepository(IDapperContext context, ILogger<AppSettingsRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApplicationSettings> GetSettingsAsync()
    {
        try
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "SELECT TOP 1 * FROM ApplicationSettings";
                var result = (await connection.QueryFirstOrDefaultAsync<ApplicationSettings>(sql));
                return result;
            }

        }
        catch (Exception ex)
        {
            _logger?.LogError(ex.Message, ex);
            return new ApplicationSettings();
        }
    }

    public async Task UpdateSettingsAsync(ApplicationSettings settings)
    {

        var parameters = new DynamicParameters();

        parameters.Add("@Password", settings?.Password);
        parameters.Add("@HostName", settings?.Hostname);
        parameters.Add("@UseSSL", settings?.UseSsl);
        parameters.Add("@Username", settings?.Username);
        parameters.Add("@ApiKey", settings?.ApiKey);

        parameters.Add("@DefaultLanguage", settings?.DefaultLanguage);
        parameters.Add("@Port", settings?.Port);
        parameters.Add("@Language", settings?.Language);
        parameters.Add("@PastaDownloadsPdf", settings?.PastaDownloadsPdf);
        parameters.Add("@PastaDownloadsMedia", settings?.PastaDownloadsMedia);

        parameters.Add("@PastaImagens", settings?.PastaImagens);
        parameters.Add("@PastaBackupUploads", settings?.PastaBackupUploads);
        parameters.Add("@PastaBackupBD", settings?.PastaBackupBD);
        parameters.Add("@PastaPublicacoes", settings?.PastaPublicacoes);
        parameters.Add("@PastaListagens", settings?.PastaListagens);

        parameters.Add("@PastaImagensAutores", settings?.PastaImagensAutores);
        parameters.Add("@PastaImagensCapasLivros", settings?.PastaImagensCapasLivros);
        parameters.Add("@PastaImagemFundo", settings?.PastaImagemFundo);
        parameters.Add("@RemoveImagemAposAlteracao", settings?.RemoveImagemAposAlteracao);
        parameters.Add("@TempoNotifier", settings?.TempoNotifier);
        parameters.Add("@FicheirosOrganizados", settings?.FicheirosOrganizados);
        parameters.Add("@AuthorMobile", settings?.AuthorMobile);
        parameters.Add("@SupportMail", settings?.SupportMail);
        parameters.Add("@ModelBaseUrl", settings?.ModelBaseUrl);
        parameters.Add("@AiModelName", settings?.AiModelName);

        // new settings
        parameters.Add("@ApiBaseUrl_Production", settings?.ApiBaseUrl_Production);
        parameters.Add("@ApiBaseUrl_Development", settings?.ApiBaseUrl_Development);
        parameters.Add("@FileManagerApiBaseUrl_Development", settings?.FileManagerApiBaseUrl_Development);
        parameters.Add("@FileManagerApiBaseUrl_Production", settings?.FileManagerApiBaseUrl_Production);
        parameters.Add("@ApiBaseUrl_Production", settings?.ApiBaseUrl_Production);
        parameters.Add("@ApiBaseUrl_Production", settings?.ApiBaseUrl_Production);
        parameters.Add("@IMDbKey", settings?.IMDbKey);
        parameters.Add("@Database_ConnectionString", settings?.Database_ConnectionString);

        parameters.Add("@LastFmApiKey", settings?.LastFmApiKey);
        parameters.Add("@LastFmApiSecret", settings?.LastFmApiSecret);

        parameters.Add("@GoogleBooksApiKey", settings?.GoogleBooksApiKey);
        parameters.Add("@GoogleBooksCountry", settings?.GoogleBooksCountry);

        parameters.Add("@YouTubeApiKey", settings?.YouTubeApiKey);
        parameters.Add("@YouTubeClientId", settings?.YouTubeClientId);
        parameters.Add("@YouTubeClientSecret", settings?.YouTubeClientSecret);
        parameters.Add("@YouTubeApplicationName", settings?.YouTubeApplicationName);
        try
        {
            using (var connection = _context.CreateConnection())
            {
                string sp_Name = "usp_AppSettings_Update";
                var result = (await connection.ExecuteAsync(sp_Name, param: parameters, commandType: CommandType.StoredProcedure));
                return;
            }

        }
        catch (Exception ex)
        {
            _logger?.LogError(ex.Message, ex);
        }
    }

    public async Task UpdateLanguageAsync(string language)
    {
        try
        {
            using (var connection = _context.CreateConnection())
            {
                string sp_Name = "usp_AppSettings_UpdateLanguage";
                var result = (await connection.ExecuteAsync(sp_Name,
                    new { Language = language },
                    commandType: CommandType.StoredProcedure));
            }
        }
        catch
        {

            throw;
        }
    }
}

