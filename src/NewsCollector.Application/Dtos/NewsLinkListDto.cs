using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Dtos;

public sealed record NewsLinkListDto(
    Guid Id,
    LinkType LinkType,
    LinkMethod LinkMethod,
    decimal Confidence,
    DateTimeOffset CreatedAt,
    NewsItemListDto NewsLow,
    NewsItemListDto NewsHigh);
