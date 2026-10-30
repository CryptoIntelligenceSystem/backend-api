using System.Globalization;
using System.Text.RegularExpressions;

namespace CryptoIntelligence.Api.Application.Services;

internal static partial class QueryRules
{
    public static void ValidateSymbol(string? symbol, bool required = true)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            if (required) throw new ArgumentException("symbol is required.");
            return;
        }
        if (!SymbolPattern().IsMatch(symbol))
            throw new ArgumentException("symbol must contain 5-20 uppercase letters or numbers.");
    }

    public static void ValidateInterval(string interval)
    {
        if (interval is not ("1m" or "5m"))
            throw new ArgumentException("interval must be either 1m or 5m.");
    }

    public static int ValidateLimit(int limit, int defaultLimit = 500) =>
        limit <= 0 ? defaultLimit : Math.Min(limit, 5000);

    public static int ValidateOffset(int offset) =>
        offset < 0 ? throw new ArgumentException("offset cannot be negative.") : offset;

    public static (DateTimeOffset From, DateTimeOffset To) Range(DateTimeOffset? from, DateTimeOffset? to)
    {
        var resolvedTo = (to ?? DateTimeOffset.UtcNow).ToUniversalTime();
        var resolvedFrom = (from ?? resolvedTo.AddHours(-24)).ToUniversalTime();
        if (resolvedFrom >= resolvedTo)
            throw new ArgumentException("from must be earlier than to.");
        return (resolvedFrom, resolvedTo);
    }

    public static string Timestamp(DateTimeOffset value) =>
        value.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

    public static string StringLiteral(string value) => $"'{value.Replace("'", "''")}'";

    [GeneratedRegex("^[A-Z0-9]{5,20}$")]
    private static partial Regex SymbolPattern();
}
