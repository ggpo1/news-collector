namespace NewsCollector.Application.Dtos;

public sealed record SecondDayAnglesDto(
    Guid NewsId,
    string NewsTitle,
    IReadOnlyList<HistoricalParallelDto> HistoricalParallels,
    IReadOnlyList<StakeholderAngleDto> Stakeholders,
    IReadOnlyList<NumberContradictionDto> NumberContradictions,
    IReadOnlyList<SuggestedAngleDto> SuggestedAngles);

public sealed record HistoricalParallelDto(
    Guid NewsId,
    string Title,
    string SourceName,
    DateTimeOffset? PublishedAt,
    string ParallelSummary,
    string StructuralSimilarity);

public sealed record StakeholderAngleDto(
    string Name,
    string EntityType,
    string Role,
    string Reason);

public sealed record NumberContradictionDto(
    string Metric,
    IReadOnlyList<SourceValueDto> Values,
    string FactCheckAngle);

public sealed record SourceValueDto(
    string SourceName,
    string Value);

public sealed record SuggestedAngleDto(
    string Title,
    string Rationale,
    string AngleType);
