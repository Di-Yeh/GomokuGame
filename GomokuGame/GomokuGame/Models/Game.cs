using System.ComponentModel.DataAnnotations;

namespace GomokuGame.Models;

/// <summary>
/// 游戏房间
/// </summary>
public class Game
{
    [Key]
    public int GameId { get; set; }
    public string RoomCode { get; set; } = string.Empty; // 6位房号
    public int MaxPlayers { get; set; } = 3;
    public int CurrentTurnOrder { get; set; } = 1; // 当前该第几个玩家(1,2,3)
    public bool IsFinished { get; set; } = false;
    public int? WinnerOrder { get; set; } // 赢家序号
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // 导航属性：一个房间有多个玩家和多条消息
    public List<GamePlayer> Players { get; set; } = new();
}
