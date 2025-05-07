using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CoinspotController : ControllerBase
{
    private readonly CoinspotScraper _scraper;

    public CoinspotController(CoinspotScraper scraper)
    {
        _scraper = scraper;
    }

    [HttpGet("coins")]
    public async Task<IActionResult> GetCoins()
    {
        try
        {
            var coins = await _scraper.FetchCoinListAsync();
            return Ok(coins);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
