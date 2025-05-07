using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ToolsController : ControllerBase
{
    private readonly CoinStrengthAnalyzer _analyzer;

    public ToolsController(CoinStrengthAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }

    [HttpGet("rsi/{coin}")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetRSI(string coin, [FromQuery] int period = 14)
    {
        var candles = await _analyzer.GetRawHistoryAsync(coin);
        var rsi = _analyzer.GetRSI(candles, period);
        return Ok(new { coin, rsi, period });
    }

    [HttpGet("macd/{coin}")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetMACD(string coin)
    {
        var candles = await _analyzer.GetRawHistoryAsync(coin);
        var (macd, signal) = _analyzer.GetMACD(candles);
        return Ok(new { coin, macd, signal });
    }

    [HttpGet("momentum/{coin}")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetMomentum(string coin)
    {
        var candles = await _analyzer.GetRawHistoryAsync(coin);
        var value = _analyzer.GetMomentum(candles);
        return Ok(new { coin, momentum = value });
    }

    [HttpGet("volatility/{coin}")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetVolatility(string coin)
    {
        var candles = await _analyzer.GetRawHistoryAsync(coin);
        var value = _analyzer.GetVolatility(candles);
        return Ok(new { coin, volatility = value });
    }

    [HttpGet("spread/{coin}")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetSpread(string coin)
    {
        var candles = await _analyzer.GetRawHistoryAsync(coin);
        var value = _analyzer.GetAverageSpread(candles);
        return Ok(new { coin, spread = value });
    }

    [HttpGet("ema/{coin}")]
    public async Task<IActionResult> GetEMA(string coin, [FromQuery] int period)
    {
        var candles = await _analyzer.GetRawHistoryAsync(coin);
        var ema = _analyzer.GetEMA(candles, period);
        return Ok(new { coin, period, ema });
    }


    [HttpGet("evaluate/{coin}")]
    [ProducesResponseType(typeof(CoinAnalysisResult), 200)]
    public async Task<IActionResult> GetEvaluation(string coin)
    {
        var result = await _analyzer.EvaluateAsync(coin);
        return Ok(result);
    }
}

public class EmaRequest
{
    public int Period { get; set; }
}
