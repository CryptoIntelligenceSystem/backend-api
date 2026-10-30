using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace CryptoIntelligence.Api.Infrastructure.ClickHouse;

public sealed class ClickHouseHttpClient : IClickHouseClient
{
    private readonly HttpClient _httpClient;
    private readonly ClickHouseOptions _options;

    public ClickHouseHttpClient(HttpClient httpClient, IOptions<ClickHouseOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildQueryUri())
        {
            Content = new StringContent(sql, Encoding.UTF8, "text/plain")
        };
        AddAuthentication(request);

        using var response = await _httpClient.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ClickHouseException($"ClickHouse returned {(int)response.StatusCode}: {body}");
        }

        var result = new List<T>();
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        foreach (var line in body.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var item = JsonSerializer.Deserialize<T>(line, jsonOptions);
            if (item is not null) result.Add(item);
        }
        return result;
    }

    public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await QueryAsync<PingRow>("SELECT 1 AS value", cancellationToken);
            return true;
        }
        catch (HttpRequestException) { return false; }
        catch (ClickHouseException) { return false; }
    }

    private Uri BuildQueryUri()
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var query = string.Join('&', new[]
        {
            $"database={Uri.EscapeDataString(_options.Database)}",
            "default_format=JSONEachRow",
            "output_format_json_quote_64bit_integers=0"
        });
        return new Uri($"{baseUrl}/?{query}");
    }

    private void AddAuthentication(HttpRequestMessage request)
    {
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_options.Username}:{_options.Password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    private sealed class PingRow { public int Value { get; set; } }
}
