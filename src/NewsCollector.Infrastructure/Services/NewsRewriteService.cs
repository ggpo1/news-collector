using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Entities;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class NewsRewriteService : INewsRewriteService
{
    private readonly NewsCollectorDbContext _db;

    public NewsRewriteService(NewsCollectorDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<NewsRewriteDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? sourceNewsId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.NewsRewrites
            .AsNoTracking()
            .AsQueryable();

        if (sourceNewsId.HasValue)
        {
            query = query.Where(r => r.SourceNewsId == sourceNewsId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        return new PagedResult<NewsRewriteDto>(items, page, pageSize, totalCount);
    }

    public async Task<NewsRewriteDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.NewsRewrites
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<NewsRewriteDto?> CreateAsync(
        CreateNewsRewriteRequest request,
        CancellationToken cancellationToken = default)
    {
        var sourceExists = await _db.NewsItems
            .AsNoTracking()
            .AnyAsync(n => n.Id == request.SourceNewsId, cancellationToken);

        if (!sourceExists)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var rewrite = new NewsRewrite
        {
            Id = Guid.NewGuid(),
            SourceNewsId = request.SourceNewsId,
            Title = request.Title.Trim(),
            Summary = NormalizeOptionalText(request.Summary),
            Content = NormalizeOptionalText(request.Content),
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.NewsRewrites.Add(rewrite);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(rewrite.Id, cancellationToken);
    }

    public async Task<NewsRewriteDto?> UpdateAsync(
        Guid id,
        UpdateNewsRewriteRequest request,
        CancellationToken cancellationToken = default)
    {
        var rewrite = await _db.NewsRewrites
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rewrite is null)
        {
            return null;
        }

        rewrite.Title = request.Title.Trim();
        rewrite.Summary = NormalizeOptionalText(request.Summary);
        rewrite.Content = NormalizeOptionalText(request.Content);
        rewrite.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rewrite = await _db.NewsRewrites
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rewrite is null)
        {
            return false;
        }

        _db.NewsRewrites.Remove(rewrite);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static System.Linq.Expressions.Expression<Func<NewsRewrite, NewsRewriteDto>> MapToDto() =>
        r => new NewsRewriteDto(
            r.Id,
            r.SourceNewsId,
            r.SourceNews.Title,
            r.Title,
            r.Summary,
            r.Content,
            r.CreatedAt,
            r.UpdatedAt);

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
