using Microsoft.EntityFrameworkCore;

public class CoinStrengthAnalyzer
{
    private readonly MarketDbContext _db;

    public CoinStrengthAnalyzer(MarketDbContext db)
    {
        _db = db;
    }

    public Task<List<Candle>> GetRawHistoryAsync(string coin) => FetchCoinHistoryAsync(coin);
    public double GetRSI(List<Candle> data, int period = 14) => CalculateRSI(data, period);
    public double GetMomentum(List<Candle> data) => CalculateMomentum(data);
    public double GetVolatility(List<Candle> data) => CalculateVolatility(data);
    public double GetAverageSpread(List<Candle> data) => CalculateAverageSpread(data);
    public double GetEMA(List<Candle> data, int period) => CalculateEMA(data, period);
    public (double macd, double signal) GetMACD(List<Candle> data) => CalculateMACD(data);

    public async Task<CoinAnalysisResult> EvaluateAsync(string coin)
    {
        var data = await FetchCoinHistoryAsync(coin);

        var momentum = CalculateMomentum(data);
        var volatility = CalculateVolatility(data);
        var spread = CalculateAverageSpread(data);
        var rsi = CalculateRSI(data);
        var (macd, signal) = CalculateMACD(data);
        var strength = CalculateStrengthScore(momentum, volatility, spread);
        var strategy = DeriveAltcoinStrategy(strength, momentum, rsi, macd, signal);

        return new CoinAnalysisResult
        {
            Coin = coin,
            Momentum = Math.Round(momentum, 2),
            Volatility = Math.Round(volatility, 2),
            Spread = Math.Round(spread, 2),
            RSI = Math.Round(rsi, 2),
            MACD = Math.Round(macd, 2),
            Signal = Math.Round(signal, 2),
            Strength = Math.Round(strength, 2),
            Strategy = strategy
        };
    }

    private async Task<List<Candle>> FetchCoinHistoryAsync(string coin)
    {
        return await _db.CoinSnapshots
            .Where(x => x.Code == coin)
            .OrderByDescending(x => x.Timestamp)
            .Take(50)
            .Select(x => new Candle
            {
                Last = (double)x.Price,
                Bid = (double)(x.Price * 0.995m),
                Ask = (double)(x.Price * 1.005m)
            })
            .ToListAsync();
    }

    private double CalculateMomentum(List<Candle> data)
    {
        if (data.Count < 2) return 0;
        var oldest = data[^1].Last;
        var latest = data[0].Last;
        return ((latest - oldest) / oldest) * 100;
    }

    private double CalculateVolatility(List<Candle> data)
    {
        var mean = data.Average(p => p.Last);
        var variance = data.Average(p => Math.Pow(p.Last - mean, 2));
        return Math.Sqrt(variance);
    }

    private double CalculateAverageSpread(List<Candle> data)
    {
        var spreads = data.Select(p => ((p.Ask - p.Bid) / p.Bid) * 100).ToList();
        return spreads.Average();
    }

    private double CalculateEMA(List<Candle> data, int period)
    {
        if (data.Count == 0) return 0;
        var k = 2.0 / (period + 1);
        var ema = data[0].Last;

        for (int i = 1; i < data.Count; i++)
        {
            ema = data[i].Last * k + ema * (1 - k);
        }

        return ema;
    }

    private (double macd, double signal) CalculateMACD(List<Candle> data)
    {
        var slice = data.Take(35).ToList();
        var ema12 = CalculateEMA(slice.Take(26).ToList(), 12);
        var ema26 = CalculateEMA(slice.Take(26).ToList(), 26);
        var macd = ema12 - ema26;

        var signalInput = slice.Select(_ => new Candle { Last = macd }).ToList();
        var signal = CalculateEMA(signalInput, 9);

        return (macd, signal);
    }

    private double CalculateRSI(List<Candle> data, int period = 14)
    {
        double gains = 0, losses = 0;
        for (int i = 1; i <= period && i < data.Count; i++)
        {
            var diff = data[i - 1].Last - data[i].Last;
            if (diff > 0) losses += diff;
            else gains -= diff;
        }

        var rs = gains / (losses != 0 ? losses : 1);
        return 100 - 100 / (1 + rs);
    }

    private double CalculateStrengthScore(double momentum, double volatility, double spread)
    {
        var m = Math.Clamp(momentum * 5, 0, 100);
        var v = Math.Clamp(100 - volatility, 0, 100);
        var s = Math.Clamp(100 - spread * 10, 0, 100);
        return (m * 0.5) + (v * 0.3) + (s * 0.2);
    }

    private List<string> DeriveAltcoinStrategy(double strength, double momentum, double rsi, double macd, double signal)
    {
        var strategy = new List<string>();

        if (strength < 30) strategy.Add("Freeze buys, exit alts if weak");
        if (strength > 70) strategy.Add("Aggressive alt entry (momentum play)");
        if (momentum > 0.5) strategy.Add("Buy high-beta altcoins (e.g., SOL, AVAX)");
        if (rsi < 30) strategy.Add("Avoid all new entries (RSI < 30)");
        if (macd > signal) strategy.Add("Enable alt trading window (MACD crossover)");

        if (strategy.Count == 0)
            strategy.Add("No strong BTC signal — rely on altcoin-specific momentum");

        return strategy;
    }
}

public class Candle
{
    public double Last { get; set; }
    public double Bid { get; set; }
    public double Ask { get; set; }
}

public class CoinAnalysisResult
{
    public string Coin { get; set; } = "";
    public double Momentum { get; set; }
    public double Volatility { get; set; }
    public double Spread { get; set; }
    public double RSI { get; set; }
    public double MACD { get; set; }
    public double Signal { get; set; }
    public double Strength { get; set; }
    public List<string>? Strategy { get; set; }
}
