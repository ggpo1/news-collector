using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Models;
using NewsCollector.Domain.Entities;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class ApiVisitWriter : IApiVisitWriter
{
    private readonly NewsCollectorDbContext _db;

    public ApiVisitWriter(NewsCollectorDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(ApiVisitEntry entry, CancellationToken cancellationToken = default)
    {
        _db.ApiVisits.Add(new ApiVisit
        {
            Id = Guid.NewGuid(),
            RequestedAt = entry.RequestedAt,
            HttpMethod = entry.HttpMethod,
            Path = entry.Path,
            QueryString = entry.QueryString,
            StatusCode = entry.StatusCode,
            DurationMs = entry.DurationMs,
            VisitorFingerprint = entry.VisitorFingerprint,
            UserAgent = entry.UserAgent
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
