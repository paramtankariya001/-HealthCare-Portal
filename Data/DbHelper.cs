using Microsoft.Data.SqlClient;
using System.Data;

namespace HealthCare_Portal.Data;

public sealed class DbHelper(DbConnectionFactory connectionFactory, ILogger<DbHelper> logger)
{
    public async Task<int> ExecuteNonQueryAsync(string procedure, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await using var command = CreateCommand(connection, procedure, parameters);
        try { await connection.OpenAsync(cancellationToken); return await command.ExecuteNonQueryAsync(cancellationToken); }
        catch (SqlException ex) { logger.LogError(ex, "Stored procedure {Procedure} failed.", procedure); throw; }
    }

    public async Task<object?> ExecuteScalarAsync(string procedure, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await using var command = CreateCommand(connection, procedure, parameters);
        try { await connection.OpenAsync(cancellationToken); var value = await command.ExecuteScalarAsync(cancellationToken); return value == DBNull.Value ? null : value; }
        catch (SqlException ex) { logger.LogError(ex, "Stored procedure {Procedure} failed.", procedure); throw; }
    }

    public async Task<List<Dictionary<string, object?>>> ExecuteReaderAsync(string procedure, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await using var command = CreateCommand(connection, procedure, parameters);
        try
        {
            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken);
            var rows = new List<Dictionary<string, object?>>();
            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < reader.FieldCount; i++) row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                rows.Add(row);
            }
            return rows;
        }
        catch (SqlException ex) { logger.LogError(ex, "Stored procedure {Procedure} failed.", procedure); throw; }
    }

    public Task<List<Dictionary<string, object?>>> ExecuteStoredProcedureAsync(string procedure, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default)
        => ExecuteReaderAsync(procedure, parameters, cancellationToken);

    private static SqlCommand CreateCommand(SqlConnection connection, string procedure, IEnumerable<SqlParameter>? parameters)
    {
        var command = new SqlCommand(procedure, connection) { CommandType = CommandType.StoredProcedure };
        if (parameters is not null) command.Parameters.AddRange(parameters.ToArray());
        return command;
    }

    public static SqlParameter Parameter(string name, object? value) => new(name, value ?? DBNull.Value);
}
