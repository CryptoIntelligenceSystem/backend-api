namespace CryptoIntelligence.Api.Application.Contracts.Responses;

public sealed class IndicatorResponse
{
    public string Symbol { get; set; } = string.Empty;
    public string Interval { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OpenTime { get; set; } = string.Empty;
    public double? Value { get; set; }
    public string ProcessedAt { get; set; } = string.Empty;
}
