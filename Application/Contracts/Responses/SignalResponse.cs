namespace CryptoIntelligence.Api.Application.Contracts.Responses;

public sealed class SignalResponse
{
    public string Symbol { get; set; } = string.Empty;
    public string SignalType { get; set; } = string.Empty;
    public string OpenTime { get; set; } = string.Empty;
    public double Value { get; set; }
    public double BaselineMean { get; set; }
    public double BaselineStddev { get; set; }
    public double Threshold { get; set; }
    public string Details { get; set; } = string.Empty;
    public string DetectedAt { get; set; } = string.Empty;
}
