using CryptoIntelligence.Api.Application.Contracts.Requests;
using CryptoIntelligence.Api.Application.Contracts.Responses;

namespace CryptoIntelligence.Api.Application.Interfaces;

public interface ICandleRepository
{
    Task<PagedResult<CandleResponse>> QueryAsync(CandleQueryRequest request, CancellationToken cancellationToken);
}

public interface IMarketDataRepository
{
    Task<PagedResult<MarketDataResponse>> QueryAsync(MarketDataQueryRequest request, CancellationToken cancellationToken);
}

public interface IIndicatorRepository
{
    Task<PagedResult<IndicatorResponse>> QueryAsync(IndicatorQueryRequest request, CancellationToken cancellationToken);
}

public interface ISignalRepository
{
    Task<PagedResult<SignalResponse>> QueryAsync(SignalQueryRequest request, CancellationToken cancellationToken);
}

public interface INewsRepository
{
    Task<PagedResult<NewsResponse>> QueryAsync(NewsQueryRequest request, CancellationToken cancellationToken);
}

public interface INewsImpactRepository
{
    Task<PagedResult<NewsImpactResponse>> QueryAsync(NewsImpactQueryRequest request, CancellationToken cancellationToken);
}

public interface IStatusRepository
{
    Task<PipelineStatusResponse> GetAsync(CancellationToken cancellationToken);
}
