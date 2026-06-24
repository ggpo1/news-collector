using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class NewsEditorialService : INewsEditorialService
{
    private readonly NewsCollectorDbContext _db;
    private readonly INewsQueryService _newsQueryService;

    public NewsEditorialService(NewsCollectorDbContext db, INewsQueryService newsQueryService)
    {
        _db = db;
        _newsQueryService = newsQueryService;
    }

    public async Task<IReadOnlyList<EditorialTagDto>> GetTagsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.EditorialTags
            .AsNoTracking()
            .OrderBy(tag => tag.Name)
            .Select(tag => new EditorialTagDto(tag.Id, tag.Slug, tag.Name, tag.Color))
            .ToListAsync(cancellationToken);
    }

    public async Task<NewsItemDetailDto?> UpdateCategoryAsync(
        Guid newsId,
        Guid? categoryId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var news = await _db.NewsItems.FirstOrDefaultAsync(item => item.Id == newsId, cancellationToken);
        if (news is null)
        {
            return null;
        }

        if (categoryId.HasValue)
        {
            var categoryExists = await _db.Categories
                .AsNoTracking()
                .AnyAsync(category => category.Id == categoryId.Value && category.IsActive, cancellationToken);

            if (!categoryExists)
            {
                throw new InvalidOperationException("Category not found or inactive.");
            }
        }

        var now = DateTimeOffset.UtcNow;
        news.CategoryId = categoryId;
        news.IsCategoryManual = true;
        news.CategoryUpdatedAt = now;
        news.CategoryUpdatedByUserId = userId;

        await _db.SaveChangesAsync(cancellationToken);

        return await _newsQueryService.GetByIdAsync(newsId, cancellationToken);
    }

    public async Task<NewsItemDetailDto?> UpdateEditorialTagsAsync(
        Guid newsId,
        IReadOnlyList<Guid> tagIds,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var news = await _db.NewsItems.FirstOrDefaultAsync(item => item.Id == newsId, cancellationToken);
        if (news is null)
        {
            return null;
        }

        var uniqueTagIds = tagIds.Distinct().ToList();
        if (uniqueTagIds.Count > 0)
        {
            var existingCount = await _db.EditorialTags
                .AsNoTracking()
                .CountAsync(tag => uniqueTagIds.Contains(tag.Id), cancellationToken);

            if (existingCount != uniqueTagIds.Count)
            {
                throw new InvalidOperationException("One or more editorial tags were not found.");
            }
        }

        var existing = await _db.NewsEditorialTags
            .Where(item => item.NewsItemId == newsId)
            .ToListAsync(cancellationToken);

        _db.NewsEditorialTags.RemoveRange(existing);

        var now = DateTimeOffset.UtcNow;
        foreach (var tagId in uniqueTagIds)
        {
            _db.NewsEditorialTags.Add(new Domain.Entities.NewsEditorialTag
            {
                NewsItemId = newsId,
                EditorialTagId = tagId,
                AddedByUserId = userId,
                CreatedAt = now
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        return await _newsQueryService.GetByIdAsync(newsId, cancellationToken);
    }
}
