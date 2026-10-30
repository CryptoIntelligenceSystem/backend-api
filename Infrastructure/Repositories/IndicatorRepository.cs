using CryptoIntelligence.Api.Application.Contracts.Requests;
using CryptoIntelligence.Api.Application.Contracts.Responses;
using CryptoIntelligence.Api.Application.Interfaces;
using CryptoIntelligence.Api.Application.Services;
using CryptoIntelligence.Api.Infrastructure.ClickHouse;

namespace CryptoIntelligence.Api.Infrastructure.Repositories;

public sealed class IndicatorRepository(IClickHouseClient clickHouse) : IIndicatorRepository
{
    public async Task<PagedResult<IndicatorResponse>> QueryAsync(IndicatorQueryRequest request, CancellationToken cancellationToken)
    {
        var readyFilter = request.OnlyReady ? "AND value IS NOT NULL" : string.Empty;
        var sql = $"""
            SELECT symbol AS Symbol, interval AS Interval, name AS Name,
                   open_time AS OpenTime, value AS Value, processed_at AS ProcessedAt
            FROM pipeline.indicators FINAL
            WHERE symbol = {QueryRules.StringLiteral(request.Symbol)}
              AND interval = {QueryRules.StringLiteral(request.Interval)}
              AND name = {QueryRules.StringLiteral(request.Name)}
              AND {ClickHouseSql.Range("open_time", request.From, request.To)}
              {readyFilter}
            ORDER BY open_time
            LIMIT {ClickHouseSql.PageLimit(request.Limit)}
            OFFSET {ClickHouseSql.Offset(request.Offset)}
            """;
        var rows = await clickHouse.QueryAsync<IndicatorResponse>(sql, cancellationToken);
        return PagedResult<IndicatorResponse>.FromRows(rows, request.Limit, request.Offset);
    }
}
