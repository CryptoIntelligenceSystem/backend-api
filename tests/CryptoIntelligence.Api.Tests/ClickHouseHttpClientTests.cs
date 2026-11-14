using System.Net;
using CryptoIntelligence.Api.Infrastructure.ClickHouse;
using Microsoft.Extensions.Options;

namespace CryptoIntelligence.Api.Tests;

public sealed class ClickHouseHttpClientTests
{
    [Fact]
    public async Task QueryAsync_wraps_transport_failure_as_clickhouse_exception()
    {
        using var httpClient = new HttpClient(new ThrowingHandler())
        {
            BaseAddress = new Uri("http://localhost:8123")
        };
        var client = new ClickHouseHttpClient(
            httpClient,
            Options.Create(new ClickHouseOptions { Password = "test" }));

        var exception = await Assert.ThrowsAsync<ClickHouseException>(() =>
            client.QueryAsync<object>("SELECT 1", CancellationToken.None));

        Assert.Contains("connect to ClickHouse", exception.Message);
        Assert.IsType<HttpRequestException>(exception.InnerException);
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new HttpRequestException("connection refused");
    }
}
