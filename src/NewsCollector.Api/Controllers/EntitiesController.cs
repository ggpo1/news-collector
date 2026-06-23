using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/entities")]
public sealed class EntitiesController : ControllerBase
{
    private readonly INewsEntityGraphQueryService _entityGraphQueryService;

    public EntitiesController(INewsEntityGraphQueryService entityGraphQueryService)
    {
        _entityGraphQueryService = entityGraphQueryService;
    }

    [HttpGet("graph")]
    [ProducesResponseType(typeof(EntityGraphDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGraph(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] NamedEntityType? type = null,
        [FromQuery] int minWeight = 3,
        [FromQuery] int maxNodes = 60,
        CancellationToken cancellationToken = default)
    {
        if (minWeight < 1)
        {
            return BadRequest(new { error = "minWeight must be >= 1" });
        }

        if (maxNodes is < 10 or > 120)
        {
            return BadRequest(new { error = "maxNodes must be between 10 and 120" });
        }

        var graph = await _entityGraphQueryService.GetCoMentionGraphAsync(
            from,
            to,
            type,
            minWeight,
            maxNodes,
            cancellationToken);

        return Ok(graph);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NamedEntityListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? q = null,
        [FromQuery] NamedEntityType? type = null,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > 200)
        {
            return BadRequest(new { error = "limit must be between 1 and 200" });
        }

        var entities = await _entityGraphQueryService.SearchEntitiesAsync(q, type, limit, cancellationToken);
        return Ok(entities);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NamedEntityDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        var entity = await _entityGraphQueryService.GetEntityDetailAsync(id, from, to, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        return Ok(entity);
    }
}
