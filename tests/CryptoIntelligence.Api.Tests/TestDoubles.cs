using CryptoIntelligence.Api.Application.Contracts.Requests;
using CryptoIntelligence.Api.Application.Contracts.Responses;
using CryptoIntelligence.Api.Application.Interfaces;
using CryptoIntelligence.Api.Infrastructure.ClickHouse;

namespace CryptoIntelligence.Api.Tests;

internal sealed class FakeClickHouseClient : IClickHouseClient
{
    public List<string> Queries { get; } = [];
    public bool PingResult { get; set; } = true;
    public Dictionary<Type, object> Results { get; } = [];

    public Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        CancellationToken cancellationToken = default)
    {
        Queries.Add(sql);
        if (Results.TryGetValue(typeof(T), out var result))
        {
            return Task.FromResult((IReadOnlyList<T>)result);
        }

        return Task.FromResult<IReadOnlyList<T>>([]);
    }

    public Task<bool> PingAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(PingResult);
}

internal sealed class FakeCandleRepository : ICandleRepository
{
    public CandleQueryRequest? LastRequest { get; private set; }
    public IReadOnlyList<CandleResponse> Result { get; set; } = [];

    public Task<PagedResult<CandleResponse>> QueryAsync(
        CandleQueryRequest request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(new PagedResult<CandleResponse>(Result, request.Limit, request.Offset, false));
    }
}

internal sealed class FakeMarketDataRepository : IMarketDataRepository
{
    public Task<PagedResult<MarketDataResponse>> QueryAsync(
        MarketDataQueryRequest request,
        CancellationToken cancellationToken) => Task.FromResult(
            new PagedResult<MarketDataResponse>([], request.Limit, request.Offset, false));
}

internal sealed class FakeIndicatorRepository : IIndicatorRepository
{
    public IndicatorQueryRequest? LastRequest { get; private set; }
    public IReadOnlyList<IndicatorResponse> Result { get; set; } = [];

    public Task<PagedResult<IndicatorResponse>> QueryAsync(
        IndicatorQueryRequest request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(new PagedResult<IndicatorResponse>(Result, request.Limit, request.Offset, false));
    }
}

internal sealed class FakeSignalRepository : ISignalRepository
{
    public SignalQueryRequest? LastRequest { get; private set; }

    public Task<PagedResult<SignalResponse>> QueryAsync(
        SignalQueryRequest request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(new PagedResult<SignalResponse>([], 500, 0, false));
    }
}

internal sealed class FakeNewsRepository : INewsRepository
{
    public NewsQueryRequest? LastRequest { get; private set; }

    public Task<PagedResult<NewsResponse>> QueryAsync(
        NewsQueryRequest request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(new PagedResult<NewsResponse>([], 50, 0, false));
    }
}

internal sealed class FakeNewsImpactRepository : INewsImpactRepository
{
    public NewsImpactQueryRequest? LastRequest { get; private set; }

    public Task<PagedResult<NewsImpactResponse>> QueryAsync(
        NewsImpactQueryRequest request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(new PagedResult<NewsImpactResponse>([], request.Limit, request.Offset, false));
    }
}

internal sealed class FakeCandleService : ICandleService
{
    public IReadOnlyList<CandleResponse> Result { get; set; } = [];

    public Task<PagedResult<CandleResponse>> GetAsync(
        CandleQueryRequest request,
        CancellationToken cancellationToken) => Task.FromResult(
            new PagedResult<CandleResponse>(Result, request.Limit, request.Offset, false));
}

internal sealed class FakeMarketDataService : IMarketDataService
{
    public IReadOnlyList<MarketDataResponse> Result { get; set; } = [];

    public Task<PagedResult<MarketDataResponse>> GetAsync(
        MarketDataQueryRequest request,
        CancellationToken cancellationToken) => Task.FromResult(
            new PagedResult<MarketDataResponse>(Result, request.Limit, request.Offset, false));
}
