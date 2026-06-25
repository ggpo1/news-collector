using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Application.Options;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class EditorialBriefService : IEditorialBriefService
{
    private readonly NewsCollectorDbContext _db;
    private readonly EditorialBriefContextBuilder _contextBuilder;
    private readonly EditorialBriefGenerator _generator;
    private readonly EditorialBriefOptions _options;
    private readonly OllamaOptions _ollamaOptions;
    private readonly ILogger<EditorialBriefService> _logger;

    public EditorialBriefService(
        NewsCollectorDbContext db,
        EditorialBriefContextBuilder contextBuilder,
        EditorialBriefGenerator generator,
        IOptions<EditorialBriefOptions> options,
        IOptions<OllamaOptions> ollamaOptions,
        ILogger<EditorialBriefService> logger)
    {
        _db = db;
        _contextBuilder = contextBuilder;
        _generator = generator;
        _options = options.Value;
        _ollamaOptions = ollamaOptions.Value;
        _logger = logger;
    }

    public async Task<EditorialBriefReportDto?> GetLatestAsync(
        EditorialBriefPeriod? period = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.EditorialBriefReports.AsNoTracking();

        if (period.HasValue)
        {
            query = query.Where(report => report.Period == period.Value);
        }

        var report = await query
            .OrderByDescending(item => item.GeneratedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return report is null ? null : Map(report);
    }

    public async Task<IReadOnlyList<EditorialBriefHistoryItemDto>> GetHistoryAsync(
        int limit = 14,
        CancellationToken cancellationToken = default)
    {
        var safeLimit = Math.Clamp(limit, 1, 50);

        return await _db.EditorialBriefReports
            .AsNoTracking()
            .OrderByDescending(report => report.GeneratedAt)
            .Take(safeLimit)
            .Select(report => new EditorialBriefHistoryItemDto(
                report.Id,
                report.Period.ToString(),
                report.GeneratedAt,
                report.WindowStart,
                report.WindowEnd))
            .ToListAsync(cancellationToken);
    }

    public async Task<EditorialBriefReportDto?> GenerateAsync(
        EditorialBriefPeriod period,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var context = await _contextBuilder.BuildAsync(period, now, cancellationToken);
        var markdown = await _generator.GenerateMarkdownAsync(context, cancellationToken);

        var report = new EditorialBriefReport
        {
            Id = Guid.NewGuid(),
            Period = period,
            Markdown = markdown,
            GeneratedAt = now,
            WindowStart = context.WindowStart,
            WindowEnd = context.WindowEnd,
            Model = _ollamaOptions.Model
        };

        _db.EditorialBriefReports.Add(report);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Saved editorial brief {BriefId} ({Period}) for window {WindowStart} — {WindowEnd}",
            report.Id,
            report.Period,
            report.WindowStart,
            report.WindowEnd);

        return Map(report);
    }

    public async Task<int> TryGenerateScheduledAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        if (!TryResolveScheduledPeriod(now, out var period))
        {
            return 0;
        }

        if (await BriefAlreadyGeneratedTodayAsync(period, now, cancellationToken))
        {
            _logger.LogDebug("Editorial brief for {Period} already generated today", period);
            return 0;
        }

        await GenerateAsync(period, cancellationToken);
        return 1;
    }

    private bool TryResolveScheduledPeriod(DateTimeOffset now, out EditorialBriefPeriod period)
    {
        var hour = now.UtcDateTime.Hour;
        var schedule = _options.ScheduleHoursUtc;

        if (schedule.Length >= 1 && hour == schedule[0])
        {
            period = EditorialBriefPeriod.Morning;
            return true;
        }

        if (schedule.Length >= 2 && hour == schedule[1])
        {
            period = EditorialBriefPeriod.Evening;
            return true;
        }

        period = default;
        return false;
    }

    private async Task<bool> BriefAlreadyGeneratedTodayAsync(
        EditorialBriefPeriod period,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var dayStart = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);

        return await _db.EditorialBriefReports
            .AsNoTracking()
            .AnyAsync(
                report => report.Period == period && report.GeneratedAt >= dayStart,
                cancellationToken);
    }

    private static EditorialBriefReportDto Map(EditorialBriefReport report) =>
        new(
            report.Id,
            report.Period.ToString(),
            report.Markdown,
            report.GeneratedAt,
            report.WindowStart,
            report.WindowEnd,
            report.Model);
}
