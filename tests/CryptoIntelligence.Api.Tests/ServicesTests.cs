using CryptoIntelligence.Api.Application.Contracts.Requests;
using CryptoIntelligence.Api.Application.Contracts.Responses;
using CryptoIntelligence.Api.Application.Services;

namespace CryptoIntelligence.Api.Tests;

public sealed class ServicesTests
{
    [Fact]
    public async Task CandleService_validates_and_delegates_request()
    {
        var repository = new FakeCandleRepository
        {
            Result = [new CandleResponse { Symbol = "BTCUSDT", Interval = "5m" }]
        };
        var service = new CandleService(repository);
        var request = new CandleQueryRequest
        {
            Symbol = "BTCUSDT",
            Interval = "5m",
            Limit = 10,
            From = new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero),
            To = new DateTimeOffset(2026, 10, 29, 13, 0, 0, TimeSpan.Zero)
        };

        var result = await service.GetAsync(request, CancellationToken.None);

        Assert.Single(result);
        Assert.Same(request, repository.LastRequest);
    }

    [Theory]
    [InlineData("")]
    [InlineData("btcUSDT")]
    [InlineData("BTC")]
    public async Task CandleService_rejects_invalid_symbol(string symbol)
    {
        var service = new CandleService(new FakeCandleRepository());

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetAsync(
            new CandleQueryRequest { Symbol = symbol },
            CancellationToken.None));
    }

    [Fact]
    public async Task CandleService_limits_large_requests_to_5000_rows()
    {
        var repository = new FakeCandleRepository();
        var service = new CandleService(repository);

        await service.GetAsync(
            new CandleQueryRequest { Limit = 9000 },
            CancellationToken.None);

        Assert.NotNull(repository.LastRequest);
        Assert.Equal(5000, repository.LastRequest!.Limit);
    }

    [Fact]
    public async Task CandleService_rejects_invalid_time_range()
    {
        var service = new CandleService(new FakeCandleRepository());
        var from = new DateTimeOffset(2026, 10, 29, 13, 0, 0, TimeSpan.Zero);
        var to = from.AddHours(-1);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetAsync(
            new CandleQueryRequest { From = from, To = to },
            CancellationToken.None));
    }

    [Fact]
    public async Task MarketDataService_accepts_only_one_minute_interval()
    {
        var service = new MarketDataService(new FakeMarketDataRepository());

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetAsync(
            new MarketDataQueryRequest { Interval = "5m" },
            CancellationToken.None));
    }

    [Fact]
    public async Task CandleService_rejects_negative_offset()
    {
        var service = new CandleService(new FakeCandleRepository());

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetAsync(
            new CandleQueryRequest { Offset = -1 },
            CancellationToken.None));
    }

    [Fact]
    public async Task IndicatorService_normalizes_supported_indicator_name()
    {
        var repository = new FakeIndicatorRepository();
        var service = new IndicatorService(repository);

        await service.GetAsync(
            new IndicatorQueryRequest { Name = "MA20" },
            CancellationToken.None);

        Assert.Equal("ma20", repository.LastRequest!.Name);
    }

    [Fact]
    public async Task IndicatorService_rejects_unknown_indicator()
    {
        var service = new IndicatorService(new FakeIndicatorRepository());

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetAsync(
            new IndicatorQueryRequest { Name = "rsi14" },
            CancellationToken.None));
    }

    [Fact]
    public async Task SignalService_rejects_unknown_signal_type()
    {
        var service = new SignalService(new FakeSignalRepository());

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetAsync(
            new SignalQueryRequest { SignalType = "unknown" },
            CancellationToken.None));
    }

    [Fact]
    public async Task NewsService_uses_default_limit_when_limit_is_not_provided()
    {
        var repository = new FakeNewsRepository();
        var service = new NewsService(repository);

        await service.GetAsync(new NewsQueryRequest { Limit = 0 }, CancellationToken.None);

        Assert.Equal(50, repository.LastRequest!.Limit);
    }

    [Fact]
    public async Task NewsImpactService_validates_horizon_and_normalizes_pagination()
    {
        var repository = new FakeNewsImpactRepository();
        var service = new NewsImpactService(repository);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetAsync(
            new NewsImpactQueryRequest { HorizonMinutes = 30 }, CancellationToken.None));

        await service.GetAsync(new NewsImpactQueryRequest
        {
            Symbol = "BTCUSDT",
            HorizonMinutes = 5,
            Limit = 0
        }, CancellationToken.None);
        Assert.Equal(50, repository.LastRequest!.Limit);
        Assert.Equal(5, repository.LastRequest.HorizonMinutes);
    }
}
