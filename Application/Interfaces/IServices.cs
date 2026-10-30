using CryptoIntelligence.Api.Application.Contracts.Requests;
using CryptoIntelligence.Api.Application.Contracts.Responses;

namespace CryptoIntelligence.Api.Application.Interfaces;

public interface ICandleService
{
    Task<PagedResult<CandleResponse>> GetAsync(CandleQueryRequest request, CancellationToken cancellationToken);
}

public interface IMarketDataService
{
    Task<PagedResult<MarketDataResponse>> GetAsync(MarketDataQueryRequest request, CancellationToken cancellationToken);
}

public interface IIndicatorService
{
    Task<PagedResult<IndicatorResponse>> GetAsync(IndicatorQueryRequest request, CancellationToken cancellationToken);
}

public interface ISignalService
{
    Task<PagedResult<SignalResponse>> GetAsync(SignalQueryRequest request, CancellationToken cancellationToken);
}

public interface INewsService
{
    Task<PagedResult<NewsResponse>> GetAsync(NewsQueryRequest request, CancellationToken cancellationToken);
}

public interface IStatusService
{
    Task<PipelineStatusResponse> GetAsync(CancellationToken cancellationToken);
}
