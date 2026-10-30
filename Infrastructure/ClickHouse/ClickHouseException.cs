namespace CryptoIntelligence.Api.Infrastructure.ClickHouse;

public sealed class ClickHouseException : Exception
{
    public ClickHouseException(string message) : base(message) { }
}
