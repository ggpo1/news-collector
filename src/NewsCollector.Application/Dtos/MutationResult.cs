namespace NewsCollector.Application.Dtos;

public enum MutationStatus
{
    Success,
    NotFound,
    Forbidden,
    Conflict
}

public sealed record MutationResult<T>(T? Value, MutationStatus Status);
