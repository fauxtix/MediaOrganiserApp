/* ============================================================
   CreateChatFeedback.sql
   Avaliação gosto/não gosto (👍/👎, como no ChatGPT) das respostas
   do assistente nos dois chats (OpenRouter cloud + Ollama local).

   Liga-se às conversas persistidas (Conversations.Json) por
   (UserID, Interface, ConversationId, MessageIndex): o índice é a
   posição da mensagem do assistente no JSON da conversa.

   Aplicar: sqlcmd -S "(local)" -d Publicacoes_Prod -i CreateChatFeedback.sql
   ============================================================ */
IF OBJECT_ID(N'dbo.ChatFeedback', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChatFeedback
    (
        Id             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ChatFeedback PRIMARY KEY,
        UserID         NVARCHAR(450)     NOT NULL,
        Interface      NVARCHAR(20)      NOT NULL,   -- 'openrouter' | 'ollama'
        ConversationId NVARCHAR(64)      NULL,
        MessageIndex   INT               NOT NULL,   -- índice da msg do assistente na conversa
        PromptText     NVARCHAR(MAX)     NULL,       -- último user prompt (contexto para juiz)
        ResponseText   NVARCHAR(MAX)     NOT NULL,   -- snapshot da resposta avaliada
        ModelName      NVARCHAR(200)     NULL,       -- modelo efetivo (cloud) / NULL (local)
        Rating         INT               NOT NULL,   -- 1 gosto/correta | -1 não gosto/incorreta | 0 remove
        CreatedAt      DATETIME2(2)      NOT NULL CONSTRAINT DF_ChatFeedback_CreatedAt DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_ChatFeedback_User         ON dbo.ChatFeedback (UserID, Interface, ConversationId);
    CREATE INDEX IX_ChatFeedback_Conversation ON dbo.ChatFeedback (ConversationId, MessageIndex);
END
GO

IF OBJECT_ID(N'dbo.usp_ChatFeedback_Upsert', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_ChatFeedback_Upsert;
GO
CREATE PROCEDURE dbo.usp_ChatFeedback_Upsert
    @UserID         nvarchar(450),
    @Interface      nvarchar(20),
    @ConversationId nvarchar(64)  = NULL,
    @MessageIndex   int,
    @PromptText     nvarchar(max) = NULL,
    @ResponseText   nvarchar(max),
    @ModelName      nvarchar(200) = NULL,
    @Rating         int
AS
BEGIN
    SET NOCOUNT ON;

    -- Rating 0 = o utilizador desfez a avaliação (toggle): persiste-se a desmarcação
    -- (a linha mantém-se com Rating 0 para a chave). Só se atualiza quando já existia
    -- um voto anterior; respostas nunca votadas NÃO criam linha.
    IF @Rating = 0
    BEGIN
        UPDATE dbo.ChatFeedback
        SET PromptText   = @PromptText,
            ResponseText = @ResponseText,
            ModelName    = @ModelName,
            Rating       = 0,
            CreatedAt    = SYSUTCDATETIME()
        WHERE UserID = @UserID
          AND Interface = @Interface
          AND MessageIndex = @MessageIndex
          AND ((@ConversationId IS NULL AND ConversationId IS NULL) OR ConversationId = @ConversationId);
        RETURN;
    END

    IF EXISTS (
        SELECT 1 FROM dbo.ChatFeedback
        WHERE UserID = @UserID
          AND Interface = @Interface
          AND MessageIndex = @MessageIndex
          AND ((@ConversationId IS NULL AND ConversationId IS NULL) OR ConversationId = @ConversationId)
    )
    BEGIN
        UPDATE dbo.ChatFeedback
        SET PromptText   = @PromptText,
            ResponseText = @ResponseText,
            ModelName    = @ModelName,
            Rating       = @Rating,
            CreatedAt    = SYSUTCDATETIME()
        WHERE UserID = @UserID
          AND Interface = @Interface
          AND MessageIndex = @MessageIndex
          AND ((@ConversationId IS NULL AND ConversationId IS NULL) OR ConversationId = @ConversationId);
    END
    ELSE
    BEGIN
        INSERT INTO dbo.ChatFeedback
            (UserID, Interface, ConversationId, MessageIndex, PromptText, ResponseText, ModelName, Rating)
        VALUES
            (@UserID, @Interface, @ConversationId, @MessageIndex, @PromptText, @ResponseText, @ModelName, @Rating);
    END
END
GO

IF OBJECT_ID(N'dbo.usp_ChatFeedback_GetByConversation', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_ChatFeedback_GetByConversation;
GO
CREATE PROCEDURE dbo.usp_ChatFeedback_GetByConversation
    @UserID         nvarchar(450),
    @Interface      nvarchar(20),
    @ConversationId nvarchar(64) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, UserID, Interface, ConversationId, MessageIndex, PromptText,
           ResponseText, ModelName, Rating, CreatedAt
    FROM dbo.ChatFeedback
    WHERE UserID = @UserID
      AND Interface = @Interface
      AND ((@ConversationId IS NULL AND ConversationId IS NULL) OR ConversationId = @ConversationId)
    ORDER BY MessageIndex;
END
GO