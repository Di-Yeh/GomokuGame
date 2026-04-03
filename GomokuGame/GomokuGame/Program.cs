using GomokuGame.Data;
using GomokuGame.Hubs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

/********************************* --- 注册服务 --- ******************************** */
var builder = WebApplication.CreateBuilder(args);

// 注册数据库上下文
builder.Services.AddDbContext<GomokuDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("conn")));

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:9000", "http://localhost:9001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

/********************************* --- 启动时自动清空数据 --- ******************************** */
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GomokuDbContext>();

    // 1. 先删除所有依赖于 Games 的子表数据
    db.ChatMessages.RemoveRange(db.ChatMessages);
    db.Moves.RemoveRange(db.Moves);
    db.GamePlayers.RemoveRange(db.GamePlayers);

    // 先保存一次，把子表清空
    db.SaveChanges();

    // 2. 现在子表空了，可以安全删除主表 Games 了
    db.Games.RemoveRange(db.Games);

    // 再次保存
    db.SaveChanges();

    Console.WriteLine(">>> 数据库已重置，外键冲突已化解，环境清洁！ <<<");
}


/********************************* --- 配置中间件 --- ******************************** */
app.UseCors("CorsPolicy");

// 映射 SignalR Hub
app.MapHub<GomokuHub>("/gomokuHub");


/********************************* --- API --- ******************************** */
// 创建房间
app.MapPost("/api/games/create", async (GomokuDbContext db) =>
{
    var newGame = new GomokuGame.Models.Game
    {
        RoomCode = Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
        MaxPlayers = 3,
        CreatedAt = DateTime.Now
    };
    db.Games.Add(newGame);
    await db.SaveChangesAsync();
    return Results.Ok(new { newGame.GameId, newGame.RoomCode });
});

// 测试
// app.MapGet("/", () => "Gomoku Game Server is running!");


/********************************* --- 五子棋 启动! XD --- ******************************** */
app.Run();