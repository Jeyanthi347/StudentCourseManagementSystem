using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;

namespace StudentCourseManagement.Repositories;

public class EnrollmentRepository
{
    private const string SelectSql = @"SELECT e.EnrollmentId, e.StudentId, e.CourseId, e.EnrolledAt, e.Status, s.Name, c.CourseName
        FROM Enrollments e INNER JOIN Students s ON s.StudentId=e.StudentId
        INNER JOIN Courses c ON c.CourseId=e.CourseId";

    public async Task<int> AddWithAuditAsync(int studentId, int courseId, AuditLogRepository auditRepository)
    {
        using var connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();
        using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
        try
        {
            const string sql = @"INSERT INTO Enrollments (StudentId, CourseId)
                VALUES (@StudentId, @CourseId); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            int enrollmentId;
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@StudentId", System.Data.SqlDbType.Int).Value = studentId;
                command.Parameters.Add("@CourseId", System.Data.SqlDbType.Int).Value = courseId;
                enrollmentId = (int)(await command.ExecuteScalarAsync() ?? 0);
            }

            await auditRepository.AddAsync("Enrollment", enrollmentId, "Created",
                $"Student {studentId} enrolled in course {courseId}", connection, transaction);
            await transaction.CommitAsync();
            return enrollmentId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Enrollment>> GetAllAsync()
    {
        var list = new List<Enrollment>();
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(SelectSql + " ORDER BY e.EnrollmentId DESC;", connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(Map(reader));
        return list;
    }

    public async Task<List<Enrollment>> GetForStudentAsync(int studentId)
    {
        var list = new List<Enrollment>();
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(SelectSql + " WHERE e.StudentId=@Id ORDER BY e.EnrolledAt DESC;", connection);
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = studentId;
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(Map(reader));
        return list;
    }

    public async Task<List<Enrollment>> GetForCourseAsync(int courseId)
    {
        var list = new List<Enrollment>();
        using var connection = DatabaseHelper.CreateConnection();
        using var command = new SqlCommand(SelectSql + " WHERE e.CourseId=@Id ORDER BY e.EnrolledAt DESC;", connection);
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = courseId;
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(Map(reader));
        return list;
    }

    public async Task<bool> ChangeStatusAsync(int id, string status, AuditLogRepository auditRepository)
    {
        using var connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();
        using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
        try
        {
            const string sql = "UPDATE Enrollments SET Status=@Status WHERE EnrollmentId=@Id AND Status=N'Enrolled';";
            using var command = new SqlCommand(sql, connection, transaction);
            command.Parameters.Add("@Status", System.Data.SqlDbType.NVarChar, 20).Value = status;
            command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
            bool changed = await command.ExecuteNonQueryAsync() > 0;
            if (changed)
                await auditRepository.AddAsync("Enrollment", id, status, $"Enrollment status changed to {status}", connection, transaction);
            await transaction.CommitAsync();
            return changed;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static Enrollment Map(SqlDataReader r) => new Enrollment(
        r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetDateTime(3), r.GetString(4))
        { StudentName = r.GetString(5), CourseName = r.GetString(6) };
}
