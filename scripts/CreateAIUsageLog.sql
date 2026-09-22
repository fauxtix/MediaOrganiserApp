/* ============================================================
   CreateAIUsageLog.sql
   Registo do uso interno de IA (OpenRouter / Ollama) nas
   funcionalidades da app (translate, media-notes, media-metadata,
   edition-year, author-books, book-summary, author-profile).

   Separado de AIResponses/AIPrompts (que refletem apenas o chat
   cloud). Chat mantém-se como está.

   Aplicar: sqlcmd -S "(local)" -d Publicacoes_Prod -i CreateAIUsageLog.sql
   ============================================================ */
IF OBJECT_ID(N'dbo.AIUsageLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AIUsageLog
    (
        AIUsageId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AIUsageLog PRIMARY KEY,
        UserID       NVARCHAR(450)     NULL,
        Operacao     NVARCHAR(64)      NOT NULL,
        Provider     NVARCHAR(16)      NOT NULL,   -- 'openrouter' | 'ollama' | 'openai-sdk'
        Modelo       NVARCHAR(128)     NULL,
        Contexto     NVARCHAR(512)     NULL,       -- ex.: 'Artista - Titulo', 'origem->destino', autor
        Scenario     NVARCHAR(64)      NULL,       -- ex.: 'cloud-fallback', 'googlebooks', 'author-api', 'media-api', 'direct'
        DurationMs   FLOAT             NOT NULL CONSTRAINT DF_AIUsageLog_DurationMs DEFAULT (0),
        TokenCount   INT               NULL,
        EraFallback  BIT               NOT NULL CONSTRAINT DF_AIUsageLog_EraFallback DEFAULT (0),
        Sucesso      BIT               NOT NULL CONSTRAINT DF_AIUsageLog_Sucesso DEFAULT (1),
        DataCriacao  DATETIME2(3)      NOT NULL CONSTRAINT DF_AIUsageLog_DataCriacao DEFAULT (SYSUTCDATETIME())
    );

    CREATE NONCLUSTERED INDEX IX_AIUsageLog_DataCriacao ON dbo.AIUsageLog (DataCriacao DESC);
    CREATE NONCLUSTERED INDEX IX_AIUsageLog_UserID      ON dbo.AIUsageLog (UserID) INCLUDE (Operacao, Provider);
    CREATE NONCLUSTERED INDEX IX_AIUsageLog_Operacao    ON dbo.AIUsageLog (Operacao) INCLUDE (Provider, EraFallback);
END
GO