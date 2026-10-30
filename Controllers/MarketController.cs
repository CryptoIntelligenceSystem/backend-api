using CryptoIntelligence.Api.Application.Contracts.Requests;
using CryptoIntelligence.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CryptoIntelligence.Api.Controllers;

[ApiController]
[Route("api/v1/market")]
public sealed class MarketController(ICandleService candleService, IMarketDataService marketDataService) : ControllerBase
{
    [HttpGet("candles")]
    public async Task<IActionResult> GetCandles(
        [FromQuery] CandleQueryRequest request,
        CancellationToken cancellationToken)
    {
        var page = await candleService.GetAsync(request, cancellationToken);
        return Ok(new
        {
            data = page.Data,
            count = page.Count,
            limit = page.Limit,
            offset = page.Offset,
            hasMore = page.HasMore,
            nextOffset = page.NextOffset
        });
    }

    [HttpGet("minute-candles")]
    public async Task<IActionResult> GetMinuteCandles(
        [FromQuery] MarketDataQueryRequest request,
        CancellationToken cancellationToken)
    {
        var page = await marketDataService.GetAsync(request, cancellationToken);
        return Ok(new
        {
            data = page.Data,
            count = page.Count,
            limit = page.Limit,
            offset = page.Offset,
            hasMore = page.HasMore,
            nextOffset = page.NextOffset
        });
    }
}
