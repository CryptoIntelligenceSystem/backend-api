using CryptoIntelligence.Api.Application.Contracts.Requests;
using CryptoIntelligence.Api.Application.Contracts.Responses;
using CryptoIntelligence.Api.Application.Interfaces;

namespace CryptoIntelligence.Api.Application.Services;

public sealed class CandleService(ICandleRepository repository) : ICandleService
{
    public Task<PagedResult<CandleResponse>> GetAsync(CandleQueryRequest request, CancellationToken cancellationToken)
    {
        QueryRules.ValidateSymbol(request.Symbol);
        QueryRules.ValidateInterval(request.Interval);
        if (request.Interval != "5m")
            throw new ArgumentException("The candles endpoint currently supports interval=5m only.");
        request.Limit = QueryRules.ValidateLimit(request.Limit);
        request.Offset = QueryRules.ValidateOffset(request.Offset);
        QueryRules.Range(request.From, request.To);
        return repository.QueryAsync(request, cancellationToken);
    }
}

public sealed class MarketDataService(IMarketDataRepository repository) : IMarketDataService
{
    public Task<PagedResult<MarketDataResponse>> GetAsync(MarketDataQueryRequest request, CancellationToken cancellationToken)
    {
        QueryRules.ValidateSymbol(request.Symbol);
        QueryRules.ValidateInterval(request.Interval);
        if (request.Interval != "1m")
            throw new ArgumentException("The market data endpoint currently supports interval=1m only.");
        request.Limit = QueryRules.ValidateLimit(request.Limit);
        request.Offset = QueryRules.ValidateOffset(request.Offset);
        QueryRules.Range(request.From, request.To);
        return repository.QueryAsync(request, cancellationToken);
    }
}

public sealed class IndicatorService(IIndicatorRepository repository) : IIndicatorService
{
    public Task<PagedResult<IndicatorResponse>> GetAsync(IndicatorQueryRequest request, CancellationToken cancellationToken)
    {
        QueryRules.ValidateSymbol(request.Symbol);
        QueryRules.ValidateInterval(request.Interval);
        if (!string.Equals(request.Name, "ma20", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only the ma20 indicator is currently available.");
        request.Name = request.Name.ToLowerInvariant();
        request.Limit = QueryRules.ValidateLimit(request.Limit);
        request.Offset = QueryRules.ValidateOffset(request.Offset);
        QueryRules.Range(request.From, request.To);
        return repository.QueryAsync(request, cancellationToken);
    }
}

public sealed class SignalService(ISignalRepository repository) : ISignalService
{
    public Task<PagedResult<SignalResponse>> GetAsync(SignalQueryRequest request, CancellationToken cancellationToken)
    {
        QueryRules.ValidateSymbol(request.Symbol, required: false);
        if (!string.IsNullOrWhiteSpace(request.SignalType) &&
            !string.Equals(request.SignalType, "volume_anomaly", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only the volume_anomaly signal is currently available.");
        request.SignalType = request.SignalType?.ToLowerInvariant();
        request.Limit = QueryRules.ValidateLimit(request.Limit);
        request.Offset = QueryRules.ValidateOffset(request.Offset);
        QueryRules.Range(request.From, request.To);
        return repository.QueryAsync(request, cancellationToken);
    }
}

public sealed class NewsService(INewsRepository repository) : INewsService
{
    public Task<PagedResult<NewsResponse>> GetAsync(NewsQueryRequest request, CancellationToken cancellationToken)
    {
        QueryRules.ValidateSymbol(request.Symbol, required: false);
        request.Limit = QueryRules.ValidateLimit(request.Limit, 50);
        request.Offset = QueryRules.ValidateOffset(request.Offset);
        QueryRules.Range(request.From, request.To);
        return repository.QueryAsync(request, cancellationToken);
    }
}

public sealed class StatusService(IStatusRepository repository) : IStatusService
{
    public Task<PipelineStatusResponse> GetAsync(CancellationToken cancellationToken) =>
        repository.GetAsync(cancellationToken);
}
