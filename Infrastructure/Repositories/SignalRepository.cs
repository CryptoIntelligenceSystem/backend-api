using CryptoIntelligence.Api.Application.Contracts.Requests;
using CryptoIntelligence.Api.Application.Contracts.Responses;
using CryptoIntelligence.Api.Application.Interfaces;
using CryptoIntelligence.Api.Application.Services;
using CryptoIntelligence.Api.Infrastructure.ClickHouse;

namespace CryptoIntelligence.Api.Infrastructure.Repositories;

public sealed class SignalRepository(IClickHouseClient clickHouse) : ISignalRepository
{
    public async Task<PagedResult<SignalResponse>> QueryAsync(SignalQueryRequest request, CancellationToken cancellationToken)
    {
        var symbolFilter = string.IsNullOrWhiteSpace(request.Symbol)
            ? string.Empty
            : $"AND symbol = {QueryRules.StringLiteral(request.Symbol)}";
        var typeFilter = string.IsNullOrWhiteSpace(request.SignalType)
            ? string.Empty
            : $"AND signal_type = {QueryRules.StringLiteral(request.SignalType)}";

        var sql = $"""
            SELECT symbol AS Symbol, signal_type AS SignalType,
                   open_time AS OpenTime, value AS Value,
                   baseline_mean AS BaselineMean, baseline_stddev AS BaselineStddev,
                   threshold AS Threshold, details AS Details, detected_at AS DetectedAt
            FROM pipeline.signals FINAL
            WHERE {ClickHouseSql.Range("open_time", request.From, request.To)}
              {symbolFilter}
              {typeFilter}
            ORDER BY open_time DESC
            LIMIT {ClickHouseSql.PageLimit(request.Limit)}
            OFFSET {ClickHouseSql.Offset(request.Offset)}
            """;
        var rows = await clickHouse.QueryAsync<SignalResponse>(sql, cancellationToken);
        return PagedResult<SignalResponse>.FromRows(rows, request.Limit, request.Offset);
    }
}
