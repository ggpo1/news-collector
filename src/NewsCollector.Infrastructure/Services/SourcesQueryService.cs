using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class SourcesQueryService : ISourcesQueryService
{
    private readonly NewsCollectorDbContext _db;

    public SourcesQueryService(NewsCollectorDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SourceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Sources
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SourceDto(
                s.Id,
                s.Name,
                s.Type,
                s.Url,
                s.IsActive,
                s.FetchIntervalMinutes,
                s.LastFetchedAt,
                s.CreatedAt,
                s.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<SourceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Sources
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SourceDto(
                s.Id,
                s.Name,
                s.Type,
                s.Url,
                s.IsActive,
                s.FetchIntervalMinutes,
                s.LastFetchedAt,
                s.CreatedAt,
                s.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
