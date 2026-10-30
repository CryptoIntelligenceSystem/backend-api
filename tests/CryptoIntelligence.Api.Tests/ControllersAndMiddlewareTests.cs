using System.Text;
using CryptoIntelligence.Api.Application.Contracts.Responses;
using CryptoIntelligence.Api.Controllers;
using CryptoIntelligence.Api.Infrastructure.ClickHouse;
using CryptoIntelligence.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace CryptoIntelligence.Api.Tests;

public sealed class ControllersAndMiddlewareTests
{
    [Fact]
    public async Task HealthController_returns_ok_when_clickhouse_is_available()
    {
        var controller = new HealthController(new FakeClickHouseClient { PingResult = true });

        var result = await controller.Get(CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(response.Value);
    }

    [Fact]
    public async Task HealthController_returns_503_when_clickhouse_is_unavailable()
    {
        var controller = new HealthController(new FakeClickHouseClient { PingResult = false });

        var result = await controller.Get(CancellationToken.None);

        var response = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task MarketController_returns_data_and_count()
    {
        var service = new FakeCandleService
        {
            Result = [new CandleResponse { Symbol = "BTCUSDT" }]
        };
        var controller = new MarketController(service, new FakeMarketDataService());

        var result = await controller.GetCandles(
            new() { Symbol = "BTCUSDT", Interval = "5m" },
            CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result);
        var count = (int)response.Value!.GetType().GetProperty("count")!.GetValue(response.Value)!;
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Middleware_maps_argument_exception_to_400()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ArgumentException("invalid request"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Contains("invalid request", await ReadBodyAsync(context));
    }

    [Fact]
    public async Task Middleware_maps_clickhouse_exception_to_503()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ClickHouseException("database unavailable"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        Assert.Contains("ClickHouse", await ReadBodyAsync(context));
    }

    [Fact]
    public async Task Middleware_maps_unexpected_exception_to_500()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("unexpected"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "test-trace-id"
        };
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return await new StreamReader(
            context.Response.Body,
            Encoding.UTF8,
            leaveOpen: true).ReadToEndAsync();
    }
}
