namespace CryptoIntelligence.Api.Application.Contracts.Requests;

public sealed class CandleQueryRequest
{
    public string Symbol { get; set; } = "BTCUSDT";
    public string Interval { get; set; } = "5m";
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public int Limit { get; set; } = 500;
    public int Offset { get; set; }
}
