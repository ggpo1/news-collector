using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/telegram/channels")]
public sealed class TelegramChannelsController : ControllerBase
{
    private readonly ITelegramChannelService _channelService;

    public TelegramChannelsController(ITelegramChannelService channelService)
    {
        _channelService = channelService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TelegramChannelDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? sourceId = null,
        [FromQuery] Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var channels = sourceId.HasValue || categoryId.HasValue
            ? await _channelService.GetForNewsAsync(sourceId, categoryId, cancellationToken)
            : await _channelService.GetAllAsync(cancellationToken);

        return Ok(channels);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TelegramChannelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var channel = await _channelService.GetByIdAsync(id, cancellationToken);
        return channel is null ? NotFound() : Ok(channel);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.ChiefEditor))]
    [ProducesResponseType(typeof(TelegramChannelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTelegramChannelRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.ChatId))
        {
            return BadRequest(new { error = "Name and ChatId are required" });
        }

        try
        {
            var channel = await _channelService.CreateAsync(request, cancellationToken);
            return channel is null
                ? BadRequest(new { error = "Failed to create channel. Check bot id and chat id uniqueness." })
                : CreatedAtAction(nameof(GetById), new { id = channel.Id }, channel);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.ChiefEditor))]
    [ProducesResponseType(typeof(TelegramChannelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTelegramChannelRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var channel = await _channelService.UpdateAsync(id, request, cancellationToken);
            return channel is null ? NotFound() : Ok(channel);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.ChiefEditor))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _channelService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
