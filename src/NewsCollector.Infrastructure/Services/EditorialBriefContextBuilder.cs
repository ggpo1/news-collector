using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Application.Options;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Linking;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class EditorialBriefContextBuilder
{
    private readonly NewsCollectorDbContext _db;
    private readonly IEditorialDashboardService _dashboardService;
    private readonly EditorialBriefOptions _options;

    public EditorialBriefContextBuilder(
        NewsCollectorDbContext db,
        IEditorialDashboardService dashboardService,
        IOptions<EditorialBriefOptions> options)
    {
        _db = db;
        _dashboardService = dashboardService;
        _options = options.Value;
    }

    public async Task<EditorialBriefBuildContext> BuildAsync(
        EditorialBriefPeriod period,
        DateTimeOffset windowEnd,
        CancellationToken cancellationToken)
    {
        var windowStart = windowEnd.AddHours(-_options.WindowHours);
        var dashboard = await _dashboardService.GetDashboardAsync(_options.WindowHours, cancellationToken);

        var previousBrief = await _db.EditorialBriefReports
            .AsNoTracking()
            .OrderByDescending(report => report.GeneratedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var stories = await _db.Stories
            .AsNoTracking()
            .Where(story => story.LastActivityAt >= windowStart && story.LastActivityAt <= windowEnd)
            .OrderByDescending(story => story.SourceCount)
            .ThenByDescending(story => story.LastActivityAt)
            .Take(_options.MaxTopics)
            .ToListAsync(cancellationToken);

        var telegramSent = await (
            from delivery in _db.TelegramDeliveries.AsNoTracking()
            join channel in _db.TelegramChannels.AsNoTracking() on delivery.TelegramChannelId equals channel.Id
            where delivery.Status == TelegramDeliveryStatus.Sent
                  && delivery.SentAt >= windowStart
                  && delivery.SentAt <= windowEnd
            orderby delivery.SentAt descending
            select new TelegramSentItem(
                channel.Name,
                delivery.NewsItemId,
                delivery.NewsRewriteId,
                delivery.SentAt!.Value,
                delivery.MessageText))
            .Take(_options.MaxTelegramItems)
            .ToListAsync(cancellationToken);

        var newsTitles = await LoadNewsTitlesAsync(telegramSent, cancellationToken);

        var coverageGaps = BuildCoverageGaps(dashboard.DevelopingTopics);

        var newSincePrevious = previousBrief is null
            ? []
            : await LoadNewArticlesSinceAsync(previousBrief.GeneratedAt, windowEnd, cancellationToken);

        return new EditorialBriefBuildContext(
            period,
            windowStart,
            windowEnd,
            dashboard,
            stories,
            telegramSent,
            newsTitles,
            coverageGaps,
            newSincePrevious,
            previousBrief?.GeneratedAt);
    }

    private IReadOnlyList<CoverageGapItem> BuildCoverageGaps(IReadOnlyList<DevelopingTopicDto> topics)
    {
        var gaps = new List<CoverageGapItem>();

        foreach (var topic in topics.Take(_options.MaxTopics))
        {
            var sources = topic.SourceNames
                .Select(name => (SourceName: name, SourceUrl: name))
                .ToList();

            if (SourceRegionClassifier.HasOnlyRegion(sources, SourceRegion.Western, _options))
            {
                gaps.Add(new CoverageGapItem(
                    topic.Headline,
                    topic.SourceCount,
                    topic.ArticleCount,
                    topic.SourceNames,
                    "WesternOnly",
                    "Тема есть в западных источниках, RU-покрытия нет"));
            }
            else if (SourceRegionClassifier.HasOnlyRegion(sources, SourceRegion.Ru, _options))
            {
                gaps.Add(new CoverageGapItem(
                    topic.Headline,
                    topic.SourceCount,
                    topic.ArticleCount,
                    topic.SourceNames,
                    "RuOnly",
                    "Тема есть в RU-источниках, западного покрытия нет"));
            }
        }

        return gaps;
    }

    private async Task<IReadOnlyDictionary<Guid, string>> LoadNewsTitlesAsync(
        IReadOnlyList<TelegramSentItem> deliveries,
        CancellationToken cancellationToken)
    {
        var newsIds = deliveries
            .Where(item => item.NewsItemId.HasValue)
            .Select(item => item.NewsItemId!.Value)
            .Distinct()
            .ToList();

        if (newsIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        return await _db.NewsItems
            .AsNoTracking()
            .Where(news => newsIds.Contains(news.Id))
            .ToDictionaryAsync(news => news.Id, news => news.Title, cancellationToken);
    }

    private async Task<IReadOnlyList<NewArticleItem>> LoadNewArticlesSinceAsync(
        DateTimeOffset since,
        DateTimeOffset until,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from news in _db.NewsItems.AsNoTracking()
            join source in _db.Sources.AsNoTracking() on news.SourceId equals source.Id
            where (news.PublishedAt ?? news.FetchedAt) > since
                  && (news.PublishedAt ?? news.FetchedAt) <= until
            orderby news.PublishedAt ?? news.FetchedAt descending
            select new NewArticleItem(
                news.Title,
                source.Name,
                news.PublishedAt ?? news.FetchedAt))
            .Take(40)
            .ToListAsync(cancellationToken);

        return rows;
    }
}

public sealed record EditorialBriefBuildContext(
    EditorialBriefPeriod Period,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    EditorialDashboardDto Dashboard,
    IReadOnlyList<Domain.Entities.Story> ActiveStories,
    IReadOnlyList<TelegramSentItem> TelegramSent,
    IReadOnlyDictionary<Guid, string> NewsTitles,
    IReadOnlyList<CoverageGapItem> CoverageGaps,
    IReadOnlyList<NewArticleItem> NewSincePrevious,
    DateTimeOffset? PreviousBriefAt);

public sealed record TelegramSentItem(
    string ChannelName,
    Guid? NewsItemId,
    Guid? NewsRewriteId,
    DateTimeOffset SentAt,
    string MessageText);

public sealed record CoverageGapItem(
    string Headline,
    int SourceCount,
    int ArticleCount,
    IReadOnlyList<string> SourceNames,
    string GapType,
    string Description);

public sealed record NewArticleItem(
    string Title,
    string SourceName,
    DateTimeOffset PublishedAt);
