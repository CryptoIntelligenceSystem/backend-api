namespace CryptoIntelligence.Api.Application.Contracts.Responses;

public sealed class MarketDataResponse
{
    public string DataType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Interval { get; set; } = string.Empty;
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    public double Volume { get; set; }
    public double QuoteVolume { get; set; }
    public ulong TradeCount { get; set; }
    public string CollectedAt { get; set; } = string.Empty;
    public string SchemaVersion { get; set; } = string.Empty;
}
