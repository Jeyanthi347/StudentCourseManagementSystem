using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;

namespace StudentCourseManagement.Repositories;

public class StudentRepository
{
    private const string SelectSql = @"SELECT s.StudentId, s.Name, s.Email, s.Age, s.DepartmentId,
        s.AdmissionDate, s.IsActive, d.DepartmentName
        FROM Students s INNER JOIN Departments d ON s.DepartmentId=d.DepartmentId";

    public async Task<int> AddAsync(string name, string email, int age, int departmentId)
    {
        const string sql = @"INSERT INTO Students (Name, Email, Age, DepartmentId)
            VALUES (@Name, @Email, @Age, @DepartmentId); SELECT CAST(SCOPE_IDENTITY() AS INT);";
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        AddStudentParameters(command, name, email, age, departmentId);
        await connection.OpenAsync();
        return (int)(await command.ExecuteScalarAsync() ?? 0);
    }

    public async Task<List<Student>> GetAllAsync()
    {
        var list = new List<Student>();
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(SelectSql + " ORDER BY s.StudentId;", connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(Map(reader));
        return list;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(SelectSql + " WHERE s.StudentId=@Id;", connection);
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task<List<Student>> SearchByNameAsync(string name)
    {
        var list = new List<Student>();
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(SelectSql + " WHERE s.Name LIKE @Name ORDER BY s.Name;", connection);
        command.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 100).Value = "%" + name + "%";
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(Map(reader));
        return list;
    }

    public async Task<List<Student>> GetByDepartmentAsync(int departmentId)
    {
        var list = new List<Student>();
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(SelectSql + " WHERE s.DepartmentId=@DepartmentId ORDER BY s.Name;", connection);
        command.Parameters.Add("@DepartmentId", System.Data.SqlDbType.Int).Value = departmentId;
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(Map(reader));
        return list;
    }

    public async Task<bool> UpdateAsync(int id, string name, string email, int age, int departmentId)
    {
        const string sql = @"UPDATE Students SET Name=@Name, Email=@Email, Age=@Age, DepartmentId=@DepartmentId
            WHERE StudentId=@Id AND IsActive=1;";
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
        AddStudentParameters(command, name, email, age, departmentId);
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        const string sql = "UPDATE Students SET IsActive=0 WHERE StudentId=@Id AND IsActive=1;";
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<List<Student>> GetWithoutEnrollmentsAsync()
    {
        var list = new List<Student>();
        const string sql = @"SELECT s.StudentId, s.Name, s.Email, s.Age, s.DepartmentId, s.AdmissionDate, s.IsActive, d.DepartmentName
            FROM Students s INNER JOIN Departments d ON d.DepartmentId=s.DepartmentId
            WHERE NOT EXISTS (SELECT 1 FROM Enrollments e WHERE e.StudentId=s.StudentId)
            ORDER BY s.Name;";
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(Map(reader));
        return list;
    }

    public async Task<List<(string Name, int Count)>> GetStudentCountByDepartmentAsync()
    {
        const string sql = @"SELECT d.DepartmentName, COUNT(s.StudentId)
            FROM Departments d LEFT JOIN Students s ON s.DepartmentId=d.DepartmentId
            GROUP BY d.DepartmentName ORDER BY d.DepartmentName;";
        var list = new List<(string, int)>();
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add((reader.GetString(0), reader.GetInt32(1)));
        return list;
    }

    private static void AddStudentParameters(SqlCommand command, string name, string email, int age, int departmentId)
    {
        command.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 100).Value = name;
        command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 255).Value = email;
        command.Parameters.Add("@Age", System.Data.SqlDbType.Int).Value = age;
        command.Parameters.Add("@DepartmentId", System.Data.SqlDbType.Int).Value = departmentId;
    }

    private static Student Map(SqlDataReader r) => new Student(
        r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetInt32(3), r.GetInt32(4),
        r.GetDateTime(5), r.GetBoolean(6)) { DepartmentName = r.GetString(7) };
}
