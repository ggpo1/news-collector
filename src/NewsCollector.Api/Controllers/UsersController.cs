using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.ChiefEditor))]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserContext _userContext;

    public UsersController(IUserService userService, IUserContext userContext)
    {
        _userService = userService;
        _userContext = userContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Login)
            || string.IsNullOrWhiteSpace(request.Password)
            || string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return BadRequest(new { error = "login, password and displayName are required" });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { error = "password must be at least 6 characters" });
        }

        var user = await _userService.CreateAsync(request, cancellationToken);
        return user is null
            ? Conflict(new { error = "user with this login already exists" })
            : CreatedAtAction(nameof(GetAll), new { id = user.Id }, user);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return BadRequest(new { error = "displayName is required" });
        }

        if (!string.IsNullOrWhiteSpace(request.Password) && request.Password.Length < 6)
        {
            return BadRequest(new { error = "password must be at least 6 characters" });
        }

        var user = await _userService.UpdateAsync(id, request, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (_userContext.UserId is not Guid currentUserId)
        {
            return Unauthorized();
        }

        var result = await _userService.DeleteAsync(id, currentUserId, cancellationToken);

        return result switch
        {
            UserDeleteResult.NotFound => NotFound(),
            UserDeleteResult.CannotDeleteSelf => Conflict(new { error = "cannot delete your own account" }),
            UserDeleteResult.HasRewrites => Conflict(new { error = "cannot delete user with existing rewrites" }),
            UserDeleteResult.Deleted => NoContent(),
            _ => NotFound()
        };
    }
}
