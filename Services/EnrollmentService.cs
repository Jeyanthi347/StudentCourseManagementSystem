using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class EnrollmentService(EnrollmentRepository repository, StudentRepository students,
    CourseRepository courses, AuditLogRepository audit)
{
    public Task<List<Enrollment>> GetAllAsync() => repository.GetAllAsync();
    public Task<List<Enrollment>> GetForStudentAsync(int id) => repository.GetForStudentAsync(id);
    public Task<List<Enrollment>> GetForCourseAsync(int id) => repository.GetForCourseAsync(id);

    public async Task<string> EnrollAsync(int studentId, int courseId)
    {
        var student = await students.GetByIdAsync(studentId);
        if (student == null || !student.IsActive) return "Student not found or inactive.";
        var course = await courses.GetByIdAsync(courseId);
        if (course == null || !course.IsActive) return "Course not found or inactive.";
        int id = await repository.AddWithAuditAsync(studentId, courseId, audit);
        return $"Enrollment created. ID: {id}";
    }

    public async Task<string> ChangeStatusAsync(int id, string status)
    {
        if (status != "Completed" && status != "Cancelled") return "Invalid enrollment status.";
        bool changed = await repository.ChangeStatusAsync(id, status, audit);
        return changed ? $"Enrollment marked {status.ToLower()}." : "Enrollment not found or is no longer active.";
    }
}
