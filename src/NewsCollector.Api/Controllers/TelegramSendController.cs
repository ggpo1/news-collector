using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Telegram;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/telegram")]
public sealed class TelegramSendController : ControllerBase
{
    private readonly ITelegramDeliveryService _deliveryService;
    private readonly TelegramProxyDiagnosticsService _proxyDiagnostics;

    public TelegramSendController(
        ITelegramDeliveryService deliveryService,
        TelegramProxyDiagnosticsService proxyDiagnostics)
    {
        _deliveryService = deliveryService;
        _proxyDiagnostics = proxyDiagnostics;
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

    [HttpGet("proxy-diagnostics")]
    [Authorize(Roles = nameof(UserRole.ChiefEditor))]
    [ProducesResponseType(typeof(TelegramProxyDiagnosticsResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProxyDiagnostics(CancellationToken cancellationToken)
    {
        var result = await _proxyDiagnostics.RunAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("deliveries/{deliveryId:guid}")]
    [ProducesResponseType(typeof(TelegramDeliveryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDelivery(Guid deliveryId, CancellationToken cancellationToken)
    {
        var delivery = await _deliveryService.GetDeliveryAsync(deliveryId, cancellationToken);
        return delivery is null ? NotFound() : Ok(delivery);
    }
}
