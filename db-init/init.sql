-- init.sql
IF DB_ID('CommentsDb') IS NULL
BEGIN
    CREATE DATABASE CommentsDb;
END
GO
USE CommentsDb;
GO

CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    HomePage NVARCHAR(255) NULL,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
);

CREATE TABLE Comments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    CommentText NVARCHAR(MAX) NOT NULL,
    FilePath NVARCHAR(255) NULL,
    ImagePath NVARCHAR(255) NULL,
    ParentId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Comments_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);

INSERT INTO Users (UserName, Email) VALUES
('Alice','alice@example.com'),
('Bob','bob@example.com'),
('Charlie','charlie@example.com'),
('Dave','dave@example.com');

INSERT INTO Comments (UserId, CommentText)
SELECT Id, 'First comment from ' + UserName
FROM Users;

INSERT INTO Comments (UserId, CommentText)
SELECT Id, 'Second comment from ' + UserName
FROM Users;

INSERT INTO Comments (UserId, CommentText)
SELECT Id, 'Third comment from ' + UserName
FROM Users;


-- Каскадные ответы
-- Выбираем первый комментарий каждого пользователя
DECLARE @FirstComments TABLE (UserId UNIQUEIDENTIFIER, CommentId UNIQUEIDENTIFIER);

DECLARE @parent1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Comments WHERE CommentText LIKE 'First comment%' ORDER BY CreatedAt);
DECLARE @parent2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Comments WHERE CommentText LIKE 'Second comment%' ORDER BY CreatedAt);

INSERT INTO Comments (UserId, CommentText, ParentId)
SELECT Id, 'Reply 1 to first comment from ' + UserName, @parent1 FROM Users;

INSERT INTO Comments (UserId, CommentText, ParentId)
SELECT Id, 'Reply 2 to first comment from ' + UserName, @parent1 FROM Users;

INSERT INTO Comments (UserId, CommentText, ParentId)
SELECT Id, 'Nested reply to reply 1 from ' + UserName, (SELECT TOP 1 Id FROM Comments WHERE CommentText LIKE 'Reply 1%' ORDER BY CreatedAt) FROM Users;