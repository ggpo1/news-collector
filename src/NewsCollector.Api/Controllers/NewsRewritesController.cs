using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/news-rewrites")]
public sealed class NewsRewritesController : ControllerBase
{
    private const int MaxPageSize = 100;

    private readonly INewsRewriteService _newsRewriteService;

    public NewsRewritesController(INewsRewriteService newsRewriteService)
    {
        _newsRewriteService = newsRewriteService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NewsRewriteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? sourceNewsId = null,
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

        var result = await _newsRewriteService.GetPagedAsync(page, pageSize, sourceNewsId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NewsRewriteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var rewrite = await _newsRewriteService.GetByIdAsync(id, cancellationToken);
        return rewrite is null ? NotFound() : Ok(rewrite);
    }

    [HttpPost]
    [ProducesResponseType(typeof(NewsRewriteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateNewsRewriteRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { error = "title is required" });
        }

        var result = await _newsRewriteService.CreateAsync(request, cancellationToken);

        return result.Status switch
        {
            MutationStatus.Success => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value),
            MutationStatus.NotFound => NotFound(new { error = "source news not found" }),
            MutationStatus.Conflict => Conflict(new { error = "you already have a rewrite for this news item" }),
            MutationStatus.Forbidden => Forbid(),
            _ => BadRequest()
        };
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(NewsRewriteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateNewsRewriteRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { error = "title is required" });
        }

        var result = await _newsRewriteService.UpdateAsync(id, request, cancellationToken);

        return result.Status switch
        {
            MutationStatus.Success => Ok(result.Value),
            MutationStatus.NotFound => NotFound(),
            MutationStatus.Forbidden => Forbid(),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _newsRewriteService.DeleteAsync(id, cancellationToken);

        return result.Status switch
        {
            MutationStatus.Success => NoContent(),
            MutationStatus.NotFound => NotFound(),
            MutationStatus.Forbidden => Forbid(),
            _ => BadRequest()
        };
    }
}
