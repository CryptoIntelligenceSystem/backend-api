using CryptoIntelligence.Api.Infrastructure.ClickHouse;
using Microsoft.AspNetCore.Mvc;

namespace CryptoIntelligence.Api.Controllers;

[ApiController]
[Route("api/v1/health")]
public sealed class HealthController(IClickHouseClient clickHouse) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var clickHouseIsHealthy = await clickHouse.PingAsync(cancellationToken);

        return clickHouseIsHealthy
            ? Ok(new { status = "ok", clickHouse = "ok" })
            : StatusCode(503, new { status = "degraded", clickHouse = "unavailable" });
    }
}
