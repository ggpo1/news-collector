using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/sources")]
public sealed class SourcesController : ControllerBase
{
    private readonly ISourcesService _sourcesService;

    public SourcesController(ISourcesService sourcesService)
    {
        _sourcesService = sourcesService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SourceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var sources = await _sourcesService.GetAllAsync(cancellationToken);
        return Ok(sources);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SourceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var source = await _sourcesService.GetByIdAsync(id, cancellationToken);
        return source is null ? NotFound() : Ok(source);
    }

    [Authorize(Roles = nameof(UserRole.ChiefEditor))]
    [HttpPost]
    [ProducesResponseType(typeof(SourceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSourceRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "name is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest(new { error = "url is required" });
        }

        if (request.FetchIntervalMinutes < 1)
        {
            return BadRequest(new { error = "fetchIntervalMinutes must be >= 1" });
        }

        var source = await _sourcesService.CreateAsync(request, cancellationToken);
        if (source is null)
        {
            return Conflict(new { error = "source with this url already exists" });
        }

        return CreatedAtAction(nameof(GetById), new { id = source.Id }, source);
    }

    [Authorize(Roles = nameof(UserRole.ChiefEditor))]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SourceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSourceRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "name is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest(new { error = "url is required" });
        }

        if (request.FetchIntervalMinutes < 1)
        {
            return BadRequest(new { error = "fetchIntervalMinutes must be >= 1" });
        }

        var existing = await _sourcesService.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        var source = await _sourcesService.UpdateAsync(id, request, cancellationToken);
        if (source is null)
        {
            return Conflict(new { error = "source with this url already exists" });
        }

        return Ok(source);
    }

    [Authorize(Roles = nameof(UserRole.ChiefEditor))]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sourcesService.DeleteAsync(id, cancellationToken);

        return result switch
        {
            SourceDeleteResult.NotFound => NotFound(),
            SourceDeleteResult.HasNews => Conflict(new { error = "cannot delete source with existing news items" }),
            SourceDeleteResult.Deleted => NoContent(),
            _ => NotFound()
        };
    }
}
