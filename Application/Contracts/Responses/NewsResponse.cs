namespace CryptoIntelligence.Api.Application.Contracts.Responses;

public sealed class NewsResponse
{
    public string DataType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string ArticleId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string PublishedAt { get; set; } = string.Empty;
    public string CollectedAt { get; set; } = string.Empty;
    public string[] Symbols { get; set; } = [];
    public string? Sentiment { get; set; }
}
