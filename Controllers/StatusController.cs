using CryptoIntelligence.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CryptoIntelligence.Api.Controllers;

[ApiController]
[Route("api/v1/status")]
public sealed class StatusController(IStatusService statusService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        return Ok(await statusService.GetAsync(cancellationToken));
    }
}
