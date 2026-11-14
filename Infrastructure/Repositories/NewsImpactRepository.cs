using CryptoIntelligence.Api.Application.Contracts.Requests;
using CryptoIntelligence.Api.Application.Contracts.Responses;
using CryptoIntelligence.Api.Application.Interfaces;
using CryptoIntelligence.Api.Application.Services;
using CryptoIntelligence.Api.Infrastructure.ClickHouse;

namespace CryptoIntelligence.Api.Infrastructure.Repositories;

public sealed class NewsImpactRepository(IClickHouseClient clickHouse) : INewsImpactRepository
{
    public async Task<PagedResult<NewsImpactResponse>> QueryAsync(NewsImpactQueryRequest request, CancellationToken cancellationToken)
    {
        var symbolFilter = string.IsNullOrWhiteSpace(request.Symbol)
            ? string.Empty
            : $"AND symbol = {QueryRules.StringLiteral(request.Symbol)}";
        var horizonFilter = request.HorizonMinutes.HasValue
            ? $"AND horizon_minutes = {request.HorizonMinutes.Value}"
            : string.Empty;

        var sql = $"""
            SELECT source AS Source, article_id AS ArticleId, title AS Title,
                   symbol AS Symbol, published_at AS PublishedAt,
                   horizon_minutes AS HorizonMinutes, baseline_time AS BaselineTime,
                   baseline_close AS BaselineClose, target_time AS TargetTime,
                   target_close AS TargetClose, return_pct AS ReturnPct,
                   calculated_at AS CalculatedAt
            FROM pipeline.news_impact FINAL
            WHERE {ClickHouseSql.Range("published_at", request.From, request.To)}
              {symbolFilter}
              {horizonFilter}
            ORDER BY published_at DESC, source, article_id, symbol, horizon_minutes
            LIMIT {ClickHouseSql.PageLimit(request.Limit)}
            OFFSET {ClickHouseSql.Offset(request.Offset)}
            """;
        var rows = await clickHouse.QueryAsync<NewsImpactResponse>(sql, cancellationToken);
        return PagedResult<NewsImpactResponse>.FromRows(rows, request.Limit, request.Offset);
    }
}
