using System.Net;
using System.Net.Http.Json;
using CryptoIntelligence.Api.Application.Contracts.Responses;
using CryptoIntelligence.Api.Infrastructure.ClickHouse;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CryptoIntelligence.Api.Tests;

public sealed class ApiIntegrationTests
{
    [Fact]
    public async Task Health_endpoint_runs_through_the_full_http_pipeline()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal("ok", body!.Status);
        Assert.Equal("ok", body.ClickHouse);
    }

    [Fact]
    public async Task Invalid_market_query_returns_bad_request_json()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/v1/market/candles?symbol=bad&interval=5m");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("symbol", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Swagger_json_is_exposed_by_the_http_pipeline()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/v1/market/candles", body);
        Assert.Contains("/api/v1/market/minute-candles", body);
        Assert.Contains("/api/v1/news", body);
        Assert.Contains("/api/v1/signals", body);
    }

    private sealed class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IClickHouseClient>();
                services.AddSingleton<IClickHouseClient>(new FakeClickHouseClient
                {
                    PingResult = true
                });
            });
        }
    }

    private sealed class HealthResponse
    {
        public string Status { get; set; } = string.Empty;
        public string ClickHouse { get; set; } = string.Empty;
    }
}
