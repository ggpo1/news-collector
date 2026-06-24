using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/stories")]
public sealed class StoriesController : ControllerBase
{
    private const int MaxPageSize = 100;

    private readonly IStoryQueryService _storyQueryService;
    private readonly IStoryCommandService _storyCommandService;
    private readonly IStorySyncService _storySyncService;

    public StoriesController(
        IStoryQueryService storyQueryService,
        IStoryCommandService storyCommandService,
        IStorySyncService storySyncService)
    {
        _storyQueryService = storyQueryService;
        _storyCommandService = storyCommandService;
        _storySyncService = storySyncService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<StoryListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] StoryStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > MaxPageSize)
        {
            return BadRequest(new { error = "Invalid pagination parameters." });
        }

        var result = await _storyQueryService.GetPagedAsync(page, pageSize, status, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StoryDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var story = await _storyQueryService.GetByIdAsync(id, cancellationToken);
        return story is null ? NotFound() : Ok(story);
    }

    [HttpGet("by-cluster/{clusterKey}")]
    [ProducesResponseType(typeof(StoryDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByClusterKey(string clusterKey, CancellationToken cancellationToken)
    {
        var story = await _storyQueryService.GetByClusterKeyAsync(clusterKey, cancellationToken);
        return story is null ? NotFound() : Ok(story);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(StoryDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateStoryStatusRequest request,
        [FromServices] IUserContext userContext,
        CancellationToken cancellationToken)
    {
        if (userContext.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var story = await _storyCommandService.UpdateStatusAsync(id, request.Status, userId, cancellationToken);
        return story is null ? NotFound() : Ok(story);
    }

    [HttpPost("sync")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sync(CancellationToken cancellationToken)
    {
        var synced = await _storySyncService.SyncFromLinksAsync(cancellationToken);
        return Ok(new { synced });
    }
}
