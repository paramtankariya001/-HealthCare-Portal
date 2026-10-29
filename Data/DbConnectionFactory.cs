using Microsoft.Data.SqlClient;

namespace HealthCare_Portal.Data;

public sealed class DbConnectionFactory(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("The DefaultConnection connection string is missing.");

    public SqlConnection CreateConnection() => new(_connectionString);
}
