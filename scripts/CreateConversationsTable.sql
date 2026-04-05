CREATE TABLE Conversations (
    Id nvarchar(100) NOT NULL PRIMARY KEY,
    Title nvarchar(250) NOT NULL,
    Json nvarchar(max) NOT NULL,
    CreatedAt datetime2 NOT NULL,
    UpdatedAt datetime2 NULL
);
