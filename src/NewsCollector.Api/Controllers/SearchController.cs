using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/search")]
public sealed class SearchController : ControllerBase
{
    private readonly ISearchQueryService _searchQueryService;

    public SearchController(ISearchQueryService searchQueryService)
    {
        _searchQueryService = searchQueryService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] SearchDocumentType[]? types = null,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { error = "Query parameter q is required." });
        }

        if (limit is < 1 or > 50)
        {
            return BadRequest(new { error = "limit must be between 1 and 50." });
        }

        var results = await _searchQueryService.SearchAsync(
            q,
            types,
            limit,
            cancellationToken);

        return Ok(results);
    }
}
