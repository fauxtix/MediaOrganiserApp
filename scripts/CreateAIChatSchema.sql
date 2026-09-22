-- =============================================================================
-- AIChat schema for the OpenRouter "Test Chat" feature (MediaOrganizer)
-- - AIChatSettings : key/value settings (UserID NULL = global / admin-managed)
-- - AIPrompts      : auto-recorded prompts (unique per TextoPrompt + UserID)
-- - AIResponses    : auto-recorded model responses (single-slot, 12-metric future-ready)
-- - Conversations  : adds optional UserID column (per-user isolation of chat data)
-- Idempotent: safe to run multiple times.
-- =============================================================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
GO

-- -----------------------------------------------------------------------------
-- AIChatSettings
-- -----------------------------------------------------------------------------
IF OBJECT_ID('dbo.AIChatSettings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AIChatSettings (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AIChatSettings PRIMARY KEY,
        UserID nvarchar(450) NULL,          -- NULL = global / admin-managed setting
        Chave nvarchar(100) NOT NULL,
        Valor nvarchar(max) NOT NULL,
        UpdatedAt datetime2 NOT NULL CONSTRAINT DF_AIChatSettings_UpdatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_AIChatSettings_UserID_Chave' AND object_id = OBJECT_ID('dbo.AIChatSettings')
)
    CREATE UNIQUE INDEX UX_AIChatSettings_UserID_Chave
        ON dbo.AIChatSettings(UserID, Chave);
GO

