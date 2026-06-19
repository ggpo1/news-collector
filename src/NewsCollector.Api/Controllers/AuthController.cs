using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IInvitationCodeService _invitationCodeService;
    private readonly IUserContext _userContext;

    public AuthController(
        IAuthService authService,
        IInvitationCodeService invitationCodeService,
        IUserContext userContext)
    {
        _authService = authService;
        _invitationCodeService = invitationCodeService;
        _userContext = userContext;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "login and password are required" });
        }

        var result = await _authService.LoginAsync(request, cancellationToken);
        return result is null ? Unauthorized(new { error = "invalid login or password" }) : Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("validate-invitation")]
    [ProducesResponseType(typeof(ValidateInvitationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateInvitation(
        [FromBody] ValidateInvitationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new { error = "code is required" });
        }

        var result = await _invitationCodeService.ValidateAsync(request.Code, cancellationToken);
        return result is null
            ? NotFound(new { error = "invitation code is invalid or already used" })
            : Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.InvitationCode)
            || string.IsNullOrWhiteSpace(request.Login)
            || string.IsNullOrWhiteSpace(request.Password)
            || string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return BadRequest(new { error = "invitationCode, login, password and displayName are required" });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { error = "password must be at least 6 characters" });
        }

        var invitation = await _invitationCodeService.ValidateAsync(request.InvitationCode, cancellationToken);
        if (invitation is null)
        {
            return NotFound(new { error = "invitation code is invalid or already used" });
        }

        var result = await _authService.RegisterAsync(request, cancellationToken);

        return result.Status switch
        {
            RegisterStatus.Success => CreatedAtAction(nameof(GetCurrentUser), result.Response),
            RegisterStatus.LoginExists => Conflict(new { error = "user with this login already exists" }),
            RegisterStatus.InvalidInvitation => NotFound(new { error = "invitation code is invalid or already used" }),
            _ => BadRequest(new { error = "invalid registration data" })
        };
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (_userContext.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var user = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        return user is null ? Unauthorized() : Ok(user);
    }
}
