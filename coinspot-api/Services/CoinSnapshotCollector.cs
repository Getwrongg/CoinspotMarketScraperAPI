using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class CoinSnapshotCollector : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly CoinspotScraper _scraper;

    public CoinSnapshotCollector(IServiceProvider services, CoinspotScraper scraper)
    {
        _services = services;
        _scraper = scraper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var coins = await _scraper.FetchCoinListAsync();

                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MarketDbContext>();

                var snapshots = coins.Select(c => new CoinInfoSnapshot
                {
                    Code = c.Code,
                    Name = c.Name,
                    Price = c.Price ?? 0,
                    PriceAUD = c.PriceAUD ?? 0,
                    MarketCap = c.MarketCap ?? 0,
                    Volume = c.Volume ?? 0,
                    Change = c.Change ?? 0,
                    Icon = c.Icon,
                    Timestamp = DateTime.UtcNow
                });

                db.CoinSnapshots.AddRange(snapshots);
                await db.SaveChangesAsync(stoppingToken);

                Console.WriteLine($"[{DateTime.UtcNow:O}] Saved {snapshots.Count()} snapshots.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CoinSnapshotCollector: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
