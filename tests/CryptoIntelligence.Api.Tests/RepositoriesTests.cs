using CryptoIntelligence.Api.Application.Contracts.Requests;
using CryptoIntelligence.Api.Application.Contracts.Responses;
using CryptoIntelligence.Api.Infrastructure.Repositories;

namespace CryptoIntelligence.Api.Tests;

public sealed class RepositoriesTests
{
    [Fact]
    public async Task CandleRepository_queries_candles_table_with_dto_aliases()
    {
        var client = new FakeClickHouseClient();
        client.Results[typeof(CandleResponse)] = new List<CandleResponse>
        {
            new() { Symbol = "BTCUSDT", Interval = "5m", Close = 80000 }
        };
        var repository = new CandleRepository(client);

        var result = await repository.QueryAsync(
            FixedCandleRequest(),
            CancellationToken.None);

        Assert.Single(result);
        Assert.Contains("FROM pipeline.candles_5m FINAL", client.Queries.Single());
        Assert.Contains("open_time AS OpenTime", client.Queries.Single());
        Assert.Contains("LIMIT 11", client.Queries.Single());
        Assert.Contains("OFFSET 0", client.Queries.Single());
    }

    [Fact]
    public async Task IndicatorRepository_can_filter_out_unready_values()
    {
        var client = new FakeClickHouseClient();
        var repository = new IndicatorRepository(client);

        await repository.QueryAsync(
            new IndicatorQueryRequest
            {
                Symbol = "BTCUSDT",
                Interval = "1m",
                Name = "ma20",
                OnlyReady = true,
                Limit = 20,
                From = FixedFrom(),
                To = FixedTo()
            },
            CancellationToken.None);

        var sql = client.Queries.Single();
        Assert.Contains("FROM pipeline.indicators FINAL", sql);
        Assert.Contains("value IS NOT NULL", sql);
    }

    [Fact]
    public async Task SignalRepository_filters_signal_type_and_symbol()
    {
        var client = new FakeClickHouseClient();
        var repository = new SignalRepository(client);

        await repository.QueryAsync(
            new SignalQueryRequest
            {
                Symbol = "BTCUSDT",
                SignalType = "volume_anomaly",
                Limit = 10,
                From = FixedFrom(),
                To = FixedTo()
            },
            CancellationToken.None);

        var sql = client.Queries.Single();
        Assert.Contains("FROM pipeline.signals FINAL", sql);
        Assert.Contains("signal_type = 'volume_anomaly'", sql);
        Assert.Contains("symbol = 'BTCUSDT'", sql);
    }

    [Fact]
    public async Task NewsRepository_queries_news_table()
    {
        var client = new FakeClickHouseClient();
        var repository = new NewsRepository(client);

        await repository.QueryAsync(
            new NewsQueryRequest
            {
                Symbol = "BTCUSDT",
                Limit = 10,
                From = FixedFrom(),
                To = FixedTo()
            },
            CancellationToken.None);

        var sql = client.Queries.Single();
        Assert.Contains("FROM pipeline.news_data FINAL", sql);
        Assert.Contains("has(symbols, 'BTCUSDT')", sql);
    }

    [Fact]
    public async Task MarketDataRepository_queries_one_minute_market_table_with_pagination()
    {
        var client = new FakeClickHouseClient();
        var repository = new MarketDataRepository(client);

        await repository.QueryAsync(
            new MarketDataQueryRequest
            {
                Symbol = "BTCUSDT",
                Interval = "1m",
                Limit = 20,
                Offset = 40,
                From = FixedFrom(),
                To = FixedTo()
            },
            CancellationToken.None);

        var sql = client.Queries.Single();
        Assert.Contains("FROM pipeline.market_data FINAL", sql);
        Assert.Contains("interval = '1m'", sql);
        Assert.Contains("LIMIT 21", sql);
        Assert.Contains("OFFSET 40", sql);
    }

    [Fact]
    public async Task StatusRepository_returns_pipeline_counts()
    {
        var client = new FakeClickHouseClient();
        client.Results[typeof(PipelineStatusResponse)] = new List<PipelineStatusResponse>
        {
            new() { MarketDataCount = 198, Candle5mCount = 38, SignalCount = 8 }
        };
        var repository = new StatusRepository(client);

        var result = await repository.GetAsync(CancellationToken.None);

        Assert.Equal(198, result.MarketDataCount);
        Assert.Equal(38, result.Candle5mCount);
        Assert.Contains("pipeline.market_data", client.Queries.Single());
    }

    private static CandleQueryRequest FixedCandleRequest() => new()
    {
        Symbol = "BTCUSDT",
        Interval = "5m",
        Limit = 10,
        From = FixedFrom(),
        To = FixedTo()
    };

    private static DateTimeOffset FixedFrom() =>
        new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

    private static DateTimeOffset FixedTo() =>
        new(2026, 10, 29, 13, 0, 0, TimeSpan.Zero);
}
