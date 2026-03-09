IF DB_ID('CommentsDb') IS NULL
BEGIN
    CREATE DATABASE CommentsDb;
END
GO

USE CommentsDb;
GO

-- Пользователи
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    HomePage NVARCHAR(255) NULL,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
);

-- Комментарии с файлами прямо в базе
CREATE TABLE Comments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    CommentText NVARCHAR(MAX) NOT NULL,
    FileName NVARCHAR(255) NULL,
    FileData VARBINARY(MAX) NULL,
    FileContentType NVARCHAR(50) NULL,
    ParentId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Comments_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Тестовые пользователи
INSERT INTO Users (UserName, Email) VALUES
('Alice','alice@example.com'),
('Bob','bob@example.com'),
('Charlie','charlie@example.com'),
('Dave','dave@example.com');

-- Первые комментарии
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
DECLARE @parent1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Comments WHERE CommentText LIKE 'First comment%' ORDER BY CreatedAt);

INSERT INTO Comments (UserId, CommentText, ParentId)
SELECT Id, 'Reply 1 to first comment from ' + UserName, @parent1 FROM Users;

INSERT INTO Comments (UserId, CommentText, ParentId)
SELECT Id, 'Reply 2 to first comment from ' + UserName, @parent1 FROM Users;

INSERT INTO Comments (UserId, CommentText, ParentId)
SELECT Id, 'Nested reply to reply 1 from ' + UserName, 
       (SELECT TOP 1 Id FROM Comments WHERE CommentText LIKE 'Reply 1%' ORDER BY CreatedAt)
FROM Users;