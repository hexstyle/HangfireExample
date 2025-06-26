using Hangfire.PostgreSql;
using Npgsql;
namespace HangfireExample;
public class PostgresConnectionFactory : IConnectionFactory
{
    public PostgresConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }
    string _connectionString;

    public NpgsqlConnection GetOrCreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
