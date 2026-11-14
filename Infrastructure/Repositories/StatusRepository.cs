using CryptoIntelligence.Api.Application.Contracts.Responses;
using CryptoIntelligence.Api.Application.Interfaces;
using CryptoIntelligence.Api.Infrastructure.ClickHouse;

namespace CryptoIntelligence.Api.Infrastructure.Repositories;

public sealed class StatusRepository(IClickHouseClient clickHouse) : IStatusRepository
{
    public async Task<PipelineStatusResponse> GetAsync(CancellationToken cancellationToken)
    {
        var rows = await clickHouse.QueryAsync<PipelineStatusResponse>("""
            SELECT
              (SELECT count() FROM pipeline.market_data FINAL) AS marketDataCount,
              (SELECT count() FROM pipeline.news_data FINAL) AS newsDataCount,
              (SELECT count() FROM pipeline.candles_5m FINAL) AS candle5mCount,
              (SELECT count() FROM pipeline.indicators FINAL) AS indicatorCount,
              (SELECT count() FROM pipeline.signals FINAL) AS signalCount,
              (SELECT toString(max(open_time)) FROM pipeline.market_data FINAL) AS latestMarketTime,
              (SELECT toString(max(published_at)) FROM pipeline.news_data FINAL) AS latestNewsTime,
              (SELECT toString(max(open_time)) FROM pipeline.candles_5m FINAL) AS latestCandle5mTime,
              (SELECT toString(max(open_time)) FROM pipeline.indicators FINAL) AS latestIndicatorTime,
              (SELECT toString(max(open_time)) FROM pipeline.signals FINAL) AS latestSignalTime
            """, cancellationToken);

        return rows.FirstOrDefault() ?? new PipelineStatusResponse();
    }
}
