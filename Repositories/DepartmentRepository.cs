using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;

namespace StudentCourseManagement.Repositories;

public class DepartmentRepository
{
    public async Task<int> AddAsync(string name, string? location)
    {
        const string sql = "INSERT INTO Departments (DepartmentName, Location) VALUES (@Name, @Location); SELECT CAST(SCOPE_IDENTITY() AS INT);";
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 100).Value = name;
        command.Parameters.Add("@Location", System.Data.SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(location) ? DBNull.Value : location;
        await connection.OpenAsync();
        return (int)(await command.ExecuteScalarAsync() ?? 0);
    }

    public async Task<List<Department>> GetAllAsync()
    {
        const string sql = "SELECT DepartmentId, DepartmentName, Location, IsActive FROM Departments ORDER BY DepartmentName;";
        var list = new List<Department>();
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(Map(reader));
        return list;
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        const string sql = "SELECT DepartmentId, DepartmentName, Location, IsActive FROM Departments WHERE DepartmentId=@Id;";
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task<bool> UpdateAsync(int id, string name, string? location)
    {
        const string sql = "UPDATE Departments SET DepartmentName=@Name, Location=@Location WHERE DepartmentId=@Id AND IsActive=1;";
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
        command.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 100).Value = name;
        command.Parameters.Add("@Location", System.Data.SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(location) ? DBNull.Value : location;
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        const string sql = "UPDATE Departments SET IsActive=0 WHERE DepartmentId=@Id AND IsActive=1;";
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() > 0;
    }

    private static Department Map(SqlDataReader r) => new(
        r.GetInt32(0), r.GetString(1), r.IsDBNull(2) ? null : r.GetString(2), r.GetBoolean(3));
}
