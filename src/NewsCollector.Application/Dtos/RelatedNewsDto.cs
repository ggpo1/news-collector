using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Dtos;

public sealed record RelatedNewsDto(
    Guid LinkId,
    LinkType LinkType,
    LinkMethod LinkMethod,
    decimal Confidence,
    NewsItemListDto News);
