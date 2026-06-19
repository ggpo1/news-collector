using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Application.Models;
using NewsCollector.Domain.Entities;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class SourcesService : ISourcesService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly NewsCollectorDbContext _db;

    public SourcesService(NewsCollectorDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SourceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sources = await _db.Sources
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return sources.Select(MapSource).ToList();
    }

    public async Task<SourceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var source = await _db.Sources
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        return source is null ? null : MapSource(source);
    }

    public async Task<SourceDto?> CreateAsync(
        CreateSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        var url = request.Url.Trim();

        if (await _db.Sources.AnyAsync(s => s.Url == url, cancellationToken))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var source = new Source
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = request.Type,
            Url = url,
            IsActive = request.IsActive,
            FetchIntervalMinutes = NormalizeFetchInterval(request.FetchIntervalMinutes),
            Config = BuildConfigJson(request.ContentFetchEnabled, request.ContentSelector),
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Sources.Add(source);
        await _db.SaveChangesAsync(cancellationToken);

        return MapSource(source);
    }

    public async Task<SourceDto?> UpdateAsync(
        Guid id,
        UpdateSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        var source = await _db.Sources
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (source is null)
        {
            return null;
        }

        var name = request.Name.Trim();
        var url = request.Url.Trim();

        if (await _db.Sources.AnyAsync(s => s.Url == url && s.Id != id, cancellationToken))
        {
            return null;
        }

        source.Name = name;
        source.Type = request.Type;
        source.Url = url;
        source.IsActive = request.IsActive;
        source.FetchIntervalMinutes = NormalizeFetchInterval(request.FetchIntervalMinutes);
        source.Config = BuildConfigJson(request.ContentFetchEnabled, request.ContentSelector);
        source.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return MapSource(source);
    }

    public async Task<SourceDeleteResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var source = await _db.Sources
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (source is null)
        {
            return SourceDeleteResult.NotFound;
        }

        var hasNews = await _db.NewsItems
            .AnyAsync(n => n.SourceId == id, cancellationToken);

        if (hasNews)
        {
            return SourceDeleteResult.HasNews;
        }

        _db.Sources.Remove(source);
        await _db.SaveChangesAsync(cancellationToken);

        return SourceDeleteResult.Deleted;
    }

    private static SourceDto MapSource(Source source)
    {
        var config = ParseConfig(source.Config);

        return new SourceDto(
            source.Id,
            source.Name,
            source.Type,
            source.Url,
            source.IsActive,
            source.FetchIntervalMinutes,
            source.LastFetchedAt,
            config.ContentFetchEnabled,
            config.ContentSelector,
            source.CreatedAt,
            source.UpdatedAt);
    }

    private static SourceScrapingConfig ParseConfig(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return new SourceScrapingConfig();
        }

        try
        {
            return JsonSerializer.Deserialize<SourceScrapingConfig>(configJson, JsonOptions)
                ?? new SourceScrapingConfig();
        }
        catch (JsonException)
        {
            return new SourceScrapingConfig();
        }
    }

    private static int NormalizeFetchInterval(int minutes) =>
        minutes < 1 ? 1 : minutes;

    private static string? BuildConfigJson(bool contentFetchEnabled, string? contentSelector)
    {
        var config = new SourceScrapingConfig
        {
            ContentFetchEnabled = contentFetchEnabled,
            ContentSelector = NormalizeOptionalText(contentSelector)
        };

        return JsonSerializer.Serialize(config, JsonOptions);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
