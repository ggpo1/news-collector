using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.ChiefEditor))]
[Route("api/invitation-codes")]
public sealed class InvitationCodesController : ControllerBase
{
    private readonly IInvitationCodeService _invitationCodeService;
    private readonly IUserContext _userContext;

    public InvitationCodesController(
        IInvitationCodeService invitationCodeService,
        IUserContext userContext)
    {
        _invitationCodeService = invitationCodeService;
        _userContext = userContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InvitationCodeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var codes = await _invitationCodeService.GetAllAsync(cancellationToken);
        return Ok(codes);
    }

    [HttpPost]
    [ProducesResponseType(typeof(InvitationCodeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateInvitationCodeRequest request,
        CancellationToken cancellationToken)
    {
        if (_userContext.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var code = await _invitationCodeService.CreateAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { code = code!.Code }, code);
    }
}
