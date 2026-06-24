using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/editorial-tags")]
public sealed class EditorialTagsController : ControllerBase
{
    private readonly INewsEditorialService _editorialService;

    public EditorialTagsController(INewsEditorialService editorialService)
    {
        _editorialService = editorialService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EditorialTagDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var tags = await _editorialService.GetTagsAsync(cancellationToken);
        return Ok(tags);
    }
}
