using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Route("api/news")]
public sealed class NewsController : ControllerBase
{
    private const int MaxPageSize = 100;

    private readonly INewsQueryService _newsQueryService;
    private readonly IArticleContentEnrichmentService _contentEnrichmentService;
    private readonly IAiNewsRewriteService _aiNewsRewriteService;

    public NewsController(
        INewsQueryService newsQueryService,
        IArticleContentEnrichmentService contentEnrichmentService,
        IAiNewsRewriteService aiNewsRewriteService)
    {
        _newsQueryService = newsQueryService;
        _contentEnrichmentService = contentEnrichmentService;
        _aiNewsRewriteService = aiNewsRewriteService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NewsItemListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? sourceId = null,
        [FromQuery] bool? hasContent = null,
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

        var result = await _newsQueryService.GetPagedAsync(
            page,
            pageSize,
            sourceId,
            hasContent,
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
