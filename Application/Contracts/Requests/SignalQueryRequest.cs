namespace CryptoIntelligence.Api.Application.Contracts.Requests;

public sealed class SignalQueryRequest
{
    public string? Symbol { get; set; }
    public string? SignalType { get; set; } = "volume_anomaly";
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public int Limit { get; set; } = 500;
    public int Offset { get; set; }
}
