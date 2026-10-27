using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;

namespace StudentCourseManagement.Repositories;

public class CourseRepository
{
    private const string SelectSql = @"SELECT c.CourseId, c.CourseName, c.DurationMonths, c.Fee, c.DepartmentId, c.IsActive, d.DepartmentName
        FROM Courses c INNER JOIN Departments d ON c.DepartmentId=d.DepartmentId";

    public async Task<int> AddAsync(string name, int duration, decimal fee, int departmentId)
    {
        const string sql = @"INSERT INTO Courses (CourseName, DurationMonths, Fee, DepartmentId)
            VALUES (@Name, @Duration, @Fee, @DepartmentId); SELECT CAST(SCOPE_IDENTITY() AS INT);";
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        AddParameters(command, name, duration, fee, departmentId);
        await connection.OpenAsync();
        return (int)(await command.ExecuteScalarAsync() ?? 0);
    }

    public async Task<List<Course>> GetAllAsync()
    {
        var list = new List<Course>();
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(SelectSql + " ORDER BY c.CourseId;", connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(Map(reader));
        return list;
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(SelectSql + " WHERE c.CourseId=@Id;", connection);
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task<List<Course>> GetByDepartmentAsync(int departmentId)
    {
        var list = new List<Course>();
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(SelectSql + " WHERE c.DepartmentId=@DepartmentId ORDER BY c.CourseName;", connection);
        command.Parameters.Add("@DepartmentId", System.Data.SqlDbType.Int).Value = departmentId;
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(Map(reader));
        return list;
    }

    public async Task<bool> UpdateAsync(int id, string name, int duration, decimal fee, int departmentId)
    {
        const string sql = @"UPDATE Courses SET CourseName=@Name, DurationMonths=@Duration, Fee=@Fee, DepartmentId=@DepartmentId
            WHERE CourseId=@Id AND IsActive=1;";
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
        AddParameters(command, name, duration, fee, departmentId);
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        const string sql = "UPDATE Courses SET IsActive=0 WHERE CourseId=@Id AND IsActive=1;";
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<List<Course>> GetWithoutEnrollmentsAsync()
    {
        var list = new List<Course>();
        const string sql = @"SELECT c.CourseId, c.CourseName, c.DurationMonths, c.Fee, c.DepartmentId, c.IsActive, d.DepartmentName
            FROM Courses c INNER JOIN Departments d ON d.DepartmentId=c.DepartmentId
            LEFT JOIN Enrollments e ON e.CourseId=c.CourseId
            WHERE e.EnrollmentId IS NULL ORDER BY c.CourseName;";
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(Map(reader));
        return list;
    }

    public async Task<List<(string Name, int Count)>> GetEnrollmentCountAsync()
    {
        const string sql = @"SELECT c.CourseName, COUNT(e.EnrollmentId)
            FROM Courses c LEFT JOIN Enrollments e ON e.CourseId=c.CourseId
            GROUP BY c.CourseName ORDER BY c.CourseName;";
        var list = new List<(string, int)>();
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add((reader.GetString(0), reader.GetInt32(1)));
        return list;
    }

    private static void AddParameters(SqlCommand command, string name, int duration, decimal fee, int departmentId)
    {
        command.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 100).Value = name;
        command.Parameters.Add("@Duration", System.Data.SqlDbType.Int).Value = duration;
        command.Parameters.Add("@Fee", System.Data.SqlDbType.Decimal).Value = fee;
        command.Parameters["@Fee"].Precision = 10;
        command.Parameters["@Fee"].Scale = 2;
        command.Parameters.Add("@DepartmentId", System.Data.SqlDbType.Int).Value = departmentId;
    }

    private static Course Map(SqlDataReader r) => new Course(
        r.GetInt32(0), r.GetString(1), r.GetInt32(2), r.GetDecimal(3), r.GetInt32(4), r.GetBoolean(5))
        { DepartmentName = r.GetString(6) };
}
