using Dapper;
using Hangfire.Console;
using Hangfire.PostgreSql;
using Hangfire.Server;

namespace HangfireExample.Jobs;

public class MaintenanceJob
{
    private readonly IConnectionFactory _connFactory;

    public MaintenanceJob(IConnectionFactory connFactory)
    {
        _connFactory = connFactory;
    }

    public async Task CleanupAsync(PerformContext context)
    {
        using var conn = _connFactory.GetOrCreateConnection();
        await conn.OpenAsync();

        // Очистка успешно завершённых задач
        const string deleteJobsSql = @"
            DELETE FROM hangfire.job 
            WHERE id IN (
                SELECT j.id
                FROM hangfire.job j
                JOIN hangfire.state s ON j.id = s.jobid
                WHERE s.name = 'Succeeded' AND s.createdat < NOW() - INTERVAL '1 minute'
            );";

        var deletedJobs = await conn.ExecuteAsync(deleteJobsSql);
        context.WriteLine($"🧹 Удалено {deletedJobs} завершённых задач старше 1 минуты");

        // Очистка неактивных серверов 
        const string deleteServersSql = @"
            DELETE FROM hangfire.server
            WHERE lastheartbeat < NOW() - INTERVAL '1 minute';";

        var deletedServers = await conn.ExecuteAsync(deleteServersSql);
        context.WriteLine($"🧹 Удалено {deletedServers} неактивных серверов");

        context.WriteLine("✅ Очистка Hangfire завершена.");
    }
}
