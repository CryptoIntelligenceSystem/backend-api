namespace CryptoIntelligence.Api.Application.Contracts.Responses;

public sealed class NewsImpactResponse
{
    public string Source { get; set; } = string.Empty;
    public string ArticleId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string PublishedAt { get; set; } = string.Empty;
    public int HorizonMinutes { get; set; }
    public string BaselineTime { get; set; } = string.Empty;
    public double BaselineClose { get; set; }
    public string TargetTime { get; set; } = string.Empty;
    public double TargetClose { get; set; }
    public double ReturnPct { get; set; }
    public string CalculatedAt { get; set; } = string.Empty;
}
