using System.ComponentModel.DataAnnotations;

namespace GomokuGame.Models;

/// <summary>
/// 落子记录
/// </summary>
public class Move
{
    [Key]
    public int MoveId { get; set; }
    public int GameId { get; set; }
    public int StepNumber { get; set; }
    public int PlayerOrder { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}