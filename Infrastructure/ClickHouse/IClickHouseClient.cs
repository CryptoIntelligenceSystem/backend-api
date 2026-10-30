namespace CryptoIntelligence.Api.Infrastructure.ClickHouse;

public interface IClickHouseClient
{
    Task<IReadOnlyList<T>> QueryAsync<T>(string sql, CancellationToken cancellationToken = default);
    Task<bool> PingAsync(CancellationToken cancellationToken = default);
}
