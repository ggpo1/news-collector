using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Application.Enums;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/news")]
public sealed class NewsController : ControllerBase
{
    private const int MaxPageSize = 100;

    private readonly INewsQueryService _newsQueryService;
    private readonly IArticleContentEnrichmentService _contentEnrichmentService;
    private readonly IAiNewsRewriteService _aiNewsRewriteService;
    private readonly ISecondDayAngleService _secondDayAngleService;

    public NewsController(
        INewsQueryService newsQueryService,
        IArticleContentEnrichmentService contentEnrichmentService,
        IAiNewsRewriteService aiNewsRewriteService,
        ISecondDayAngleService secondDayAngleService)
    {
        _newsQueryService = newsQueryService;
        _contentEnrichmentService = contentEnrichmentService;
        _aiNewsRewriteService = aiNewsRewriteService;
        _secondDayAngleService = secondDayAngleService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NewsItemListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? sourceId = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? uncategorized = null,
        [FromQuery] bool? hasContent = null,
        [FromQuery] string? toneFilter = null,
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

        if (categoryId.HasValue && uncategorized == true)
        {
            return BadRequest(new { error = "Use either categoryId or uncategorized, not both" });
        }

        NewsToneFilter? parsedToneFilter = null;
        if (!string.IsNullOrWhiteSpace(toneFilter))
        {
            if (!Enum.TryParse<NewsToneFilter>(toneFilter, ignoreCase: true, out var parsed))
            {
                return BadRequest(new
                {
                    error = "Invalid toneFilter. Allowed: positive, negative, neutral, strong, unanalyzed"
                });
            }

            parsedToneFilter = parsed;
        }

        var result = await _newsQueryService.GetPagedAsync(
            page,
            pageSize,
            sourceId,
            categoryId,
            uncategorized,
            hasContent,
            parsedToneFilter,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NewsItemDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var news = await _newsQueryService.GetByIdAsync(id, cancellationToken);
        return news is null ? NotFound() : Ok(news);
    }

    [HttpPost("{id:guid}/enrich-content")]
    [ProducesResponseType(typeof(ArticleEnrichmentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnrichContent(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contentEnrichmentService.EnrichArticleAsync(id, cancellationToken);

        if (result.Status == ArticleEnrichmentResult.StatusNotFound)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost("{id:guid}/ai-rewrite")]
    [RequestTimeout("AiRewrite")]
    [ProducesResponseType(typeof(AiNewsRewriteResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AiRewrite(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiNewsRewriteService.RewriteAsync(id, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/second-day-angles")]
    [RequestTimeout("AiRewrite")]
    [ProducesResponseType(typeof(SecondDayAnglesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateSecondDayAngles(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _secondDayAngleService.GenerateAsync(id, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}/related")]
    [ProducesResponseType(typeof(IReadOnlyList<RelatedNewsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRelated(Guid id, CancellationToken cancellationToken)
    {
        var news = await _newsQueryService.GetByIdAsync(id, cancellationToken);
        if (news is null)
        {
            return NotFound();
        }

        var related = await _newsQueryService.GetRelatedAsync(id, cancellationToken);
        return Ok(related);
    }
}
