using System.ComponentModel.DataAnnotations;

namespace GomokuGame.Models;

/// <summary>
/// 聊天消息
/// </summary>
public class ChatMessage
{
    [Key]
    public int MessageId { get; set; }
    public int GameId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}