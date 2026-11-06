-- YouTube settings (2026-11-06)
-- Adds YouTube API key / OAuth client id / client secret columns to ApplicationSettings
-- so they can be managed in the admin "Serviços" tab (AppSettings2 page),
-- replacing the hardcoded values in MediaOrganizerApp.Persistence/Services/Youtube/Settings/Keys.cs.

IF COL_LENGTH('dbo.ApplicationSettings', 'YouTubeApiKey') IS NULL
BEGIN
    ALTER TABLE dbo.ApplicationSettings ADD YouTubeApiKey nvarchar(255) NULL;
END;

IF COL_LENGTH('dbo.ApplicationSettings', 'YouTubeClientId') IS NULL
BEGIN
    ALTER TABLE dbo.ApplicationSettings ADD YouTubeClientId nvarchar(255) NULL;
END;

IF COL_LENGTH('dbo.ApplicationSettings', 'YouTubeClientSecret') IS NULL
BEGIN
    ALTER TABLE dbo.ApplicationSettings ADD YouTubeClientSecret nvarchar(255) NULL;
END;

IF COL_LENGTH('dbo.ApplicationSettings', 'YouTubeApplicationName') IS NULL
BEGIN
    ALTER TABLE dbo.ApplicationSettings ADD YouTubeApplicationName nvarchar(128) NULL;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_AppSettings_Update
    @Password nvarchar(255),
    @HostName nvarchar(255),
    @UseSSL bit,
    @Username nvarchar(255),
    @ApiKey nvarchar(255),
    @DefaultLanguage nvarchar(5),
    @Port int,
    @Language nvarchar(255),
    @PastaDownloadsPdf nvarchar(255),
    @PastaDownloadsMedia nvarchar(255),
    @PastaImagens nvarchar(255),
    @PastaBackupUploads nvarchar(255),
    @PastaBackupBD nvarchar(255),
    @PastaPublicacoes nvarchar(255),
    @PastaListagens nvarchar(255),
    @PastaImagensAutores nvarchar(255),
    @PastaImagensCapasLivros nvarchar(255),
    @PastaImagemFundo nvarchar(255),
    @RemoveImagemAposAlteracao bit,
    @TempoNotifier int,
    @FicheirosOrganizados bit,
    @AuthorMobile nvarchar(20),
    @SupportMail nvarchar(255),
    @ModelBaseUrl nvarchar(128),
    @AiModelName nvarchar(50),
    @ApiBaseUrl_Development nvarchar(128),
    @ApiBaseUrl_Production nvarchar(128),
    @FileManagerApiBaseUrl_Development nvarchar(128),
    @FileManagerApiBaseUrl_Production nvarchar(128),
    @IMDBKey nvarchar(50),
    @Database_ConnectionString nvarchar(255),
    @LastFmApiKey nvarchar(128),
    @LastFmApiSecret nvarchar(128),
    @GoogleBooksApiKey nvarchar(255),
    @GoogleBooksCountry nvarchar(10),
    @YouTubeApiKey nvarchar(255),
    @YouTubeClientId nvarchar(255),
    @YouTubeClientSecret nvarchar(255),
    @YouTubeApplicationName nvarchar(128)
AS
UPDATE dbo.ApplicationSettings
   SET [Password] = @Password
      ,[HostName] = @HostName
      ,[UseSSL] = @UseSSL
      ,[Username] = @Username
      ,[ApiKey] = @ApiKey
      ,[DefaultLanguage] = @DefaultLanguage
      ,[Port] = @Port
      ,[Language] = @Language
      ,[PastaDownloadsPdf] = @PastaDownloadsPdf
      ,[PastaDownloadsMedia] = @PastaDownloadsMedia
      ,[PastaImagens] = @PastaImagens
      ,[PastaBackupUploads] = @PastaBackupUploads
      ,[PastaBackupBD] = @PastaBackupBD
      ,[PastaPublicacoes] = @PastaPublicacoes
      ,[PastaListagens] = @PastaListagens
      ,[PastaImagensAutores] = @PastaImagensAutores
      ,[PastaImagensCapasLivros] = @PastaImagensCapasLivros
      ,[PastaImagemFundo] = @PastaImagemFundo
      ,[RemoveImagemAposAlteracao] = @RemoveImagemAposAlteracao
      ,[TempoNotifier] = @TempoNotifier
      ,[FicheirosOrganizados] = @FicheirosOrganizados
      ,[AuthorMobile] = @AuthorMobile
      ,[SupportMail] = @SupportMail
      ,[ModelBaseUrl] = @ModelBaseUrl
      ,[AiModelName] = @AiModelName
      ,ApiBaseUrl_Development = @ApiBaseUrl_Development
      ,ApiBaseUrl_Production = @ApiBaseUrl_Production
      ,FileManagerApiBaseUrl_Development = @FileManagerApiBaseUrl_Development
      ,FileManagerApiBaseUrl_Production = @FileManagerApiBaseUrl_Production
      ,IMDBKey = @IMDBKey
      ,Database_ConnectionString = @Database_ConnectionString
      ,LastFmApiKey = @LastFmApiKey
      ,LastFmApiSecret = @LastFmApiSecret
      ,GoogleBooksApiKey = @GoogleBooksApiKey
      ,GoogleBooksCountry = @GoogleBooksCountry
      ,YouTubeApiKey = @YouTubeApiKey
      ,YouTubeClientId = @YouTubeClientId
      ,YouTubeClientSecret = @YouTubeClientSecret
      ,YouTubeApplicationName = @YouTubeApplicationName;
GO
