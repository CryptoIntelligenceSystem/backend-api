namespace CryptoIntelligence.Api.Application.Contracts.Responses;

public sealed class PipelineStatusResponse
{
    public long MarketDataCount { get; set; }
    public long Candle5mCount { get; set; }
    public long IndicatorCount { get; set; }
    public long SignalCount { get; set; }
    public string? LatestMarketTime { get; set; }
    public string? LatestCandle5mTime { get; set; }
    public string? LatestIndicatorTime { get; set; }
    public string? LatestSignalTime { get; set; }
}
