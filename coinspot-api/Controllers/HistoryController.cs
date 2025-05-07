using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly MarketDbContext _db;

    public HistoryController(MarketDbContext db)
    {
        _db = db;
    }

    // GET: /api/history
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var all = await _db.CoinSnapshots
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();

        return Ok(all);
    }

    // GET: /api/history/BTC
    [HttpGet("{coin}")]
    public async Task<IActionResult> GetByCoin(string coin)
    {
        var records = await _db.CoinSnapshots
            .Where(x => x.Code.ToLower() == coin.ToLower())
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();

        return Ok(records);
    }
}