IF OBJECT_ID('dbo.usp_AIChatSettings_GetAll', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_AIChatSettings_GetAll;
GO
CREATE PROCEDURE dbo.usp_AIChatSettings_GetAll
    @UserID nvarchar(450) = NULL,
    @IsAdmin bit = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @IsAdmin = 1
    BEGIN
        -- Admin: sees everything (global + every user override)
        SELECT UserID, Chave, Valor, UpdatedAt
        FROM dbo.AIChatSettings;
        RETURN;
    END

    IF @UserID IS NULL OR @UserID = ''
    BEGIN
        -- No user context: only global settings
        SELECT UserID, Chave, Valor, UpdatedAt
        FROM dbo.AIChatSettings
        WHERE UserID IS NULL;
        RETURN;
    END

    -- Regular user: global defaults first, then that user's overrides
    SELECT UserID, Chave, Valor, UpdatedAt
    FROM dbo.AIChatSettings
    WHERE UserID IS NULL OR UserID = @UserID
    ORDER BY CASE WHEN UserID IS NULL THEN 0 ELSE 1 END, Chave;
END
GO

IF OBJECT_ID('dbo.usp_AIChatSettings_GetValue', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_AIChatSettings_GetValue;
GO
CREATE PROCEDURE dbo.usp_AIChatSettings_GetValue
    @Chave nvarchar(100),
    @UserID nvarchar(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- 1) user-specific value if present
    IF @UserID IS NOT NULL AND @UserID <> ''
    BEGIN
        DECLARE @UserValue nvarchar(max);
        SELECT @UserValue = Valor
        FROM dbo.AIChatSettings
        WHERE Chave = @Chave AND UserID = @UserID;

        IF @UserValue IS NOT NULL
        BEGIN
            SELECT @UserValue AS Valor;
            RETURN;
        END
    END

    -- 2) fallback to global value
    SELECT Valor
    FROM dbo.AIChatSettings
    WHERE Chave = @Chave AND UserID IS NULL;
END
GO

IF OBJECT_ID('dbo.usp_AIChatSettings_Upsert', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_AIChatSettings_Upsert;
GO
CREATE PROCEDURE dbo.usp_AIChatSettings_Upsert
    @Chave nvarchar(100),
    @Valor nvarchar(max),
    @UserID nvarchar(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.AIChatSettings
    SET Valor = @Valor, UpdatedAt = SYSUTCDATETIME()
    WHERE Chave = @Chave
      AND ((UserID IS NULL AND @UserID IS NULL) OR (UserID = @UserID));

    IF @@ROWCOUNT = 0
    BEGIN
        INSERT INTO dbo.AIChatSettings (UserID, Chave, Valor, UpdatedAt)
        VALUES (@UserID, @Chave, @Valor, SYSUTCDATETIME());
    END
END
GO

IF OBJECT_ID('dbo.usp_AIChatSettings_Delete', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_AIChatSettings_Delete;
GO
CREATE PROCEDURE dbo.usp_AIChatSettings_Delete
    @Chave nvarchar(100),
    @UserID nvarchar(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.AIChatSettings WHERE Chave = @Chave AND UserID = @UserID;
END
GO

-- -----------------------------------------------------------------------------
-- AIPrompts (auto-recorded benchmark prompts)
-- -----------------------------------------------------------------------------
IF OBJECT_ID('dbo.AIPrompts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AIPrompts (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AIPrompts PRIMARY KEY,
        Descricao nvarchar(500) NULL,
        TextoPrompt nvarchar(max) NOT NULL,
        UserID nvarchar(450) NOT NULL DEFAULT (''),
        DataCriacao datetime2 NOT NULL CONSTRAINT DF_AIPrompts_DataCriacao DEFAULT (SYSUTCDATETIME()),
        Temperatura float NULL,
        PromptHash AS (CONVERT(binary(32), HASHBYTES('SHA2_256', TextoPrompt))) PERSISTED,
        NormalizedUserID AS (ISNULL(NULLIF(UserID, ''), '')) PERSISTED
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_AIPrompts_PromptHash_UserID' AND object_id = OBJECT_ID('dbo.AIPrompts')
)
    CREATE UNIQUE INDEX UX_AIPrompts_PromptHash_UserID
        ON dbo.AIPrompts(PromptHash, NormalizedUserID);
GO

IF OBJECT_ID('dbo.usp_AIPrompt_GetOrCreate', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_AIPrompt_GetOrCreate;
GO
CREATE PROCEDURE dbo.usp_AIPrompt_GetOrCreate
    @TextoPrompt nvarchar(max),
    @UserID nvarchar(450) = '',
    @Descricao nvarchar(500) = NULL,
    @Temperatura float = NULL,
    @PromptId int = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NormalizedUser nvarchar(450) = ISNULL(NULLIF(@UserID, ''), '');
    DECLARE @Hash binary(32) = CONVERT(binary(32), HASHBYTES('SHA2_256', @TextoPrompt));

    SELECT @PromptId = Id
    FROM dbo.AIPrompts
    WHERE PromptHash = @Hash
      AND NormalizedUserID = @NormalizedUser;

    IF @PromptId IS NULL
    BEGIN
        INSERT INTO dbo.AIPrompts (Descricao, TextoPrompt, UserID, DataCriacao, Temperatura)
        VALUES (@Descricao, @TextoPrompt, @NormalizedUser, SYSUTCDATETIME(), @Temperatura);

        SET @PromptId = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        -- Keep the description fresh without touching uniqueness
        UPDATE dbo.AIPrompts
        SET Descricao = COALESCE(@Descricao, Descricao),
            Temperatura = COALESCE(@Temperatura, Temperatura)
        WHERE Id = @PromptId;
    END
END
GO

-- -----------------------------------------------------------------------------
-- AIResponses (auto-recorded benchmark responses, single slot + 12 metrics)
-- -----------------------------------------------------------------------------
IF OBJECT_ID('dbo.AIResponses', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AIResponses (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AIResponses PRIMARY KEY,
        PromptId int NOT NULL CONSTRAINT FK_AIResponses_AIPrompts FOREIGN KEY
            REFERENCES dbo.AIPrompts(Id) ON DELETE CASCADE,
        UserID nvarchar(450) NOT NULL DEFAULT (''),
        NomeModelo nvarchar(200) NOT NULL,
        ModeloResolvido nvarchar(200) NULL,
        TextoResposta nvarchar(max) NOT NULL,
        TempoProcessamento float NULL,      -- total seconds (stream duration)
        TokensPorSegundo float NULL,
        TempoPuroMs float NULL,
        TempoCargaMs float NULL,            -- time to first chunk
        TamanhoTokens int NULL,             -- estimated
        IdiomaSessao nvarchar(50) NULL,
        DataCriacao datetime2 NOT NULL CONSTRAINT DF_AIResponses_DataCriacao DEFAULT (SYSUTCDATETIME()),

        -- Single-slot 12-metric jury columns (NULL until a judge UI exists)
        FactualRating int NULL,
        FormattingRating int NULL,
        ComplianceRating int NULL,
        RelevanceRating int NULL,
        ToneRating int NULL,
        ConcisenessRating int NULL,
        ClarityRating int NULL,
        ReadabilityRating int NULL,
        HaloEffectRating int NULL,
        SafetyRating int NULL,
        LanguageConsistencyRating int NULL,
        LoopDetectionRating int NULL
    );

    CREATE INDEX IX_AIResponses_PromptId ON dbo.AIResponses(PromptId);
    CREATE INDEX IX_AIResponses_UserID ON dbo.AIResponses(UserID);
END
GO

IF OBJECT_ID('dbo.usp_AIResponse_Insert', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_AIResponse_Insert;
GO
CREATE PROCEDURE dbo.usp_AIResponse_Insert
    @PromptId int,
    @UserID nvarchar(450) = '',
    @NomeModelo nvarchar(200),
    @ModeloResolvido nvarchar(200) = NULL,
    @TextoResposta nvarchar(max),
    @TempoProcessamento float = NULL,
    @TokensPorSegundo float = NULL,
    @TempoPuroMs float = NULL,
    @TempoCargaMs float = NULL,
    @TamanhoTokens int = NULL,
    @IdiomaSessao nvarchar(50) = NULL,
    @AIResponseId int = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.AIResponses (
        PromptId, UserID, NomeModelo, ModeloResolvido, TextoResposta,
        TempoProcessamento, TokensPorSegundo, TempoPuroMs, TempoCargaMs,
        TamanhoTokens, IdiomaSessao, DataCriacao
    )
    VALUES (
        @PromptId, ISNULL(NULLIF(@UserID, ''), ''), @NomeModelo, @ModeloResolvido, @TextoResposta,
        @TempoProcessamento, @TokensPorSegundo, @TempoPuroMs, @TempoCargaMs,
        @TamanhoTokens, @IdiomaSessao, SYSUTCDATETIME()
    );

    SET @AIResponseId = SCOPE_IDENTITY();
END
GO

-- -----------------------------------------------------------------------------
-- Conversations : per-user isolation (backward compatible, old rows stay global)
-- -----------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Conversations') AND name = 'UserID'
)
    ALTER TABLE dbo.Conversations ADD UserID nvarchar(450) NULL;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Conversations_UserID' AND object_id = OBJECT_ID('dbo.Conversations')
)
    CREATE INDEX IX_Conversations_UserID ON dbo.Conversations(UserID);
GO

IF OBJECT_ID('dbo.usp_Conversation_GetAll', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Conversation_GetAll;
GO
CREATE PROCEDURE dbo.usp_Conversation_GetAll
    @UserID nvarchar(450) = NULL,
    @IsAdmin bit = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Title, Json, CreatedAt, UpdatedAt, UserID
    FROM dbo.Conversations
    WHERE @IsAdmin = 1
       OR (@UserID IS NOT NULL AND @UserID <> '' AND UserID = @UserID)
       OR UserID IS NULL
    ORDER BY COALESCE(UpdatedAt, CreatedAt) DESC;
END
GO

IF OBJECT_ID('dbo.usp_Conversation_GetById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Conversation_GetById;
GO
CREATE PROCEDURE dbo.usp_Conversation_GetById
    @Id nvarchar(100),
    @UserID nvarchar(450) = NULL,
    @IsAdmin bit = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Title, Json, CreatedAt, UpdatedAt, UserID
    FROM dbo.Conversations
    WHERE Id = @Id
      AND (@IsAdmin = 1
           OR UserID IS NULL
           OR (@UserID IS NOT NULL AND @UserID <> '' AND UserID = @UserID));
END
GO

IF OBJECT_ID('dbo.usp_Conversation_Insert', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Conversation_Insert;
GO
CREATE PROCEDURE dbo.usp_Conversation_Insert
    @Id nvarchar(100),
    @Title nvarchar(250),
    @Json nvarchar(max),
    @CreatedAt datetime2,
    @UpdatedAt datetime2 = NULL,
    @UserID nvarchar(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Conversations (Id, Title, Json, CreatedAt, UpdatedAt, UserID)
    VALUES (@Id, @Title, @Json, @CreatedAt, @UpdatedAt,
            CASE WHEN @UserID = '' THEN NULL ELSE @UserID END);
END
GO

IF OBJECT_ID('dbo.usp_Conversation_Update', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Conversation_Update;
GO
CREATE PROCEDURE dbo.usp_Conversation_Update
    @Id nvarchar(100),
    @Title nvarchar(250),
    @Json nvarchar(max),
    @UpdatedAt datetime2,
    @UserID nvarchar(450) = NULL,
    @IsAdmin bit = 0
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Conversations
    SET Title = @Title,
        Json = @Json,
        UpdatedAt = @UpdatedAt,
        UserID = CASE WHEN @UserID = '' THEN UserID ELSE COALESCE(@UserID, UserID) END
    WHERE Id = @Id
      AND (@IsAdmin = 1
           OR UserID IS NULL
           OR (@UserID IS NOT NULL AND @UserID <> '' AND UserID = @UserID));
END
GO

IF OBJECT_ID('dbo.usp_Conversation_Delete', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Conversation_Delete;
GO
CREATE PROCEDURE dbo.usp_Conversation_Delete
    @Id nvarchar(100),
    @UserID nvarchar(450) = NULL,
    @IsAdmin bit = 0
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Conversations
    WHERE Id = @Id
      AND (@IsAdmin = 1
           OR UserID IS NULL
           OR (@UserID IS NOT NULL AND @UserID <> '' AND UserID = @UserID));
END
GO

PRINT 'AIChat schema applied successfully.';
GO