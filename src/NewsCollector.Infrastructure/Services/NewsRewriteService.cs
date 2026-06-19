using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Entities;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class NewsRewriteService : INewsRewriteService
{
    private readonly NewsCollectorDbContext _db;
    private readonly IUserContext _userContext;

    public NewsRewriteService(NewsCollectorDbContext db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<PagedResult<NewsRewriteDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? sourceNewsId = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyAccessFilter(_db.NewsRewrites.AsNoTracking());

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
        return await ApplyAccessFilter(_db.NewsRewrites.AsNoTracking())
            .Where(r => r.Id == id)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MutationResult<NewsRewriteDto>> CreateAsync(
        CreateNewsRewriteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_userContext.UserId is not Guid authorId)
        {
            return new MutationResult<NewsRewriteDto>(null, MutationStatus.Forbidden);
        }

        var sourceExists = await _db.NewsItems
            .AsNoTracking()
            .AnyAsync(n => n.Id == request.SourceNewsId, cancellationToken);

        if (!sourceExists)
        {
            return new MutationResult<NewsRewriteDto>(null, MutationStatus.NotFound);
        }

        var duplicateExists = await _db.NewsRewrites
            .AnyAsync(
                r => r.SourceNewsId == request.SourceNewsId && r.AuthorId == authorId,
                cancellationToken);

        if (duplicateExists)
        {
            return new MutationResult<NewsRewriteDto>(null, MutationStatus.Conflict);
        }

        var now = DateTimeOffset.UtcNow;
        var rewrite = new NewsRewrite
        {
            Id = Guid.NewGuid(),
            SourceNewsId = request.SourceNewsId,
            AuthorId = authorId,
            Title = request.Title.Trim(),
            Summary = NormalizeOptionalText(request.Summary),
            Content = NormalizeOptionalText(request.Content),
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.NewsRewrites.Add(rewrite);
        await _db.SaveChangesAsync(cancellationToken);

        var created = await GetByIdAsync(rewrite.Id, cancellationToken);
        return new MutationResult<NewsRewriteDto>(created, MutationStatus.Success);
    }

    public async Task<MutationResult<NewsRewriteDto>> UpdateAsync(
        Guid id,
        UpdateNewsRewriteRequest request,
        CancellationToken cancellationToken = default)
    {
        var rewrite = await _db.NewsRewrites
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rewrite is null)
        {
            return new MutationResult<NewsRewriteDto>(null, MutationStatus.NotFound);
        }

        if (!CanModify(rewrite.AuthorId))
        {
            return new MutationResult<NewsRewriteDto>(null, MutationStatus.Forbidden);
        }

        rewrite.Title = request.Title.Trim();
        rewrite.Summary = NormalizeOptionalText(request.Summary);
        rewrite.Content = NormalizeOptionalText(request.Content);
        rewrite.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        var updated = await GetByIdAsync(id, cancellationToken);
        return new MutationResult<NewsRewriteDto>(updated, MutationStatus.Success);
    }

    public async Task<MutationResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rewrite = await _db.NewsRewrites
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rewrite is null)
        {
            return new MutationResult<bool>(false, MutationStatus.NotFound);
        }

        if (!CanModify(rewrite.AuthorId))
        {
            return new MutationResult<bool>(false, MutationStatus.Forbidden);
        }

        _db.NewsRewrites.Remove(rewrite);
        await _db.SaveChangesAsync(cancellationToken);

        return new MutationResult<bool>(true, MutationStatus.Success);
    }

    private IQueryable<NewsRewrite> ApplyAccessFilter(IQueryable<NewsRewrite> query)
    {
        if (_userContext.IsChiefEditor)
        {
            return query;
        }

        if (_userContext.UserId is Guid userId)
        {
            return query.Where(r => r.AuthorId == userId);
        }

        return query.Where(_ => false);
    }

    private bool CanModify(Guid authorId)
    {
        if (_userContext.IsChiefEditor)
        {
            return true;
        }

        return _userContext.UserId == authorId;
    }

    private static System.Linq.Expressions.Expression<Func<NewsRewrite, NewsRewriteDto>> MapToDto() =>
        r => new NewsRewriteDto(
            r.Id,
            r.SourceNewsId,
            r.SourceNews.SourceId,
            r.SourceNews.Source.Name,
            r.SourceNews.Title,
            r.SourceNews.Url,
            r.SourceNews.PublishedAt,
            r.AuthorId,
            r.Author.Login,
            r.Author.DisplayName,
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
