namespace NewsCollector.Application.Dtos;

public sealed record CategoryDto(
    Guid Id,
    string Slug,
    string Name,
    int SortOrder);
