using CryptoIntelligence.Api.Application.Contracts.Requests;
using CryptoIntelligence.Api.Application.Contracts.Responses;
using CryptoIntelligence.Api.Application.Interfaces;
using CryptoIntelligence.Api.Application.Services;
using CryptoIntelligence.Api.Infrastructure.ClickHouse;

namespace CryptoIntelligence.Api.Infrastructure.Repositories;

public sealed class MarketDataRepository(IClickHouseClient clickHouse) : IMarketDataRepository
{
    public async Task<PagedResult<MarketDataResponse>> QueryAsync(
        MarketDataQueryRequest request,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT data_type AS DataType, source AS Source,
                   symbol AS Symbol, interval AS Interval,
                   open_time AS OpenTime, close_time AS CloseTime,
                   open AS Open, high AS High, low AS Low, close AS Close,
                   volume AS Volume, quote_volume AS QuoteVolume,
                   trade_count AS TradeCount, collected_at AS CollectedAt,
                   schema_version AS SchemaVersion
            FROM pipeline.market_data FINAL
            WHERE symbol = {QueryRules.StringLiteral(request.Symbol)}
              AND interval = {QueryRules.StringLiteral(request.Interval)}
              AND {ClickHouseSql.Range("open_time", request.From, request.To)}
            ORDER BY open_time
            LIMIT {ClickHouseSql.PageLimit(request.Limit)}
            OFFSET {ClickHouseSql.Offset(request.Offset)}
            """;

        var rows = await clickHouse.QueryAsync<MarketDataResponse>(sql, cancellationToken);
        return PagedResult<MarketDataResponse>.FromRows(rows, request.Limit, request.Offset);
    }
}
