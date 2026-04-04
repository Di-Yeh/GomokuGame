using GomokuGame.Data;
using GomokuGame.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GomokuGame.Hubs;

public class GomokuHub : Hub
{
    private readonly GomokuDbContext _db;

    public GomokuHub(GomokuDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 加入房间（按房号分组，这样消息不会发错给别人）
    /// </summary>
    /// <param name="roomCode"></param>
    /// <param name="playerName"></param>
    /// <returns></returns>
    public async Task JoinRoom(string roomCode, string playerName)
    {
        // 查出房间
        var game = await _db.Games
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.RoomCode == roomCode);

        if (game == null)
        {
            await Clients.Caller.SendAsync("ReceiveError", "房间不存在");
            return;
        }

        // --- 【关键修改】 先把当前这个连接拉进组里 ---
        // 这样不管是新来的，还是刷新的，都能保证在"玩家名单"里
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        // 检查是否已经是房间成员（重连逻辑）
        var existingPlayer = game.Players.FirstOrDefault(p => p.PlayerName == playerName);
        if (existingPlayer != null)
        {
            await Clients.Caller.SendAsync("JoinSuccess", new { order = existingPlayer.PlayerOrder, color = existingPlayer.Color });
            return;
        }

        int order = game.Players.Count + 1;

        if (order > game.MaxPlayers)
        {
            await Clients.Caller.SendAsync("ReceiveError", "房间已满");
            return;
        }

        string[] colors = { "Black", "White", "Red" };
        var player = new GamePlayer
        {
            GameId = game.GameId,
            PlayerName = playerName,
            PlayerOrder = order,
            Color = colors[order - 1]
        };

        // 保存到数据库
        _db.GamePlayers.Add(player);
        await _db.SaveChangesAsync();

        // 发送加入消息
        await Clients.Group(roomCode).SendAsync("PlayerJoined", new
        {
            playerName,
            order,
            color = player.Color
        });

        // 发送确认消息
        await Clients.Caller.SendAsync("JoinSuccess", new { order = order, color = player.Color });
    }

    /// <summary>
    /// 落子同步
    /// </summary>
    /// <param name="roomCode"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="playerOrder"></param>
    /// <returns></returns>
    public async Task PlaceStone(string roomCode, int x, int y, int playerOrder)
    {
        var game = await _db.Games.FirstOrDefaultAsync(g => g.RoomCode == roomCode);
        if (game == null || game.IsFinished) return;

        // 保存到数据库
        var move = new Move
        {
            GameId = game.GameId,
            PosX = x,
            PosY = y,
            PlayerOrder = playerOrder,
            Timestamp = DateTime.Now
        };
        _db.Moves.Add(move);

        // 切换回合 (1->2, 2->3, 3->1)
        game.CurrentTurnOrder = (game.CurrentTurnOrder % 3) + 1;

        await _db.SaveChangesAsync();

        // 广播给房间所有人落子位置和下一个该谁
        await Clients.Group(roomCode).SendAsync("StonePlaced", x, y, playerOrder, game.CurrentTurnOrder);
    }

    /// <summary>
    /// 聊天同步
    /// </summary>
    /// <param name="roomCode"></param>
    /// <param name="user"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendMessage(string roomCode, string user, string message)
    {
        var game = await _db.Games.FirstOrDefaultAsync(g => g.RoomCode == roomCode);
        if (game == null) return;

        var chat = new ChatMessage
        {
            GameId = game.GameId,
            SenderName = user, // 对应你的实体字段
            Content = message,
            Timestamp = DateTime.Now
        };
        _db.ChatMessages.Add(chat);
        await _db.SaveChangesAsync();

        // 广播给所有人
        await Clients.Group(roomCode).SendAsync("ReceiveMessage", user, message);
    }

    /// <summary>
    /// 转发下棋消息
    /// </summary>
    /// <param name="roomCode"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public async Task SendMove(string roomCode, int row, int col, int player)
    {
        // 告诉房间里的所有人（包括发送者自己）
        await Clients.Group(roomCode).SendAsync("ReceiveMove", row, col, player);
    }

    /// <summary>
    /// 获取玩家数据（解决刷新页面无法显示资料问题）
    /// </summary>
    /// <param name="roomCode"></param>
    /// <returns></returns>
    public async Task GetRoomPlayers(string roomCode)
    {
        var game = await _db.Games
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.RoomCode == roomCode);

        if (game != null)
        {
            // 只发给请求的那个人
            await Clients.Caller.SendAsync("UpdatePlayerList", game.Players.Select(p => new
            {
                playerName = p.PlayerName,
                order = p.PlayerOrder,
                color = p.Color
            }));
        }
    }

    /// <summary>
    /// 获取棋盘状态（解决刷新页面无法显示资料问题）
    /// </summary>
    /// <param name="roomCode"></param>
    /// <returns></returns>
    public async Task GetBoardState(string roomCode)
    {
        // 先根据房间号查出 Game 实体
        var game = await _db.Games
            .FirstOrDefaultAsync(g => g.RoomCode == roomCode);

        if (game == null) return;

        // 查出该游戏下所有的落子，并按步数排序
        var moves = await _db.Moves
            .Where(m => m.GameId == game.GameId)
            .OrderBy(m => m.StepNumber)
            .Select(m => new
            {
                row = m.PosX,      // 注意：前端 handleCanvasClick 传的是 (row, col)
                col = m.PosY,
                playerOrder = m.PlayerOrder
            })
            .ToListAsync();

        // 将棋盘数据和“当前该谁下”一起发回给刷新页面的玩家
        await Clients.Caller.SendAsync("UpdateBoardState", new
        {
            moves = moves,
            currentTurn = game.CurrentTurnOrder
        });
    }

    /// <summary>
    /// 获取聊天室讯息（解决刷新页面无法显示资料问题）
    /// </summary>
    /// <param name="roomCode"></param>
    /// <returns></returns>
    public async Task GetChatHistory(string roomCode)
    {
        // 找到对应的游戏
        var game = await _db.Games.FirstOrDefaultAsync(g => g.RoomCode == roomCode);
        if (game == null) return;

        // 查出记录，使用 SenderName
        var chatHistory = await _db.ChatMessages
            .Where(m => m.GameId == game.GameId)
            .OrderBy(m => m.Timestamp)
            .Select(m => new
            {
                // 这里一定要对应前端 ChatMessage 接口里的属性名
                user = m.SenderName,
                text = m.Content,
                time = m.Timestamp.ToString("HH:mm")
            })
            .ToListAsync();

        // 3. 发送
        await Clients.Caller.SendAsync("ReceiveChatHistory", chatHistory);
    }
}