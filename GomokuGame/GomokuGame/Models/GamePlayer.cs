using System.ComponentModel.DataAnnotations;

namespace GomokuGame.Models;

/// <summary>
/// 房间里的玩家
/// </summary>
public class GamePlayer
{
    [Key]
    public int GamePlayerId { get; set; }
    public int GameId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int PlayerOrder { get; set; } // 1, 2, 或 3
    public string? Color { get; set; } // 棋子颜色
}