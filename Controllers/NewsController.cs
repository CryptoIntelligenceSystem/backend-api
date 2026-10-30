using CryptoIntelligence.Api.Application.Contracts.Requests;
using CryptoIntelligence.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CryptoIntelligence.Api.Controllers;

[ApiController]
[Route("api/v1/news")]
public sealed class NewsController(INewsService newsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] NewsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var page = await newsService.GetAsync(request, cancellationToken);
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
