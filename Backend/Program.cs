using System.Text.Json;
using Backend;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Open", policy => 
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddControllers();

// Add Postgres
builder.Services.AddDbContext<GameDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Add Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "GameOfLife_";
});

var app = builder.Build();
app.UseCors("Open");

// Start Game
var game = new GameOfLife(32,64);

app.MapGet("api/game/state", async (IDistributedCache cache) =>
{
    var cachedJson = await cache.GetStringAsync("current_grid");
    if (!string.IsNullOrEmpty(cachedJson))
    {
        return Results.Content(cachedJson, "application/json");
    }

    var currentGrid = game.CurrentGeneration.Data;
    await cache.SetStringAsync("current_grid", JsonSerializer.Serialize(currentGrid));
    return Results.Json(currentGrid); 
});

app.MapPost("api/game/state", async (IDistributedCache cache, GamePattern newPattern) =>
{
    var data = JsonSerializer.Deserialize<bool[][]>(newPattern.Data);
    if(data != null)
    {
        game.CurrentGeneration.Data = data;
        await cache.SetStringAsync("current_grid", JsonSerializer.Serialize(game.CurrentGeneration.Data));
    }
    
});

app.MapGet("api/game/new", async (IDistributedCache cache) =>
{
    game = new GameOfLife(32,64);
    var currentGrid = game.CurrentGeneration.Data;

    await cache.SetStringAsync("current_grid", JsonSerializer.Serialize(currentGrid));
    
    return Results.Json(currentGrid);
});

app.MapGet("api/game/next", async (IDistributedCache cache) =>
{
    game.NextGeneration();
    var currentGrid = game.CurrentGeneration.Data;

    await cache.SetStringAsync("current_grid", JsonSerializer.Serialize(currentGrid));
    
    return Results.Json(currentGrid);
});

app.MapPost("/api/patterns", async (GamePattern pattern, GameDbContext db) =>
{
    pattern.CreatedAt = DateTime.UtcNow;
    db.Patterns.Add(pattern);
    await db.SaveChangesAsync();
    return Results.Ok(new {message = $"Pattern {pattern.Name} add to database"}); 
});

app.MapGet("/api/patterns", async (GameDbContext db) => await db.Patterns.ToListAsync());

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var db = services.GetRequiredService<GameDbContext>();
        Console.WriteLine("Start migration");
        await db.Database.MigrateAsync();
        Console.WriteLine("Database ready");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

await app.RunAsync();