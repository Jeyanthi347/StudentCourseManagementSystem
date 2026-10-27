using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;

namespace StudentCourseManagement.Repositories;

public class AuditLogRepository
{
    public async Task AddAsync(string entityName, int entityId, string action, string? details,
        SqlConnection? existingConnection = null, SqlTransaction? transaction = null)
    {
        const string sql = "INSERT INTO AuditLogs (EntityName, EntityId, Action, Details) VALUES (@EntityName, @EntityId, @Action, @Details);";
        if (existingConnection != null)
        {
            using var command = new SqlCommand(sql, existingConnection, transaction);
            AddParameters(command, entityName, entityId, action, details);
            await command.ExecuteNonQueryAsync();
            return;
        }

        using var connection = DatabaseHelper.CreateConnection();
        using var standaloneCommand = new SqlCommand(sql, connection);
        AddParameters(standaloneCommand, entityName, entityId, action, details);
        await connection.OpenAsync();
        await standaloneCommand.ExecuteNonQueryAsync();
    }

    public async Task<List<AuditLog>> GetRecentAsync(int count)
    {
        const string sql = "SELECT TOP (@Count) AuditId, EntityName, EntityId, Action, Details, CreatedAt FROM AuditLogs ORDER BY CreatedAt DESC, AuditId DESC;";
        var list = new List<AuditLog>();
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Count", System.Data.SqlDbType.Int).Value = count;
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new AuditLog
            {
                AuditId = reader.GetInt64(0),
                EntityName = reader.GetString(1),
                EntityId = reader.GetInt32(2),
                Action = reader.GetString(3),
                Details = reader.IsDBNull(4) ? null : reader.GetString(4),
                CreatedAt = reader.GetDateTime(5)
            });
        }
        return list;
    }

    private static void AddParameters(SqlCommand command, string entityName, int entityId, string action, string? details)
    {
        command.Parameters.Add("@EntityName", System.Data.SqlDbType.NVarChar, 50).Value = entityName;
        command.Parameters.Add("@EntityId", System.Data.SqlDbType.Int).Value = entityId;
        command.Parameters.Add("@Action", System.Data.SqlDbType.NVarChar, 20).Value = action;
        command.Parameters.Add("@Details", System.Data.SqlDbType.NVarChar, 500).Value = string.IsNullOrWhiteSpace(details) ? DBNull.Value : details;
    }
}
