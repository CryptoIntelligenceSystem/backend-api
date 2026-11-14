namespace CryptoIntelligence.Api.Application.Contracts.Requests;

public sealed class NewsImpactQueryRequest
{
    public string? Symbol { get; set; }
    public int? HorizonMinutes { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; }
}
