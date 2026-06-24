using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class StoryCommandService : IStoryCommandService
{
    private readonly NewsCollectorDbContext _db;
    private readonly IStoryQueryService _storyQueryService;

    public StoryCommandService(NewsCollectorDbContext db, IStoryQueryService storyQueryService)
    {
        _db = db;
        _storyQueryService = storyQueryService;
    }

    public async Task<StoryDetailDto?> UpdateStatusAsync(
        Guid storyId,
        StoryStatus status,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var story = await _db.Stories.FirstOrDefaultAsync(item => item.Id == storyId, cancellationToken);
        if (story is null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        story.Status = status;
        story.StatusUpdatedByUserId = userId;
        story.StatusUpdatedAt = now;
        story.UpdatedAt = now;

        await _db.SaveChangesAsync(cancellationToken);

        return await _storyQueryService.GetByIdAsync(storyId, cancellationToken);
    }
}
