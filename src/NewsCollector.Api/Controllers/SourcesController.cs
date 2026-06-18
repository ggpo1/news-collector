using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Route("api/sources")]
public sealed class SourcesController : ControllerBase
{
    private readonly ISourcesQueryService _sourcesQueryService;

    public SourcesController(ISourcesQueryService sourcesQueryService)
    {
        _sourcesQueryService = sourcesQueryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SourceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var sources = await _sourcesQueryService.GetAllAsync(cancellationToken);
        return Ok(sources);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SourceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var source = await _sourcesQueryService.GetByIdAsync(id, cancellationToken);
        return source is null ? NotFound() : Ok(source);
    }
}
