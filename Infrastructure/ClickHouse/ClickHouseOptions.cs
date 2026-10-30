namespace CryptoIntelligence.Api.Infrastructure.ClickHouse;

public sealed class ClickHouseOptions
{
    public const string SectionName = "ClickHouse";
    public string BaseUrl { get; set; } = "http://localhost:8123";
    public string Database { get; set; } = "pipeline";
    public string Username { get; set; } = "pipeline_user";
    public string Password { get; set; } = string.Empty;
}
