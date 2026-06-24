using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/telegram")]
public sealed class TelegramSendController : ControllerBase
{
    private readonly ITelegramDeliveryService _deliveryService;

    public TelegramSendController(ITelegramDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    [HttpPost("news/{newsId:guid}/send")]
    [ProducesResponseType(typeof(TelegramSendResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendNews(
        Guid newsId,
        [FromBody] TelegramSendRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _deliveryService.QueueNewsAsync(newsId, request.ChannelId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("rewrites/{rewriteId:guid}/send")]
    [ProducesResponseType(typeof(TelegramSendResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendRewrite(
        Guid rewriteId,
        [FromBody] TelegramSendRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _deliveryService.QueueRewriteAsync(rewriteId, request.ChannelId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
