using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/editorial")]
public sealed class EditorialDashboardController : ControllerBase
{
    private readonly IEditorialDashboardService _dashboardService;

    public EditorialDashboardController(IEditorialDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(Application.Dtos.EditorialDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] int? windowHours = null,
        CancellationToken cancellationToken = default)
    {
        if (windowHours is <= 0 or > 168)
        {
            return BadRequest(new { error = "windowHours must be between 1 and 168." });
        }

        var dashboard = await _dashboardService.GetDashboardAsync(windowHours, cancellationToken);
        return Ok(dashboard);
    }
}
