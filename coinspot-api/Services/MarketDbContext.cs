using Microsoft.EntityFrameworkCore;

public class MarketDbContext : DbContext
{
    public DbSet<CoinInfoSnapshot> CoinSnapshots { get; set; }

    public MarketDbContext(DbContextOptions<MarketDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CoinInfoSnapshot>().HasKey(s => s.Id);
        modelBuilder.Entity<CoinInfoSnapshot>().HasIndex(s => new { s.Code, s.Timestamp });
    }
}

public class CoinInfoSnapshot
{
    public int Id { get; set; }

    public string Code { get; set; } = "";
    public string Name { get; set; } = "";

    public decimal Price { get; set; }
    public decimal PriceAUD { get; set; }
    public decimal MarketCap { get; set; }
    public decimal Volume { get; set; }
    public decimal Change { get; set; }

    public string? Icon { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
