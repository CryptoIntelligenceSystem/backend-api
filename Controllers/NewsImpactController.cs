using CryptoIntelligence.Api.Application.Contracts.Requests;
using CryptoIntelligence.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CryptoIntelligence.Api.Controllers;

[ApiController]
[Route("api/v1/news/impact")]
public sealed class NewsImpactController(INewsImpactService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] NewsImpactQueryRequest request,
        CancellationToken cancellationToken)
    {
        var page = await service.GetAsync(request, cancellationToken);
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
