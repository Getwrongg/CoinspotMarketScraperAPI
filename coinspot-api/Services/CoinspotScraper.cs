using HtmlAgilityPack;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

public class CoinspotScraper
{
    private readonly HttpClient _httpClient;
    private List<CoinInfo> _cachedCoinList = new();

    public CoinspotScraper()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
    }

    public async Task<List<CoinInfo>> FetchCoinListAsync()
    {
        var url = "https://www.coinspot.com.au/tradecoins";
        var html = await _httpClient.GetStringAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var rows = doc.DocumentNode.SelectNodes("//tr[contains(@class, 'tradeitem') and contains(@class, 'coinrow')]");
        var coins = new List<CoinInfo>();

        foreach (var row in rows)
        {
            var tds = row.SelectNodes("td");
            if (tds == null || tds.Count < 6) continue;

            var icon = tds[0].SelectSingleNode(".//img")?.GetAttributeValue("src", null)?.Trim();
            var name = tds[1].SelectSingleNode(".//div")?.InnerText.Trim();
            var code = tds[1].SelectSingleNode(".//div[contains(@class, 'grey-500')]")?.InnerText.Trim();

            var priceWithChange = tds[2].InnerText.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var price = ParseCurrency(priceWithChange.ElementAtOrDefault(0));
            var change = ParsePercentage(priceWithChange.ElementAtOrDefault(1));

            var priceAUD = ParseCurrency(tds[3].InnerText.Trim());
            var marketCap = ParseAbbreviatedNumber(tds[4].InnerText.Trim());
            var volume = ParseAbbreviatedNumber(tds[5].InnerText.Trim());

            if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name))
            {
                coins.Add(new CoinInfo
                {
                    Code = code,
                    Name = name,
                    Price = price,
                    PriceAUD = priceAUD,
                    MarketCap = marketCap,
                    Volume = volume,
                    Change = change,
                    Icon = icon != null ? $"https://www.coinspot.com.au{icon.Replace("./", "/")}" : null
                });
            }
        }

        _cachedCoinList = coins;
        return coins;
    }

    public List<CoinInfo> GetCachedCoinList() => _cachedCoinList;

    private decimal? ParseCurrency(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        input = Regex.Replace(input, @"[^0-9.]", "");
        return decimal.TryParse(input, out var val) ? val : null;
    }

    private decimal? ParsePercentage(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        input = input.Replace("%", "").Trim();
        return decimal.TryParse(input, out var val) ? val : null;
    }

    private decimal? ParseAbbreviatedNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        input = input.Trim();

        decimal multiplier = 1;
        if (input.EndsWith("B")) multiplier = 1_000_000_000;
        else if (input.EndsWith("M")) multiplier = 1_000_000;
        else if (input.EndsWith("K")) multiplier = 1_000;

        input = Regex.Replace(input, @"[^0-9.]", "");
        return decimal.TryParse(input, out var num) ? num * multiplier : null;
    }
}

public class CoinInfo
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal? Price { get; set; }
    public decimal? PriceAUD { get; set; }
    public decimal? MarketCap { get; set; }
    public decimal? Volume { get; set; }
    public decimal? Change { get; set; }
    public string? Icon { get; set; }
}
