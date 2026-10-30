using CryptoIntelligence.Api.Application.Contracts.Requests;
using CryptoIntelligence.Api.Application.Contracts.Responses;
using CryptoIntelligence.Api.Application.Interfaces;
using CryptoIntelligence.Api.Application.Services;
using CryptoIntelligence.Api.Infrastructure.ClickHouse;

namespace CryptoIntelligence.Api.Infrastructure.Repositories;

public sealed class NewsRepository(IClickHouseClient clickHouse) : INewsRepository
{
    public async Task<PagedResult<NewsResponse>> QueryAsync(NewsQueryRequest request, CancellationToken cancellationToken)
    {
        var symbolFilter = string.IsNullOrWhiteSpace(request.Symbol)
            ? string.Empty
            : $"AND has(symbols, {QueryRules.StringLiteral(request.Symbol)})";

        var sql = $"""
            SELECT data_type AS DataType, source AS Source, article_id AS ArticleId,
                   title AS Title, summary AS Summary, url AS Url,
                   published_at AS PublishedAt, collected_at AS CollectedAt,
                   symbols AS Symbols, sentiment AS Sentiment
            FROM pipeline.news_data FINAL
            WHERE {ClickHouseSql.Range("published_at", request.From, request.To)}
              {symbolFilter}
            ORDER BY published_at DESC
            LIMIT {ClickHouseSql.PageLimit(request.Limit)}
            OFFSET {ClickHouseSql.Offset(request.Offset)}
            """;
        var rows = await clickHouse.QueryAsync<NewsResponse>(sql, cancellationToken);
        return PagedResult<NewsResponse>.FromRows(rows, request.Limit, request.Offset);
    }
}
