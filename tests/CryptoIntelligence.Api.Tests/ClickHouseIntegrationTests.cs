using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CryptoIntelligence.Api.Tests;

public sealed class ClickHouseIntegrationTests
{
    [IntegrationFact]
    public async Task Real_clickhouse_serves_market_minute_and_news_api_queries()
    {
        using var factory = new RealClickHouseApiFactory();
        using var client = factory.CreateClient();
        var articleId = $"backend-integration-{Guid.NewGuid():N}";

        await ExecuteSqlAsync("""
            INSERT INTO pipeline.market_data
            (data_type, source, symbol, interval, open_time, close_time,
             open, high, low, close, volume, quote_volume, trade_count,
             collected_at, schema_version)
            VALUES
            ('market', 'backend_integration', 'ITBTCUSDT', '1m',
             '2099-01-01 00:00:00.000', '2099-01-01 00:00:59.999',
             100.0, 101.0, 99.0, 100.5, 10.0, 1005.0, 20,
             now64(3), '1.0')
            """);

        await ExecuteSqlAsync($"""
            INSERT INTO pipeline.news_data
            (data_type, source, article_id, title, summary, url,
             published_at, collected_at, symbols, sentiment, schema_version)
            VALUES
            ('news', 'backend_integration', '{articleId}',
             'Integration test article', 'Integration test summary',
             'https://example.com/{articleId}',
             '2099-01-01 00:00:00.000', now64(3), ['ITBTCUSDT'], NULL, '1.0')
            """);

        using var marketResponse = await client.GetAsync(
            "/api/v1/market/minute-candles?symbol=ITBTCUSDT&from=2099-01-01T00:00:00Z&to=2099-01-01T00:01:00Z&limit=10");
        marketResponse.EnsureSuccessStatusCode();
        using var marketJson = JsonDocument.Parse(await marketResponse.Content.ReadAsStringAsync());
        Assert.Equal(1, marketJson.RootElement.GetProperty("count").GetInt32());
        Assert.Equal("1m", marketJson.RootElement.GetProperty("data")[0].GetProperty("interval").GetString());

        using var newsResponse = await client.GetAsync(
            "/api/v1/news?from=2099-01-01T00:00:00Z&to=2099-01-01T00:01:00Z&limit=10");
        newsResponse.EnsureSuccessStatusCode();
        using var newsJson = JsonDocument.Parse(await newsResponse.Content.ReadAsStringAsync());
        Assert.True(newsJson.RootElement.GetProperty("count").GetInt32() >= 1);
        Assert.Contains(
            newsJson.RootElement.GetProperty("data").EnumerateArray(),
            item => item.GetProperty("articleId").GetString() == articleId);
    }

    private static async Task ExecuteSqlAsync(string sql)
    {
        var baseUrl = Environment.GetEnvironmentVariable("CLICKHOUSE_TEST_BASE_URL") ?? "http://localhost:8123";
        var database = Environment.GetEnvironmentVariable("CLICKHOUSE_TEST_DATABASE") ?? "pipeline";
        var username = Environment.GetEnvironmentVariable("CLICKHOUSE_TEST_USERNAME") ?? "pipeline_user";
        var password = Environment.GetEnvironmentVariable("CLICKHOUSE_TEST_PASSWORD") ?? "change-this-clickhouse-password";

        using var http = new HttpClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl.TrimEnd('/')}/?database={Uri.EscapeDataString(database)}");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
        request.Content = new StringContent(sql, Encoding.UTF8, "text/plain");

        using var response = await http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"ClickHouse seed failed: {body}");
    }

    private sealed class RealClickHouseApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ClickHouse:BaseUrl"] = Environment.GetEnvironmentVariable("CLICKHOUSE_TEST_BASE_URL") ?? "http://localhost:8123",
                    ["ClickHouse:Database"] = Environment.GetEnvironmentVariable("CLICKHOUSE_TEST_DATABASE") ?? "pipeline",
                    ["ClickHouse:Username"] = Environment.GetEnvironmentVariable("CLICKHOUSE_TEST_USERNAME") ?? "pipeline_user",
                    ["ClickHouse:Password"] = Environment.GetEnvironmentVariable("CLICKHOUSE_TEST_PASSWORD") ?? "change-this-clickhouse-password"
                });
            });
        }
    }

    private sealed class IntegrationFactAttribute : FactAttribute
    {
        public IntegrationFactAttribute()
        {
            if (!string.Equals(
                    Environment.GetEnvironmentVariable("RUN_CLICKHOUSE_INTEGRATION_TESTS"),
                    "1",
                    StringComparison.Ordinal))
            {
                Skip = "Set RUN_CLICKHOUSE_INTEGRATION_TESTS=1 after starting ClickHouse Docker to run this test.";
            }
        }
    }
}
