using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Abstractions;

public interface IInvitationCodeService
{
    Task<IReadOnlyList<InvitationCodeDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<InvitationCodeDto?> CreateAsync(
        CreateInvitationCodeRequest request,
        Guid createdByUserId,
        CancellationToken cancellationToken = default);

    Task<ValidateInvitationResponse?> ValidateAsync(
        string code,
        CancellationToken cancellationToken = default);
}
