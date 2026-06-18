using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Route("api/news-links")]
public sealed class NewsLinksController : ControllerBase
{
    private const int MaxPageSize = 100;

    private readonly INewsLinkQueryService _newsLinkQueryService;

    public NewsLinksController(INewsLinkQueryService newsLinkQueryService)
    {
        _newsLinkQueryService = newsLinkQueryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NewsLinkListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] LinkType? linkType = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            return BadRequest(new { error = "page must be >= 1" });
        }

        if (pageSize is < 1 or > MaxPageSize)
        {
            return BadRequest(new { error = $"pageSize must be between 1 and {MaxPageSize}" });
        }

        var result = await _newsLinkQueryService.GetPagedAsync(page, pageSize, linkType, cancellationToken);
        return Ok(result);
    }
}
