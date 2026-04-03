using GomokuGame.Models;
using Microsoft.EntityFrameworkCore;

namespace GomokuGame.Data;

public class GomokuDbContext : DbContext
{
    public GomokuDbContext(DbContextOptions<GomokuDbContext> options)
        : base(options)
    {
    }

    // 对应数据库中的四张表
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GamePlayer> GamePlayers => Set<GamePlayer>();
    public DbSet<Move> Moves => Set<Move>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
}