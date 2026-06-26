using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Persistence;
using Npgsql;
using NpgsqlTypes;

namespace NewsCollector.Infrastructure.Services;

public sealed class SearchQueryService : ISearchQueryService
{
    private readonly NewsCollectorDbContext _db;
    private readonly ILogger<SearchQueryService> _logger;

    public SearchQueryService(NewsCollectorDbContext db, ILogger<SearchQueryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SearchResultDto>> SearchAsync(
        string query,
        IReadOnlyList<SearchDocumentType>? types = null,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var trimmed = query.Trim();
        if (trimmed.Length < 2)
        {
            return [];
        }

        limit = Math.Clamp(limit, 1, 50);
        var typeFilter = types is { Count: > 0 }
            ? types.Distinct().ToArray()
            : Enum.GetValues<SearchDocumentType>();

        var connection = _db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            WITH queries AS (
                SELECT
                    plainto_tsquery('russian', @query) AS q_ru,
                    plainto_tsquery('english', @query) AS q_en
            )
            SELECT
                d."DocumentType",
                d."EntityId",
                d."Title",
                d."SourceName",
                d."PublishedAt",
                GREATEST(
                    CASE WHEN d."Language" = 'Ru' THEN ts_rank(d."SearchVector", q.q_ru) ELSE 0 END,
                    CASE WHEN d."Language" = 'En' THEN ts_rank(d."SearchVector", q.q_en) ELSE 0 END
                ) AS score,
                CASE
                    WHEN d."Language" = 'Ru' THEN ts_headline(
                        'russian',
                        coalesce(d."Body", d."Title"),
                        q.q_ru,
                        'MaxWords=35, MinWords=8, StartSel=<b>, StopSel=</b>'
                    )
                    ELSE ts_headline(
                        'english',
                        coalesce(d."Body", d."Title"),
                        q.q_en,
                        'MaxWords=35, MinWords=8, StartSel=<b>, StopSel=</b>'
                    )
                END AS snippet
            FROM search_documents d
            CROSS JOIN queries q
            WHERE d."DocumentType" = ANY(@types)
              AND (
                    (d."Language" = 'Ru' AND d."SearchVector" @@ q.q_ru)
                 OR (d."Language" = 'En' AND d."SearchVector" @@ q.q_en)
              )
            ORDER BY score DESC, d."PublishedAt" DESC NULLS LAST
            LIMIT @limit
            """;

        command.Parameters.Add(new NpgsqlParameter("query", trimmed));
        command.Parameters.Add(new NpgsqlParameter("types", typeFilter.Select(type => type.ToString()).ToArray())
        {
            NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Text,
        });
        command.Parameters.Add(new NpgsqlParameter("limit", limit));

        var results = new List<SearchResultDto>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var documentType = Enum.Parse<SearchDocumentType>(reader.GetString(0));
            results.Add(new SearchResultDto(
                documentType,
                reader.GetGuid(1),
                reader.GetString(2),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetFieldValue<DateTimeOffset>(4),
                reader.IsDBNull(5) ? 0 : reader.GetDouble(5)));
        }

        _logger.LogDebug("Search for '{Query}' returned {Count} hits", trimmed, results.Count);

        return results;
    }
}
