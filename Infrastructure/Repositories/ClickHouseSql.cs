using System.Globalization;
using CryptoIntelligence.Api.Application.Services;

namespace CryptoIntelligence.Api.Infrastructure.Repositories;

internal static class ClickHouseSql
{
    public static string Timestamp(DateTimeOffset value) =>
        $"toDateTime64({QueryRules.StringLiteral(QueryRules.Timestamp(value))}, 3, 'UTC')";

    public static string Range(string column, DateTimeOffset? from, DateTimeOffset? to)
    {
        var range = QueryRules.Range(from, to);
        return $"{column} >= {Timestamp(range.From)} AND {column} < {Timestamp(range.To)}";
    }

    public static string Limit(int limit) =>
        Math.Clamp(limit, 1, 5000).ToString(CultureInfo.InvariantCulture);

    public static string PageLimit(int limit) =>
        Math.Clamp(limit + 1, 1, 5001).ToString(CultureInfo.InvariantCulture);

    public static string Offset(int offset) =>
        Math.Max(offset, 0).ToString(CultureInfo.InvariantCulture);
}
