-- Stored procedures for Conversations table

IF OBJECT_ID('dbo.usp_Conversation_GetAll', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Conversation_GetAll;
GO
CREATE PROCEDURE dbo.usp_Conversation_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Title, Json, CreatedAt, UpdatedAt
    FROM Conversations
    ORDER BY COALESCE(UpdatedAt, CreatedAt) DESC;
END
GO

IF OBJECT_ID('dbo.usp_Conversation_GetById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Conversation_GetById;
GO
CREATE PROCEDURE dbo.usp_Conversation_GetById
    @Id nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Title, Json, CreatedAt, UpdatedAt
    FROM Conversations
    WHERE Id = @Id;
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
    @UpdatedAt datetime2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Conversations (Id, Title, Json, CreatedAt, UpdatedAt)
    VALUES (@Id, @Title, @Json, @CreatedAt, @UpdatedAt);
END
GO

IF OBJECT_ID('dbo.usp_Conversation_Update', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Conversation_Update;
GO
CREATE PROCEDURE dbo.usp_Conversation_Update
    @Id nvarchar(100),
    @Title nvarchar(250),
    @Json nvarchar(max),
    @UpdatedAt datetime2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Conversations
    SET Title = @Title,
        Json = @Json,
        UpdatedAt = @UpdatedAt
    WHERE Id = @Id;
END
GO

IF OBJECT_ID('dbo.usp_Conversation_Delete', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Conversation_Delete;
GO
CREATE PROCEDURE dbo.usp_Conversation_Delete
    @Id nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Conversations WHERE Id = @Id;
END
GO
