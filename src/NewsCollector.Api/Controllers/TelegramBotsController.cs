using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/telegram/bots")]
public sealed class TelegramBotsController : ControllerBase
{
    private readonly ITelegramBotService _botService;

    public TelegramBotsController(ITelegramBotService botService)
    {
        _botService = botService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TelegramBotDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var bots = await _botService.GetAllAsync(cancellationToken);
        return Ok(bots);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TelegramBotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var bot = await _botService.GetByIdAsync(id, cancellationToken);
        return bot is null ? NotFound() : Ok(bot);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.ChiefEditor))]
    [ProducesResponseType(typeof(TelegramBotDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTelegramBotRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.BotToken))
        {
            return BadRequest(new { error = "Name and BotToken are required" });
        }

        var bot = await _botService.CreateAsync(request, cancellationToken);
        return bot is null
            ? BadRequest(new { error = "Failed to create bot" })
            : CreatedAtAction(nameof(GetById), new { id = bot.Id }, bot);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.ChiefEditor))]
    [ProducesResponseType(typeof(TelegramBotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTelegramBotRequest request,
        CancellationToken cancellationToken)
    {
        var bot = await _botService.UpdateAsync(id, request, cancellationToken);
        return bot is null ? NotFound() : Ok(bot);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.ChiefEditor))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _botService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/restart")]
    [Authorize(Roles = nameof(UserRole.ChiefEditor))]
    [ProducesResponseType(typeof(TelegramBotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restart(Guid id, CancellationToken cancellationToken)
    {
        var bot = await _botService.RestartContainerAsync(id, cancellationToken);
        return bot is null ? NotFound() : Ok(bot);
    }
}
