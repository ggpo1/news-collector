using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace NewsCollector.Infrastructure.Persistence;

public static class DatabaseMigrationExtensions
{
    // Shared across all services that call MigrateAsync on startup.
    private const long AdvisoryLockKey = 2_026_061_824L;

    public static async Task MigrateWithAdvisoryLockAsync(
        this DatabaseFacade database,
        CancellationToken cancellationToken = default)
    {
        await database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_lock({0})",
            [AdvisoryLockKey],
            cancellationToken);

        try
        {
            await database.MigrateAsync(cancellationToken);
        }
        finally
        {
            await database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_unlock({0})",
                [AdvisoryLockKey],
                cancellationToken);
        }
    }
}
