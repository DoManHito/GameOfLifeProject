using Microsoft.EntityFrameworkCore;

namespace Backend;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) {}
    public DbSet<GamePattern> Patterns { get; set; }
}

public class GamePattern
{
    public int Id { get; set; }
    public string Name { get; set; } = "New pattern";
    public string Data { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
