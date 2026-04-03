-- 1. 遊戲房間主表
CREATE TABLE Games (
    GameId INT PRIMARY KEY IDENTITY(1,1),
    RoomCode NVARCHAR(10) NOT NULL,
    MaxPlayers INT DEFAULT 3,         -- 設定為 3 人
    CurrentTurnOrder INT DEFAULT 1,   -- 當前該第幾個玩家落子
    IsFinished BIT DEFAULT 0,
    WinnerOrder INT NULL,             -- 贏家的序號
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 2. 玩家與房間的關聯表 (支持多人)
CREATE TABLE GamePlayers (
    GamePlayerId INT PRIMARY KEY IDENTITY(1,1),
    GameId INT NOT NULL,
    PlayerName NVARCHAR(50) NOT NULL,
    PlayerOrder INT NOT NULL,         -- 1, 2, 3
    Color NVARCHAR(20),               -- 比如：Black, White, Red
    CONSTRAINT FK_GamePlayers_Games FOREIGN KEY (GameId) REFERENCES Games(GameId)
);

-- 3. 落子記錄表
CREATE TABLE Moves (
    MoveId INT PRIMARY KEY IDENTITY(1,1),
    GameId INT NOT NULL,
    StepNumber INT NOT NULL,          -- 第幾手
    PlayerOrder INT NOT NULL,         -- 誰下的
    PosX INT NOT NULL,
    PosY INT NOT NULL,
    Timestamp DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Moves_Games FOREIGN KEY (GameId) REFERENCES Games(GameId)
);

CREATE TABLE ChatMessages (
    MessageId INT PRIMARY KEY IDENTITY(1,1),
    GameId INT NOT NULL,              -- 属于哪个房间
    SenderName NVARCHAR(50) NOT NULL, -- 发送者名字
    Content NVARCHAR(MAX) NOT NULL,   -- 聊天内容
    Timestamp DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Chat_Games FOREIGN KEY (GameId) REFERENCES Games(GameId)
);