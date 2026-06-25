using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/editorial")]
public sealed class EditorialBriefController : ControllerBase
{
    private readonly IEditorialBriefService _briefService;

    public EditorialBriefController(IEditorialBriefService briefService)
    {
        _briefService = briefService;
    }

    [HttpGet("brief")]
    [ProducesResponseType(typeof(Application.Dtos.EditorialBriefReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatest(
        [FromQuery] string? period = null,
        CancellationToken cancellationToken = default)
    {
        EditorialBriefPeriod? parsedPeriod = null;
        if (!string.IsNullOrWhiteSpace(period))
        {
            if (!Enum.TryParse<EditorialBriefPeriod>(period, ignoreCase: true, out var value))
            {
                return BadRequest(new { error = "period must be Morning or Evening." });
            }

            parsedPeriod = value;
        }

        var brief = await _briefService.GetLatestAsync(parsedPeriod, cancellationToken);
        return brief is null ? NotFound() : Ok(brief);
    }

    [HttpGet("brief/history")]
    [ProducesResponseType(typeof(IReadOnlyList<Application.Dtos.EditorialBriefHistoryItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int limit = 14,
        CancellationToken cancellationToken = default)
    {
        var history = await _briefService.GetHistoryAsync(limit, cancellationToken);
        return Ok(history);
    }

    [HttpPost("brief/generate")]
    [Authorize(Roles = nameof(UserRole.ChiefEditor))]
    [ProducesResponseType(typeof(Application.Dtos.EditorialBriefReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Generate(
        [FromQuery] string period = "Morning",
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<EditorialBriefPeriod>(period, ignoreCase: true, out var parsedPeriod))
        {
            return BadRequest(new { error = "period must be Morning or Evening." });
        }

        var brief = await _briefService.GenerateAsync(parsedPeriod, cancellationToken);
        return Ok(brief);
    }
}
